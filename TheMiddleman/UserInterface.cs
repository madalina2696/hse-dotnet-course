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

    public int QueryParticipantCount()
    {
        Console.WriteLine("Wieviel Zwischenhändler nehmen teil?");
        return int.Parse(Console.ReadLine() ?? "0");
    }

    public string FetchTraderName(int position)
    {
        Console.WriteLine($"Name von Zwischenhändler {position}:");
        return Console.ReadLine() ?? "";
    }

    public string FetchFirmName(string traderName)
    {
        Console.WriteLine($"Name der Firma von {traderName}:");
        return Console.ReadLine() ?? "";
    }

    public int GetInitialBalance()
    {
        Console.WriteLine("Wähle den Schwierigkeitsgrad aus (Einfach, Normal, Schwer):");
        string difficulty = Console.ReadLine()?.ToLower() ?? "normal";
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

    public void RenderTraderInfo(Intermediary trader, int currentDay)
    {
        Console.WriteLine($"{trader.Name} von {trader.Company} | ${trader.AccountBalance} | Tag {currentDay}");
    }

    public string ReadProductName(string line)
    {
        return line.Substring(8);
    }

    public int ReadProductDurability(string line)
    {
        return int.Parse(line.Substring(14));
    }

    public void DisplayOptions(Intermediary trader, ref int currentDay)
    {
        bool endRound = false;

        while (!endRound)
        {
            RenderTraderInfo(trader, currentDay);
            Console.WriteLine("e) Einkaufen");
            Console.WriteLine("v) Verkaufen");
            Console.WriteLine("b) Runde beenden");
            string userChoice = Console.ReadLine() ?? "";
            HandleUserChoice(userChoice, trader, ref endRound);
        }
    }

    public void HandleUserChoice(string choice, Intermediary trader, ref bool endRound)
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
            default:
                Console.WriteLine("Ungültige Auswahl. Bitte erneut versuchen.");
                break;
        }
    }

    public void ShowShoppingMenu(Intermediary trader)
    {
        Console.WriteLine("Verfügbare Produkte:");
        foreach (Product product in businessLogic.ReadProducts())
        {
            Console.WriteLine($"{product.Id} {product.Name} ({product.Durability} Tage) ${product.BasePrice}/Stück");
        }
        Console.WriteLine("z) Zurück");
        string? choice = Console.ReadLine();
        if (choice == "z")
        {
            return;
        }
        int selectedProductId;
        if (!int.TryParse(choice, out selectedProductId) || int.Parse(choice) <= 0)
        {
            return;
        }
        Product? selectedProduct = businessLogic.ReadProducts().Find(p => p.Id == selectedProductId);
        Console.WriteLine($"Wieviel von {selectedProduct?.Name} kaufen?");
        int quantity = int.Parse(Console.ReadLine()!);
        if (quantity <= 0)
        {
            return;
        }
        businessLogic.ExecutePurchase(trader, selectedProduct, quantity);
    }

    static void ExecutePurchase(Intermediary trader, Product selectedProduct, int quantity)
    {
        int totalCost = quantity * selectedProduct.BasePrice;
        if (trader.AccountBalance < totalCost)
        {
            Console.WriteLine("Nicht genügend Geld vorhanden.");
            return;
        }
        trader.AccountBalance -= totalCost;
        trader.OwnedProducts.Add(selectedProduct, quantity);
        Console.WriteLine($"Kauf erfolgreich. Neuer Kontostand: ${trader.AccountBalance}");
    }

    public void ShowSellingMenu(Intermediary trader)
    {
        Console.WriteLine("Produkte im Besitz:");
        int index = 1;
        foreach (var entry in trader.OwnedProducts)
        {
            Console.WriteLine($"{index}) {entry.Key.Name} ({entry.Value}) ${entry.Key.SellingPrice}/Stück");
            index++;
        }
        Console.WriteLine("z) Zurück");
        string userChoice = Console.ReadLine() ?? "";
        if (userChoice == "z")
        {
            return;
        }
        int selectedProductIndex = int.Parse(userChoice);
        var selectedEntry = trader.OwnedProducts.ElementAt(selectedProductIndex - 1);
        var selectedProduct = selectedEntry.Key;
        var availableQuantity = selectedEntry.Value;
        Console.WriteLine($"Wieviel von {selectedProduct.Name} verkaufen (max. {availableQuantity})?");
        int quantityToSell = int.Parse(Console.ReadLine() ?? "0");
        if (quantityToSell <= 0 || quantityToSell > availableQuantity)
        {
            Console.WriteLine("Ungültige Menge.");
            return;
        }
        businessLogic.ExecuteSale(trader, selectedProduct, quantityToSell);
    }
}
