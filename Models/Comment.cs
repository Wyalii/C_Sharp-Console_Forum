namespace Forum{
    public class Comment{
        public int Id{get;set;}
        public string Text{get;set;}
        public int UserId{get;set;}
        public User user{get;set;}
        public int PostId{get;set;}
        public Post post{get;set;}
    }
}