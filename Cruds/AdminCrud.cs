using Microsoft.EntityFrameworkCore;
using Terminal.Gui;

namespace Forum
{
    public class AdminCurd
    {
        Database database = new Database();
        public void AdminViewAllPosts(Toplevel top, User LoggedInUser)
        {

            top.RemoveAll();
            var window = new FrameView()
            {
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            var Posts = database.Posts.Include(p => p.user).ToList();

            var FormatedPosts = Posts.Any()
            ? Posts.Select(p => $"Post Id: {p.Id}, Post Title: {p.Title}, Author: {p.user.Username}").ToList()
            : new List<string> { "No Posts Avaiable." };

            var PostsLabel = new Label("All Posts:")
            {
                X = Pos.Center(),
                Y = 1
            };

            var PostsListView = new ListView(FormatedPosts)
            {
                Y = Pos.Bottom(PostsLabel) + 1,
                Width = Dim.Fill(),
                Height = 20,
            };


            PostsListView.KeyDown += (e) =>
            {
                var PostIndex = PostsListView.SelectedItem;

                if (e.KeyEvent.Key == Key.Enter)
                {
                    if (FormatedPosts[PostIndex] == "No Posts Avaiable.")
                    {
                        e.Handled = true;
                        MessageBox.ErrorQuery("No Posts", "No Posts Avaiable.", "OK");
                        return;
                    }

                    if (PostIndex != -1)
                    {
                        var SelectedPost = Posts[PostIndex];
                        if (SelectedPost == null)
                        {
                            MessageBox.ErrorQuery("Post Error", "Post Doesn't Exists.", "OK");
                            return;
                        }
                        var Comments = database.Comments
                        .Include(comment => comment.user).
                        Where(c => c.PostId == SelectedPost.Id).ToList();

                        var FormattedComments = Comments.Any()
                        ? Comments.Select(comment => $"User: {comment.user.Username} | Comment: {comment.Text}").ToList()
                        : new List<string> { "no comments found" };

                        var PostWindow = new Window($"Post Id: {SelectedPost.Id} Post Author: {SelectedPost.user.Username}, Post Title: {SelectedPost.Title}")
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
                            Text = $"{SelectedPost.Content}",
                            ReadOnly = true,

                        };

                        var CommentsListView = new ListView(FormattedComments)
                        {
                            Y = Pos.Bottom(PostContentView) + 2,
                            Width = Dim.Fill(),
                            Height = 10
                        };

                        CommentsListView.KeyDown += e =>
                        {
                            if (e.KeyEvent.Key == Key.Backspace)
                            {
                                var CommentIndex = CommentsListView.SelectedItem;
                                if (CommentIndex != -1)
                                {
                                    var SelectedComment = Comments[CommentIndex];
                                    var GetComment = database.Comments.FirstOrDefault(c => c.Id == SelectedComment.Id);
                                    try
                                    {
                                        var result = MessageBox.Query("Delete Post", "Do you want to delete this comment?", "YES", "NO");
                                        if (result == 0)
                                        {
                                            database.Comments.Remove(GetComment);
                                            database.SaveChanges();
                                            MessageBox.ErrorQuery("Success", "Removed Comment.", "OK");
                                            top.RemoveAll();
                                            AdminViewAllPosts(top, LoggedInUser);
                                        }

                                        if (result == 1)
                                        {
                                            MessageBox.Query("Delete Canceled", "Delete of comment was canceled", "OK");
                                        }


                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
                                    }
                                }
                            }

                        };

                        var DeleteBtn = new Button("Delte Post")
                        {
                            X = Pos.Center(),
                            Y = Pos.Bottom(CommentsListView) + 2
                        };


                        var ExitComments = new Button("Exit Comments")
                        {
                            X = Pos.Center(),
                            Y = Pos.Bottom(DeleteBtn) + 1,
                        };

                        ExitComments.Clicked += () =>
                        {
                            AdminViewAllPosts(top, LoggedInUser);
                        };

                        DeleteBtn.Clicked += () =>
                        {
                            var Posts = database.Posts.Include(p => p.user).ToList();
                            if (Posts.Count == 0 || Posts == null)
                            {
                                MessageBox.ErrorQuery("Posts Error", "No Posts Avaiable", "OK");
                                AdminMenu adminMenu = new AdminMenu();
                                adminMenu.ShowAdminMenu(top, LoggedInUser);
                            }
                            var post = database.Posts.FirstOrDefault(p => p.Id == SelectedPost.Id);
                            if (post == null)
                            {
                                MessageBox.ErrorQuery("Post Error", "Post with provided id doesn't exists", "OK");
                                return;
                            }

                            database.Posts.Remove(post);
                            database.SaveChanges();
                            MessageBox.Query("Success", $"Post: {post.Title} was removed.", "OK");
                            top.RemoveAll();
                            AdminViewAllPosts(top, LoggedInUser);
                        };

                        PostWindow.Add(PostContentView, ExitComments, CommentsListView, DeleteBtn);
                        top.RemoveAll();
                        top.Add(PostWindow);
                    }

                }
            };

            var ExitButton = new Button("Exit")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(PostsListView) + 2,
            };

