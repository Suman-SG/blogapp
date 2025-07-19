//using Microsoft.EntityFrameworkCore;
//using blogapp.Models;

//namespace blogapp.Data
//{
//    public class BlogDBContext : DbContext
//    {
//        public BlogDBContext(DbContextOptions<BlogDBContext> options) : base(options) { }

//        public DbSet<BlogPost> BlogPosts { get; set; }
//        public DbSet<User> Users { get; set; }
//        public DbSet<Comment> Comments { get; set; }
//        public DbSet<Like> Likes { get; set; }
//        public DbSet<Bookmark> Bookmarks { get; set; }
//        public DbSet<Notification> Notifications { get; set; }


//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);

//            // LIKEs Config
//            modelBuilder.Entity<Like>()
//                .HasOne(l => l.BlogPost)
//                .WithMany(p => p.Likes)
//                .HasForeignKey(l => l.BlogPostId)
//                .OnDelete(DeleteBehavior.Cascade);

//            modelBuilder.Entity<Like>()
//                .HasOne(l => l.User)
//                .WithMany()
//                .HasForeignKey(l => l.UserId)
//                .OnDelete(DeleteBehavior.Restrict); // ✅ No cascade

//            // BOOKMARKs Config (No cascade anywhere)
//            modelBuilder.Entity<Bookmark>()
//                .HasOne(b => b.Post)
//                .WithMany(p => p.Bookmarks)
//                .HasForeignKey(b => b.PostId)
//                .OnDelete(DeleteBehavior.Restrict); // ✅ No cascade

//            modelBuilder.Entity<Bookmark>()
//                .HasOne(b => b.User)
//                .WithMany()
//                .HasForeignKey(b => b.UserId)
//                .OnDelete(DeleteBehavior.Restrict); // ✅ No cascade

//            // COMMENT replies (No cascade for self-ref)
//            modelBuilder.Entity<Comment>()
//                .HasOne(c => c.ParentComment)
//                .WithMany(c => c.Replies)
//                .HasForeignKey(c => c.ParentCommentId)
//                .OnDelete(DeleteBehavior.Restrict);

//        }


//    }
//}






using blogapp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace blogapp.Data
{
    public class BlogDBContext : IdentityDbContext<IdentityUser>
    {
        public BlogDBContext(DbContextOptions<BlogDBContext> options) : base(options) { }

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Must call base for Identity schema

            // LIKEs Config
            modelBuilder.Entity<Like>()
                .HasOne(l => l.BlogPost)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // BOOKMARKs Config
            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.Post)
                .WithMany(p => p.Bookmarks)
                .HasForeignKey(b => b.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // COMMENT replies
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
