using Spectre.Console;
using TheMiddleman.Entity;
using static TheMiddleman.Entity.Trader;

class UserInterface
{
    private readonly BusinessLogic businessLogic;

    public UserInterface(BusinessLogic paramBusinessLogic)
    {
        businessLogic = paramBusinessLogic;
    }

    public void Initialize()
    {
        businessLogic.OnTraderChange += DisplayOptions;
        businessLogic.OnBankruptcy += DisplayBankruptcy;
        businessLogic.OnSimulationEnd += DisplayRanking;
        businessLogic.OnDayChange += ShowCurrentDay;
    }

    private void DisplayBankruptcy(Trader trader)
    {
        ShowError($"Zwischenhändler {trader.Name} ist bankrott gegangen.");
    }

    public int ReadParticipantCount()
    {
        Console.WriteLine("\nWieviel Zwischenhändler nehmen teil?");
        string? input = Console.ReadLine();
        if (int.TryParse(input, out int count))
        {
            return count;
        }
        else
        {
            ShowError("Ungültige Eingabe. Bitte erneut versuchen.");
            return ReadParticipantCount();
        }
    }

    public string ReadTraderNameAtPosition(int position)
    {
        Console.WriteLine($"\nName von Zwischenhändler {position}:");
        return Console.ReadLine() ?? "";
    }

    public string FetchFirmName(string traderName)
    {
        Console.WriteLine($"\nName der Firma von {traderName}:");
        return Console.ReadLine() ?? "";
    }

    public string AskForDifficultyLevel()
    {
        Console.WriteLine("\nWähle den Schwierigkeitsgrad aus (Einfach, Normal, Schwer):");
        string difficulty = Console.ReadLine()?.ToLower() ?? "normal";
        if (difficulty != "einfach" && difficulty != "normal" && difficulty != "schwer")
        {
            ShowError("Ungültige Eingabe. Bitte erneut versuchen.");
            return AskForDifficultyLevel();
        }
        return difficulty;
    }

    public int AssignStartingBalance(string difficulty)
    {
        switch (difficulty)
        {
            case "einfach":
                return 15000;
            case "normal":
                return 10000;
            case "schwer":
                return 7000;
            default:
                return 10000;
        }
    }

    public void DisplayTraderStatus(Trader trader, int currentDay, int usedStorage)
    {
        var panel = new Panel(
                new Markup(
                    $"[bold]Kontostand:[/] ${trader.AccountBalance.ToString("F2")}" + "  |  " +
                    $"[bold]Lagerkapazität:[/] {usedStorage}/{trader.StorageCapacity}" + "  |  " +
                    $"[bold]Tag:[/] {currentDay}"
                ))
                .Header($" {trader.Name} von {trader.Company} ")
                .Border(BoxBorder.Rounded)
                .BorderStyle(new Style(Color.Cyan1));
        AnsiConsole.Write(panel);
    }

    public string ReadProductName(string line)
    {
        return line.Substring(8);
    }
    public int ReadProductDurability(string line)
    {
        return int.Parse(line.Substring(14));
    }

    private void ShowUserOptions()
    {
        Console.WriteLine("e) Einkaufen");
        Console.WriteLine("v) Verkaufen");
        Console.WriteLine("l) Lager vergrößern");
        Console.WriteLine("k) Kredit aufnehmen");
        Console.WriteLine("b) Runde beenden");
    }

    private void ProcessUserChoice(Trader trader, ref bool endRound, int currentDay)
    {
        string userChoice = Console.ReadLine() ?? "";
        ProcessUserAction(userChoice, trader, ref endRound, currentDay);
    }

    public void DisplayOptions(Trader trader, int currentDay)
    {
        DisplayDailyReport(trader);
        trader.ResetDailyFinances();
        bool endRound = false;
        while (!endRound)
        {
            DisplayTraderStatus(trader, currentDay, businessLogic.CalculateUsedStorage(trader));
            ShowUserOptions();
            ProcessUserChoice(trader, ref endRound, currentDay);
        }
    }

    public void ProcessUserAction(string choice, Trader trader, ref bool endRound, int currentDay)
    {
        switch (choice)
        {
            case "b":
                endRound = true;
                break;
            case "e":
                ShowShoppingMenu(trader);
                break;
            case "v":
                ShowSellingMenu(trader);
                break;
            case "l":
                ShowStorageUpgradeMenu(trader);
                break;
            case "k":
                ShowLoanMenu(trader);
                break;
            default:
                ShowError("Ungültige Auswahl. Bitte erneut versuchen.");
                break;
        }
    }

