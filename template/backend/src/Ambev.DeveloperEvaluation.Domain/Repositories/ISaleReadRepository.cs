using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface ISaleReadRepository
{
    Task UpsertAsync(Sale sale, CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Sale> Items, int TotalCount)> GetPaginatedAsync(
        int page, int size, string? order,
        string? customerName, string? branchName, string? status,
        DateTime? startDate, DateTime? endDate,
        CancellationToken cancellationToken = default);
}
