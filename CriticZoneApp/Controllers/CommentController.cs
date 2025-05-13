using System.Reflection.Emit;
using Microsoft.AspNetCore.Mvc;
using CriticZoneApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    public readonly AppDbContext _Context;

    public CommentController(AppDbContext context){_Context = context;}

    /// <summary>Récupère les commentaires d'une review</summary>
    /// <param name="reviewId">L'identifiant de la review</param>
    /// <returns>Une liste de commentaires</returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    [HttpGet("review/{reviewId}")]
    public async Task<ActionResult<IEnumerable<CommentWithAuthorDto[]>>> GetComments(int reviewId)
    {
        // Ici, on récupère l'ID de l'utilisateur connecté depuis le token
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var comments = await _Context.Comments
            .Where(c => c.ReviewId == reviewId)
            .Include(c => c.User)
            .ToListAsync();

        if(comments==null)
            return NotFound($"La review d'id {reviewId} n'existe pas...");

        var commentDtos = comments.Select(c => new CommentWithAuthorDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                Author = c.User!.Username
            }).ToList();

        return Ok(commentDtos);
    }

    [Authorize]
    [HttpDelete("{commentId}")]
    public async Task<ActionResult> RemoveComment(int commentId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole("Admin");

        var comment = await _Context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);

        if(comment==null)
            return NotFound($"Le commetaire d'id {commentId} n'existe pas...");
        
        if(comment.UserId != userId && !isAdmin)
            return Forbid($"Vous n'êtes pas authorisé à modifier ce commentaire : Vous n'êtes pas son auteur");

        _Context.Comments.Remove(comment);
        await _Context.SaveChangesAsync();

        return Ok(new { message = "Le commentaire a bien été supprimé" });
    }

    [HttpPost("{reviewId}")]
    public async Task<ActionResult> AddComment(int reviewId, CreateCommentDto dto)
    {
        // Ici, on récupère l'ID de l'utilisateur connecté depuis le token
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var review = await _Context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);
        if(review==null)
            return NotFound($"La review d'id {reviewId} n'existe pas...");

        var newComment = new Comment
        {
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            ReviewId = reviewId,

        };

        _Context.Comments.Add(newComment);
        await _Context.SaveChangesAsync();

        return Ok(new { message = $"Le commentaire a été ajouté à la review {review.Title}" });
    }

    [Authorize]
    [HttpPost("reply/{commentId}")]
    public async Task<IActionResult> ReplyToComment(int commentId, [FromBody] CreateCommentDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var parentComment = await _Context.Comments
            .Include(c => c.Review) // pour lier à la même review
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (parentComment == null)
            return NotFound($"Commentaire avec l'id {commentId} non trouvé.");

        int depth = 0;
        var current = parentComment;
        while (current.ParentCommentId != null)
        {
            depth++;
            current = await _Context.Comments.FindAsync(current.ParentCommentId);
        }
        if (depth >= 2) return BadRequest("Tu ne peux pas répondre à ce niveau.");

        var reply = new Comment
        {
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            ParentCommentId = commentId,
            ReviewId = parentComment.ReviewId // même review que le parent
        };

        _Context.Comments.Add(reply);
        await _Context.SaveChangesAsync();

        return Ok("Réponse enregistrée !");
    }

    [Authorize]
    [HttpGet("thread/{commentId}")]
    public async Task<IActionResult> GetCommentThread(int commentId)
    {
        var comment = await _Context.Comments
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.Replies)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.ParentCommentId == null);

        if (comment == null)
            return NotFound("Commentaire non trouvé ou ce n'est pas un commentaire racine.");

        var thread = MapToThreadDto(comment);
        return Ok(thread);
    }

    //Map
    private CommentThreadDto MapToThreadDto(Comment comment)
    {
        return new CommentThreadDto
        {
            Id = comment.Id,
            Content = comment.Content,
            Author = comment.User?.Username ?? "Inconnu",
            CreatedAt = comment.CreatedAt,
            Replies = comment.Replies
                .OrderBy(c => c.CreatedAt)
                .Select(MapToThreadDto) // récursif !
                .ToList()
        };
    }


}