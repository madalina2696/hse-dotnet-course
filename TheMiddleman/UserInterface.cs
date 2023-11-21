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
        Console.WriteLine($"\n{trader.Name} von {trader.Company} | ${trader.AccountBalance} | Lager: {usedStorage}/{trader.StorageCapacity} | Tag {currentDay}\n");
    }

    public string ReadProductName(string line)
    {
        return line.Substring(8);
    }

    public int ReadProductDurability(string line)
    {
        return int.Parse(line.Substring(14));
    }

    /*     public void DisplayOptions(Trader trader, ref int currentDay)
        {
            bool endRound = false;
            while (!endRound)
            {
                DisplayTraderStatus(trader, currentDay, businessLogic.CalculateUsedStorage(trader));
                Console.WriteLine("e) Einkaufen");
                Console.WriteLine("v) Verkaufen");
                Console.WriteLine("l) Lager vergrößern");
                Console.WriteLine("b) Runde beenden");
                string userChoice = Console.ReadLine() ?? "";
                HandleUserChoice(userChoice, trader, ref endRound);
            }
        } */

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
        HandleUserChoice(userChoice, trader, ref endRound, currentDay);
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

    public void HandleUserChoice(string choice, Trader trader, ref bool endRound, int currentDay)
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

    public void ShowShoppingMenu(Trader trader)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("\nVerfügbare Produkte:\n");
        Console.ResetColor();
        foreach (Product product in businessLogic.GetProducts())
        {
            Console.WriteLine($"{product.Id} {product.Name} ({product.Durability} Tage) - Verfügbar: {product.Availability} ${product.BasePrice}/Stück");
        }
        Console.WriteLine("\nz) Zurück");
        string? userChoice = Console.ReadLine();
        if (userChoice == "z")
        {
            return;
        }
        else if (!int.TryParse(userChoice, out int _))
        {
            ShowShoppingMenu(trader);
        }
        int selectedProductId;
        if (!int.TryParse(userChoice, out selectedProductId) || int.Parse(userChoice) <= 0)
        {
            return;
        }
        Product? selectedProduct = businessLogic.GetProducts().Find(p => p.Id == selectedProductId);
        Console.WriteLine($"Wieviel von {selectedProduct?.Name} kaufen?");
        int quantity = int.Parse(Console.ReadLine()!);
        if (quantity <= 0)
        {
            return;
        }
        if (selectedProduct == null)
        {
            return;
        }
        businessLogic.Purchase(trader, selectedProduct, quantity);
    }

    public void ShowSellingMenu(Trader trader)
    {
        Console.WriteLine("\nProdukte im Besitz:");
        int index = 1;
        foreach (var entry in trader.OwnedProducts)
        {
            Console.WriteLine($"{index}) {entry.Key.Name} ({entry.Value}) ${entry.Key.SellingPrice}/Stück");
            index++;
        }
        Console.WriteLine("\nz) Zurück");
        string userChoice = Console.ReadLine() ?? "";
        if (userChoice == "z")
        {
            return;
        }
        else if (!int.TryParse(userChoice, out int _))
        {
            ShowSellingMenu(trader);
        }
        int selectedProductIndex = int.Parse(userChoice);
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

    public void ShowStorageUpgradeMenu(Trader trader)
    {
        Console.WriteLine("\nUm wie viele Einheiten möchten Sie das Lager vergrößern? Kosten: $50 pro Einheit.");
        int increaseAmount;
        if (!int.TryParse(Console.ReadLine(), out increaseAmount) || increaseAmount <= 0)
        {
            ShowError("Ungültige Eingabe oder Abbruch durch den Benutzer.");
            return;
        }
        businessLogic.UpgradeTraderStorage(trader, increaseAmount);
    }
    public int ReadSimulationDuration()
    {
        Console.WriteLine("Wie viele Tage soll die Simulation laufen?");
        return int.Parse(Console.ReadLine() ?? "0");
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