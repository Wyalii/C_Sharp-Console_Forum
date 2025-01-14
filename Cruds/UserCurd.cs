using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Identity.Client;
using Terminal.Gui;

namespace Forum
{
  public class UserCrud
  {
    Database database = new Database();
    MainMenu mainMenu = new MainMenu();
    UserMenu userMenu = new UserMenu();
    public void AddPost(Toplevel top, User LoggedInUser)
    {
      if (!database.Users.Any(u => u.Id == LoggedInUser.Id))
      {
        mainMenu.ShowMainMenu(top);
      }
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
        userMenu.ShowUserMenu(top, LoggedInUser);
      };

      CreatePost.Clicked += () =>
      {

        try
        {

          var PostTitle = PostTitleField.Text.ToString();
          if (string.IsNullOrWhiteSpace(PostTitle))
          {
            MessageBox.ErrorQuery("Validation Error", "Invalid Post Title Input.", "OK");
            return;
          }
          var PostContent = PostContentField.Text.ToString();
          if (string.IsNullOrWhiteSpace(PostContent))
          {
            MessageBox.ErrorQuery("Validation Error", "Invalid Post Content Input.", "OK");
            return;
          }
          if (database.Posts.Any(p => p.Title.ToLower() == PostTitle.ToLower()))
          {
            MessageBox.ErrorQuery("Post Error", "Post with this title already exists.", "OK");
            return;
          }
          Post NewPost = new Post { Title = PostTitle, Content = PostContent, UserId = LoggedInUser.Id };
          database.Posts.Add(NewPost);
          database.SaveChanges();
          MessageBox.Query("Success", $"Post: {NewPost.Title} was created!", "OK");
          ViewAllPosts(top, LoggedInUser);
        }
        catch (Exception ex)
        {
          MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
        }
      };

      window.Add(TitleLabel, PostTitleLabel, PostTitleField, PostContentLabel, PostContentField, CreatePost, ExitButton);
      top.Add(window);
    }

    public void ViewAllPosts(Toplevel top, User LoggedInUser)
    {
      if (!database.Users.Any(u => u.Id == LoggedInUser.Id))
      {
        MessageBox.ErrorQuery("User Error", "LoggedIn User Coudn't be found.", "OK");
        MainMenu mainMenu = new MainMenu();
        mainMenu.ShowMainMenu(top);
      }

      top.RemoveAll();
      var WindowFrame = new FrameView()
      {
        Width = Dim.Fill(),
        Height = Dim.Fill()
      };

      var AllPosts = database.Posts
      .Include(p => p.user)
      .Where(p => p.UserId != LoggedInUser.Id)
      .ToList();

      var MyPosts = database.Posts
      .Include(p => p.user)
      .Where(p => p.UserId == LoggedInUser.Id)
      .ToList()
      ;

      var FromatedAllPosts = AllPosts.Any()
      ? database.Posts
      .Include(p => p.user)
      .Select(p => $"Post Id: {p.Id}, Author: {p.user.Username}")
      .ToList()

      : new List<string> { "No Posts avaiable." };

      var FormatedMyPosts = MyPosts.Any()
      ? database.Posts
      .Include(p => p.user)
      .Select(p => $"Post Id: {p.Id}, Author: {p.user.Username}")
      .ToList()

      : new List<string> { "No posts Created yet." };



      var AllPostsLabel = new Label("All Posts:")
      {
        X = Pos.Center(),
        Y = 1
      };
      var AllPostsListView = new ListView(FromatedAllPosts)
      {
        Width = Dim.Fill(),
        Height = 10,
        Y = Pos.Bottom(AllPostsLabel) + 1
      };

      var MyPostsLabel = new Label("My Posts:")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(AllPostsListView) + 2
      };
      var MyPostsListView = new ListView(FormatedMyPosts)
      {
        Width = Dim.Fill(),
        Height = 10,
        Y = Pos.Bottom(MyPostsLabel) + 1
      };

