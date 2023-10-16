using System;
using System.Collections.Generic;
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

    static Intermediary AssembleTrader(int position)
    {
        string traderName = FetchTraderName(position);
        string firmName = FetchFirmName(traderName);
        return new Intermediary { Name = traderName, Company = firmName };
    }

    static List<Intermediary> GenerateParticipantList()
    {
        List<Intermediary> participants = new List<Intermediary>();
        int traderCount = QueryParticipantCount();

        for (int i = 1; i <= traderCount; i++)
        {
            participants.Add(AssembleTrader(i));
        }

        return participants;
    }

    static void RenderTraderInfo(Intermediary trader, int currentDay)
    {
        Console.WriteLine($"{trader.Name} von {trader.Company} | Tag {currentDay}");
        Console.WriteLine("b) Runde beenden");
    }

    static void DisplayOptions(Intermediary trader, ref int currentDay)
    {
        RenderTraderInfo(trader, currentDay);
        string choice = Console.ReadLine() ?? "";

        if (choice == "b")
        {
            // Do nothing, just continue to next middleman
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