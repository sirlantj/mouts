using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.MongoDB.Documents;
using global::MongoDB.Bson;
using global::MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.ORM.MongoDB.Repositories;

public class SaleReadRepository : ISaleReadRepository
{
    private readonly IMongoCollection<SaleReadModel> _collection;

    public SaleReadRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<SaleReadModel>("sales_read");
    }

    public async Task UpsertAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        var document = new SaleReadModel
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            SaleDate = sale.SaleDate,
            CustomerExternalId = sale.CustomerExternalId,
            CustomerName = sale.CustomerName,
            BranchExternalId = sale.BranchExternalId,
            BranchName = sale.BranchName,
            TotalAmount = sale.TotalAmount,
            Status = sale.Status.ToString(),
            CreatedAt = sale.CreatedAt,
            UpdatedAt = sale.UpdatedAt,
            Items = sale.Items.Select(i => new SaleItemReadModel
            {
                Id = i.Id,
                ProductExternalId = i.ProductExternalId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Discount = i.Discount,
                TotalAmount = i.TotalAmount,
                IsCancelled = i.IsCancelled
            }).ToList()
        };

        var filter = Builders<SaleReadModel>.Filter.Eq(s => s.Id, sale.Id);
        await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<SaleReadModel>.Filter.Eq(s => s.Id, id);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (document == null) return null;
        return MapToSale(document);
    }

    public async Task<(IEnumerable<Sale> Items, int TotalCount)> GetPaginatedAsync(
        int page, int size, string? order,
        string? customerName, string? branchName, string? status,
        DateTime? startDate, DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<SaleReadModel>.Filter;
        var filters = new List<FilterDefinition<SaleReadModel>>();

        if (!string.IsNullOrWhiteSpace(customerName))
            filters.Add(filterBuilder.Regex(s => s.CustomerName, new BsonRegularExpression(customerName, "i")));

        if (!string.IsNullOrWhiteSpace(branchName))
            filters.Add(filterBuilder.Regex(s => s.BranchName, new BsonRegularExpression(branchName, "i")));

        if (!string.IsNullOrWhiteSpace(status))
            filters.Add(filterBuilder.Eq(s => s.Status, status));

        if (startDate.HasValue)
            filters.Add(filterBuilder.Gte(s => s.SaleDate, startDate.Value));

        if (endDate.HasValue)
            filters.Add(filterBuilder.Lte(s => s.SaleDate, endDate.Value));

        var combinedFilter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;

        var sortDefinition = BuildSort(order);

        var totalCount = await _collection.CountDocumentsAsync(combinedFilter, cancellationToken: cancellationToken);

        var documents = await _collection
            .Find(combinedFilter)
            .Sort(sortDefinition)
            .Skip((page - 1) * size)
            .Limit(size)
            .ToListAsync(cancellationToken);

        var sales = documents.Select(MapToSale);
        return (sales, (int)totalCount);
    }

    private static SortDefinition<SaleReadModel> BuildSort(string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return Builders<SaleReadModel>.Sort.Descending(s => s.SaleDate);

        var parts = order.Split(' ', 2);
        var field = parts[0];
        var descending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return descending
            ? Builders<SaleReadModel>.Sort.Descending(field)
            : Builders<SaleReadModel>.Sort.Ascending(field);
    }

    private static Sale MapToSale(SaleReadModel doc)
    {
        var sale = new Sale
        {
            Id = doc.Id,
            SaleNumber = doc.SaleNumber,
            SaleDate = doc.SaleDate,
            CustomerExternalId = doc.CustomerExternalId,
            CustomerName = doc.CustomerName,
            BranchExternalId = doc.BranchExternalId,
            BranchName = doc.BranchName,
            CreatedAt = doc.CreatedAt,
            UpdatedAt = doc.UpdatedAt
        };

        var items = doc.Items.Select(i =>
        {
            var item = new SaleItem
            {
                Id = i.Id,
                SaleId = doc.Id,
                ProductExternalId = i.ProductExternalId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            };
            item.CalculateDiscount();

            // Preserve the cancelled-item state from the read model — otherwise
            // every item came back as IsCancelled=false and the UI offered to
            // cancel items that were already cancelled (which the backend then
            // rejected with 400).
            if (i.IsCancelled)
                item.Cancel();

            return item;
        }).ToList();

        sale.SetItems(items);

        if (Enum.TryParse<SaleStatus>(doc.Status, out var parsedStatus) && parsedStatus == SaleStatus.Cancelled)
            sale.Cancel();

        return sale;
    }
}
