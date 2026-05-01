namespace EnterpriseDataAnalyst.Domain
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int StockQty { get; set; }

        public ICollection<Sales> Sales { get; set; } = new List<Sales>();
    }
}
