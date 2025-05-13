using System.ComponentModel.DataAnnotations;

namespace CriticZoneApp.Models{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        [Range(0, 10)]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public int UserId { get; set; }
        public User? User { get; set; }

        public List<Comment> Comments { get; set; } = new();

        public List<Category> Categories { get; set; } = new();
    }
}
