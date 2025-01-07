using Terminal.Gui;

namespace Forum
{
    public class UserMenu
    {
        public void ShowUserMenu(Toplevel top,User LoggedInUser)
        {
          UserCrud userCrud = new UserCrud();
          top.RemoveAll();
          var window = new FrameView()
          {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
          };

          var TitleLabel = new Label($"User: {LoggedInUser.Username}'s Actions:")
          {
            X = Pos.Center(),
            Y = 0
          };

          var ViewPosts = new  Button("View Posts")
          {
            X = Pos.Center(),
            Y = Pos.Bottom(TitleLabel) + 2,
          };

          var AddPost = new Button("Add Post")
          {
            X = Pos.Center(),
            Y = Pos.Bottom(ViewPosts) + 1,
          };

          var SearchPost = new Button("Search Post")
          {
            X = Pos.Center(),
            Y = Pos.Bottom(AddPost) + 1,
          };

          var ViewGroups = new Button("View Groups")
          {
            X = Pos.Center(),
            Y = Pos.Bottom(SearchPost) + 1,
          };

          var ExitButton = new Button("Exit")
          {
            X = Pos.Center(),
            Y = 12,
          };

          ExitButton.Clicked += () =>
          {
            MainMenu mainMenu = new MainMenu();
            mainMenu.ShowMainMenu(top);
          };

          AddPost.Clicked += () =>
          {
            userCrud.AddPost(top,LoggedInUser);
          };

          ViewPosts.Clicked += ()=>
          {
            userCrud.ViewAllPosts(top,LoggedInUser);
          };

          SearchPost.Clicked += () =>
          {
            userCrud.SearchPost(top,LoggedInUser);
          };
          
          ViewGroups.Clicked += () =>
          {
            userCrud.ViewGroups(top,LoggedInUser);
          };
          

          window.Add(TitleLabel,ViewPosts,AddPost,SearchPost,ViewGroups,ExitButton);
          top.Add(window);
          

        }
    }
}