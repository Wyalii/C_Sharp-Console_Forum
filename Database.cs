using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace Forum
{
   public class Database:DbContext{
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupComment> GroupComments { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            DotEnv.Load();

            var server = Environment.GetEnvironmentVariable("DB_SERVER");
            var database = Environment.GetEnvironmentVariable("DB_NAME");
            var user = Environment.GetEnvironmentVariable("DB_USER");
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

            optionsBuilder.UseSqlServer($"Server={server};Database={database};User Id={user};Password={password};TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<Comment>()
            .HasOne(c => c.user)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comment>()
            .HasOne(c => c.post)
            .WithMany()
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<UserGroup>()
            .HasKey(ug => new { ug.UserId, ug.GroupId });

            modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.user)
            .WithMany()
            .HasForeignKey(ug => ug.UserId)
            .OnDelete(DeleteBehavior.Cascade);

             modelBuilder.Entity<UserGroup>()
             .HasOne(ug => ug.group)
             .WithMany()
             .HasForeignKey(ug => ug.GroupId)
             .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<GroupComment>()
            .HasOne(gc => gc.user)
            .WithMany()
            .HasForeignKey(gc => gc.UserId)
            .OnDelete(DeleteBehavior.NoAction);    

            modelBuilder.Entity<GroupComment>()
            .HasOne(gc => gc.group)
            .WithMany()
            .HasForeignKey(gc => gc.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

            
        }
    }    
}