using System.ComponentModel.DataAnnotations;
using CriticZoneApp.Models;

namespace CriticZoneApp.Models{
    public class ReviewDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId {get; set;}

        public List<CommentDto> Comments { get; set; } = new();

        public List<CategoryDto> Categories {get;set;} = new();
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

}