            ExitButton.Clicked += () =>
            {
                AdminMenu adminMenu = new AdminMenu();
                adminMenu.ShowAdminMenu(top, LoggedInUser);
            };
            window.Add(PostsListView, ExitButton, PostsLabel);
            top.Add(window);
        }

        public void AdminViewAllUsers(Toplevel top, User LoggedInUser)
        {
            top.RemoveAll();
            var window = new FrameView()
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };
            var AllUsers = database.Users.ToList();
            var FormatedUsers = AllUsers.Any()
            ? AllUsers.Select(au => $"Id: {au.Id}, Username: {au.Username}").ToList()
            : new List<string> { "No Users Avaiable." };


            var UsersLabel = new Label("Users:")
            {
                X = Pos.Center(),
                Y = 1,
            };
            var UsersListView = new ListView(FormatedUsers)
            {
                Width = Dim.Fill(),
                Y = Pos.Bottom(UsersLabel) + 1,
                Height = 10,
            };



            var ExitBtn = new Button("Exit")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(UsersListView) + 1,
            };

            ExitBtn.Clicked += () =>
            {
                AdminMenu adminMenu = new AdminMenu();
                adminMenu.ShowAdminMenu(top, LoggedInUser);
            };

            UsersListView.KeyDown += e =>
            {
                var UserIndex = UsersListView.SelectedItem;

                if (e.KeyEvent.Key == Key.Backspace)
                {
                    if (FormatedUsers[UserIndex] == "No Users Avaiable.")
                    {
                        e.Handled = true;
                        MessageBox.ErrorQuery("No Users", "No Users Avaiable", "OK");
                        return;
                    }
                    var SelectedUser = AllUsers[UserIndex];
                    var userToRemove = database.Users.FirstOrDefault(u => u.Id == SelectedUser.Id);
                    var UserPostsToRemove = database.Posts.Where(p => p.UserId == SelectedUser.Id).ToList();
                    var UserGroupsToRemove = database.Groups.Where(g => g.AdminId == SelectedUser.Id).ToList();
                    var UserGroupCommentsToRemove = database.GroupComments.Where(gc => gc.UserId == SelectedUser.Id).ToList();
                    var UserJoinedGroupsToRemove = database.UserGroups.Where(ug => ug.UserId == SelectedUser.Id).ToList();
                    if (userToRemove == null)
                    {
                        MessageBox.ErrorQuery("User Error", "User with provided id doesn't exists.", "OK");
                        return;
                    }
                    try
                    {
                        var result = MessageBox.Query("Removing User", $"Do You want To remove user: {userToRemove.Username}?", "YES", "NO");
                        if (result == 0)
                        {
                            if (UserPostsToRemove.Any())
                            {
                                database.Posts.RemoveRange(UserPostsToRemove);
                            }

                            if (UserGroupsToRemove.Any())
                            {
                                database.Groups.RemoveRange(UserGroupsToRemove);
                            }

                            if (UserGroupCommentsToRemove.Any())
                            {
                                database.GroupComments.RemoveRange(UserGroupCommentsToRemove);
                            }

                            if (UserJoinedGroupsToRemove.Any())
                            {
                                database.UserGroups.RemoveRange(UserJoinedGroupsToRemove);
                            }
                            database.Users.Remove(userToRemove);

                            database.SaveChanges();
                            MessageBox.Query("Success", $"User: {userToRemove.Username} was Removed.", "OK");
                            AdminViewAllUsers(top, LoggedInUser);

                        }

                        if (result == 1)
                        {
                            MessageBox.Query("Canceled", "Canceled User remove operation", "OK");
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
                    }
                }
            };

            window.Add(UsersLabel, UsersListView, ExitBtn);
            top.Add(window);
        }

    }
}