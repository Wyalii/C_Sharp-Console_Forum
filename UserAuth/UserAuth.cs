namespace Forum
{
    using System.Data.Common;
    using System.Security.Cryptography.X509Certificates;
    using BCrypt.Net;
    using Terminal.Gui;
    public class UserAuth{
        public void Register()
        {
            
          Application.Init();
          var top = Application.Top;

          var window = new FrameView()
          {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
          };

          var TitleLabel = new Label("Registration Form"){
             X = Pos.Center(),
             Y = 0,
             
          };


          var UsernameLabel = new Label("Username:"){
            X = Pos.Center(), 
            Y = 2,
          };

          var PasswordLabel = new Label("Password:"){
             X = Pos.Center(),
             Y = 4,
          };

          var UsernameField = new TextField(){
             X = Pos.Center(),
             Y = Pos.Top(UsernameLabel) + 1,
             Width = 20,
             
          };

          var PasswordField = new TextField()
          {
            X = Pos.Center(),
            Y = Pos.Top(PasswordLabel) + 1,
            Width = 20
          };

          var submitButton = new Button("Submit")
          {
             X = Pos.Center(),
             Y = Pos.Top(PasswordField) + 2
          };

          submitButton.Clicked += () =>{
            try
            {
              Database database = new Database();
              var Users = database.Users.ToList();

              var NewUsername = UsernameField.Text.ToString();
              var NewPassword = PasswordField.Text.ToString();

              if(Users.Any(u => u.Username == NewUsername))
              {
                MessageBox.ErrorQuery("Error","User with provided Username Aleardy Exists.");
                return;
              }

              if(string.IsNullOrWhiteSpace(NewUsername))
              {
                MessageBox.ErrorQuery("Username Validation Error","Please Provide Username","OK");
                return;
              }

              if(NewUsername.All(char.IsDigit))
              {
                MessageBox.ErrorQuery("Username Validation Error","User cant have only numbers as a Username.","OK");
                return;
              }

              if(NewUsername.Length < 4)
              {
                MessageBox.ErrorQuery("Username Validation Error","Username must contain more than 4 characters.","OK");
                return;
              }

              if(string.IsNullOrWhiteSpace(NewPassword))
              {
                MessageBox.ErrorQuery("Password Validation Error","Please Provide Password","OK");
                return;
              }

              if(NewPassword.All(char.IsDigit))
              {
                MessageBox.ErrorQuery("Password Validation Error","User cant have only numbers as a Password.", "OK");
                return;
              }

              if(NewPassword.Length < 4)
              {
                MessageBox.ErrorQuery("Password Validation Error","Password must contain more than 4 characters.","OK");
                return;
              }

              string HashedPassword = BCrypt.HashPassword(NewPassword);
              User NewUser = new User{Username = NewUsername, Password = HashedPassword};
              database.Add(NewUser);
              database.SaveChanges();
              MessageBox.Query("Success",$"User: {NewUser.Username} Registered!","OK");
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error",$"Unexpected Error: {ex.Message}","OK");
            }

          };

          window.Add(UsernameField,UsernameLabel,PasswordField,PasswordLabel,submitButton,TitleLabel);
          window.Border.BorderStyle = BorderStyle.None;
         
          top.Add(window);
          Application.Run();

        }
    }
}