using System;
using System.Collections.Generic;

namespace EnterpriseDataAnalyst.Domain
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }

        public ICollection<Sales> Sales { get; set; } = new List<Sales>();
    }
}
