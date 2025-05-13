using System.ComponentModel.DataAnnotations;
using CriticZoneApp.Models;

namespace CriticZoneApp.Models{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relation avec User
        public int UserId { get; set; }
        public User? User { get; set; }

        // Relation avec Review
        public int ReviewId { get; set; }
        public Review? Review { get; set; }

        public int? ParentCommentId { get; set; }  // null si ce n’est pas une réponse
        public Comment? ParentComment { get; set; }

        public List<Comment> Replies { get; set; } = new();
    }
}
