using Microsoft.EntityFrameworkCore;

namespace Forum
{
   public class Database:DbContext{
        public DbSet<User>Users{get;set;}
        public DbSet<Post>Posts{get;set;}
        public DbSet<Comment>Comments{get;set;}
   }    
}