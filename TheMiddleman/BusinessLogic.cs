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
        products = parser.ExtractProductsFromYAML();
    }

    public Trader CreateTrader(int position, UserInterface ui)
    {
        string traderName = ui.ReadTraderNameAtPosition(position);
        string firmName = ui.FetchFirmName(traderName);
        double initialBalance = GetInitialBalance(ui);
        return new Trader(traderName, firmName, initialBalance);
    }

    private double GetInitialBalance(UserInterface ui)
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

    private bool IsSufficientBalanceForPurchase(Trader trader, double totalCost)
    {
        if (trader.AccountBalance < totalCost)
        {
            UserInterface.ShowError("Nicht genügend Geld vorhanden.");
            return false;
        }
        return true;
    }

    private bool HasSufficientStorageForPurchase(Trader trader, int quantity)
    {
        int usedStorage = CalculateUsedStorage(trader);
        if (usedStorage + quantity > trader.StorageCapacity)
        {
            UserInterface.ShowError("Nicht genug Lagerplatz verfügbar.");
            return false;
        }
        return true;
    }

    private bool IsProductAvailableInRequiredQuantity(Product selectedProduct, int quantity)
    {
        if (selectedProduct.Availability < quantity)
        {
            UserInterface.ShowError("Nicht genügend Produkte verfügbar.");
            return false;
        }
        return true;
    }

    private void UpdateTraderStatus(Trader trader, Product selectedProduct, int quantity, double totalCost)
    {
        trader.AccountBalance -= totalCost;
        if (trader.OwnedProducts.ContainsKey(selectedProduct))
        {
            trader.OwnedProducts[selectedProduct] += quantity;
        }
        else
        {
            trader.OwnedProducts.Add(selectedProduct, quantity);
        }
        selectedProduct.Availability -= quantity;
    }

    public void Purchase(Trader trader, Product selectedProduct, int quantity)
    {
        double discount = trader.ProductDiscounts.ContainsKey(selectedProduct) ? trader.ProductDiscounts[selectedProduct] : 0;
        double discountedPrice = selectedProduct.BuyingPrice * (1 - discount);
        double totalCost = quantity * discountedPrice;
        if (!IsSufficientBalanceForPurchase(trader, totalCost) ||
            !HasSufficientStorageForPurchase(trader, quantity) ||
            !IsProductAvailableInRequiredQuantity(selectedProduct, quantity))
        {
            return;
        }
        UpdateTraderStatus(trader, selectedProduct, quantity, totalCost);
        trader.UpdateExpenses(totalCost);
        trader.CalculateDiscountsForAllProducts(products);
        UserInterface.ShowMessage($"Kauf erfolgreich. Neuer Kontostand: ${trader.AccountBalance.ToString("F2")}");
    }

    private bool IsProductAvailableForSale(Trader trader, Product selectedProduct, int quantityToSell)
    {
        if (!trader.OwnedProducts.ContainsKey(selectedProduct) || trader.OwnedProducts[selectedProduct] < quantityToSell)
        {
            UserInterface.ShowError("Nicht genügend Ware vorhanden.");
            return false;
        }
        return true;
    }

    private void UpdateTraderStatusAfterSale(Trader trader, Product selectedProduct, int quantityToSell)
    {
        double saleRevenue = quantityToSell * selectedProduct.SellingPrice;
        trader.AccountBalance += saleRevenue;
        trader.OwnedProducts[selectedProduct] -= quantityToSell;
    }

    private void UpdateOwnedProductsAfterSale(Trader trader, Product selectedProduct)
    {
        if (trader.OwnedProducts[selectedProduct] == 0)
        {
            trader.OwnedProducts.Remove(selectedProduct);
        }
    }

    public void Sell(Trader trader, Product selectedProduct, int quantityToSell)
    {
        if (!IsProductAvailableForSale(trader, selectedProduct, quantityToSell))
        {
            return;
        }
        double saleRevenue = quantityToSell * selectedProduct.SellingPrice;
        trader.UpdateRevenue(saleRevenue);
        UpdateTraderStatusAfterSale(trader, selectedProduct, quantityToSell);
        UpdateOwnedProductsAfterSale(trader, selectedProduct);
        trader.CalculateDiscountsForAllProducts(products);
        UserInterface.ShowMessage($"Verkauf erfolgreich. Neuer Kontostand: ${trader.AccountBalance.ToString("F2")}");
    }

    public void MoveFirstTraderToEnd(List<Trader> traders)
    {
        if (traders.Count > 1)
        {
            var firstTrader = traders[0];
            traders.RemoveAt(0);
            traders.Add(firstTrader);
        }
    }

    private void ProcessBankruptcies(List<Trader> traders)
    {
        CheckForBankruptcy(traders);
    }

    private void UpdateProducts(int currentDay)
    {
        if (currentDay > 1)
        {
            UpdateProductAvailability();
            UpdateProductPrices();
        }
    }

    private void RotateTraders(List<Trader> traders)
    {
        MoveFirstTraderToEnd(traders);
    }

    public void RunDayCycle(List<Trader> traders, UserInterface ui, ref int currentDay)
    {
        ProcessBankruptcies(traders);
        UpdateProducts(currentDay);
        foreach (Trader trader in traders)
        {
            trader.CalculateDiscountsForAllProducts(products);
            if (currentDay > 1)
            {
                ApplyStorageCosts(trader);
                ui.DisplayDailyReport(trader);
                trader.ResetDailyFinances();
            }
            ui.DisplayOptions(trader, ref currentDay);
        }
        RotateTraders(traders);
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
            product.BuyingPrice = newBuyingPrice;
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

    public double CalculateStorageCosts(Trader trader)
    {
        double costPerOccupiedUnit = 5;
        double costPerEmptyUnit = 1;
        int usedStorage = CalculateUsedStorage(trader);
        int emptyStorage = trader.StorageCapacity - usedStorage;
        double occupiedStorageCosts = usedStorage * costPerOccupiedUnit;
        double emptyStorageCosts = emptyStorage * costPerEmptyUnit;
        return occupiedStorageCosts + emptyStorageCosts;
    }

    private bool IsUpgradeAmountValid(int increaseAmount)
    {
        if (increaseAmount <= 0)
        {
            UserInterface.ShowError("Die Vergrößerung des Lagers wurde abgebrochen.");
            return false;
        }
        return true;
    }

    private bool HasSufficientFundsForUpgrade(Trader trader, double upgradeCost)
    {
        if (trader.AccountBalance < upgradeCost)
        {
            UserInterface.ShowError("Nicht genügend Geld für das Upgrade vorhanden.");
            return false;
        }
        return true;
    }

    private void ApplyStorageUpgrade(Trader trader, int increaseAmount, double upgradeCost)
    {
        trader.StorageCapacity += increaseAmount;
        trader.AccountBalance -= upgradeCost;
    }

    private void DisplayUpgradeSuccessMessage(int increaseAmount, double upgradeCost)
    {
        UserInterface.ShowMessage($"Lager erfolgreich um {increaseAmount} Einheiten erweitert. Kosten: ${upgradeCost}.00.");
    }

    public bool UpgradeStorageCapacity(Trader trader, int increaseAmount)
    {
        if (!IsUpgradeAmountValid(increaseAmount))
        {
            return false;
        }
        double upgradeCost = increaseAmount * 50;
        if (!HasSufficientFundsForUpgrade(trader, upgradeCost))
        {
            return false;
        }
        ApplyStorageUpgrade(trader, increaseAmount, upgradeCost);
        DisplayUpgradeSuccessMessage(increaseAmount, upgradeCost);
        return true;
    }

    public void ApplyStorageCosts(Trader trader)
    {
        int usedStorage = CalculateUsedStorage(trader);
        int emptyStorage = trader.StorageCapacity - usedStorage;
        double storageCosts = (usedStorage * 5) + (emptyStorage * 1);
        trader.UpdateStorageCosts(storageCosts);
        trader.AccountBalance -= storageCosts;
    }

    private List<Trader> IdentifyBankruptTraders(List<Trader> traders)
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
        return tradersToRemove;
    }

    private void RemoveBankruptTraders(List<Trader> traders, List<Trader> tradersToRemove)
    {
        foreach (var trader in tradersToRemove)
        {
            traders.Remove(trader);
        }
    }

    private void TerminateSimulationIfAllBankrupt(List<Trader> traders)
    {
        if (traders.Count == 0)
        {
            UserInterface.ShowError("Alle Zwischenhändler sind bankrott. Die Simulation wird beendet.");
            Environment.Exit(0);
        }
    }

    public void CheckForBankruptcy(List<Trader> traders)
    {
        List<Trader> tradersToRemove = IdentifyBankruptTraders(traders);
        RemoveBankruptTraders(traders, tradersToRemove);
        TerminateSimulationIfAllBankrupt(traders);
    }

    public List<Product> GetProducts()
    {
        return products;
    }
}