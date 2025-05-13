namespace CriticZoneApp.Models
{
    public class UserProfileDto
    {
        public string Username { get; set; } = string.Empty;
        public int ReviewCount { get; set; }
        public int CommentCount { get; set; }
        public DateTime RegisteredAt { get; set; }
    }

}