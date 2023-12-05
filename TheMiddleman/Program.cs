using TheMiddleman.Entity;

class Application
{
    static void Main()
    {
        BusinessLogic businessLogic = new BusinessLogic();
        businessLogic.Initialize();
        UserInterface ui = new UserInterface(businessLogic);
        ui.StartSimulation();
    }
}