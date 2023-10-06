using System.Globalization;
using System.Reflection.Metadata.Ecma335;

class Application
{
    static void Main()
    {
        var intermediaries = new List<Intermediary>();
        var currentDay = 1;

        Console.WriteLine("Wieviel Zwischenhändler nehmen teil? ");

        int.TryParse(Console.ReadLine(), out int intermediaryCount);

        for (int index = 1; index <= intermediaryCount; index++)
        {
            Console.WriteLine($"Name von Zwischenhändler {index}: ");
            var individualName = Console.ReadLine() ?? "";

            Console.WriteLine($"Name der Firma von {individualName}: ");
            var organizationName = Console.ReadLine() ?? "";

            intermediaries.Add(new Intermediary { Name = individualName, Company = organizationName });
        }

        while (true)
        {
            foreach (var intermediary in intermediaries)
            {
                Console.WriteLine($"{intermediary.Name} von {intermediary.Company} | Tag {currentDay}");
                Console.WriteLine("b) Runde beenden");
                Console.WriteLine("q) Programm schiessen");

                var selectedOption = Console.ReadLine() ?? "q";

                if (selectedOption.Equals("b", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                else if (selectedOption.Equals("q", StringComparison.OrdinalIgnoreCase) || selectedOption == "")
                {
                    return;
                }
            }

            currentDay++;
        }
    }

    private class Intermediary
    {
        public string? Name { get; set; }
        public string? Company { get; set; }
    }
}
