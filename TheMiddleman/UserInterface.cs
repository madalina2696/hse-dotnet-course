using System;
using System.Collections.Generic;
using System.IO;
using TheMiddleman.Entity;

class UserInterface
{
    private readonly BusinessLogic businessLogic;

    public UserInterface(BusinessLogic? paramBusinessLogic)
    {
        businessLogic = paramBusinessLogic ?? throw new ArgumentNullException(nameof(paramBusinessLogic));
    }

    public int ReadParticipantCount()
    {
        Console.WriteLine("\nWieviel Zwischenhändler nehmen teil?");
        return int.Parse(Console.ReadLine() ?? "0");
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
        string traderInfo = $"{trader.Name} von {trader.Company} | ${trader.AccountBalance} | Lager: {usedStorage}/{trader.StorageCapacity} | Tag {currentDay}";
        int dynamicWidth = Math.Max(80, traderInfo.Length + 2);
        string border = new string('-', dynamicWidth);
        string sideBorder = "|";
        string paddedInfo = traderInfo.PadLeft((dynamicWidth - 2 + traderInfo.Length) / 2).PadRight(dynamicWidth - 2);
        Console.WriteLine("\n" + border);
        Console.WriteLine(sideBorder + paddedInfo + sideBorder);
        Console.WriteLine(border + "\n");
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
        Console.WriteLine("b) Runde beenden");
    }

    private void ProcessUserChoice(Trader trader, ref bool endRound, int currentDay)
    {
        string userChoice = Console.ReadLine() ?? "";
        ProcessUserAction(userChoice, trader, ref endRound, currentDay);
    }

    public void DisplayOptions(Trader trader, ref int currentDay)
    {
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
            default:
                ShowError("Ungültige Auswahl. Bitte erneut versuchen.");
                break;
        }
    }

    private void ShowShoppingMenuOptions()
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("\nVerfügbare Produkte:\n");
        Console.ResetColor();
        Console.WriteLine($"{"ID",-10} {"Name",-25} {"Haltbarkeit",-15} {"Verfügbare Menge",-20} {"Preis pro Stück",-20}");
        Console.WriteLine(new string('-', 90));
        foreach (Product product in businessLogic.GetProducts())
        {
            Console.WriteLine($"{product.Id,-10} {product.Name,-25} {product.Durability + " Tage",-15} {product.Availability,-20} {"$" + product.BuyingPrice,-20}");
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
        return businessLogic.GetProducts().Find(p => p.Id == selectedProductId);
    }

    private void ProcessPurchase(Trader trader, Product? selectedProduct)
    {
        if (selectedProduct == null)
        {
            return;
        }
        Console.WriteLine($"Wieviel von {selectedProduct.Name} kaufen?");
        int quantity;
        if (!int.TryParse(Console.ReadLine(), out quantity) || quantity <= 0)
        {
            return;
        }
        businessLogic.Purchase(trader, selectedProduct, quantity);
    }

    public void ShowShoppingMenu(Trader trader)
    {
        ShowShoppingMenuOptions();
        Product? selectedProduct = GetUserSelectedProduct(trader);
        ProcessPurchase(trader, selectedProduct);
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
            Console.WriteLine($"{index,-10} {entry.Key.Name,-25} {entry.Value,-20} {"$" + entry.Key.SellingPrice,-20}");
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

    private void ProcessSale(Trader trader, int selectedProductIndex)
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
        Console.WriteLine($"Wieviel von {selectedProduct.Name} verkaufen (max. {availableQuantity})?");
        int quantityToSell = int.Parse(Console.ReadLine() ?? "0");
        if (quantityToSell <= 0 || quantityToSell > availableQuantity)
        {
            ShowError("Ungültige Menge.");
            return;
        }
        businessLogic.Sale(trader, selectedProduct, quantityToSell);
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
        ProcessSale(trader, selectedProductIndex);
    }

    private void ShowStorageUpgradeInformation()
    {
        Console.WriteLine("\nUm wie viele Einheiten möchten Sie das Lager vergrößern? Kosten: $50 pro Einheit.");
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
        if (increaseAmount > 0)
        {
            businessLogic.UpgradeStorageCapacity(trader, increaseAmount);
        }
    }

    private void AskSimulationDuration()
    {
        Console.WriteLine("Wie viele Tage soll die Simulation laufen?");
    }

    private int ReadDurationInput()
    {
        while (true)
        {
            string input = Console.ReadLine() ?? "";
            if (int.TryParse(input, out int duration) && duration > 0)
            {
                return duration;
            }
            else
            {

                ShowError("Ungültige Eingabe.");
            }
        }
    }

    public int ReadSimulationDuration()
    {
        AskSimulationDuration();
        return ReadDurationInput();
    }

    public void DisplayRanking(List<Trader> traders)
    {
        var sortedTraders = traders.OrderByDescending(trader => trader.AccountBalance).ToList();
        int rank = 1;
        Console.WriteLine("Rangliste der Zwischenhändler am Ende der Simulation:");
        foreach (var trader in sortedTraders)
        {
            Console.WriteLine($"Platz {rank} - {trader.Name} - Kontostand: ${trader.AccountBalance}");
            rank++;
        }
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
        Console.WriteLine("\n" + message + "\n");
        Console.ResetColor();
    }
}