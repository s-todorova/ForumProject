using System.ComponentModel.DataAnnotations;

namespace ForumApp.Models;

/// <summary>
/// Представя отделна дискусионна тема (публикация) във форума.
/// </summary>
public class Post
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Заглавието е задължително.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Заглавието трябва да е между 5 и 100 символа.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Съдържанието е задължително.")]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Връзка към създателя на темата
    [Required]
    public string AuthorId { get; set; } = string.Empty;
    public ApplicationUser? Author { get; set; }

    // Колекция от коментари към тази тема
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}