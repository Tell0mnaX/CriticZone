namespace CriticZoneApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<Review> Reviews { get; set; } = new(); // relation inverse
    }
}