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
            if (Posts.Count == 0 || Posts == null)
            {
                MessageBox.ErrorQuery("Posts Error", "No Posts Avaiable", "OK");
                top.RemoveAll();
                AdminMenu adminMenu = new AdminMenu();
                adminMenu.ShowAdminMenu(top, LoggedInUser);
            }

            var FormatedPosts = Posts.Select(post => $"Post Id: {post.Id} | Post Author: {post.user.Username}  | Title: {post.Title}").ToList();

            var PostsListView = new ListView(FormatedPosts)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = 20,
            };


            PostsListView.KeyDown += (e) =>
            {
                if (e.KeyEvent.Key == Key.Enter)
                {
                    var PostIndex = PostsListView.SelectedItem;
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
                                            MessageBox.Query("Info", "Delete was canceled", "OK");
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
            top.RemoveAll();
            window.Add(PostsListView, ExitButton);
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

            var FormatedUsers = database.Users.Select(u => $"User Id: {u.Id},Username: {u.Username}").ToList();
            var AllUsers = database.Users.ToList();

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

                if (e.KeyEvent.Key == Key.Backspace)
                {
                    var UserIndex = UsersListView.SelectedItem;
                    var SelectedUser = AllUsers[UserIndex];
                    var user = database.Users.FirstOrDefault(u => u.Id == SelectedUser.Id);
                    if (user == null)
                    {
                        MessageBox.ErrorQuery("User Error", "User with provided id doesn't exists.", "OK");
                        return;
                    }
                    try
                    {
                        var result = MessageBox.Query("Removing User", $"Do You want To remove user: {user.Username}?", "YES", "NO");
                        if (result == 0)
                        {
                            database.Users.Remove(user);
                            FormatedUsers.RemoveAt(UserIndex);
                            AllUsers.RemoveAt(UserIndex);
                            database.SaveChanges();
                            UsersListView.SetSource(FormatedUsers);
                            MessageBox.Query("Success", $"User: {user.Username} was Removed.", "OK");

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