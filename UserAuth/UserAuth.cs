namespace Forum
{
  using System.Data.Common;
  using System.Security.Cryptography.X509Certificates;
  using BCrypt.Net;
  using Microsoft.Extensions.Options;
  using Terminal.Gui;
  public class UserAuth
  {

    User LoggedInUser = new User();
    Database database = new Database();
    public void Register(Toplevel top)
    {
      top.RemoveAll();

      var window = new FrameView()
      {
        X = 0,
        Y = 0,
        Width = Dim.Fill(),
        Height = Dim.Fill(),
      };

      var TitleLabel = new Label("Registration Form")
      {
        X = Pos.Center(),
        Y = 0,

      };


      var UsernameLabel = new Label("Username:")
      {
        X = Pos.Center(),
        Y = 2
      };

      var UsernameField = new TextField()
      {
        X = Pos.Center(),
        Y = 3,
        Width = 20,

      };

      var PasswordLabel = new Label("Password:")
      {
        X = Pos.Center(),
        Y = 5,
      };

      var PasswordField = new TextField()
      {
        X = Pos.Center(),
        Y = 6,
        Width = 20
      };

      var submitButton = new Button("Submit")
      {
        X = Pos.Center(),
        Y = 8
      };

      var exitButton = new Button("Exit")
      {
        X = Pos.Center(),
        Y = 10
      };

      exitButton.Clicked += () =>
      {
        MainMenu mainMenu = new MainMenu();
        top.RemoveAll();
        mainMenu.ShowMainMenu(top);
      };

      submitButton.Clicked += () =>
      {
        try
        {

          var Users = database.Users.ToList();

          var NewUsername = UsernameField.Text.ToString();
          var NewPassword = PasswordField.Text.ToString();

          if (NewUsername == "Admin" || NewUsername == "admin")
          {
            MessageBox.ErrorQuery("Username Validation Error", "cant register admin", "OK");
            return;
          }

          if (string.IsNullOrWhiteSpace(NewUsername))
          {
            MessageBox.ErrorQuery("Username Validation Error", "Please Provide Username", "OK");
            return;
          }

          if (NewUsername.All(char.IsDigit))
          {
            MessageBox.ErrorQuery("Username Validation Error", "User cant have only numbers as a Username.", "OK");
            return;
          }

          if (NewUsername.Length < 4)
          {
            MessageBox.ErrorQuery("Username Validation Error", "Username must contain more than 4 characters.", "OK");
            return;
          }

          if (Users.Any(u => u.Username.ToLower() == NewUsername.ToLower()))
          {
            MessageBox.ErrorQuery("Error", "User with provided Username Aleardy Exists.", "OK");
            return;
          }

          if (string.IsNullOrWhiteSpace(NewPassword))
          {
            MessageBox.ErrorQuery("Password Validation Error", "Please Provide Password", "OK");
            return;
          }

          if (NewPassword.All(char.IsDigit))
          {
            MessageBox.ErrorQuery("Password Validation Error", "User cant have only numbers as a Password.", "OK");
            return;
          }

          if (NewPassword.Length < 4)
          {
            MessageBox.ErrorQuery("Password Validation Error", "Password must contain more than 4 characters.", "OK");
            return;
          }

          string HashedPassword = BCrypt.HashPassword(NewPassword);
          User NewUser = new User { Username = NewUsername, Password = HashedPassword, Role = "Member" };
          database.Users.Add(NewUser);
          database.SaveChanges();
          MessageBox.Query("Success", $"User: {NewUser.Username} Registered!", "OK");
        }
        catch (Exception ex)
        {
          MessageBox.ErrorQuery("Error", $"Unexpected Error: {ex.Message}\n{ex.InnerException?.Message}", "OK");
        }

      };

      window.Add(UsernameField, UsernameLabel, PasswordField, PasswordLabel, submitButton, TitleLabel, exitButton);
      top.Add(window);
      Application.Run();

    }



    public void Login(Toplevel top)
    {
      top.RemoveAll();
      var window = new FrameView()
      {
        X = 0,
        Y = 0,
        Width = Dim.Fill(),
        Height = Dim.Fill(),
      };

      var title = new Label("Login")
      {
        X = Pos.Center(),
        Y = 0,
      };

      var UsernameLabel = new Label("Username:")
      {
        X = Pos.Center(),
        Y = 2
      };

      var UsernameField = new TextField()
      {
        X = Pos.Center(),
        Y = 3,
        Width = 20
      };

      var PasswordLabel = new Label("Password:")
      {
        X = Pos.Center(),
        Y = 5
      };

      var PasswordField = new TextField()
      {
        X = Pos.Center(),
        Y = 6,
        Width = 20
      };

      var submitButton = new Button("Submit")
      {
        X = Pos.Center(),
        Y = 8
      };

      var ExitButton = new Button("Exit")
      {
        X = Pos.Center(),
        Y = 10,
      };

      submitButton.Clicked += () =>
      {
        var UsernameInput = UsernameField.Text.ToString();

        if (UsernameInput == "Admin" || UsernameInput == "admin")
        {
          LoggedInUser.IsAdmin = true;
          LoggedInUser.Username = "Admin";
          AdminMenu adminMenu = new AdminMenu();
          adminMenu.ShowAdminMenu(top, LoggedInUser);
        }
        if (string.IsNullOrWhiteSpace(UsernameInput))
        {
          MessageBox.ErrorQuery("Validation Error", "Invalid Username Input.", "OK");
          return;
        }
        var PasswordInput = PasswordField.Text.ToString();
        if (string.IsNullOrWhiteSpace(PasswordInput))
        {
          MessageBox.ErrorQuery("Validation Error", "Invalid Password Input.", "OK");
          return;
        }
        var Users = database.Users.ToList();
        var user = Users.FirstOrDefault(u => u.Username == UsernameInput);

        if (user == null)
        {
          MessageBox.ErrorQuery("Login Error", "User with provided Username Doesn't Exists.", "OK");
          return;
        }

        var verifyPassword = BCrypt.Verify(PasswordInput, user.Password);

        if (verifyPassword == false)
        {
          MessageBox.ErrorQuery("Login Error", "Wrong Password", "OK");
          return;
        }
        LoggedInUser.Id = user.Id;
        LoggedInUser.Username = user.Username;
        LoggedInUser.Role = user.Role;
        LoggedInUser.Posts = user.Posts;
        LoggedInUser.Comments = user.Comments;
        LoggedInUser.UserGroups = user.UserGroups;
        MessageBox.Query("Success", $"User {LoggedInUser.Username} has loggedin.", "OK");
        UserMenu userMenu = new UserMenu();
        userMenu.ShowUserMenu(top, LoggedInUser);
      };

      ExitButton.Clicked += () =>
      {
        MainMenu mainMenu = new MainMenu();
        top.RemoveAll();
        mainMenu.ShowMainMenu(top);
      };

      window.Add(ExitButton, title, PasswordField, PasswordLabel, UsernameField, UsernameLabel, submitButton);
      top.Add(window);

    }
  }
}