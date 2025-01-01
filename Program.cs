namespace Forum
{
    using Microsoft.Identity.Client;
    using Terminal.Gui;
    public class Program
    {
      static void Main()
      {
        Application.Init();
        var top = Application.Top;
        MainMenu mainMenu = new MainMenu();
        mainMenu.ShowMainMenu(top);
        Application.Run();

      }
    }
}