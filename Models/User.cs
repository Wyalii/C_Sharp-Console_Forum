namespace Forum{
    public class User{
        public int Id{get;set;}
        public string Username {get;set;}
        public string Password{get;set;}
        public string Role{get;set;}
        public bool IsAdmin{get;set;}
        public List<Post>Posts{get;set;}
        public List<Comment>Comments{get;set;}
        public List<UserGroup> UserGroups{get;set;}
        public List<Group> AdministeredGroups { get; set; }

    }
}