namespace CriticZoneApp.Models
{
    public class CommentThreadDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<CommentThreadDto> Replies { get; set; } = new();
    }

}