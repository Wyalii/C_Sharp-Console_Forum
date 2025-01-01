using Terminal.Gui;

namespace Forum
{
    public class MainMenu
    {
        public void ShowMainMenu(Toplevel top)
        {
            top.RemoveAll();
            var mainMenuWindow = new FrameView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            var titleLabel = new Label("Main Menu")
            {
                X = Pos.Center(),
                Y = 1,
            };

            var registerButton = new Button("Register")
            {
                X = Pos.Center(),
                Y = Pos.Top(titleLabel) + 2,
            };

            var loginButton = new Button("Login")
            {
                X = Pos.Center(),
                Y = Pos.Top(registerButton) + 2,
            };

            var exitButton = new Button("Exit")
            {
                X = Pos.Center(),
                Y = Pos.Top(loginButton) + 2,
            };

            registerButton.Clicked += () =>
            {
                UserAuth userAuth = new UserAuth();
                userAuth.Register(top);
            };

            loginButton.Clicked += () =>
            {
                UserAuth userAuth = new UserAuth();
                userAuth.Login(top);
            };

            exitButton.Clicked += () =>
            {
                Application.RequestStop();
            };

            mainMenuWindow.Add(titleLabel, registerButton, loginButton, exitButton);
            top.Add(mainMenuWindow);
        }
    }
}
