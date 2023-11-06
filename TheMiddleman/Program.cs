using TheMiddleman.Entity;

class Application
{
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