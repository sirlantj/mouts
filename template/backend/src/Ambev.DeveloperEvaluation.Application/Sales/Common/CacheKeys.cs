namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public static class CacheKeys
{
    public const string SalePrefix = "sale:";
    public const string SalesListPrefix = "sales:";

    public static string ForSale(Guid id) => $"{SalePrefix}{id}";

    public static string ForSalesList(int page, int size, string? order, string? customerName, string? branchName, string? status, DateTime? startDate, DateTime? endDate)
    {
        var key = $"{SalesListPrefix}p{page}_s{size}";
        if (!string.IsNullOrEmpty(order)) key += $"_o{order}";
        if (!string.IsNullOrEmpty(customerName)) key += $"_cn{customerName}";
        if (!string.IsNullOrEmpty(branchName)) key += $"_bn{branchName}";
        if (!string.IsNullOrEmpty(status)) key += $"_st{status}";
        if (startDate.HasValue) key += $"_sd{startDate.Value:yyyyMMdd}";
        if (endDate.HasValue) key += $"_ed{endDate.Value:yyyyMMdd}";
        return key;
    }
}
