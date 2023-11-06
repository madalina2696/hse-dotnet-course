using System;
using System.Collections.Generic;
using System.Dynamic;
using TheMiddleman.Entity;

class Application
{
    /* static int QueryParticipantCount()
    {
        Console.WriteLine("Wieviel Zwischenhändler nehmen teil?");
        return int.Parse(Console.ReadLine() ?? "0");
    }

    static string FetchTraderName(int position)
    {
        Console.WriteLine($"Name von Zwischenhändler {position}:");
        return Console.ReadLine() ?? "";
    }

    static string FetchFirmName(string traderName)
    {
        Console.WriteLine($"Name der Firma von {traderName}:");
        return Console.ReadLine() ?? "";
    }

    static Intermediary CreateTrader(int position)
    {
        string traderName = FetchTraderName(position);
        string firmName = FetchFirmName(traderName);
        int initialBalance = GetInitialBalance();

        return new Intermediary(traderName, firmName, initialBalance);
    }

    static int GetInitialBalance()
    {
        Console.WriteLine("Wähle der Schwierigkeitsgrad aus (Einfach, Normal, Schwer):");
        string difficulty = Console.ReadLine()?.ToLower() ?? "Normal";

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

    static List<Intermediary> CreateTraderList()
    {
        List<Intermediary> participants = new List<Intermediary>();
        int traderCount = QueryParticipantCount();

        for (int i = 1; i <= traderCount; i++)
        {
            participants.Add(CreateTrader(i));
        }

        return participants;
    }

    static void RenderTraderInfo(Intermediary trader, int currentDay)
    {
        Console.WriteLine($"{trader.Name} von {trader.Company} | ${trader.AccountBalance} | Tag {currentDay}"); //geupdated3
    }

    static string ReadProductName(string line)
    {
        return line.Substring(8);
    }

    static int ReadProductDurability(string line)
    {
        return int.Parse(line.Substring(14));
    }

    static Product CreateProduct(int id, string name, int durability)
    {
        return new Product { Id = id, Name = name, Durability = durability };
    }

    static List<Product> ReadProducts()
    {
        string[] lines = System.IO.File.ReadAllLines("produkte.yml");
        List<Product> products = new List<Product>();
        Product? currentProduct = null;
        int idCounter = 1;

        foreach (var line in lines)
        {
            if (line.StartsWith("- name: "))
            {
                string name = ReadProductName(line);
                currentProduct = CreateProduct(idCounter++, name, 0);
            }
            else if (line.StartsWith("  durability: "))
            {
                if (currentProduct != null)
                {
                    int durability = ReadProductDurability(line);
                    currentProduct.Durability = durability;
                    products.Add(currentProduct);
                }
            }
            else if (line.StartsWith("  baseprice: "))
            {
                int basePrice = int.Parse(line.Substring(13));
                if (currentProduct != null)
                {
                    currentProduct.BasePrice = basePrice;
                }
            }

        }

        return products;
    }

    static void DisplayOptions(Intermediary trader, ref int currentDay, List<Product> products)
    {
        bool endRound = false;

        while (!endRound)
        {
            RenderTraderInfo(trader, currentDay);
            Console.WriteLine("e) Einkaufen");
            Console.WriteLine("v) Verkaufen");
            Console.WriteLine("b) Runde beenden");

            string userChoice = Console.ReadLine() ?? "";

            HandleUserChoice(userChoice, trader, products, ref endRound);
        }
    }

    static void HandleUserChoice(string choice, Intermediary trader, List<Product> products, ref bool endRound)
    {
        switch (choice)
        {
            case "b":
                endRound = true;
                break;
            case "e":
                ShowShoppingMenu(products, trader);
                break;
            case "v":
                ShowSellingMenu(trader);
                break;
            default:
                Console.WriteLine("Ungültige Auswahl. Bitte erneut versuchen.");
                break;
        }
    }

    static void ShowShoppingMenu(List<Product> products, Intermediary trader)
    {
        Console.WriteLine("Verfügbare Produkte:");
        foreach (Product product in products)
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
        Product selectedProduct = products.Find(p => p.Id == selectedProductId);

        Console.WriteLine($"Wieviel von {selectedProduct.Name} kaufen?");
        int quantity = int.Parse(Console.ReadLine());

        if (quantity <= 0)
        {
            return;
        }

        ExecutePurchase(trader, selectedProduct, quantity);
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

    static void ShowSellingMenu(Intermediary trader)
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

        ExecutePurchase(trader, selectedProduct, quantityToSell);
    }

    static void ExecuteSale(Intermediary trader, Product selectedProduct, int quantityToSell)
    {
        trader.AccountBalance += quantityToSell * selectedProduct.SellingPrice;
        trader.OwnedProducts[selectedProduct] -= quantityToSell;

        if (trader.OwnedProducts[selectedProduct] == 0)
        {
            trader.OwnedProducts.Remove(selectedProduct);
        }

        Console.WriteLine($"Verkauf erfolgreich. Neuer Kontostand: ${trader.AccountBalance}");
    }


    static void RotateIntermediary(List<Intermediary> traders)
    {
        if (traders.Count > 1)
        {
            var firstMiddleman = traders[0];
            traders.RemoveAt(0);
            traders.Add(firstMiddleman);
        }
    }

    static void RunDayCycle(List<Intermediary> traders, ref int currentDay, List<Product> products)
    {
        foreach (var trader in traders)
        {
            DisplayOptions(trader, ref currentDay, products);
        }

        RotateIntermediary(traders);
        currentDay++;
    } */

    static void Main()
    {
        // Instanzen BusinessLogic und UserInterface
        BusinessLogic businessLogic = new BusinessLogic();
        UserInterface ui = new UserInterface(businessLogic);

        // Tradersliste
        List<Intermediary> traders = businessLogic.CreateTraderList(ui);

        // Produkliste ablesen
        List<Product> products = businessLogic.GetProducts();

        int currentDay = 1; // fängt bei 1 an

        while (true)
        {
            businessLogic.RunDayCycle(traders, ui, ref currentDay);
        }
    }
}