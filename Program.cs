namespace Forum{
    public class Program{
        static void Main()
        {
            Database database = new Database();
            var user = database.Users.FirstOrDefault(u => u.Username == "Giorgi");
            var post = database.Posts.FirstOrDefault(p => p.Title == "Testing Post");
            var comment = database.Comments.FirstOrDefault(c => c.UserId == user.Id);
            
            

            
            
        }
    }
}