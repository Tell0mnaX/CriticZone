using System.ComponentModel.DataAnnotations;

namespace CriticZoneApp.Models{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }

        public DateTime RegisteredAt { get; set; }

        public string Role { get; set; } = "User"; // "User" ou "Admin"

        public List<Review> Reviews { get; set; } = new();

        public List<Comment> Comments { get; set; } = new();

        public string Bio { get; set; } = string.Empty;
        
        public string? PhotoUrl { get; set; } // lien vers une photo de profil


    }
}
