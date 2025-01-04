using Microsoft.EntityFrameworkCore;
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
                
                var PostTitle = PostTitleField.Text.ToString();
                if(string.IsNullOrWhiteSpace(PostTitle))
                {
                  MessageBox.ErrorQuery("Validation Error","Invalid Post Title Input.","OK");
                  return;
                }
                var PostContent = PostContentField.Text.ToString();
                if(string.IsNullOrWhiteSpace(PostContent))
                {
                  MessageBox.ErrorQuery("Validation Error","Invalid Post Content Input.","OK");
                  return;
                }
                Post NewPost = new Post{Title = PostTitle, Content = PostContent, UserId = LoggedInUser.Id};
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
          var UserPosts = database.Posts.Include(post => post.user).Where(p => p.UserId == LoggedInUser.Id).ToList();
          if(UserPosts.Count == 0)
          {
            MessageBox.ErrorQuery("Post Error","User doesnt have any posts.","OK");
            UserMenu userMenu = new UserMenu();
            userMenu.ShowUserMenu(top,LoggedInUser);
          }
          var Formated = UserPosts.Select(post => $"Id: {post.Id} | Title: {post.Title}").ToList();

          var window = new ListView(Formated)
          {
            X = Pos.Center(),
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill() - 3
          };

          window.KeyDown += (e) =>
          {
            if(e.KeyEvent.Key == Key.Enter)
            {
              var PostIndex = window.SelectedItem;
              if(PostIndex != -1)
              {
                var SelectedPost = UserPosts[PostIndex];
                if(SelectedPost == null)
                {
                  MessageBox.ErrorQuery("Post Error","Post Doesn't Exists.","OK");
                  return;
                }
                var Comments = database.Comments.Include(comment => comment.user).Where(c => c.PostId == SelectedPost.Id).ToList();
                var FormattedComments = Comments.Any() ? Comments.Select(comment => $"User: {comment.user.Username} | Comment: {comment.Text}").ToList() : new List<string>{"no comments found"};
                var PostContent = SelectedPost.Content;

                var PostWindow = new Window($"Post Id: {SelectedPost.Id}, Post Author: {SelectedPost.user.Username}, Post Title: {SelectedPost.Title}")
                {
                  X = 0,
                  Y = 0,
                  Width = Dim.Fill(),
                  Height = Dim.Fill() - 3,

                };

                var PostContentView = new TextView()
                {
                  X = 0,
                  Y = 0,
                  Width = Dim.Fill(),
                  Height = 10,
                  Text = PostContent, 
                  ReadOnly = true,
                    
                };

                var CommentsListView = new ListView(FormattedComments)
                {
                  X = 0,
                  Y = Pos.Bottom(PostContentView) + 2,
                  Width = Dim.Fill(),
                  Height = 10
                };

                var ExitComments = new Button("Exit Comments")
                {
                  X = Pos.Center(),
                  Y = Pos.Bottom(CommentsListView) + 2,
                };

                ExitComments.Clicked += () =>
                {
                  ViewAllPosts(top,LoggedInUser);
                };

                PostWindow.Add(PostContentView,ExitComments,CommentsListView);
                top.RemoveAll();
                top.Add(PostWindow);
              }
            }
          };
         

          var ExitButton = new Button("Exit")
          {
            X = Pos.Center(),
            Y = Pos.Bottom(window) + 1
          };

          ExitButton.Clicked += () =>
          {
            UserMenu userMenu = new UserMenu();
            userMenu.ShowUserMenu(top,LoggedInUser);
          };
          
          
          top.Add(window,ExitButton);
        }

        public void ViewAllPosts(Toplevel top,User LoggedInUser)
        {
            top.RemoveAll();

            var Posts = database.Posts.Include(post => post.user).ToList();
            if(Posts.Count == 0)
            {
              MessageBox.ErrorQuery("Posts Error","No Posts Avaiable","OK");
              UserMenu userMenu = new UserMenu();
              userMenu.ShowUserMenu(top,LoggedInUser);
            }

            var FormatedPosts = Posts.Select(post => $"Post Id: {post.Id} | Post Author: {post.user.Username}  | Title: {post.Title}").ToList();

            var window = new ListView(FormatedPosts)
            {
              X = 0,
              Y = 0,
              Width = Dim.Fill(),
              Height = Dim.Fill() - 3
            };

            window.KeyDown += (e) =>
            {
              if(e.KeyEvent.Key == Key.Enter)
              {
                var PostIndex = window.SelectedItem;
                if(PostIndex != -1)
                {
                   var SelectedPost = Posts[PostIndex];
                   if(SelectedPost == null)
                   {
                     MessageBox.ErrorQuery("Post Error","Post Doesn't Exists.","OK");
                     return;
                   }
                   var Comments = database.Comments.Include(comment => comment.user).Where(c => c.PostId == SelectedPost.Id).ToList();
                   var FormattedComments = Comments.Any() ? Comments.Select(comment => $"User: {comment.user.Username} | Comment: {comment.Text}").ToList() : new List<string>{"no comments found"};
                   var PostContent = SelectedPost.Content;

                   var PostWindow = new Window($"Post Author: {SelectedPost.user.Username}, Post Title: {SelectedPost.Title}")
                   {
                     X = 0,
                     Y = 0,
                     Width = Dim.Fill(),
                     Height = Dim.Fill() - 3,

                   };

                   var PostContentView = new TextView()
                   {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = 10,
                    Text = PostContent, 
                    ReadOnly = true,
                    
                   };

                   var CommentsListView = new ListView(FormattedComments)
                   {
                     X = 0,
                     Y = Pos.Bottom(PostContentView) + 2,
                     Width = Dim.Fill(),
                     Height = 10
                   };

                   var ExitComments = new Button("Exit Comments")
                   {
                     X = Pos.Center(),
                     Y = Pos.Bottom(CommentsListView) + 2,
                   };

                   ExitComments.Clicked += () =>
                   {
                     ViewAllPosts(top,LoggedInUser);
                   };

                   PostWindow.Add(PostContentView,ExitComments,CommentsListView);
                   top.RemoveAll();
                   top.Add(PostWindow);
                }

              }
            };
         

            var ExitButton = new Button("Exit")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(window) + 1
            };

            ExitButton.Clicked += () =>
            {
               UserMenu userMenu = new UserMenu();
               userMenu.ShowUserMenu(top,LoggedInUser);
            };
          
          top.RemoveAll();
          top.Add(window,ExitButton);
        }

        public void AddComment(Toplevel top, User LoggedInUser)
        {
           top.RemoveAll();

           var Frame = new FrameView()
           {
             X = 0,
             Y = 0,
             Width = Dim.Fill(),
             Height = Dim.Fill(),
           };
           
           var PostIdLabel = new Label("Write Post Id")
           {
             X = Pos.Center(),
             Y = 1,
           };

           var PostIdField = new TextField()
           {
             X = Pos.Center(),
             Y = 2,
             Width = 20,
           };

           var CommentLabel = new Label("Write Your Comment")
           {
             X = Pos.Center(),
             Y = 4
           };

           var CommentField = new TextField()
           {
             X = Pos.Center(),
             Y = 5,
             Width = 20
           };

           var SubmitButton = new Button("Submit")
           {
             X = Pos.Center(),
             Y = 7,
           }; 

           var ExitButton = new Button("Exit")
           {
             X = Pos.Center(),
             Y = 9,
           };

           ExitButton.Clicked += () =>
           {
             UserMenu userMenu = new UserMenu();
             userMenu.ShowUserMenu(top,LoggedInUser);

           };

           SubmitButton.Clicked += () =>
           {
             try
             {
               var PostId = PostIdField.Text.ToString();
               if(string.IsNullOrEmpty(PostId))
               {
                 MessageBox.ErrorQuery("Validation Error","Invalid Post id input.","OK");
                 return;
               }
               var comment = CommentField.Text.ToString();
               if(string.IsNullOrWhiteSpace(comment))
               {
                 MessageBox.ErrorQuery("Validation Error","Invalid comment input.","OK");
                 return;
               }
               var IntPostId = int.Parse(PostId);

               var post = database.Posts.FirstOrDefault(P => P.Id.ToString() == PostId);
               if(post == null)
               {
                 MessageBox.ErrorQuery("Post Error","Post with provided Id Doesn't Exists.","OK");
                 return;
               }
               Comment NewComment = new Comment{PostId = IntPostId, UserId = LoggedInUser.Id, Text = comment};
               database.Comments.Add(NewComment);
               database.SaveChanges();
               MessageBox.Query("Success",$"Message: {NewComment.Text}, was added","OK");
               
             }
             catch (Exception ex)
             {
               MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
             }
           };

           Frame.Add(PostIdLabel,PostIdField,CommentLabel,CommentField,ExitButton,SubmitButton);
           top.Add(Frame);
        }


        public void RemoveMyPost(Toplevel top,User LoggedInUser)
        {
           top.RemoveAll();

           var Frame = new FrameView()
           {
             Width = Dim.Fill(),
             Height = Dim.Fill() - 3
           };

           var PostIdLabel = new Label("Write Post Id")
           {
             X = Pos.Center(),
             Y = 1,
           };

           var PostIdField = new TextField()
           {
             X = Pos.Center(),
             Y = Pos.Bottom(PostIdLabel),
             Width = 20
           };

           var SubmitButton = new Button("Submit")
           {
             X = Pos.Center(),
             Y = Pos.Bottom(PostIdField) + 1
           };

           var ExitButton = new Button("Exit")
           {
             X = Pos.Center(),
             Y = Pos.Bottom(SubmitButton) + 1
           };

           ExitButton.Clicked += () =>
           {
             UserMenu userMenu = new UserMenu();
             userMenu.ShowUserMenu(top,LoggedInUser);
           };

           SubmitButton.Clicked += () =>
           {
             var PostIdInput = PostIdField.Text.ToString();
             if(string.IsNullOrWhiteSpace(PostIdInput))
             {
               MessageBox.ErrorQuery("Validation Error","Invalid Id Input.","OK");
               return;
             }
             var PostIdInt = int.Parse(PostIdInput);
             
             try
             {
               var post = database.Posts.FirstOrDefault(p => p.Id == PostIdInt && p.UserId == LoggedInUser.Id);
               if(post == null)
               {
                 MessageBox.ErrorQuery("Post Error","Post with provided id doesn't exists.","OK");
                 return;
               }

               database.Posts.Remove(post);
               database.SaveChanges();
               MessageBox.Query("Succes",$"Removed Post - Id: {post.Id}, Title: {post.Title}","OK");
             }
             catch (Exception ex)
             {
               MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
             }

           };

           Frame.Add(PostIdLabel, PostIdField, SubmitButton, ExitButton);
           top.Add(Frame);
        }

    }
}

