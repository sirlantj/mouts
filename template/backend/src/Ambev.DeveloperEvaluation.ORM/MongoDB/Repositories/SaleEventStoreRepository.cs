using System.Text.Json;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.MongoDB.Documents;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.ORM.MongoDB.Repositories;

public class SaleEventStoreRepository : ISaleEventStore
{
    private readonly IMongoCollection<SaleEventDocument> _collection;

    public SaleEventStoreRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<SaleEventDocument>("sale_events");
    }

    public async Task StoreEventAsync(string eventType, object domainEvent, Guid saleId, string saleNumber, CancellationToken cancellationToken = default)
    {
        var document = new SaleEventDocument
        {
            SaleId = saleId,
            SaleNumber = saleNumber,
            EventType = eventType,
            EventData = JsonSerializer.Serialize(domainEvent),
            OccurredAt = DateTime.UtcNow
        };

        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<object>> GetEventsAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<SaleEventDocument>.Filter.Eq(e => e.SaleId, saleId);
        var sort = Builders<SaleEventDocument>.Sort.Ascending(e => e.OccurredAt);

        var results = await _collection.Find(filter).Sort(sort).ToListAsync(cancellationToken);
        return results;
    }
}
