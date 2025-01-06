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
            Y = 1,
          };

          var ViewMyPosts = new Button("View My Posts")
          {
            X = Pos.Center(),
            Y = 2,
          };

          var AddPost = new Button("Add Post")
          {
            X = Pos.Center(),
            Y = 3,
          };

          
          var AddCommentToPost = new Button("Add Comment To a Post")
          {
            X = Pos.Center(),
            Y = 4,
          };

          var RemoveMyPost = new Button("Remove my Post")
          {
            X = Pos.Center(),
            Y = 5,
          };

          var SearchPost = new Button("Search Post")
          {
            X = Pos.Center(),
            Y = 6,
          };

          var ViewGroups = new Button("View Groups")
          {
            X = Pos.Center(),
            Y = 7,
          };

          var CreateGroup = new Button("Create Group")
          {
            X = Pos.Center(),
            Y = 8,
          };

          var ViewGroupComments = new Button("View Group Comments")
          {
            X = Pos.Center(),
            Y = 10,
          };

          var AddCommentToGroup = new Button("Add Comment to Group")
          {
            X = Pos.Center(),
            Y = 11,
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

          ViewMyPosts.Clicked += () =>
          {
            userCrud.ViewMyPosts(top,LoggedInUser);
          };

          ViewPosts.Clicked += ()=>
          {
            userCrud.ViewAllPosts(top,LoggedInUser);
          };

          AddCommentToPost.Clicked += () =>
          {
            userCrud.AddComment(top,LoggedInUser);
          };
          RemoveMyPost.Clicked += () =>
          {
            userCrud.RemoveMyPost(top,LoggedInUser);
          };
          SearchPost.Clicked += () =>
          {
            userCrud.SearchPost(top,LoggedInUser);
          };
          CreateGroup.Clicked += () =>
          {
            userCrud.CreateGroup(top,LoggedInUser);
          };
          ViewGroups.Clicked += () =>
          {
            userCrud.ViewGroups(top,LoggedInUser);
          };
          

          window.Add(TitleLabel,ViewPosts,ViewMyPosts,AddPost,AddCommentToPost,RemoveMyPost,SearchPost,ViewGroups,CreateGroup,ViewGroupComments,AddCommentToGroup,ExitButton);
          top.Add(window);
          

        }
    }
}