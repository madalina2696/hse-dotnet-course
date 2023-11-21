using TheMiddleman.Entity;

class Application
{
    static void Main()
    {
        // Instanzen BusinessLogic und UserInterface
        BusinessLogic businessLogic = new BusinessLogic();
        UserInterface ui = new UserInterface(businessLogic);
        int totalDays = ui.ReadSimulationDuration();
        // Tradersliste
        List<Trader> traders = businessLogic.CreateTraders(ui);
        // Produkliste ablesen
        List<Product> products = businessLogic.GetProducts();

        int currentDay = 1; // fängt bei 1 an
        while (currentDay <= totalDays)
        {
            businessLogic.RunDayCycle(traders, ui, ref currentDay);
        }
        ui.DisplayRanking(traders);
    }
}