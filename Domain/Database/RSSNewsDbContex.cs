using Contracts.Database;
using Microsoft.EntityFrameworkCore;

namespace Domain.Database
{
    public class RSSNewsDbContext : DbContext
    {
        public DbSet<User> Users { get; init; }
        public DbSet<Feed> Feeds { get; init; }
        public DbSet<Subscription> Subscriptions { get; init; }
        public DbSet<Post> Posts { get; init; }
        public DbSet<ReadPost> ReadPosts { get; init; }

        public RSSNewsDbContext() : base()
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<Subscription>().HasKey(nameof(Subscription.UserId), nameof(Subscription.FeedId));
            _ = modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId);
            _ = modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Feed)
                .WithMany(f => f.Subscribers)
                .HasForeignKey(s => s.FeedId);

            _ = modelBuilder.Entity<Feed>()
                .HasMany(f => f.Posts)
                .WithOne(p => p.Author);

            _ = modelBuilder.Entity<ReadPost>().HasKey(nameof(ReadPost.UserId), nameof(ReadPost.PostId));
            _ = modelBuilder.Entity<ReadPost>()
                .HasOne(rp => rp.User)
                .WithMany(u => u.ReadPosts)
                .HasForeignKey(rp => rp.UserId);
            _ = modelBuilder.Entity<ReadPost>()
                .HasOne(rp => rp.Post)
                .WithMany(p => p.ReadBy)
                .HasForeignKey(rp => rp.PostId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            _ = optionsBuilder.UseSqlite("Data Source=RSSNewsReader.db");
        }
    }
}