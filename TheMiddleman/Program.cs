using System;
using System.Collections.Generic;
using System.Dynamic;
using TheMiddleman.Entity;

class Application
{
    static int QueryParticipantCount()
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

        return new Intermediary { Name = traderName, Company = firmName, AccountBalance = initialBalance };
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

    static List<Intermediary> GenerateParticipantList()
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
        Console.WriteLine($"{trader.Name} von {trader.Company} | ${trader.AccountBalance} | Tag {currentDay}"); //geupdated
        Console.WriteLine("b) Runde beenden");
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
        Product currentProduct = null;
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
        }

        return products;
    }



    static void DisplayOptions(Intermediary trader, ref int currentDay)
    {
        RenderTraderInfo(trader, currentDay);
        Console.WriteLine("e) Einkaufen");
        Console.WriteLine("b) Runde beenden");

        string choice = Console.ReadLine() ?? "";

        if (choice == "b")
        {
            // nichts machen, weitergehen zum nächsten Trader
        }
        else if (choice == "e")
        {
            Console.WriteLine("Verfügbare Produkte:");
            foreach (var product in trader.Products)
            {
                Console.WriteLine($"{product.Id}) {product.Name} ({product.Durability} Tage)");
            }
            Console.WriteLine("z) Zurück");
            if (Console.ReadLine() == "z")
            {
                // Züruck zum Hauptmenü
            }
        }
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

    static void RunDayCycle(List<Intermediary> traders, ref int currentDay)
    {
        foreach (var trader in traders)
        {
            DisplayOptions(trader, ref currentDay);
        }

        RotateIntermediary(traders);
        currentDay++;
    }

    static void Main()
    {
        List<Intermediary> traders = GenerateParticipantList();
        int currentDay = 1;

        while (true)
        {
            RunDayCycle(traders, ref currentDay);
        }
    }
}