      var ExitBtn = new Button("Exit")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(MyPostsListView) + 2
      };

      ExitBtn.Clicked += () =>
      {
        userMenu.ShowUserMenu(top, LoggedInUser);
      };

      AllPostsListView.KeyPress += e =>
      {
        var PostIndex = AllPostsListView.SelectedItem;
        if (e.KeyEvent.Key == Key.Enter)
        {
          if (FromatedAllPosts[PostIndex] == "No Posts avaiable.")
          {
            MessageBox.ErrorQuery("No Posts", "No Posts are avaiable.", "OK");
            e.Handled = true;
            return;
          }
          else
          {
            e.Handled = false;
            var SelectedPost = AllPosts[PostIndex];
            var PostWindow = new Window($"Post Id: {SelectedPost.Id}, Author: {SelectedPost.user.Username}, Title: {SelectedPost.Title}")
            {
              Width = Dim.Fill(),
              Height = Dim.Fill(),
            };

            var AllComments = database.Comments
            .Include(c => c.user)
            .Where(c => c.PostId == SelectedPost.Id)
            .ToList();

            var FormatedComments = AllComments.Any()
            ? AllComments.Select(c => $"User: {c.user.Username}, Comment: {c.Text}").ToList()
            : new List<string> { "No Comments Avaible." };

            var CommentsLabel = new Label("Comments: ")
            {
              X = Pos.Center(),
              Y = 1,
            };

            var CommentsListView = new ListView(FormatedComments)
            {
              Width = Dim.Fill(),
              Height = 10,
              Y = Pos.Bottom(CommentsLabel) + 1
            };

            var AddCommentBtn = new Button("Add Comment")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(CommentsListView) + 2
            };

            var ExitPost = new Button("Exit")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(AddCommentBtn) + 1
            };

            ExitPost.Clicked += () =>
            {
              ViewAllPosts(top, LoggedInUser);
            };

            AddCommentBtn.Clicked += () =>
            {
              AddComment(top, LoggedInUser, SelectedPost.Id);
            };


            PostWindow.Add(CommentsLabel, CommentsListView, ExitPost, AddCommentBtn);
            top.RemoveAll();
            top.Add(PostWindow);
          }
        }
      };

      MyPostsListView.KeyPress += e =>
      {
        var PostIndex = MyPostsListView.SelectedItem;

        if (e.KeyEvent.Key == Key.Backspace)
        {
          if (FormatedMyPosts[PostIndex] == "No posts Created yet.")
          {
            MessageBox.ErrorQuery("No Posts", "No Posts avaiable yet.", "OK");
            e.Handled = true;
            return;
          }

          var SelectedPost = MyPosts[PostIndex];
          if (SelectedPost != null)
          {
            try
            {
              var result = MessageBox.Query("Deleting Post", "Do You Want do delete this post?", "YES", "NO");
              if (result == 0)
              {
                database.Posts.Remove(SelectedPost);
                database.SaveChanges();
                MessageBox.Query("Success", $"Deleted Post: {SelectedPost.Title}", "OK");
                ViewAllPosts(top, LoggedInUser);
              }

              if (result == 1)
              {
                MessageBox.Query("Delete canceled", "Delete action of post was cannceled", "OK");
                return;
              }
            }
            catch (Exception ex)
            {
              MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
            }
          }
        }

        if (e.KeyEvent.Key == Key.Enter)
        {
          if (FormatedMyPosts[PostIndex] == "No posts Created yet.")
          {
            MessageBox.ErrorQuery("No Posts", "No Posts avaiable yet.", "OK");
            e.Handled = true;
            return;
          }
          else
          {
            e.Handled = false;
            var SelectedPost = MyPosts[PostIndex];
            if (SelectedPost != null)
            {
              var MyPostsWindow = new Window($"Post Id: {SelectedPost.Id}, Author: {SelectedPost.user.Username}, Title: {SelectedPost.Title}")
              {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
              };

              var MyPostsComments = database.Comments
              .Include(c => c.user)
              .Where(c => c.PostId == SelectedPost.Id)
              .ToList();

              var FormatedComments = MyPostsComments.Any()
              ? MyPostsComments.Select(c => $"Comment Id: {c.Id} User: {c.user.Username}, Comment: {c.Text}")
              .ToList()
              : new List<string> { "No Comments avaiable." };

              var MyPostsCommentsLabel = new Label("Comments")
              {
                X = Pos.Center(),
                Y = 1
              };

              var MyPostsCommentsListView = new ListView(FormatedComments)
              {
                Width = Dim.Fill(),
                Y = Pos.Bottom(MyPostsCommentsLabel) + 1,
                Height = 10,
              };

              var AddCommentBtn = new Button("Add Comment")
              {
                X = Pos.Center(),
                Y = Pos.Bottom(MyPostsCommentsListView) + 2,
              };

              var ExitPost = new Button("Exit")
              {
                X = Pos.Center(),
                Y = Pos.Bottom(AddCommentBtn) + 1,
              };

              ExitPost.Clicked += () =>
              {
                ViewAllPosts(top, LoggedInUser);
              };

              AddCommentBtn.Clicked += () =>
              {
                AddComment(top, LoggedInUser, SelectedPost.Id);
              };

              MyPostsCommentsListView.KeyPress += e =>
              {
                var CommentIndex = MyPostsCommentsListView.SelectedItem;
                if (e.KeyEvent.Key == Key.Backspace)
                {
                  if (FormatedComments[CommentIndex] == "No Comments avaiable.")
                  {
                    MessageBox.ErrorQuery("No Comments", "No Comments Avaiable,", "OK");
                    e.Handled = true;
                    return;
                  }
                  var SelectedComment = MyPostsComments[CommentIndex];
                  if (SelectedComment != null)
                  {
                    var result = MessageBox.Query("Deleting Comment", $"DO you want to delete comment: {SelectedComment.Text}?", "YES", "NO");
                    if (result == 0)
                    {
                      try
                      {
                        database.Comments.Remove(SelectedComment);
                        database.SaveChanges();
                        MyPostsComments = database.Comments
                        .Include(c => c.user)
                        .Where(c => c.PostId == SelectedPost.Id)
                        .ToList();

                        FormatedComments = MyPostsComments.Any()
                        ? MyPostsComments.Select(c => $"Comment Id: {c.Id} User: {c.user.Username}, Comment: {c.Text}")
                        .ToList()
                        : new List<string> { "No Comments avaiable." };

                        MyPostsCommentsListView.SetSource(FormatedComments);
                        MessageBox.Query("Success", "Removed Comment!", "OK");
                      }
                      catch (Exception ex)
                      {
                        MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
                      }
                    }
                  }
                }
              };

              MyPostsWindow.Add(MyPostsCommentsLabel, MyPostsCommentsListView, ExitPost, AddCommentBtn);
              top.RemoveAll();
              top.Add(MyPostsWindow);
            };
          };
        }
      };

      WindowFrame.Add(AllPostsLabel, AllPostsListView, MyPostsLabel, MyPostsListView, ExitBtn);
      top.Add(WindowFrame);

    }

    public void AddComment(Toplevel top, User LoggedInUser, int PostId)
    {
      if (!database.Users.Any(u => u.Id == LoggedInUser.Id))
      {
        mainMenu.ShowMainMenu(top);
      }
      top.RemoveAll();

      var Frame = new FrameView()
      {
        X = 0,
        Y = 0,
        Width = Dim.Fill(),
        Height = Dim.Fill(),
      };

      var CommentLabel = new Label("Write Your Comment")
      {
        X = Pos.Center(),
        Y = 4
      };

      var CommentField = new TextView()
      {
        X = Pos.Center(),
        Y = 5,
        Width = 20,
        Height = 10
      };

      var SubmitButton = new Button("Submit")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(CommentField) + 1,
      };

      var ExitButton = new Button("Exit")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(SubmitButton) + 1,
      };

      ExitButton.Clicked += () =>
      {
        UserMenu userMenu = new UserMenu();
        userMenu.ShowUserMenu(top, LoggedInUser);

      };

      SubmitButton.Clicked += () =>
      {
        try
        {
          var comment = CommentField.Text.ToString();
          if (string.IsNullOrWhiteSpace(comment))
          {
            MessageBox.ErrorQuery("Validation Error", "Invalid comment input.", "OK");
            return;
          }

          var post = database.Posts.FirstOrDefault(P => P.Id == PostId);
          if (post == null)
          {
            MessageBox.ErrorQuery("Post Error", "Post with provided Id Doesn't Exists.", "OK");
            return;
          }
          Comment NewComment = new Comment { PostId = PostId, UserId = LoggedInUser.Id, Text = comment };
          database.Comments.Add(NewComment);
          database.SaveChanges();
          MessageBox.Query("Success", $"Message: {NewComment.Text}, was added", "OK");
          ViewAllPosts(top, LoggedInUser);

        }
        catch (Exception ex)
        {
          MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
        }
      };

      Frame.Add(CommentLabel, CommentField, ExitButton, SubmitButton);
      top.Add(Frame);
    }

    public void SearchPost(Toplevel top, User LoggedInUser)
    {
      if (!database.Users.Any(u => u.Id == LoggedInUser.Id))
      {
        mainMenu.ShowMainMenu(top);
      }

      top.RemoveAll();
      var Frame = new FrameView()
      {
        Width = Dim.Fill(),
        Height = Dim.Fill(),
      };

      var PostIdLabel = new Label("Provide Id")
      {
        X = Pos.Center(),
        Y = 0,
      };

      var PostIdField = new TextField()
      {
        Width = 20,
        X = Pos.Center(),
        Y = Pos.Bottom(PostIdLabel)
      };

      var SearchWithId = new Button("Search with Id")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(PostIdField) + 1
      };

      var PostTitleLabel = new Label("Provide Title")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(SearchWithId) + 2,
      };

      var PostTitleField = new TextField()
      {
        Width = 20,
        X = Pos.Center(),
        Y = Pos.Bottom(PostTitleLabel)
      };

      var SearchWithTitle = new Button("Search with Title")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(PostTitleField) + 1
      };

      var ExitButton = new Button("Exit")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(SearchWithTitle) + 2,
      };

      ExitButton.Clicked += () =>
      {
        UserMenu userMenu = new UserMenu();
        userMenu.ShowUserMenu(top, LoggedInUser);
      };

      SearchWithId.Clicked += () =>
      {
        var PostIdInput = PostIdField.Text.ToString();
        if (string.IsNullOrWhiteSpace(PostIdInput))
        {
          MessageBox.ErrorQuery("Validation Errror", "Invalid Id Input.", "OK");
          return;
        }
        var PostIdInt = int.Parse(PostIdInput);
        var post = database.Posts.Include(post => post.user).FirstOrDefault(p => p.Id == PostIdInt);
        if (post == null)
        {
          MessageBox.ErrorQuery("Post Error", "Post with provided Id doesn't exists.", "OK");
          return;
        }

        var IdWindow = new Window($"Id: {post.Id}, User: {post.user.Username}, Title: {post.Title}")
        {
          Width = Dim.Fill(),
          Height = Dim.Fill()
        };

        var IdPostContentView = new TextView()
        {
          Width = Dim.Fill(),
          Height = 20,
          Text = $"{post.Content}"
        };

        var comments = database.Comments
        .Include(comment => comment.user)
        .Where(c => c.PostId == post.Id);

        var FormatedComments = comments.Any()
        ? comments.Select(c => $"User: {c.user.Username}, Comment: {c.Text}").ToList()
        : new List<string> { "No comments avaiable" };

        var IdPostCommentsView = new ListView(FormatedComments)
        {
          X = Pos.Center(),
          Y = Pos.Bottom(IdPostContentView) + 3,
          Width = Dim.Fill(),
          Height = 10,
        };

        var ExitFromIdPost = new Button("Exit")
        {
          X = Pos.Center(),
          Y = Pos.Bottom(IdPostCommentsView) + 1
        };
        ExitFromIdPost.Clicked += () =>
        {
          SearchPost(top, LoggedInUser);
        };

        IdWindow.Add(IdPostContentView, IdPostCommentsView, ExitFromIdPost);
        top.RemoveAll();
        top.Add(IdWindow);

      };


      SearchWithTitle.Clicked += () =>
      {
        var PostTitleInput = PostTitleField.Text.ToString();
        if (string.IsNullOrWhiteSpace(PostTitleInput))
        {
          MessageBox.ErrorQuery("Validation Error", "Invalid title input.", "OK");
          return;
        }
        var post = database.Posts
        .Include(post => post.user)
        .FirstOrDefault(p => p.Title.ToLower() == PostTitleInput.ToLower());

        if (post == null)
        {
          MessageBox.ErrorQuery("Post Error", "Post with provided title doesn't exists.", "OK");
          return;
        }

        var TitleWindow = new Window($"Post Id: {post.Id}, User: {post.user.Username}, Title: {post.Title}")
        {
          Width = Dim.Fill(),
          Height = Dim.Fill(),
        };

        var TitlePostContent = new TextView()
        {
          Width = Dim.Fill(),
          Height = 20,
          Text = $"{post.Content}",
          ReadOnly = true,
        };

        var comments = database.Comments.Where(c => c.PostId == post.Id);
        var FormatedComments = comments.Any()
        ? comments.Select(c => $"User: {c.user.Username}, Comment: {c.Text}").ToList()
        : new List<string> { "No Comments found." };

        var TitlePostCommentsView = new ListView(FormatedComments)
        {
          Width = Dim.Fill(),
          Height = 10,
          Y = Pos.Bottom(TitlePostContent) + 2,
        };

        var ExitButton = new Button("Exit")
        {
          X = Pos.Center(),
          Y = Pos.Bottom(TitlePostCommentsView) + 2
        };

        ExitButton.Clicked += () =>
        {
          SearchPost(top, LoggedInUser);
        };

        TitleWindow.Add(TitlePostContent, TitlePostCommentsView, ExitButton);
        top.RemoveAll();
        top.Add(TitleWindow);

      };
      Frame.Add(PostIdLabel, PostIdField, SearchWithId, PostTitleLabel, PostTitleField, SearchWithTitle, ExitButton);
      top.Add(Frame);
    }

    public void CreateGroup(Toplevel top, User LoggedInUser)
    {
      if (!database.Users.Any(u => u.Id == LoggedInUser.Id))
      {
        mainMenu.ShowMainMenu(top);
      }

      top.RemoveAll();
      var window = new FrameView()
      {
        Width = Dim.Fill(),
        Height = Dim.Fill(),
      };

      var GroupNameLabel = new Label("Provide Group Name")
      {
        X = Pos.Center(),
        Y = 1,
      };

      var GroupNameField = new TextField()
      {
        X = Pos.Center(),
        Y = Pos.Bottom(GroupNameLabel),
        Width = 20,
      };

      var SubmitButton = new Button("Submit")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(GroupNameField) + 1,
      };

      var ExitButton = new Button("Exit")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(SubmitButton) + 1,

      };

      ExitButton.Clicked += () =>
      {
        UserMenu userMenu = new UserMenu();
        userMenu.ShowUserMenu(top, LoggedInUser);
      };

      SubmitButton.Clicked += () =>
      {
        var GroupNameInput = GroupNameField.Text.ToString();
        if (string.IsNullOrWhiteSpace(GroupNameInput))
        {
          MessageBox.ErrorQuery("Validation Error", "Wrong Group name input.", "OK");
          return;
        }
        var group = database.Groups.FirstOrDefault(g => g.Name.ToLower() == GroupNameInput.ToLower());
        if (group != null)
        {
          MessageBox.ErrorQuery("Group Error", "Group with provided name already exists.", "OK");
          return;
        }

        try
        {
          Group NewGroup = new Group { Name = GroupNameInput, AdminId = LoggedInUser.Id };
          database.Groups.Add(NewGroup);
          database.SaveChanges();
          var createdGroup = database.Groups.FirstOrDefault(g => g.Name.ToLower() == NewGroup.Name.ToLower());
          if (createdGroup == null)
          {
            MessageBox.ErrorQuery("Group Error", "Group was not found.", "OK");
            return;
          }
          UserGroup NewUserGroup = new UserGroup { UserId = createdGroup.AdminId, GroupId = createdGroup.Id };
          database.UserGroups.Add(NewUserGroup);
          database.SaveChanges();
          MessageBox.Query("Success", $"Group: {NewGroup.Name} was created!", "OK");
        }
        catch (Exception ex)
        {
          MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
        }


      };

      window.Add(GroupNameLabel, GroupNameField, SubmitButton, ExitButton);
      top.Add(window);
    }

    public void ViewGroups(Toplevel top, User LoggedInUser)
    {
      if (!database.Users.Any(u => u.Id == LoggedInUser.Id))
      {
        MessageBox.ErrorQuery("User Error", "LoggedIn User Coudn't be found.", "OK");
        MainMenu mainMenu = new MainMenu();
        mainMenu.ShowMainMenu(top);
      }

      top.RemoveAll();
      var WindowFrame = new FrameView()
      {
        Width = Dim.Fill(),
        Height = Dim.Fill()
      };

      var AllGroups = database.Groups.Include(g => g.Admin).Where(g => g.AdminId != LoggedInUser.Id).ToList();

      var CreatedGroups = database.Groups
      .Include(g => g.UserGroups)
      .Include(g => g.Admin)
      .Include(g => g.GroupComments)
      .Where(g => g.AdminId == LoggedInUser.Id)
      .ToList();


      var filteredGroups = AllGroups.Where(g => g.AdminId != LoggedInUser.Id).Select(g => g.Id).ToList();
      var JoinedGroups = database.UserGroups
      .Include(ug => ug.group)
      .Include(ug => ug.user)
      .Include(ug => ug.group.Admin)
      .Where(ug => ug.UserId == LoggedInUser.Id && filteredGroups.Contains(ug.GroupId))
      .ToList();


      var FormatedAllGroups = AllGroups.Any()
      ? AllGroups.Select(al => $"Group Id: {al.Id}, Group Name: {al.Name}, Admin: {al.Admin.Username}")
      .ToList()
      : new List<string> { "No Groups Avaiable." };

      var FormatedJoinedGroups = JoinedGroups.Any()
      ? JoinedGroups.Select(jg => $"Group Id: {jg.GroupId}, Group Name: {jg.group.Name}, Admin: {jg.group.Admin.Username}")
      .ToList()
      : new List<string> { "No Joined Groups Avaiable." };

      var FormatedCreatedGroups = CreatedGroups.Any()
      ? CreatedGroups.Select(cg => $"Group Id: {cg.Id}, Group Name: {cg.Name}, Admin: {cg.Admin.Username}")
      .ToList()
      : new List<string> { "No Created Groups Avaiable." };

      var AllGroupsLabel = new Label("All Groups List:")
      {
        X = Pos.Center(),
        Y = 1,
      };

      var AllGroupsListView = new ListView(FormatedAllGroups)
      {
        Width = Dim.Fill(),
        Height = 10,
        Y = Pos.Bottom(AllGroupsLabel) + 1,
      };

      var JoinedGroupsLabel = new Label("All Joined Groups:")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(AllGroupsListView) + 2,
      };

      var JoinedGroupsListView = new ListView(FormatedJoinedGroups)
      {
        Width = Dim.Fill(),
        Height = 10,
        Y = Pos.Bottom(JoinedGroupsLabel) + 1,
      };

      var CreatedGroupsLabel = new Label("All Created Groups:")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(JoinedGroupsListView) + 2
      };

      var CreatedGroupsListView = new ListView(FormatedCreatedGroups)
      {
        Y = Pos.Bottom(CreatedGroupsLabel) + 1,
        Width = Dim.Fill(),
        Height = 10,
      };

      var ExitBtn = new Button("Exit")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(CreatedGroupsListView) + 2
      };

      ExitBtn.Clicked += () =>
      {
        userMenu.ShowUserMenu(top, LoggedInUser);
      };

      AllGroupsListView.KeyPress += e =>
      {
        var GroupIndex = AllGroupsListView.SelectedItem;

        if (e.KeyEvent.Key == Key.Enter)
        {
          if (FormatedAllGroups[GroupIndex] == "No Groups Avaiable.")
          {
            e.Handled = true;
            MessageBox.ErrorQuery("No Groups", "No Groups are Avaiabe.", "OK");
            return;
          }
          var SelectedGroup = AllGroups[GroupIndex];

          var GroupWindow = new Window($"Group Id: {SelectedGroup.Id}, Group Name: {SelectedGroup.Name}, Admin: {SelectedGroup.Admin.Username}")
          {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
          };

          var GroupUsers = database.UserGroups.Include(ug => ug.user).Where(ug => ug.GroupId == SelectedGroup.Id).ToList();

          var FormatedGroupUsers = GroupUsers.Any()
          ? GroupUsers.Select(ug => $"User: {ug.user.Username}").ToList()
          : new List<string> { "No Members Avaiable." };

          var GroupUsersLabel = new Label("Group Members:")
          {
            X = Pos.Center(),
            Y = 1,
          };

          var GroupUsersListView = new ListView(FormatedGroupUsers)
          {
            Width = Dim.Fill(),
            Y = Pos.Bottom(GroupUsersLabel) + 1,
            Height = 10,
          };

          var IsMember = database.UserGroups.FirstOrDefault(ug => ug.UserId == LoggedInUser.Id && ug.GroupId == SelectedGroup.Id);

          if (IsMember != null)
          {
            var GroupComments = database.GroupComments.Include(gc => gc.user).Where(gc => gc.GroupId == SelectedGroup.Id).ToList();
            var FormatedGroupComments = GroupComments.Any()
            ? GroupComments.Select(gc => $"User: {gc.user.Username}, Comment: {gc.Text}").ToList()
            : new List<string> { "No Group Comments Avaiable." };

            var GroupCommentsLabel = new Label("Group Comments:")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(GroupUsersListView) + 2,
            };

            var GroupCommentsListView = new ListView(FormatedGroupComments)
            {
              Width = Dim.Fill(),
              Y = Pos.Bottom(GroupCommentsLabel) + 1,
              Height = 10,
            };

            var AddGroupCommentBtn = new Button("Add Comment")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(GroupCommentsListView) + 2
            };

            var LeaveGroupBtn = new Button("Leave Group")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(AddGroupCommentBtn) + 1,
            };

            LeaveGroupBtn.Clicked += () =>
            {
              try
              {
                var UserGroupsToRemove = database.UserGroups.FirstOrDefault(ug => ug.UserId == LoggedInUser.Id && ug.GroupId == SelectedGroup.Id);
                var GroupCommentsToRemove = database.GroupComments.Where(gc => gc.UserId == LoggedInUser.Id && gc.GroupId == SelectedGroup.Id).ToList();
                if (UserGroupsToRemove != null)
                {
                  database.UserGroups.Remove(UserGroupsToRemove);
                  database.SaveChanges();
                  if (GroupCommentsToRemove.Any())
                  {
                    database.GroupComments.RemoveRange(GroupCommentsToRemove);
                    database.SaveChanges();
                  }
                  MessageBox.Query("Success", $"You left group", "OK");
                  ViewGroups(top, LoggedInUser);
                }


              }
              catch (Exception ex)
              {
                MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
              }
            };
            AddGroupCommentBtn.Clicked += () =>
            {
              AddGroupComment(SelectedGroup.Id, LoggedInUser, top);
            };

            GroupWindow.Add(GroupCommentsLabel, GroupCommentsListView, AddGroupCommentBtn, LeaveGroupBtn);
          }
          else
          {
            var CantViewCommentsLabel = new Label("No Access to comments since you are not a member.")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(GroupUsersListView) + 2,
            };
            var JoinGroupBtn = new Button("Join Group")
            {
              Y = Pos.Bottom(CantViewCommentsLabel) + 2,
              X = Pos.Center(),
            };

            JoinGroupBtn.Clicked += () =>
            {
              UserGroup NewUserGroup = new UserGroup { UserId = LoggedInUser.Id, GroupId = SelectedGroup.Id };
              try
              {
                database.UserGroups.Add(NewUserGroup);
                database.SaveChanges();
                MessageBox.Query("Success", $"Joined Group: {SelectedGroup.Name}", "OK");
                ViewGroups(top, LoggedInUser);
              }
              catch (Exception ex)
              {
                MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
              }

            };

            GroupWindow.Add(CantViewCommentsLabel, JoinGroupBtn);
          }

          var ExitBtn = new Button("Exit")
          {
            Y = 40,
            X = Pos.Center(),
          };

          ExitBtn.Clicked += () =>
          {
            ViewGroups(top, LoggedInUser);
          };

          GroupWindow.Add(GroupUsersLabel, GroupUsersListView, ExitBtn);
          top.RemoveAll();
          top.Add(GroupWindow);
        }
      };

      JoinedGroupsListView.KeyPress += e =>
      {
        var GroupIndex = JoinedGroupsListView.SelectedItem;

        if (e.KeyEvent.Key == Key.Enter)
        {
          if (FormatedJoinedGroups[GroupIndex] == "No Joined Groups Avaiable.")
          {
            e.Handled = true;
            MessageBox.ErrorQuery("No Groups", "No Joined Groups Avaiable.", "OK");
            return;
          }
          top.RemoveAll();
          var SelectedGroup = JoinedGroups[GroupIndex];
          if (SelectedGroup != null)
          {
            var GroupWindow = new Window($"Group Id: {SelectedGroup.group.Id}, Group Name: {SelectedGroup.group.Name}, Admin: {SelectedGroup.group.Admin.Username}")
            {
              Width = Dim.Fill(),
              Height = Dim.Fill(),
            };

            var GroupUsers = database.UserGroups.Include(ug => ug.user).Where(ug => ug.GroupId == SelectedGroup.GroupId).ToList();

            var FormatedGroupUsers = GroupUsers.Any()
            ? GroupUsers.Select(ug => $"User: {ug.user.Username}").ToList()
            : new List<string> { "No Members Avaiable." };

            var GroupUsersLabel = new Label("Group Members:")
            {
              X = Pos.Center(),
              Y = 1,
            };

            var GroupUsersListView = new ListView(FormatedGroupUsers)
            {
              Width = Dim.Fill(),
              Y = Pos.Bottom(GroupUsersLabel) + 1,
              Height = 10,
            };

            var GroupComments = database.GroupComments.Include(gc => gc.user).Where(gc => gc.GroupId == SelectedGroup.GroupId).ToList();

            var FormatedGroupComments = GroupComments.Any()
            ? GroupComments.Select(gc => $"User: {gc.user.Username}, Comment: {gc.Text}").ToList()
            : new List<string> { "No Group Comments Avaiable." };

            var GroupCommentsLabel = new Label("Group Comments:")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(GroupUsersListView) + 2,
            };

            var GroupCommentsListView = new ListView(FormatedGroupComments)
            {
              Width = Dim.Fill(),
              Y = Pos.Bottom(GroupCommentsLabel) + 1,
              Height = 10,
            };

            var AddGroupCommentBtn = new Button("Add Comment")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(GroupCommentsListView) + 2
            };

            var LeaveGroupBtn = new Button("Leave Group")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(AddGroupCommentBtn) + 1
            };

            var ExitBtn = new Button("Exit")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(LeaveGroupBtn) + 1
            };

            ExitBtn.Clicked += () =>
            {
              ViewGroups(top, LoggedInUser);
            };

            AddGroupCommentBtn.Clicked += () =>
            {
              AddGroupComment(SelectedGroup.GroupId, LoggedInUser, top);
            };

            LeaveGroupBtn.Clicked += () =>
            {
              try
              {
                database.UserGroups.Remove(SelectedGroup);
                var GroupCommentsToRemove = database.GroupComments.Where(gc => gc.GroupId == SelectedGroup.GroupId && gc.UserId == LoggedInUser.Id).ToList();
                if (GroupCommentsToRemove != null)
                {
                  database.GroupComments.RemoveRange(GroupCommentsToRemove);
                }
                database.SaveChanges();

                MessageBox.Query("Success", "You left group.", "OK");
                ViewGroups(top, LoggedInUser);
              }
              catch (Exception ex)
              {
                MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
              }
            };
            GroupWindow.Add(GroupUsersLabel, GroupUsersListView, GroupCommentsLabel, GroupCommentsListView, AddGroupCommentBtn, ExitBtn, LeaveGroupBtn);
            top.Add(GroupWindow);
          }
        }
      };

      CreatedGroupsListView.KeyPress += e =>
      {
        var GroupIndex = CreatedGroupsListView.SelectedItem;

        if (e.KeyEvent.Key == Key.Enter)
        {
          if (FormatedCreatedGroups[GroupIndex] == "No Created Groups Avaiable.")
          {
            e.Handled = true;
            MessageBox.ErrorQuery("No Groups", "No Created Groups Avaiable.", "OK");
            return;
          }
          top.RemoveAll();

          var SelectedGroup = CreatedGroups[GroupIndex];
          if (SelectedGroup != null)
          {
            var GroupWindow = new Window($"Group Id: {SelectedGroup.Id}, Group Name: {SelectedGroup.Name}, Admin: {SelectedGroup.Admin.Username}")
            {
              Width = Dim.Fill(),
              Height = Dim.Fill(),
            };

            var GroupUsers = database.UserGroups
            .Include(ug => ug.user)
            .Include(ug => ug.group)
            .Where(ug => ug.GroupId == SelectedGroup.Id && ug.UserId != SelectedGroup.AdminId)
            .ToList();

            var FormatedGroupUsers = GroupUsers.Any()
            ? GroupUsers.Select(ug => $"User: {ug.user.Username}").ToList()
            : new List<string> { "No Members Avaiable." };

            var GroupUsersLabel = new Label("Group Members:")
            {
              X = Pos.Center(),
              Y = 1,
            };

            var GroupUsersListView = new ListView(FormatedGroupUsers)
            {
              Width = Dim.Fill(),
              Y = Pos.Bottom(GroupUsersLabel) + 1,
              Height = 10,
            };

            GroupUsersListView.KeyPress += e =>
            {
              var UserIndex = GroupUsersListView.SelectedItem;
              if (e.KeyEvent.Key == Key.Backspace)
              {
                if (FormatedGroupUsers[UserIndex] == "No Members Avaiable.")
                {
                  e.Handled = true;
                  MessageBox.ErrorQuery("No Users", "No Users Avaiable.", "OK");
                  return;
                }
                var SelectedUser = GroupUsers[UserIndex];
                var result = MessageBox.Query("Deleting User", $"Do you want do delete user: {SelectedUser.user.Username}", "YES", "NO");

                if (result == 0)
                {
                  e.Handled = true;

                  var GroupUserCommentsToRemove = database.GroupComments.Where(gc => gc.GroupId == SelectedGroup.Id && gc.UserId == SelectedUser.UserId);
                  try
                  {
                    if (SelectedUser == null)
                    {
                      e.Handled = true;
                      MessageBox.ErrorQuery("Error", "Selected user is null.", "OK");
                      return;
                    }
                    if (SelectedGroup == null)
                    {
                      e.Handled = true;
                      MessageBox.ErrorQuery("Error", "Selected group is null.", "OK");
                      return;
                    }

                    if (GroupUserCommentsToRemove.Any())
                    {

                      database.GroupComments.RemoveRange(GroupUserCommentsToRemove);
                      database.SaveChanges();
                    }


                    database.UserGroups.Remove(SelectedUser);
                    database.SaveChanges();
                    MessageBox.Query("Success", $"Removed Member: {SelectedUser.user.Username}", "OK");
                    ViewGroups(top, LoggedInUser);

                  }
                  catch (Exception ex)
                  {
                    MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
                  }
                }

                if (result == 1)
                {
                  MessageBox.Query("Canceled", "Canceled Delete action of user", "OK");
                }

              }
            };

            var GroupComments = database.GroupComments.Include(gc => gc.user).Where(gc => gc.GroupId == SelectedGroup.Id).ToList();

            var FormatedGroupComments = GroupComments.Any()
            ? GroupComments.Select(gc => $"User: {gc.user.Username}, Comment: {gc.Text}").ToList()
            : new List<string> { "No Group Comments Avaiable." };

            var GroupCommentsLabel = new Label("Group Comments:")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(GroupUsersListView) + 2,
            };

            var GroupCommentsListView = new ListView(FormatedGroupComments)
            {
              Width = Dim.Fill(),
              Y = Pos.Bottom(GroupCommentsLabel) + 1,
              Height = 10,
            };

            GroupCommentsListView.KeyPress += e =>
            {
              var CommentIndex = GroupCommentsListView.SelectedItem;

              if (e.KeyEvent.Key == Key.Backspace)
              {
                if (FormatedGroupComments[CommentIndex] == "No Group Comments Avaiable.")
                {
                  e.Handled = true;
                  MessageBox.ErrorQuery("No Comments", "No Comments Avaiable", "OK");
                  return;
                }
                e.Handled = true;
                var SelectedComment = GroupComments[CommentIndex];
                if (SelectedComment != null)
                {
                  try
                  {
                    var result = MessageBox.Query("Deleting Comment", "Do you want to delete this comment?", "YES", "NO");
                    if (result == 0)
                    {
                      database.GroupComments.Remove(SelectedComment);
                      database.SaveChanges();
                      MessageBox.Query("Success", $"Removed Comment: {SelectedComment.Text}", "OK");
                      GroupComments = database.GroupComments.Include(gc => gc.user).Where(gc => gc.GroupId == SelectedGroup.Id).ToList();
                      FormatedGroupComments = GroupComments.Any()
                      ? GroupComments.Select(gc => $"User: {gc.user.Username}, Comment: {gc.Text}").ToList()
                      : new List<string> { "No Group Comments Avaiable." };
                      GroupCommentsListView.SetSource(FormatedGroupComments);
                    }
                    if (result == 1)
                    {
                      MessageBox.Query("Canceled Delete", "Delete action stopped.", "OK");
                      return;
                    }
                  }
                  catch (Exception ex)
                  {
                    MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
                  }
                }
              }
            };

            var ExitBtn = new Button("Exit")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(GroupCommentsListView) + 2
            };

            ExitBtn.Clicked += () =>
            {
              ViewGroups(top, LoggedInUser);
            };

            var DeleteBtn = new Button("Delete Group")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(ExitBtn) + 1,
            };

            DeleteBtn.Clicked += () =>
            {
              try
              {
                var UserGroupsToRemove = database.UserGroups.Where(ug => ug.GroupId == SelectedGroup.Id).ToList();
                if (UserGroupsToRemove.Any())
                {
                  database.UserGroups.RemoveRange(UserGroupsToRemove);
                }

                database.Groups.Remove(SelectedGroup);
                database.SaveChanges();
                e.Handled = true;
                MessageBox.Query("Success", $"Removed Group: {SelectedGroup.Name}", "OK");
                ViewGroups(top, LoggedInUser);
              }
              catch (Exception ex)
              {
                MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
              }
            };

            var AddGroupCommentBtn = new Button("Add Comment")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(DeleteBtn) + 1
            };

            AddGroupCommentBtn.Clicked += () =>
            {
              AddGroupComment(SelectedGroup.Id, LoggedInUser, top);
            };

            GroupWindow.Add(GroupUsersLabel, GroupUsersListView, GroupCommentsLabel, GroupCommentsListView, ExitBtn, DeleteBtn, AddGroupCommentBtn);
            top.Add(GroupWindow);

          }
        }
      };

      WindowFrame.Add(AllGroupsLabel, AllGroupsListView, JoinedGroupsLabel, JoinedGroupsListView, CreatedGroupsLabel, CreatedGroupsListView, ExitBtn);
      top.Add(WindowFrame);
    }

    public void AddGroupComment(int GroupId, User LoggedInUser, Toplevel top)
    {
      top.RemoveAll();
      if (!database.Users.Any(u => u.Id == LoggedInUser.Id))
      {
        mainMenu.ShowMainMenu(top);
      }

      var group = database.Groups.FirstOrDefault(g => g.Id == GroupId);
      if (group == null)
      {
        MessageBox.ErrorQuery("Group Error", "Group with provided id doesnt exists.", "OK");
        return;
      }

      var IsMemberOfGroup = database.UserGroups.FirstOrDefault(ug => ug.GroupId == group.Id && ug.UserId == LoggedInUser.Id);

      if (IsMemberOfGroup == null)
      {
        MessageBox.ErrorQuery("Group Error", "You are not part of the group.", "OK");
        userMenu.ShowUserMenu(top, LoggedInUser);
        return;
      }

      var window = new FrameView()
      {
        Width = Dim.Fill(),
        Height = Dim.Fill()
      };

      var AddCommentLabel = new Label("Write your comment")
      {
        X = Pos.Center(),
        Y = 1,
      };

      var CommentTextView = new TextView()
      {
        X = Pos.Center(),
        Y = Pos.Bottom(AddCommentLabel) + 1,
        Height = 10,
        Width = 20,
      };

      var AddCommentBtn = new Button("Add Comment")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(CommentTextView) + 1
      };

      var ExitBtn = new Button("Exit")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(AddCommentBtn) + 1
      };

      ExitBtn.Clicked += () =>
      {
        userMenu.ShowUserMenu(top, LoggedInUser);
      };

      AddCommentBtn.Clicked += () =>
      {
        try
        {
          var CommentTextInput = CommentTextView.Text.ToString();
          if (string.IsNullOrWhiteSpace(CommentTextInput))
          {
            MessageBox.ErrorQuery("Validation Error", "Invalid Comment input.", "OK");
            return;
          }
          GroupComment NewGroupComment = new GroupComment { GroupId = group.Id, UserId = LoggedInUser.Id, Text = CommentTextInput };
          database.GroupComments.Add(NewGroupComment);
          database.SaveChanges();
          MessageBox.Query("Success", $"Added Comment: {CommentTextInput}", "OK");
        }
        catch (Exception ex)
        {
          MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
        }

      };

      window.Add(AddCommentLabel, CommentTextView, AddCommentBtn, ExitBtn);
      top.Add(window);
    }

  }
}

