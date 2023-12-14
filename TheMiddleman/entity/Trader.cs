using System.Collections.Generic;

namespace TheMiddleman.Entity
{
    public class Trader
    {
        public int StorageCapacity { get; set; } = 100;
        public string? Name { get; set; }
        public string? Company { get; set; }
        public double AccountBalance { get; set; }
        public double StartingBalance { get; set; }
        public double Expenses { get; set; }
        public double Revenue { get; set; }
        public double StorageCosts { get; set; }
        public Loan? CurrentLoan { get; set; }
        public bool LoanRepaymentToday { get; set; } = false;
        public Dictionary<Product, int> OwnedProducts { get; set; } = new Dictionary<Product, int>();
        public Dictionary<Product, double> ProductDiscounts { get; private set; } = new Dictionary<Product, double>();

        public Trader(string name, string company, double accountBalance)
        {
            Name = name;
            Company = company;
            AccountBalance = accountBalance;
            StartingBalance = accountBalance;
        }
        public class Loan
        {
            public double Amount { get; set; }
            public double RepaymentAmount { get; set; }
            public int DueDay { get; set; }
        }

        public void UpdateExpenses(double amount)
        {
            Expenses += amount;
        }

        public void UpdateRevenue(double amount)
        {
            Revenue += amount;
        }

        public void UpdateStorageCosts(double amount)
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

        public double CalculateDiscountForProduct(Product product)
        {
            OwnedProducts.TryGetValue(product, out int quantityOwned);
            if (quantityOwned >= 75)
            {
                return 0.10;
            }
            else if (quantityOwned >= 50)
            {
                return 0.05;
            }
            else if (quantityOwned >= 25)
            {
                return 0.02;
            }
            else
            {
                return 0.00;
            }
        }
    }
}