namespace CriticZoneApp.Models
{
    public class CommentWithAuthorDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
    
}