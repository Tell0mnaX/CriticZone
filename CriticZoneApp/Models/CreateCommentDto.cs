using System.ComponentModel.DataAnnotations;

namespace CriticZoneApp.Models
{
    public class CreateCommentDto{
        [Required]
        public string Content {get; set;} = string.Empty;
    }
}