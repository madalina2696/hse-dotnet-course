using System.Collections.Generic;

namespace TheMiddleman.Entity
{
    public class Intermediary
    {
        public int StorageCapacity { get; set; } = 100;
        
        
        public Intermediary(string name, string company, int accountBalance)
        {
            Name = name;
            Company = company;
            AccountBalance = accountBalance;
        }

        public string? Name { get; set; }
        public string? Company { get; set; }
        public int AccountBalance { get; set; }
        public Dictionary<Product, int> OwnedProducts { get; set; } = new Dictionary<Product, int>();
    }
}
