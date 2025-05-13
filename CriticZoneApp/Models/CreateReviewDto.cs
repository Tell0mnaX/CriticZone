using System.ComponentModel.DataAnnotations;

namespace CriticZoneApp.Models{
    public class CreateReviewDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        [Range(0, 10)]
        public int Rating { get; set; }

        public List<CreateCategoryDto> Categories { get; set; } = new(); // noms des catégories
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
    }
}
