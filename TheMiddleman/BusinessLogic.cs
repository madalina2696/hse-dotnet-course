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

    public Trader CreateTrader(int position, UserInterface ui)
    {
        string traderName = ui.ReadTraderNameAtPosition(position);
        string firmName = ui.FetchFirmName(traderName);
        int initialBalance = GetInitialBalance(ui);
        return new Trader(traderName, firmName, initialBalance);
    }

    private int GetInitialBalance(UserInterface ui)
    {
        return ui.AssignStartingBalance(ui.AskForDifficultyLevel());
    }

    public List<Trader> CreateTraders(UserInterface ui)
    {
        List<Trader> participants = new List<Trader>();
        int traderCount = ui.ReadParticipantCount();
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

    public void Purchase(Trader trader, Product selectedProduct, int quantity)
    {
        int totalCost = quantity * selectedProduct.BasePrice;
        int usedStorage = CalculateUsedStorage(trader);
        if (trader.AccountBalance < totalCost)
        {
            UserInterface.ShowError("Nicht genügend Geld vorhanden.");
            return;
        }
        if (usedStorage + quantity > trader.StorageCapacity)
        {
            UserInterface.ShowError("Nicht genug Lagerplatz verfügbar.");
            return;
        }
        if (trader.AccountBalance < totalCost)
        {
            UserInterface.ShowError("Nicht genügend Geld vorhanden.");
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
            UserInterface.ShowError("Nicht genügend Produkt verfügbar.");
            return;
        }
        UserInterface.ShowMessage($"Kauf erfolgreich. Neuer Kontostand: ${trader.AccountBalance}");
        selectedProduct.Availability -= quantity;
    }

    public void Sale(Trader trader, Product selectedProduct, int quantityToSell)
    {
        if (!trader.OwnedProducts.ContainsKey(selectedProduct) || trader.OwnedProducts[selectedProduct] < quantityToSell)
        {
            UserInterface.ShowError("Nicht genügend Ware vorhanden.");
            return;
        }
        int saleRevenue = quantityToSell * selectedProduct.SellingPrice;
        trader.AccountBalance += saleRevenue;
        trader.OwnedProducts[selectedProduct] -= quantityToSell;
        if (trader.OwnedProducts[selectedProduct] == 0)
        {
            trader.OwnedProducts.Remove(selectedProduct);
        }
        UserInterface.ShowMessage($"Verkauf erfolgreich. Neuer Kontostand: ${trader.AccountBalance}");
    }

    public void RotateTrader(List<Trader> traders)
    {
        if (traders.Count > 1)
        {
            var firstTrader = traders[0];
            traders.RemoveAt(0);
            traders.Add(firstTrader);
        }
    }

    public void RunDayCycle(List<Trader> traders, UserInterface ui, ref int currentDay)
    {
        CheckForBankruptcy(traders);
        if (currentDay > 1)
        {
            UpdateProductAvailability();
            UpdateProductPrices();
        }
        foreach (Trader trader in traders)
        {
            ui.DisplayOptions(trader, ref currentDay);
            ApplyStorageCosts(trader);
        }
        RotateTrader(traders);
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

    public void UpdateProductPrices()
    {
        foreach (Product product in products)
        {
            double priceChangePercent;
            double MaxAvailability = product.MaxProductionRate * product.Durability;
            double AvailabilityProcents = product.Availability / MaxAvailability;
            double newBuyingPrice;
            if (AvailabilityProcents < 0.25)
            {
                priceChangePercent = -0.1 + (random.NextDouble() * (0.4 + 0.1));
                newBuyingPrice = product.BasePrice * (1 + priceChangePercent);
            }
            else if (AvailabilityProcents >= 0.25 && AvailabilityProcents <= 0.80)
            {
                priceChangePercent = -0.05 + (random.NextDouble() * (0.05 + 0.05));
                newBuyingPrice = product.BasePrice * (1 + priceChangePercent);
            }
            else
            {
                priceChangePercent = -0.1 + (random.NextDouble() * (0.06 + 0.1));
                newBuyingPrice = product.BasePrice * (1 + priceChangePercent);
            }
            newBuyingPrice = Math.Max(newBuyingPrice, product.BasePrice * 0.25);
            newBuyingPrice = Math.Min(newBuyingPrice, product.BasePrice * 3);
            product.BuyingPrice = (int)newBuyingPrice;
        }
    }

    public void UpgradeTraderStorage(Trader trader, int increaseAmount)
    {
        if (UpgradeStorageCapacity(trader, increaseAmount))
        {
            UserInterface.ShowMessage("Lagerupgrade erfolgreich.");
        }
        else
        {
            UserInterface.ShowError("Lagerupgrade fehlgeschlagen.");
        }
    }

    public int CalculateUsedStorage(Trader trader)
    {
        int usedStorage = 0;
        foreach (var entry in trader.OwnedProducts)
        {
            usedStorage += entry.Value;
        }
        return usedStorage;
    }

    public bool UpgradeStorageCapacity(Trader trader, int increaseAmount)
    {
        if (increaseAmount <= 0)
        {
            UserInterface.ShowError("Die Vergrößerung des Lagers wurde abgebrochen.");
            return false;
        }
        int upgradeCost = increaseAmount * 50;
        if (trader.AccountBalance < upgradeCost)
        {
            UserInterface.ShowError("Nicht genügend Geld für das Upgrade vorhanden.");
            return false;
        }
        trader.StorageCapacity += increaseAmount;
        trader.AccountBalance -= upgradeCost;
        UserInterface.ShowMessage($"Lager erfolgreich um {increaseAmount} Einheiten erweitert. Kosten: ${upgradeCost}.");
        return true;
    }

    public void ApplyStorageCosts(Trader trader)
    {
        int usedStorage = CalculateUsedStorage(trader);
        int freeStorage = trader.StorageCapacity - usedStorage;
        int storageCosts = (usedStorage * 5) + (freeStorage * 1);
        trader.AccountBalance -= storageCosts;
    }

    public void CheckForBankruptcy(List<Trader> traders)
    {
        List<Trader> tradersToRemove = new List<Trader>();
        foreach (var trader in traders)
        {
            if (trader.AccountBalance < 0)
            {
                UserInterface.ShowError($"Zwischenhändler {trader.Name} ist bankrott gegangen.");
                tradersToRemove.Add(trader);
            }
        }

        foreach (var trader in tradersToRemove)
        {
            traders.Remove(trader);
        }

        if (traders.Count == 0)
        {
            UserInterface.ShowError("Alle Zwischenhändler sind bankrott. Die Simulation wird beendet.");
            Environment.Exit(0);
        }
    }

    public List<Product> GetProducts()
    {
        return products;
    }
}