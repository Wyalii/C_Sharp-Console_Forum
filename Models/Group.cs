namespace Forum{
    public class Group{
        public int Id{get;set;}
        public string Name{get;set;}
        public List<GroupComment>GroupComments{get;set;}
        public List<UserGroup> UserGroups{get;set;}
     
    }
}