    private void ShowShoppingMenuOptions(Trader trader)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("\nVerfügbare Produkte:\n");
        Console.ResetColor();
        Console.WriteLine($"{"ID",-10} {"Name",-25} {"Haltbarkeit",-15} {"Verfügbare Menge",-20} {"Preis pro Stück",-20} {"Rabatt",-10}");
        Console.WriteLine(new string('-', 105));
        foreach (Product product in businessLogic.GetProducts())
        {
            double discount = trader.ProductDiscounts.ContainsKey(product) ? trader.ProductDiscounts[product] : 0;
            double discountedPrice = product.BuyingPrice * (1 - discount);
            string priceWithDiscount = $"${discountedPrice:F2}";
            string discountPercentage = $"({discount:P2})";
            Console.WriteLine($"{product.Id,-10} {product.Name,-25} {product.Durability + " Tage",-15} {product.Availability,-20} {priceWithDiscount,-20} {discountPercentage,-10}");
        }
        Console.WriteLine("\nz) Zurück");
    }

    private Product? GetUserSelectedProduct(Trader trader)
    {
        string? userChoice = Console.ReadLine();
        if (userChoice == "z")
        {
            return null;
        }
        if (!int.TryParse(userChoice, out int selectedProductId) || selectedProductId <= 0)
        {
            ShowError("Ungültige Auswahl. Bitte erneut versuchen.");
            ShowShoppingMenu(trader);
            return null;
        }
        return businessLogic.GetProductByID(selectedProductId);
    }

    private void ConductProductPurchase(Trader trader, Product? selectedProduct)
    {
        if (selectedProduct == null)
        {
            return;
        }
        Console.WriteLine($"\nWieviel von {selectedProduct.Name} kaufen?");
        int quantity;
        if (!int.TryParse(Console.ReadLine(), out quantity) || quantity <= 0)
        {
            return;
        }
        try
        {
            businessLogic.Purchase(trader, selectedProduct, quantity);
            ShowMessage($"\nKauf erfolgreich. Neuer Kontostand: ${trader.AccountBalance.ToString("F2")}");
        }
        catch (Exception e)
        {
            ShowError(e.Message);
        }
    }

    public void ShowShoppingMenu(Trader trader)
    {
        ShowShoppingMenuOptions(trader);
        try
        {
            Product? selectedProduct = GetUserSelectedProduct(trader);
            ConductProductPurchase(trader, selectedProduct);
        }
        catch (Exception e)
        {
            ShowError(e.Message);
        }
    }

    private void ShowSellingMenuOptions(Trader trader)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("\nProdukte im Besitz:\n");
        Console.ResetColor();
        Console.WriteLine($"{"ID",-10} {"Name",-25} {"Verfügbare Menge",-20} {"Preis pro Stück",-20}");
        Console.WriteLine(new string('-', 75));
        int index = 1;
        foreach (var entry in trader.OwnedProducts)
        {
            Console.WriteLine($"{index,-10} {entry.Key.Name,-25} {entry.Value,-20} ${entry.Key.SellingPrice.ToString("F2"),-20}");
            index++;
        }
        Console.WriteLine("\nz) Zurück");
    }

    private int GetUserSelectedProductIndex()
    {
        string userChoice = Console.ReadLine() ?? "";
        if (userChoice == "z")
        {
            return -1;
        }
        if (int.TryParse(userChoice, out int selectedProductIndex))
        {
            return selectedProductIndex;
        }
        return -2;
    }

    private void ExecuteProductSale(Trader trader, int selectedProductIndex)
    {
        if (selectedProductIndex <= 0 || selectedProductIndex > trader.OwnedProducts.Count)
        {
            ShowError("Ungültige Auswahl. Bitte erneut versuchen.");
            ShowSellingMenu(trader);
            return;
        }
        var selectedEntry = trader.OwnedProducts.ElementAt(selectedProductIndex - 1);
        var selectedProduct = selectedEntry.Key;
        var availableQuantity = selectedEntry.Value;
        Console.WriteLine($"\nWieviel von {selectedProduct.Name} verkaufen (max. {availableQuantity})?");
        try
        {
            int quantityToSell = int.Parse(Console.ReadLine() ?? "0");
            businessLogic.Sell(trader, selectedProduct, quantityToSell);
        }
        catch (Exception e)
        {
            ShowError(e.Message);
            return;
        }
        ShowMessage($"\nVerkauf erfolgreich. Neuer Kontostand: ${trader.AccountBalance.ToString("F2")}");
    }

    public void ShowSellingMenu(Trader trader)
    {
        ShowSellingMenuOptions(trader);
        int selectedProductIndex = GetUserSelectedProductIndex();
        if (selectedProductIndex == -1) return;
        if (selectedProductIndex == -2)
        {
            ShowError("Ungültige Auswahl.");
            ShowSellingMenu(trader);
            return;
        }
        ExecuteProductSale(trader, selectedProductIndex);
    }

    private void ShowStorageUpgradeInformation()
    {
        Console.WriteLine("\nUm wie viele Einheiten möchten Sie das Lager vergrößern? Kosten: $50.00 pro Einheit.");
    }

    private int GetStorageUpgradeAmount()
    {
        if (!int.TryParse(Console.ReadLine(), out int increaseAmount) || increaseAmount <= 0)
        {
            ShowError("Ungültige Eingabe. Bitte erneut versuchen.");
            return -1;
        }
        return increaseAmount;
    }

    public void ShowStorageUpgradeMenu(Trader trader)
    {
        ShowStorageUpgradeInformation();
        int increaseAmount = GetStorageUpgradeAmount();
        try
        {
            businessLogic.UpgradeStorageCapacity(trader, increaseAmount);
            ShowMessage($"Lager erfolgreich um {increaseAmount} Einheiten erweitert. Kosten: ${increaseAmount * 50}.00.");
        }
        catch (Exception e)
        {
            ShowError(e.Message);
        }
    }

    private void AskSimulationDuration()
    {
        Console.WriteLine("\nWie viele Tage soll die Simulation laufen?");
    }

    private void ReadDurationInput()
    {
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out int duration) && duration > 0)
            {
                businessLogic.SetSimulationDuration(duration);
                break;
            }
            else
            {
                ShowError("Ungültige Eingabe.");
            }
        }
    }

    public void ReadSimulationDuration()
    {
        AskSimulationDuration();
        ReadDurationInput();
    }

    public void DisplayTraderRanking(List<Trader> traders)
    {
        var solventTraders = traders.Where(trader => trader.AccountBalance >= 0).ToList();
        var sortedTraders = solventTraders.OrderByDescending(trader => trader.AccountBalance).ToList();
        int rank = 1;
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("\nRangliste der Zwischenhändler am Ende der Simulation:\n");
        Console.ResetColor();
        foreach (var trader in sortedTraders)
        {
            Console.WriteLine($"Platz {rank} - {trader.Name} - Kontostand: ${trader.AccountBalance.ToString("F2")}");
            rank++;
        }
    }
    public void DisplayBankruptTraders(List<Trader> bankruptTraders)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\nBankrotte Zwischenhändler:");
        Console.ResetColor();
        foreach (var trader in bankruptTraders)
        {
            Console.WriteLine($"Zwischenhändler {trader.Name}");
        }
        Console.WriteLine("\n");
    }

    public void DisplayNotBankruptTraders()
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("\nKeine bankrotten Zwischenhändler.\n");
        Console.ResetColor();
    }

    public void DisplayRanking(List<Trader> traders, List<Trader> bankruptTraders)
    {
        if (traders.Any())
        {
            DisplayTraderRanking(traders);
        }
        if (bankruptTraders.Any())
        {
            DisplayBankruptTraders(bankruptTraders);
        }
        else
        {
            DisplayNotBankruptTraders();
        }
    }

    public void DisplayDailyReport(Trader trader)
    {

        string creditAmountMessage = trader.CurrentLoan != null
            ? $"${trader.CurrentLoan.Amount.ToString("F2")}"
            : "Kein Kredit";

        int remainingDays = trader.CurrentLoan != null
            ? trader.CurrentLoan.DueDay - businessLogic.GetCurrentDay()
            : 0;

        string remainingDaysMessage = trader.CurrentLoan != null && remainingDays >= 0
            ? $"{remainingDays} Tage verbleiben"
            : "0";

        string loanRepaymentMessage = trader.LoanRepaymentToday ? "\n[bold][green]Kredit zurückgezahlt![/][/]" : "";

        Console.WriteLine("\n");
        var panel = new Panel(
            new Markup(
                $"\n[bold][darkturquoise]Kontostand zu Beginn des letzten Tages:[/][/] ${trader.StartingBalance.ToString("F2")}\n" +
                $"[bold][darkturquoise]Ausgaben für Einkäufe:[/][/] ${trader.Expenses.ToString("F2")}\n" +
                $"[bold][darkturquoise]Einnahmen aus Verkäufen:[/][/] ${trader.Revenue.ToString("F2")}\n" +
                $"[bold][darkturquoise]Angefallene Lagerkosten:[/][/] ${trader.StorageCosts.ToString("F2")}\n" +
                $"[bold][darkturquoise]Aktueller Kontostand:[/][/] ${trader.AccountBalance.ToString("F2")}\n" +
                $"[bold][darkturquoise]Kredit:[/][/] {creditAmountMessage}\n" +
                $"[bold][darkturquoise]Verbleibende Tage bis zur Rückzahlung:[/][/] {remainingDaysMessage}\n" +
                loanRepaymentMessage
            ))
            .Header($"Tagesbericht für {trader.Name} ")
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(Color.LightGoldenrod1));
        AnsiConsole.Write(panel);
        Console.WriteLine("\nDrücken Sie Enter, um fortzufahren...");
        Console.ReadLine();
    }

    public static void ShowError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n" + message + "\n");
        Console.ResetColor();
    }

    public static void ShowMessage(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message + "\n");
        Console.ResetColor();
    }

    public void StartSimulation()
    {
        ReadSimulationDuration();
        Initialize();
        ShowTradersCreation(ReadParticipantCount());
        businessLogic.RunDayCycle();
    }

    private void ShowCurrentDay()
    {
        Console.WriteLine("\n");
        var rule = new Rule($"Tag {businessLogic.GetCurrentDay()}");
        rule.RuleStyle("blue");
        rule.Justification = Justify.Left;
        AnsiConsole.Write(rule);
    }

    private void ShowTradersCreation(int numberOfTraders)
    {
        for (int i = 0; i < numberOfTraders; i++)
        {
            string traderName = ReadTraderNameAtPosition(i + 1);
            string firmName = FetchFirmName(traderName);
            double startingBalance = AssignStartingBalance(AskForDifficultyLevel());
            businessLogic.CreateTrader(traderName, firmName, startingBalance);
        }
    }

    public void ShowLoanMenu(Trader trader)
    {
        Console.WriteLine("\nWählen Sie einen Kredit aus:");
        Console.WriteLine("1) $5000 mit 3% Zinsen ($5150 Rückzahlung)");
        Console.WriteLine("2) $10000 mit 5% Zinsen ($10500 Rückzahlung)");
        Console.WriteLine("3) $25000 mit 8% Zinsen ($27000 Rückzahlung)");
        Console.WriteLine("4) Keinen Kredit aufnehmen.\n");
        string? choice = Console.ReadLine();
        Loan loan = null!;
        switch (choice)
        {
            case "1":
                loan = new Loan { Amount = 5000, RepaymentAmount = 5150, DueDay = businessLogic.GetCurrentDay() + 7 };
                break;
            case "2":
                loan = new Loan { Amount = 10000, RepaymentAmount = 10500, DueDay = businessLogic.GetCurrentDay() + 7 };
                break;
            case "3":
                loan = new Loan { Amount = 25000, RepaymentAmount = 27000, DueDay = businessLogic.GetCurrentDay() + 7 };
                break;
            case "4":
                break;
            default:
                ShowError("Ungültige Auswahl. Bitte erneut versuchen.");
                break;
        }
        if (loan != null)
        {
            try
            {
                businessLogic.TakeLoan(trader, loan);
                ShowMessage("Kredit erfolgreich aufgenommen.");
            }
            catch (InvalidOperationException ex)
            {
                ShowError(ex.Message);
            }
        }
    }

    public void DisplayLoanRepaymentStatus(Trader trader)
    {
        int currentDay = businessLogic.GetCurrentDay();
        if (currentDay > 7)
        {
            try
            {
                businessLogic.RepayLoan(trader);
                Console.WriteLine("Kredit erfolgreich zurückgezahlt.");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}