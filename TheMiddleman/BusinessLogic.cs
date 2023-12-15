using TheMiddleman.Entity;

class BusinessLogic
{
    private Random random = new Random();
    private List<Product>? products;
    private List<Trader>? traders;
    private List<Trader> bankruptTraders;
    public Action<Trader, int> OnTraderChange { get; set; } = delegate { };
    public Action<Trader> OnBankruptcy { get; set; } = delegate { };
    public Action<List<Trader>, List<Trader>> OnSimulationEnd { get; set; } = delegate { };
    public Action OnDayChange { get; set; } = delegate { };
    private int simulationDuration;
    private int _currentDay = 1;

    public BusinessLogic()
    {
        products = new List<Product>();
        traders = new List<Trader>();
        bankruptTraders = new List<Trader>();
    }

    public void InitializeParser()
    {
        ProductsParser parser = new ProductsParser("produkte.yml");
        products = parser.ExtractProductsFromYAML();
    }

    public void CreateTrader(string traderName, string firmName, double initialBalance)
    {
        if (traders == null)
        {
            throw new NullReferenceException("traders is null");
        }
        else
        {
            traders.Add(new Trader(traderName, firmName, initialBalance));
        }
    }

    private bool IsSufficientBalanceForPurchase(Trader trader, double totalCost)
    {
        if (trader.AccountBalance < totalCost)
        {
            throw new BalanceException("Nicht genügend Geld vorhanden.");
        }
        return true;
    }

    private bool HasSufficientStorageForPurchase(Trader trader, int quantity)
    {
        if (CalculateUsedStorage(trader) + quantity > trader.StorageCapacity)
        {
            throw new StorageException("Nicht genug Lagerplatz verfügbar.");
        }
        return true;
    }

