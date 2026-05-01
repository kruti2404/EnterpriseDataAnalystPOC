using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EnterpriseDataAnalyst.Application.DTOs;
using EnterpriseDataAnalyst.Application.Interfaces;
using EnterpriseDataAnalyst.Infrastructure.Data;

namespace EnterpriseDataAnalyst.Infrastructure.Services;

public class DataAgent : IDataAgent
{
    private readonly AppDbContext _dbContext;

    public DataAgent(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SalesSummary>> FetchDataAsync(Plan plan)
    {
        // Fetch sales broken down by Product + Category + Year for YoY / product-category questions
        var byProductYear = await _dbContext.Sales
            .Include(s => s.Product)
            .GroupBy(s => new { s.Product!.Name, s.Product.Category, s.Date.Year })
            .Select(g => new SalesSummary
            {
                ProductName = g.Key.Name,
                Category = g.Key.Category,
                Year = g.Key.Year,
                TotalSales = g.Sum(s => s.Amount),
                TotalQuantity = g.Sum(s => s.Quantity),
                Period = g.Key.Year.ToString()
            })
            .OrderBy(x => x.Category).ThenBy(x => x.ProductName).ThenBy(x => x.Year)
            .ToListAsync();

        // Fetch sales broken down by Region + Year for regional trend questions
        var byRegionYear = await _dbContext.Sales
            .GroupBy(s => new { s.Region, s.Date.Year })
            .Select(g => new SalesSummary
            {
                Region = g.Key.Region,
                Year = g.Key.Year,
                TotalSales = g.Sum(s => s.Amount),
                TotalQuantity = g.Sum(s => s.Quantity),
                Period = g.Key.Year.ToString()
            })
            .OrderBy(x => x.Region).ThenBy(x => x.Year)
            .ToListAsync();

        return byProductYear.Concat(byRegionYear).ToList();
    }
}
