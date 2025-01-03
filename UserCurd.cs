using Terminal.Gui;

namespace Forum
{
    public class UserCrud
    {
        Database database = new Database();
        public void AddPost(Toplevel top, User LoggedInUser)
        {
            top.RemoveAll();

            var window = new FrameView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            var TitleLabel = new Label("Provide Post Credentials:")
            {
                X = Pos.Center(),
                Y = 0,
            };

            var PostTitleLabel = new Label("Provide Post Title: ")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(TitleLabel) + 1,
            };

            var PostTitleField = new TextField()
            {
                X = Pos.Center(),
                Y = Pos.Bottom(PostTitleLabel) + 1,
                Width = 20
            };

            var PostContentLabel = new Label("Provide Post Content: ")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(PostTitleField) + 1,
            };

            var PostContentField = new TextView()
            {
                X = Pos.Center(),
                Y = Pos.Bottom(PostContentLabel) + 1,
                Width = 40,
                Height = 10,
            };

            var CreatePost = new Button("Create a Post")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(PostContentField) + 2,
            };

            var ExitButton = new Button("Exit")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(CreatePost) + 1,
            };

            ExitButton.Clicked += () =>
            {
                top.RemoveAll();
                UserMenu userMenu = new UserMenu();
                userMenu.ShowUserMenu(top,LoggedInUser);
            };

            CreatePost.Clicked += () =>
            {

              try
              {
                if (LoggedInUser == null)
                {
                  MessageBox.ErrorQuery("Error", "User is not logged in.", "OK");
                  return;
                }

                if (LoggedInUser.Id == null)
                {
                  MessageBox.ErrorQuery("Error", "User ID is null.", "OK");
                  return;
                }

                if (database == null)
                {
                  MessageBox.ErrorQuery("Error", "Database context is null.", "OK");
                  return;
                }

                var PostTitle = PostTitleField.Text.ToString();
                var PostContent = PostContentField.Text.ToString();
                Post NewPost = new Post{Title = PostTitle, Content = PostContent, UserId = LoggedInUser.Id};
                if (NewPost == null)
                {
                  MessageBox.ErrorQuery("Error", "Failed to create new post object.", "OK");
                  return;
                }
                MessageBox.Query("Debug", $"PostTitle: {PostTitleField.Text}, PostContent: {PostContentField.Text}, LoggedInUser: {LoggedInUser?.Id}", "OK");
                database.Posts.Add(NewPost);
                database.SaveChanges();
                MessageBox.Query("Success",$"Post: {NewPost.Title} was created!","OK");
              }
              catch (Exception ex)
              {
                MessageBox.ErrorQuery("Error",$"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
              }
            };

            window.Add(TitleLabel, PostTitleLabel, PostTitleField, PostContentLabel, PostContentField, CreatePost, ExitButton);
            top.Add(window);
        }

        public void ViewMyPosts(Toplevel top, User LoggedInUser)
        {
          top.RemoveAll();

          var window = new FrameView()
          {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
          };

          var UserPosts = database.Posts.Where(p => p.UserId == LoggedInUser.Id).ToList();
          var Formated = UserPosts.Select(post => $"Id: {post.Id}\nTitle: {post.Title}\nContent: {post.Content}").ToList();
          
          var textView = new TextView()
          {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Text = string.Join("\n\n",Formated),
          };
          window.Add(textView);
          top.Add(window);
        }
    }
}
