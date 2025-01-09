using Microsoft.EntityFrameworkCore;
using Terminal.Gui;

namespace Forum
{
    public class AdminCurd
    {
        Database database = new Database();
        public void ViewAllPosts(Toplevel top, User LoggedInUser)
        {

            top.RemoveAll();

            var Posts = database.Posts.Include(p => p.user).ToList();
            if (Posts.Count == 0)
            {
                MessageBox.ErrorQuery("Posts Error", "No Posts Avaiable", "OK");
                AdminMenu adminMenu = new AdminMenu();
                adminMenu.ShowAdminMenu(top, LoggedInUser);
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
                        var Comments = database.Comments
                        .Include(comment => comment.user).
                        Where(c => c.PostId == SelectedPost.Id).ToList();

                        var FormattedComments = Comments.Any()
                        ? Comments.Select(comment => $"User: {comment.user.Username} | Comment: {comment.Text}").ToList()
                        : new List<string> { "no comments found" };

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
                            Text = $"{SelectedPost.Content}",
                            ReadOnly = true,

                        };

                        var CommentsListView = new ListView(FormattedComments)
                        {
                            Y = Pos.Bottom(PostContentView) + 2,
                            Width = Dim.Fill(),
                            Height = 10
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
                            ViewAllPosts(top, LoggedInUser);
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
                Y = 1
            };

            ExitButton.Clicked += () =>
            {
                UserMenu userMenu = new UserMenu();
                userMenu.ShowUserMenu(top, LoggedInUser);
            };
            top.RemoveAll();
            top.Add(window, ExitButton);
        }
    }
}