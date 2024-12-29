namespace Forum{
    public class GroupComment
    {
        public int Id{get;set;}
        public string Text{get;set;}
        public int UserId{get;set;}
        public User user{get;set;}
        public int GroupId{get;set;}
        public Group group{get;set;}
    }
}