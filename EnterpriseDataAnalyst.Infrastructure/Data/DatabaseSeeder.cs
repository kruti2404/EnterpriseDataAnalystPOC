using EnterpriseDataAnalyst.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseDataAnalyst.Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Products.AnyAsync() || await context.Customers.AnyAsync() || await context.Sales.AnyAsync())
            {
                return;
            }

            string basePath = "";
            var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (currentDir != null)
            {
                var targetFolder = Path.Combine(currentDir.FullName, "EnterpriseDataAnalyst.Infrastructure", "Data", "DataSeederCsv");
                if (Directory.Exists(targetFolder))
                {
                    basePath = targetFolder;
                    break;
                }
                currentDir = currentDir.Parent;
            }

            if (string.IsNullOrEmpty(basePath))
            {
                throw new DirectoryNotFoundException("Could not find the DataSeederCsv directory.");
            }

            var productsPath = Path.Combine(basePath, "synthetic_data_Products.csv");
            var customersPath = Path.Combine(basePath, "synthetic_data_Customers.csv");
            var salesPath = Path.Combine(basePath, "synthetic_data_Sales.csv");

            var products = File.ReadAllLines(productsPath)
                .Skip(1)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Split(','))
                .Select(parts => new Product
                {
                    Name = parts[1],
                    Category = parts[2],
                    UnitPrice = decimal.Parse(parts[3]),
                    StockQty = int.Parse(parts[4])
                }).ToList();

            var customers = File.ReadAllLines(customersPath)
                .Skip(1)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Split(','))
                .Select(parts => new Customer
                {
                    Name = parts[1],
                    Email = parts[2],
                    Region = parts[3],
                    JoinDate = DateTime.Parse(parts[4])
                }).ToList();

            var salesData = File.ReadAllLines(salesPath)
                .Skip(1)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Split(','))
                .Select(parts => new Sales
                {
                    Date = DateTime.Parse(parts[1]),
                    Region = parts[2],
                    ProductId = int.Parse(parts[3]),
                    CustomerId = int.Parse(parts[4]),
                    Quantity = int.Parse(parts[5]),
                    Amount = decimal.Parse(parts[6])
                }).ToList();

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();

            const int chunkSize = 1000;
            for (int i = 0; i < salesData.Count; i += chunkSize)
            {
                var chunk = salesData.Skip(i).Take(chunkSize);
                await context.Sales.AddRangeAsync(chunk);
                await context.SaveChangesAsync();
            }
        }
    }
}
