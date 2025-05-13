using System.ComponentModel.DataAnnotations;
using CriticZoneApp.Models;

namespace CriticZoneApp.Models{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
    }
}
