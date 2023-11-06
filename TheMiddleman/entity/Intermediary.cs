using System.Collections.Generic;

namespace TheMiddleman.Entity
{
    public class Intermediary
    {

        public int StorageCapacity { get; set; } = 100; // Default storage capacity

        // Existing properties and methods...

        // Add a method to calculate used storage
        public int CalculateUsedStorage()
        {
            int usedStorage = 0;
            foreach (var entry in OwnedProducts)
            {
                usedStorage += entry.Value; // Assuming each product takes up 1 unit of storage
            }
            return usedStorage;
        }
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
