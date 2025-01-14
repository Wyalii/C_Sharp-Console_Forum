using Terminal.Gui;

namespace Forum
{
    public class AdminMenu
    {
        AdminCurd adminCurd = new AdminCurd();
        public void ShowAdminMenu(Toplevel top, User LoggedInUser)
        {
            MainMenu mainMenu = new MainMenu();
            if (LoggedInUser.Username.ToLower() != "admin" && LoggedInUser.Password != "AdminSecret")
            {
                MessageBox.ErrorQuery("Auth Error", "restricted area.", "OK");
                top.RemoveAll();
                mainMenu.ShowMainMenu(top);
            }
            top.RemoveAll();
            var window = new FrameView()
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            var ViewAllPostsBtn = new Button("View All Posts")
            {
                X = Pos.Center(),
                Y = 1,
            };

            var ViewAllGroupsBtn = new Button("View All Groups")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(ViewAllPostsBtn) + 1
            };

            var ViewAllUsersBtn = new Button("View All Users")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(ViewAllGroupsBtn) + 1
            };

            var ExitBtn = new Button("Exit")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(ViewAllUsersBtn) + 1
            };

            ExitBtn.Clicked += () =>
            {
                LoggedInUser.Username = "";
                mainMenu.ShowMainMenu(top);
            };

            ViewAllPostsBtn.Clicked += () =>
            {
                adminCurd.AdminViewAllPosts(top, LoggedInUser);
            };

            ViewAllUsersBtn.Clicked += () =>
            {
                adminCurd.AdminViewAllUsers(top, LoggedInUser);
            };

            ViewAllGroupsBtn.Clicked += () =>
            {
                adminCurd.AdminViewAllGroups(top, LoggedInUser);
            };

            window.Add(ViewAllPostsBtn, ViewAllGroupsBtn, ViewAllUsersBtn, ExitBtn);
            top.Add(window);

        }
    }
}