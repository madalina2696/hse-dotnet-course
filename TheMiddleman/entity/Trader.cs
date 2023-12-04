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
        public Dictionary<Product, decimal> ProductDiscounts { get; private set; } = new Dictionary<Product, decimal>();

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

        public void CalculateDiscountsForAllProducts(List<Product> products)
        {
            foreach (var product in products)
            {
                ProductDiscounts[product] = CalculateDiscountForProduct(product);
            }
        }
        public decimal CalculateDiscountForProduct(Product product)
        {
            OwnedProducts.TryGetValue(product, out int quantityOwned);

            if (quantityOwned >= 75)
            {
                return 0.10m;
            }
            else if (quantityOwned >= 50)
            {
                return 0.05m;
            }
            else if (quantityOwned >= 25)
            {
                return 0.02m;
            }
            else
            {
                return 0.00m;
            }
        }

        public void InitializeDailyDiscounts()
        {
            ProductDiscounts.Clear();
            foreach (var product in OwnedProducts.Keys)
            {
                ProductDiscounts[product] = CalculateDiscountForProduct(product);
            }
        }
    }
}