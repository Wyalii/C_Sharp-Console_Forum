namespace Forum{
    public class Post{
        public int Id{get;set;}
        public string Title{get;set;}
        public string Content{get;set;}
        public int UserId{get;set;}
        public User user{get;set;}
        public List<Comment>Comments{get;set;}

    }
}