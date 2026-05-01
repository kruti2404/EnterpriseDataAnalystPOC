namespace EnterpriseDataAnalyst.Application.DTOs;

public class SalesSummary
{
    public string Region { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalSales { get; set; }
    public int TotalQuantity { get; set; }
    public string Period { get; set; } = string.Empty;
}
