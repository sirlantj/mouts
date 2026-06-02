namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSales;

public class GetSalesRequest
{
    public int? _page { get; set; }
    public int? _size { get; set; }
    public string? _order { get; set; }
    public string? CustomerName { get; set; }
    public string? BranchName { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
