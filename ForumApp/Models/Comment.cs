using System.ComponentModel.DataAnnotations;

namespace ForumApp.Models;

/// <summary>
/// Представя коментар към публикация във форума. Подлежи на ML модериране.
/// </summary>
public class Comment
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Текстът на коментара е задължителен.")]
    [StringLength(1000, ErrorMessage = "Коментарът е твърде дълъг.")]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CommentStatus Status { get; set; } = CommentStatus.Approved;

    // Връзка с потребителя
    [Required]
    public string AuthorId { get; set; } = string.Empty;
    public ApplicationUser? Author { get; set; }

    // Връзка с темата (Post)
    [Required]
    public int PostId { get; set; }
    public Post? Post { get; set; }
}