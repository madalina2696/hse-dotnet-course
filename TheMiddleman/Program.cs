using TheMiddleman.Entity;

class Application
{
    static void Main()
    {
        BusinessLogic businessLogic = new BusinessLogic();
        UserInterface ui = new UserInterface(businessLogic);
        int totalDays = ui.ReadSimulationDuration();
        List<Trader> traders = businessLogic.CreateTraders(ui);
        List<Product> products = businessLogic.GetProducts();

        int currentDay = 1;
        while (currentDay <= totalDays)
        {
            businessLogic.RunDayCycle(traders, ui, ref currentDay);
        }
        ui.DisplayRanking(traders);
    }
}