using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace MovieBlog.Models
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public DbSet<Image> Images { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Category> Categories { get; set; }
        
        public DbSet<UserFavPost> UserFavPosts { get; set; }
        
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base (options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserFavPost>()
                .HasOne(u => u.User)
                .WithMany(u => u.UserFavPosts)
                .HasForeignKey(u => u.UserId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserFavPost>()
                .HasKey(t => new { t.UserId, t.PostId });
            modelBuilder.Entity<UserFavPost>()
                .HasOne(pt => pt.Post)
                .WithMany(p => p.UserFavPosts)
                .HasForeignKey(pt => pt.PostId);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}