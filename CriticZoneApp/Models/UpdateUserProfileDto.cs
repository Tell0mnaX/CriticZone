namespace CriticZoneApp.Models
{
    public class UpdateUserProfileDto
    {
        public string Bio { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
    }

}