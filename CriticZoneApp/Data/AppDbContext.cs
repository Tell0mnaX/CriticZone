using Microsoft.EntityFrameworkCore;
using CriticZoneApp.Models;

public class AppDbContext : DbContext{
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users {get;set;}
    public DbSet<Review> Reviews {get;set;}
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Category> Categories { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Fake password hash/salt (pour test uniquement)
        var passwordHash = new byte[] { 1, 2, 3 };
        var passwordSalt = new byte[] { 4, 5, 6 };

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = "Admin"
            },
            new User
            {
                Id = 2,
                Username = "user1",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = "User"
            }
        );

        modelBuilder.Entity<Review>().HasData(
            new Review
            {
                Id = 1,
                Title = "Inception",
                Content = "Un chef-d'œuvre de Nolan",
                Rating = 9,
                CreatedAt = new DateTime(2024, 1, 1),
                UserId = 2
            },
            new Review
            {
                Id = 2,
                Title = "The Room",
                Content = "C'est tellement mauvais que c'est culte",
                Rating = 3,
                CreatedAt = new DateTime(2024, 1, 1),
                UserId = 2
            }
        );
    
        modelBuilder.Entity<Comment>()
            .HasMany(c => c.Replies)
            .WithOne(c => c.ParentComment)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict); // évite les suppressions en cascade
    }
}