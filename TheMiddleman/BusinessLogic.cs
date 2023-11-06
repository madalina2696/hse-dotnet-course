using System;
using System.Collections.Generic;
using TheMiddleman.Entity;

class BusinessLogic
{
    private Random random = new Random();
    private List<Product> products;

    public BusinessLogic()
    {
        ProductsParser parser = new ProductsParser("produkte.yml");
        products = parser.ReadProducts();
    }

    public Intermediary CreateTrader(int position, UserInterface ui)
    {
        string traderName = ui.FetchTraderName(position);
        string firmName = ui.FetchFirmName(traderName);
        int initialBalance = GetInitialBalance(ui);
        return new Intermediary(traderName, firmName, initialBalance);
    }

    private int GetInitialBalance(UserInterface ui)
    {
        return ui.GetInitialBalance();
    }

    public List<Intermediary> CreateTraderList(UserInterface ui)
    {
        List<Intermediary> participants = new List<Intermediary>();
        int traderCount = ui.QueryParticipantCount();
        for (int i = 1; i <= traderCount; i++)
        {
            participants.Add(CreateTrader(i, ui));
        }
        return participants;
    }

    public Product CreateProduct(int id, string name, int durability)
    {
        return new Product { Id = id, Name = name, Durability = durability };
    }

    public void ExecutePurchase(Intermediary trader, Product selectedProduct, int quantity)
    {

        int totalCost = quantity * selectedProduct.BasePrice;
        int usedStorage = trader.CalculateUsedStorage();

        if (trader.AccountBalance < totalCost)
        {
            Console.WriteLine("Nicht genügend Geld vorhanden.\n");
            return;
        }
        if (usedStorage + quantity > trader.StorageCapacity)
        {
            Console.WriteLine("Nicht genug Lagerplatz verfügbar.\n");
            return;
        }
        if (trader.AccountBalance < totalCost)
        {
            Console.WriteLine("Nicht genügend Geld vorhanden.\n");
            return;
        }
        trader.AccountBalance -= totalCost;
        if (trader.OwnedProducts.ContainsKey(selectedProduct))
        {
            trader.OwnedProducts[selectedProduct] += quantity;
        }
        else
        {
            trader.OwnedProducts.Add(selectedProduct, quantity);
        }

        if (selectedProduct.Availability < quantity)
        {
            Console.WriteLine("Nicht genügend Produkt verfügbar.");
            return;
        }
        Console.WriteLine($"Kauf erfolgreich. Neuer Kontostand: ${trader.AccountBalance}");
        selectedProduct.Availability -= quantity;
    }

    public void ExecuteSale(Intermediary trader, Product selectedProduct, int quantityToSell)
    {
        if (!trader.OwnedProducts.ContainsKey(selectedProduct) || trader.OwnedProducts[selectedProduct] < quantityToSell)
        {
            Console.WriteLine("Nicht genügend Ware vorhanden.");
            return;
        }
        int saleRevenue = quantityToSell * selectedProduct.SellingPrice;
        trader.AccountBalance += saleRevenue;
        trader.OwnedProducts[selectedProduct] -= quantityToSell;
        if (trader.OwnedProducts[selectedProduct] == 0)
        {
            trader.OwnedProducts.Remove(selectedProduct);
        }
        Console.WriteLine($"Verkauf erfolgreich. Neuer Kontostand: ${trader.AccountBalance}");
    }

    public void RotateIntermediary(List<Intermediary> traders)
    {
        if (traders.Count > 1)
        {
            var firstTrader = traders[0];
            traders.RemoveAt(0);
            traders.Add(firstTrader);
        }
    }

    public void RunDayCycle(List<Intermediary> traders, UserInterface ui, ref int currentDay)
    {
        if (currentDay > 1)
        {
            UpdateProductAvailability();
        }
        foreach (Intermediary trader in traders)
        {
            ui.DisplayOptions(trader, ref currentDay);
        }
        RotateIntermediary(traders);
        currentDay++;
    }

    public void UpdateProductAvailability()
    {
        foreach (Product product in products)
        {
            int maxAvailability = product.MaxProductionRate * product.Durability;
            int productionToday = random.Next(product.MinProductionRate, product.MaxProductionRate + 1);
            product.Availability += productionToday;
            product.Availability = Math.Max(0, product.Availability);
            product.Availability = Math.Min(maxAvailability, product.Availability);
        }
    }

    public List<Product> GetProducts()
    {
        return products;
    }
}