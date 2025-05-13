using System.Reflection.Metadata.Ecma335;
using CriticZoneApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase{

    public readonly AppDbContext _Context;
    public ReviewController(AppDbContext context){_Context = context;}

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Review>>> GetReviews(){
        
        var reviews = await _Context.Reviews
        .Include(r => r.Categories)
        .ToListAsync();

        var reviewDtos = reviews.Select(review => new ReviewDto
        {
            Id = review.Id,
            Title = review.Title,
            Content = review.Content,
            Rating = review.Rating,
            CreatedAt = review.CreatedAt,
            UserId = review.UserId,
            Comments = review.Comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UserId = c.UserId
            }).ToList(),
            Categories = review.Categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToList()
        });

        return Ok(reviewDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewDto>> GetReview(int id){
        
        var review = await _Context.Reviews
        .Include(r => r.Comments)
        .Include(r => r.Categories)
        .FirstOrDefaultAsync(r => r.Id==id);
        
        if(review==null)
            return NotFound("La review n'existe pas");

        var reviewsDto =  new ReviewDto{
            Id = id,
            Title = review.Title,
            Content = review.Content,
            Rating = review.Rating,
            CreatedAt = review.CreatedAt,
            UserId = review.UserId,
            Comments = review.Comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UserId = c.UserId
            }).ToList(),
            Categories = review.Categories.Select(c => new CategoryDto{
                Id = c.Id,
                Name = c.Name
            }).ToList()
        };
            
        return Ok(reviewsDto);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto){
        
        Console.WriteLine("Catégories reçues : " + string.Join(", ", dto.Categories.Select(c => c.Name)));

        // Ici, on récupère l'ID de l'utilisateur connecté depuis le token
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var newReview = new Review
        {
            Title = dto.Title,
            Content = dto.Content,
            Rating = dto.Rating,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Categories = new List<Category>()
        };

        // Gérer les catégories
        foreach (var categoryDto in dto.Categories.DistinctBy(c => c.Name.ToLower()))
        {
            var existingCategory = await _Context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryDto.Name.ToLower());

            if (existingCategory != null)
            {
                newReview.Categories.Add(existingCategory);
            }
            else
            {
                var newCategory = new Category { Name = categoryDto.Name };
                newReview.Categories.Add(newCategory);
            }
        }

        _Context.Reviews.Add(newReview);
        await _Context.SaveChangesAsync();

        var outPutReviewDto = new ReviewDto{
        Id = newReview.Id,
        Title = newReview.Title,
        Content = newReview.Content,
        Rating = newReview.Rating,
        CreatedAt = newReview.CreatedAt,
        UserId = newReview.UserId,
        Categories = newReview.Categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name
        }).ToList(),
        Comments = new List<CommentDto>() // ou null si pas nécessaire
    };

        return Ok(outPutReviewDto);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReview(int id, [FromBody] CreateReviewDto dto){
        
        // Ici, on récupère l'ID de l'utilisateur connecté depuis le token
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole("Admin");

        var review  = await _Context.Reviews
        .Include(r => r.Categories)
        .FirstOrDefaultAsync(r => r.Id == id);

        //Si la review n'existe pas
        if(review ==null) 
            return NotFound($"La review d'id n'a pas été trouvée");

        //Si l'utilisateur n'est pas authorisé + ce n'est pas un admin
        if(review.UserId != userId && !isAdmin)
            return Forbid($"Vous n'êtes pas authorisé à modifier cette review : Vous n'êtes pas son auteur");

        review.Title = dto.Title;
        review.Content = dto.Content;
        review.Rating = dto.Rating;

        // Mise à jour des catégories
        review.Categories.Clear(); // Retire toutes les relations actuelles

        foreach (var categoryDto in dto.Categories.DistinctBy(c => c.Name.ToLower()))
        {
            var existingCategory = await _Context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryDto.Name.ToLower());

            if (existingCategory != null)
            {
                review.Categories.Add(existingCategory);
            }
            else
            {
                var newCategory = new Category { Name = categoryDto.Name };
                _Context.Categories.Add(newCategory);
                review.Categories.Add(newCategory);
            }
        }

        await _Context.SaveChangesAsync();

        // Supprimer les catégories orphelines (non utilisées)
        var orphanCategories = await _Context.Categories
            .Where(c => !c.Reviews.Any())
            .ToListAsync();

        _Context.Categories.RemoveRange(orphanCategories);
        await _Context.SaveChangesAsync();

        return Ok(new { message = $"La review {review.Title} a été modifié !"});
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveReview(int id){
        
        // Ici, on récupère l'ID de l'utilisateur connecté depuis le token
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole("Admin");

        if(userId==null)
            return Unauthorized("Token JWT invalide ou manquant.");

        var review = await _Context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        
        //Si la review n'existe pas
        if(review==null) 
            return NotFound($"La review d'id n'a pas été trouvée");
        
        //Si l'utilisateur n'est pas authorisé + ce n'est pas un admin
        if(review.UserId != userId && !isAdmin)
            return Forbid($"Vous n'êtes pas authorisé à supprimer cette review : Vous n'êtes pas son auteur");

        _Context.Reviews.Remove(review);
        await _Context.SaveChangesAsync();

        return Ok(new { message = $"La review {review.Title} a été supprimée !"});
    }
}