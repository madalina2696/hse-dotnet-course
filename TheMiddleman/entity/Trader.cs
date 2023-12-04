using System.Collections.Generic;

namespace TheMiddleman.Entity
{
    public class Trader
    {
        public int StorageCapacity { get; set; } = 100;
        public string? Name { get; set; }
        public string? Company { get; set; }
        public int AccountBalance { get; set; }
        public int StartingBalance { get; set; }
        public int Expenses { get; set; }
        public int Revenue { get; set; }
        public int StorageCosts { get; set; }
        public Dictionary<Product, int> OwnedProducts { get; set; } = new Dictionary<Product, int>();

        public Trader(string name, string company, int accountBalance)
        {
            Name = name;
            Company = company;
            AccountBalance = accountBalance;
            StartingBalance = accountBalance;
        }
        public void UpdateExpenses(int amount)
        {
            Expenses += amount;
        }

        public void UpdateRevenue(int amount)
        {
            Revenue += amount;
        }

        public void UpdateStorageCosts(int amount)
        {
            StorageCosts += amount;
        }

        public void ResetDailyFinances()
        {
            StartingBalance = AccountBalance;
            Expenses = 0;
            Revenue = 0;
            StorageCosts = 0;
        }
    }
}