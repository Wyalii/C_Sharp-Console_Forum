using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
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

    public void ViewMyPosts(Toplevel top, User LoggedInUser)
    {
      if (!database.Users.Any(u => u.Id == LoggedInUser.Id))
      {
        mainMenu.ShowMainMenu(top);
      }
      top.RemoveAll();
      var UserPosts = database.Posts.Include(post => post.user).Where(p => p.UserId == LoggedInUser.Id).ToList();
      if (UserPosts.Count == 0)
      {
        MessageBox.ErrorQuery("Post Error", "User doesnt have any posts.", "OK");
        userMenu.ShowUserMenu(top, LoggedInUser);
      }
      var Formated = UserPosts.Select(post => $"Id: {post.Id} | Title: {post.Title}").ToList();

      var windowList = new ListView(Formated)
      {
        X = Pos.Center(),
        Y = 0,
        Width = Dim.Fill(),
        Height = Dim.Fill() - 3
      };

      windowList.KeyDown += (e) =>
      {
        if (e.KeyEvent.Key == Key.Enter)
        {
          var PostIndex = windowList.SelectedItem;
          if (PostIndex != -1)
          {
            var SelectedPost = UserPosts[PostIndex];
            if (SelectedPost == null)
            {
              MessageBox.ErrorQuery("Post Error", "Post Doesn't Exists.", "OK");
              return;
            }
            var Comments = database.Comments
            .Include(comment => comment.user)
            .Where(c => c.PostId == SelectedPost.Id)
            .ToList();

            var FormattedComments = Comments.Any()
            ? Comments.Select(comment => $"User: {comment.user.Username} | Comment: {comment.Text}").ToList()
            : new List<string> { "no comments found" };

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

            var AddCommentBtn = new Button("Add Comment")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(CommentsListView) + 2,
            };

            var RemovePostBtn = new Button("Remove Post")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(AddCommentBtn) + 1,
            };
            var ExitComments = new Button("Exit Comments")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(RemovePostBtn) + 1,
            };

            ExitComments.Clicked += () =>
            {
              ViewAllPosts(top, LoggedInUser);
            };

            AddCommentBtn.Clicked += () =>
            {
              AddComment(top, LoggedInUser, SelectedPost.Id);
            };

            RemovePostBtn.Clicked += () =>
            {
              RemoveMyPost(top, LoggedInUser, SelectedPost.Id);
              ViewMyPosts(top, LoggedInUser);
            };

            PostWindow.Add(PostContentView, ExitComments, CommentsListView, AddCommentBtn, RemovePostBtn);
            top.RemoveAll();
            top.Add(PostWindow);
          }
        }
      };


      var ExitButton = new Button("Exit")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(windowList) + 1
      };

      ExitButton.Clicked += () =>
      {
        UserMenu userMenu = new UserMenu();
        userMenu.ShowUserMenu(top, LoggedInUser);
      };


      top.Add(windowList, ExitButton);
    }

    public void ViewAllPosts(Toplevel top, User LoggedInUser)
    {
      if (!database.Users.Any(u => u.Id == LoggedInUser.Id))
      {
        mainMenu.ShowMainMenu(top);
      }
      top.RemoveAll();

      var Posts = database.Posts.Include(post => post.user).ToList();
      if (Posts.Count == 0)
      {
        MessageBox.ErrorQuery("Posts Error", "No Posts Avaiable", "OK");
        UserMenu userMenu = new UserMenu();
        userMenu.ShowUserMenu(top, LoggedInUser);
      }

      var FormatedPosts = Posts.Select(post => $"Post Id: {post.Id} | Post Author: {post.user.Username}  | Title: {post.Title}").ToList();

      var window = new ListView(FormatedPosts)
      {
        X = 0,
        Y = 0,
        Width = Dim.Fill(),
        Height = 20,
      };

      window.KeyDown += (e) =>
      {
        if (e.KeyEvent.Key == Key.Enter)
        {
          var PostIndex = window.SelectedItem;
          if (PostIndex != -1)
          {
            var SelectedPost = Posts[PostIndex];
            if (SelectedPost == null)
            {
              MessageBox.ErrorQuery("Post Error", "Post Doesn't Exists.", "OK");
              return;
            }
            var Comments = database.Comments.Include(comment => comment.user).Where(c => c.PostId == SelectedPost.Id).ToList();
            var FormattedComments = Comments.Any() ? Comments.Select(comment => $"User: {comment.user.Username} | Comment: {comment.Text}").ToList() : new List<string> { "no comments found" };
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

            var AddCommentBtn = new Button("Add Comment")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(CommentsListView) + 2
            };

            var ExitComments = new Button("Exit Comments")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(AddCommentBtn) + 1,
            };

            ExitComments.Clicked += () =>
                  {
                    ViewAllPosts(top, LoggedInUser);
                  };

            AddCommentBtn.Clicked += () =>
                  {
                    AddComment(top, LoggedInUser, SelectedPost.Id);
                  };

            PostWindow.Add(PostContentView, ExitComments, CommentsListView, AddCommentBtn);
            top.RemoveAll();
            top.Add(PostWindow);
          }

        }
      };

      var ViewMyPostsBtn = new Button("View My Posts")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(window) + 1
      };


      var ExitButton = new Button("Exit")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(ViewMyPostsBtn) + 1
      };

      ExitButton.Clicked += () =>
      {
        UserMenu userMenu = new UserMenu();
        userMenu.ShowUserMenu(top, LoggedInUser);
      };

      ViewMyPostsBtn.Clicked += () =>
      {
        ViewMyPosts(top, LoggedInUser);
      };

      top.RemoveAll();
      top.Add(window, ExitButton, ViewMyPostsBtn);
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

    public void RemoveMyPost(Toplevel top, User LoggedInUser, int PostId)
    {
      if (!database.Users.Any(u => u.Id == LoggedInUser.Id))
      {
        mainMenu.ShowMainMenu(top);
      }
      try
      {
        var post = database.Posts.FirstOrDefault(p => p.Id == PostId && p.UserId == LoggedInUser.Id);
        if (post == null)
        {
          MessageBox.ErrorQuery("Post Error", "Post with provided id doesn't exists.", "OK");
          return;
        }

        database.Posts.Remove(post);
        database.SaveChanges();
        MessageBox.Query("Succes", $"Removed Post - Id: {post.Id}, Title: {post.Title}", "OK");
      }
      catch (Exception ex)
      {
        MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
      }
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
          UserGroup NewUserGroup = new UserGroup { UserId = LoggedInUser.Id, GroupId = createdGroup.Id };
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
      top.RemoveAll();

      if (!database.Users.Any(u => u.Id == LoggedInUser.Id))
      {
        mainMenu.ShowMainMenu(top);
      }

      var window = new FrameView()
      {
        Width = Dim.Fill(),
        Height = Dim.Fill(),
      };

      var FormatedGroups = database.Groups.Any()
      ? database.Groups
      .Select(g => $"Id: {g.Id}, Name: {g.Name}").ToList()
      : new List<string> { "No Groups Avaiable" };

      var AllGroups = database.Groups.Include(g => g.UserGroups).ToList();


      var GroupList = new ListView(FormatedGroups)
      {
        Width = Dim.Fill(),
        Height = 20,
      };

      var CreateGroupBtn = new Button("Create Group")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(GroupList) + 1,
      };

      var ShowMyGroupsBtn = new Button("Show my Groups")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(CreateGroupBtn) + 1
      };

      var ExitButton = new Button("Exit")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(ShowMyGroupsBtn) + 1,
      };

      ExitButton.Clicked += () =>
      {
        userMenu.ShowUserMenu(top, LoggedInUser);
      };

      CreateGroupBtn.Clicked += () =>
      {
        CreateGroup(top, LoggedInUser);
      };

      ShowMyGroupsBtn.Clicked += () =>
      {
        ShowMyGroups(top, LoggedInUser);
      };

      GroupList.KeyDown += e =>
      {
        if (!database.Users.Any(u => u.Id == LoggedInUser.Id))
        {
          mainMenu.ShowMainMenu(top);
        }

        if (e.KeyEvent.Key == Key.Enter)
        {
          top.RemoveAll();
          var GroupIndex = GroupList.SelectedItem;
          if (GroupIndex == -1)
          {
            MessageBox.ErrorQuery("Select Error", "Please Select an item first.", "OK");
            return;
          }

          var SelectedGroup = AllGroups[GroupIndex];

          var GroupWindow = new Window($"Group Id: {SelectedGroup.Id}, Group Name: {SelectedGroup.Name}")
          {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
          };

          var UsersOfGorup = database.UserGroups
          .Include(ug => ug.user)
          .Include(ug => ug.group)
          .Where(ug => ug.GroupId == SelectedGroup.Id)
          .ToList();

          var FormatedGroupUsers = UsersOfGorup.Any()
          ? UsersOfGorup.Select(ug => $"User: {ug.user.Username},").ToList()
          : new List<string> { "No Users in Group." };

          var UsersLable = new Label("Users:")
          {
            X = Pos.Center(),
            Y = 1
          };

          var UsersList = new ListView(FormatedGroupUsers)
          {
            Y = Pos.Bottom(UsersLable) + 1,
            Width = Dim.Fill(),
            Height = 20
          };

          var GroupComments = database.GroupComments
          .Include(gc => gc.user)
          .Include(gc => gc.group)
          .Where(gc => gc.GroupId == SelectedGroup.Id);

          var FormatedGroupComments = GroupComments.Any()
          ? GroupComments.Select(gc => $"User: {gc.user.Username}, Comment: {gc.Text}").ToList()
          : new List<string> { "No Comments Avaiable" };

          var IsMemberOfGroup = UsersOfGorup.FirstOrDefault(ug => ug.UserId == LoggedInUser.Id);
          if (IsMemberOfGroup != null)
          {
            var GroupCommentsLabel = new Label("Comments:")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(UsersList) + 2
            };

            var GroupCommentsList = new ListView(FormatedGroupComments)
            {
              Width = Dim.Fill(),
              Height = 20,
              Y = Pos.Bottom(GroupCommentsLabel) + 1,
            };
            GroupWindow.Add(GroupCommentsLabel, GroupCommentsList);
          }
          else if (IsMemberOfGroup == null)
          {
            var CantViewComments = new Label("You are not group member, so you cant view comments.")
            {
              X = Pos.Center(),
              Y = Pos.Bottom(UsersList) + 2,
            };
            GroupWindow.Add(CantViewComments);
          }

          var JoinGroupButton = new Button("Join Group")
          {
            X = Pos.Center(),
            Y = Pos.Bottom(UsersList) + 6
          };

          var AddCommentBtn = new Button("Add Comment")
          {
            X = Pos.Center(),
            Y = Pos.Bottom(JoinGroupButton) + 1
          };

          var ExitButton = new Button("Exit")
          {
            X = Pos.Center(),
            Y = Pos.Bottom(AddCommentBtn) + 1,
          };


          ExitButton.Clicked += () =>
          {
            userMenu.ShowUserMenu(top, LoggedInUser);
          };

          JoinGroupButton.Clicked += () =>
          {
            var AlreadyJoined = database.UserGroups.FirstOrDefault(ug => ug.UserId == LoggedInUser.Id);
            if (AlreadyJoined != null)
            {
              MessageBox.ErrorQuery("Group Error", "You are already member of this group.", "OK");
              return;
            }
            UserGroup NewUserGroup = new UserGroup { UserId = LoggedInUser.Id, GroupId = SelectedGroup.Id };
            try
            {
              database.UserGroups.Add(NewUserGroup);
              database.SaveChanges();
              MessageBox.Query("Success", $"You joined a group: {SelectedGroup.Name}", "OK");
              ViewGroups(top, LoggedInUser);
            }
            catch (Exception ex)
            {
              MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
            }
          };

          AddCommentBtn.Clicked += () =>
          {
            AddGroupComment(SelectedGroup.Id, LoggedInUser, top);
          };


          GroupWindow.Add(UsersLable, UsersList, JoinGroupButton, ExitButton, AddCommentBtn);
          top.Add(GroupWindow);
        }
      };

      window.Add(GroupList, ExitButton, CreateGroupBtn, ShowMyGroupsBtn);
      top.Add(window);
    }

    public void ShowMyGroups(Toplevel top, User LoggedInUser)
    {
      top.RemoveAll();
      if (!database.Users.Any(u => u.Id == LoggedInUser.Id))
      {
        mainMenu.ShowMainMenu(top);
      }
      var CreatedGroups = database.Groups.
      Include(g => g.Admin)
      .Where(g => g.AdminId == LoggedInUser.Id)
      .ToList();

      var FormatedUserGroups = CreatedGroups.Any()
      ? CreatedGroups.Select(ug => $"Admin: {ug.Admin.Username}, Group: {ug.Name}").ToList()
      : new List<string> { "groups not created." };


      var JoinedGroups = database.UserGroups
      .Include(ug => ug.user)
      .Include(ug => ug.group)
      .Where(ug => ug.UserId == LoggedInUser.Id && ug.group.AdminId != LoggedInUser.Id)
      .ToList();

      var FormatedJoinedGroups = JoinedGroups.Any()
      ? JoinedGroups.Select(jg => $"Group Id: {jg.group.Id}, Group Name: {jg.group.Name}").ToList()
      : new List<string> { "no groups joined yet." };

      if (CreatedGroups == null)
      {
        MessageBox.ErrorQuery("Group Error", "User has not created any groups.", "OK");
      }

      if (JoinedGroups == null)
      {
        MessageBox.ErrorQuery("Group Error", "user has not joined any groups.", "OK");
      }

      var Window = new FrameView()
      {
        Width = Dim.Fill(),
        Height = Dim.Fill(),
      };

      var CreatedGroupsLabel = new Label("Created Groups:")
      {
        X = Pos.Center(),
        Y = 1,
        Width = 10,
      };

      var CreatedGroupsList = new ListView(FormatedUserGroups)
      {
        Y = Pos.Bottom(CreatedGroupsLabel) + 1,
        Width = Dim.Fill(),
        Height = 10,
      };

      var JoinedGroupsLabel = new Label("Joined Groups:")
      {
        Y = Pos.Bottom(CreatedGroupsList) + 3,
        X = Pos.Center(),
        Width = 10,
      };

      var JoinedGroupsList = new ListView(FormatedJoinedGroups)
      {
        Y = Pos.Bottom(CreatedGroupsList) + 3,
        Width = Dim.Fill(),
        Height = 10
      };

      var ExitButton = new Button("Exit")
      {
        X = Pos.Center(),
        Y = Pos.Bottom(JoinedGroupsList) + 2
      };

      ExitButton.Clicked += () =>
      {
        userMenu.ShowUserMenu(top, LoggedInUser);
      };

      CreatedGroupsList.KeyPress += e =>
      {
        var GroupIndex = CreatedGroupsList.SelectedItem;
        if (e.KeyEvent.Key == Key.Enter && GroupIndex != -1)
        {
          top.RemoveAll();
          var SelectedGroup = CreatedGroups[GroupIndex];
          var Window = new Window($"Group Id: {SelectedGroup.Id}, Group Name: {SelectedGroup.Name},Admin: {SelectedGroup.Admin.Username}")
          {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
          };

          var GroupComments = database.GroupComments
          .Include(gc => gc.user)
          .Include(gc => gc.group)
          .Where(gc => gc.GroupId == SelectedGroup.Id)
          .ToList();

          var FormatedGroupComments = GroupComments.Any()
          ? GroupComments.Select(gc => $"User: {gc.user.Username},Comment: {gc.Text}").ToList()
          : new List<string> { "No comments Avaiable." };

          var UsersOfGorup = database.UserGroups
          .Include(ug => ug.user)
          .Include(ug => ug.group)
          .Where(ug => ug.GroupId == SelectedGroup.Id)
          .ToList();

          var FormatedGroupUsers = UsersOfGorup.Any()
          ? UsersOfGorup.Select(ug => $"User: {ug.user.Username},").ToList()
          : new List<string> { "No Users in Group." };

          var UsersLable = new Label("Users:")
          {
            X = Pos.Center(),
            Y = 1
          };

          var UsersList = new ListView(FormatedGroupUsers)
          {
            Y = Pos.Bottom(UsersLable) + 1,
            Width = Dim.Fill(),
            Height = 10
          };

          var GroupCommentsLabel = new Label("Group Comments:")
          {
            X = Pos.Center(),
            Y = Pos.Bottom(UsersList) + 2,
          };

          var GroupCommentsList = new ListView(FormatedGroupComments)
          {
            Width = Dim.Fill(),
            Y = Pos.Bottom(GroupCommentsLabel) + 1,
            Height = 10,
          };

          var AddCommentBtn = new Button("Add Comment")
          {
            X = Pos.Center(),
            Y = Pos.Bottom(GroupCommentsList) + 1,
          };

          var DeleteGroupBtn = new Button("Delete")
          {
            X = Pos.Center(),
            Y = Pos.Bottom(AddCommentBtn) + 1
          };

          var ExitBtn = new Button("Exit")
          {
            X = Pos.Center(),
            Y = Pos.Bottom(DeleteGroupBtn) + 1,
          };

          AddCommentBtn.Clicked += () =>
          {
            AddGroupComment(SelectedGroup.Id, LoggedInUser, top);
          };

          ExitBtn.Clicked += () =>
          {
            userMenu.ShowUserMenu(top, LoggedInUser);
          };

          DeleteGroupBtn.Clicked += () =>
          {
            DeleteGroup(top, LoggedInUser, SelectedGroup.Id);
          };

          Window.Add(GroupCommentsLabel, GroupCommentsList, AddCommentBtn, ExitBtn, UsersLable, UsersList, DeleteGroupBtn);
          top.Add(Window);
        }
      };

      JoinedGroupsList.KeyPress += e =>
      {
        var GroupIndex = JoinedGroupsList.SelectedItem;
        if (e.KeyEvent.Key == Key.Enter && GroupIndex != -1)
        {
          top.RemoveAll();
          var SelectedGroup = JoinedGroups[GroupIndex];
          var Window = new Window($"Group Id: {SelectedGroup.GroupId}, Group Name: {SelectedGroup.group.Name},Admin: {SelectedGroup.group.Admin.Username}")
          {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
          };

          var GroupComments = database.GroupComments
          .Include(gc => gc.user)
          .Include(gc => gc.group)
          .Where(gc => gc.GroupId == SelectedGroup.GroupId)
          .ToList();

          var FormatedGroupComments = GroupComments.Any()
          ? GroupComments.Select(gc => $"User: {gc.user.Username},Comment: {gc.Text}").ToList()
          : new List<string> { "No comments Avaiable." };

          var UsersOfGorup = database.UserGroups
          .Include(ug => ug.user)
          .Include(ug => ug.group)
          .Where(ug => ug.GroupId == SelectedGroup.GroupId)
          .ToList();

          var FormatedGroupUsers = UsersOfGorup.Any()
          ? UsersOfGorup.Select(ug => $"User: {ug.user.Username},").ToList()
          : new List<string> { "No Users in Group." };

          var UsersLable = new Label("Users:")
          {
            X = Pos.Center(),
            Y = 1
          };

          var UsersList = new ListView(FormatedGroupUsers)
          {
            Y = Pos.Bottom(UsersLable) + 1,
            Width = Dim.Fill(),
            Height = 20
          };

          var GroupCommentsLabel = new Label("Group Comments:")
          {
            X = Pos.Center(),
            Y = Pos.Bottom(UsersList) + 2,
          };

          var GroupCommentsList = new ListView(FormatedGroupComments)
          {
            Width = Dim.Fill(),
            Y = Pos.Bottom(GroupCommentsLabel) + 1,
            Height = 20,
          };

          var AddCommentBtn = new Button("Add Comment")
          {
            X = Pos.Center(),
            Y = Pos.Bottom(GroupCommentsList) + 1,
          };

          var ExitBtn = new Button("Exit")
          {
            X = Pos.Center(),
            Y = Pos.Bottom(AddCommentBtn) + 1,
          };

          AddCommentBtn.Clicked += () =>
          {
            AddGroupComment(SelectedGroup.GroupId, LoggedInUser, top);
          };

          ExitBtn.Clicked += () =>
          {
            userMenu.ShowUserMenu(top, LoggedInUser);
          };

          Window.Add(GroupCommentsLabel, GroupCommentsList, AddCommentBtn, ExitBtn, UsersLable, UsersList);
          top.Add(Window);
        }
      };
      Window.Add(CreatedGroupsList, JoinedGroupsList, ExitButton, CreatedGroupsLabel, JoinedGroupsLabel);
      top.Add(Window);
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

    public void DeleteGroup(Toplevel top, User LoggedInUser, int GroupId)
    {
      try
      {
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

        var UserOfGroup = database.UserGroups.Where(ug => ug.GroupId == group.Id).ToList();
        if (UserOfGroup != null)
        {
          database.UserGroups.RemoveRange(UserOfGroup);
          database.SaveChanges();
        }
        database.Groups.Remove(group);
        database.SaveChanges();
        MessageBox.Query("Success", $"Group: {group.Name} was removed.", "OK");
      }
      catch (Exception ex)
      {
        MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
      }
    }
  }
}