    private bool IsProductAvailableInRequiredQuantity(Product selectedProduct, int quantity)
    {
        if (selectedProduct.Availability < quantity)
        {
            throw new ProductException("Nicht genügend Produkte verfügbar.");
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

    public Product GetProductByID(int productID)
    {
        if (products == null) { throw new NullReferenceException("Produktliste ist null."); }
        foreach (Product product in products)
        {
            if (product.Id == productID)
            {
                return product;
            }
        }
        throw new ProductException("Produkt nicht gefunden.");
    }

    public void Purchase(Trader trader, Product selectedProduct, int quantity)
    {
        if (trader.AccountBalance < selectedProduct.BuyingPrice * quantity)
        {
            throw new BalanceException("Nicht genügend Geld für den Einkauf.");
        }
        double discount = trader.ProductDiscounts.ContainsKey(selectedProduct) ? trader.ProductDiscounts[selectedProduct] : 0;
        double discountedPrice = selectedProduct.BuyingPrice * (1 - discount);
        double totalCost = quantity * discountedPrice;
        try
        {
            ValidatePurchase(trader, selectedProduct, quantity, totalCost);
        }
        catch (BalanceException) { throw new BalanceException("Nicht genügend Geld vorhanden."); }
        catch (StorageException) { throw new StorageException("Nicht genug Lagerplatz verfügbar."); }
        catch (ProductException) { throw new ProductException("Nicht genügend Produkte verfügbar."); }
        catch (UserInputException) { throw new UserInputException("Ungültige Eingabe."); }
    }

    private void ValidatePurchase(Trader trader, Product selectedProduct, int quantity, double totalCost)
    {
        if (!IsSufficientBalanceForPurchase(trader, totalCost) ||
            !HasSufficientStorageForPurchase(trader, quantity) ||
            !IsProductAvailableInRequiredQuantity(selectedProduct, quantity))
        {
            return;
        }
        UpdateTraderStatus(trader, selectedProduct, quantity, totalCost);
        trader.UpdateExpenses(totalCost);
        if (products == null) { throw new NullReferenceException("Produktliste ist null."); }
        trader.CalculateDiscountsForAllProducts(products);
    }

    private bool IsProductAvailableForSale(Trader trader, Product selectedProduct, int quantityToSell)
    {
        if (!trader.OwnedProducts.ContainsKey(selectedProduct) || trader.OwnedProducts[selectedProduct] < quantityToSell)
        {
            throw new ProductException("Nicht genügend Ware vorhanden.");
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
        if (quantityToSell <= 0) { throw new ProductException("Ungültige Menge."); }
        if (!IsProductAvailableForSale(trader, selectedProduct, quantityToSell))
        {
            throw new ProductException("Nicht genügend Ware vorhanden.");
        }
        double saleRevenue = quantityToSell * selectedProduct.SellingPrice;
        trader.UpdateRevenue(saleRevenue);
        UpdateTraderStatusAfterSale(trader, selectedProduct, quantityToSell);
        UpdateOwnedProductsAfterSale(trader, selectedProduct);
        if (products == null) { throw new NullReferenceException("Produktliste ist null."); }
        trader.CalculateDiscountsForAllProducts(products);
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
        List<Trader> tradersToRemove = IdentifyBankruptTraders(traders);
        RemoveBankruptTraders(traders, tradersToRemove);
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

    /* public void RunDayCycle()
    {
        while (_currentDay <= simulationDuration && !TerminateSimulation(traders!, bankruptTraders))
        {
            OnDayChange.Invoke();
            UpdateProducts(_currentDay);
            foreach (Trader trader in traders!)
            {
                trader.CalculateDiscountsForAllProducts(GetProducts());
                if (_currentDay > 1)
                {
                    if (trader.CurrentLoan != null && trader.CurrentLoan.DueDay == _currentDay)
                    {
                        try
                        {
                            RepayLoan(trader);
                        }
                        catch (BalanceException)
                        {
                            OnBankruptcy.Invoke(trader);
                            continue;
                        }
                    }
                    ApplyStorageCosts(trader);
                    if (trader.AccountBalance <= 0)
                    {
                        OnBankruptcy.Invoke(trader);
                        continue;
                    }
                }
                OnTraderChange.Invoke(trader, _currentDay);
            }
            foreach (Trader trader in traders)
            {
                trader.LoanRepaymentToday = false;
            }
            RotateTraders(traders);
            ProcessBankruptcies(traders);
            _currentDay++;
        }
    } */

    public void RunDayCycle()
    {
        while (_currentDay <= simulationDuration && !TerminateSimulation(traders!, bankruptTraders))
        {
            OnDayChange.Invoke();
            UpdateProducts(_currentDay);
            HandleDailyTraderActivities();
            RotateTraders(traders!);
            ProcessBankruptcies(traders!);
            _currentDay++;
        }
    }

    private void HandleDailyTraderActivities()
    {
        List<Trader> tradersToProcess = new List<Trader>(traders!);
        foreach (Trader trader in tradersToProcess)
        {
            trader.CalculateDiscountsForAllProducts(GetProducts());
            if (ProcessTraderLoans(trader)) { continue; }
            ApplyStorageCosts(trader);
            if (trader.AccountBalance <= 0)
            {
                MarkTraderAsBankrupt(trader);
                continue;
            }
            OnTraderChange.Invoke(trader, _currentDay);
        }
    }

    private bool ProcessTraderLoans(Trader trader)
    {
        if (_currentDay > 1 && trader.CurrentLoan != null && trader.CurrentLoan.DueDay == _currentDay)
        {
            try
            {
                RepayLoan(trader);
            }
            catch (BalanceException)
            {
                MarkTraderAsBankrupt(trader);
                return true;
            }
        }
        return false;
    }

    private void MarkTraderAsBankrupt(Trader trader)
    {
        if (traders == null) { throw new NullReferenceException("Traderliste ist null."); }
        traders!.Remove(trader);
        bankruptTraders.Add(trader);
        OnBankruptcy.Invoke(trader);
    }

    public int GetCurrentDay()
    {
        return _currentDay;
    }

    public void UpdateProductAvailability()
    {
        if (products == null) { throw new NullReferenceException("Produktliste ist null."); }
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
        if (products == null) { throw new NullReferenceException("Produktliste ist null."); }
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

    private bool IsUpgradeAmountValid(int increaseAmount)
    {
        if (increaseAmount <= 0) { throw new UserInputException("Ungültige Eingabe."); }
        return true;
    }

    private bool HasSufficientFundsForUpgrade(Trader trader, double upgradeCost)
    {
        if (trader.AccountBalance < upgradeCost)
        {
            throw new BalanceException("Nicht genügend Geld für das Upgrade vorhanden.");
        }
        return true;
    }

    private void ApplyStorageUpgrade(Trader trader, int increaseAmount, double upgradeCost)
    {
        trader.StorageCapacity += increaseAmount;
        trader.AccountBalance -= upgradeCost;
    }

    public bool UpgradeStorageCapacity(Trader trader, int increaseAmount)
    {
        if (!IsUpgradeAmountValid(increaseAmount))
        {
            throw new StorageException("Die Vergrößerung des Lagers wurde abgebrochen.");
        }
        double upgradeCost = increaseAmount * 50;
        if (!HasSufficientFundsForUpgrade(trader, upgradeCost))
        {
            throw new BalanceException("Nicht genügend Geld für das Upgrade vorhanden.");
        }
        ApplyStorageUpgrade(trader, increaseAmount, upgradeCost);
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
            bankruptTraders.Add(trader);
        }
    }

    private bool TerminateSimulation(List<Trader> traders, List<Trader> bankruptTraders)
    {
        if (!traders.Any() || _currentDay >= simulationDuration)
        {
            OnSimulationEnd.Invoke(traders, bankruptTraders);
            return true;
        }
        return false;
    }

    public List<Product> GetProducts()
    {
        if (products == null) { throw new NullReferenceException("Produktliste ist null."); }
        return products;
    }

    public void SetSimulationDuration(int duration)
    {
        simulationDuration = duration;
    }

    public void TakeLoan(Trader trader, Trader.Loan loan)
    {
        if (trader.CurrentLoan != null)
        {
            throw new LoanException("Bereits aktiver Kredit vorhanden.");
        }
        trader.CurrentLoan = loan;
        trader.AccountBalance += loan.Amount;
    }

    public void RepayLoan(Trader trader)
    {
        if (trader.CurrentLoan == null)
        {
            throw new LoanException("Kein aktiver Kredit vorhanden.");
        }
        else
        {
            if (trader.AccountBalance < trader.CurrentLoan.RepaymentAmount)
            {
                throw new BalanceException("Nicht genügend Guthaben für Kreditrückzahlung.");
            }
            else
            {
                trader.AccountBalance -= trader.CurrentLoan.RepaymentAmount;
                trader.CurrentLoan = null;
                trader.LoanRepaymentToday = true;
            }
        }
    }
}