using System;

namespace EnterpriseDataAnalyst.Domain
{
    public class Sales
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Region { get; set; } = string.Empty;
        
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
