using TheMiddleman.Entity;

class Application
{
    static void Main()
    {
        BusinessLogic businessLogic = new BusinessLogic();
        businessLogic.InitializeParser();
        UserInterface ui = new UserInterface(businessLogic);
        ui.StartSimulation();
    }
}