using System.ComponentModel.DataAnnotations;

namespace ForumApp.Models;

/// <summary>
/// Представлява коментар към публикация във форума.
/// Този клас е централен за системата за модериране, тъй като съдържа текстово съдържание,
/// което подлежи на автоматична класификация чрез ML модел за откриване на токсично съдържание.
/// </summary>
public class Comment
{
    /// <summary>
    /// Уникален идентификатор на коментара в базата данни.
    /// Използва се като първичен ключ в таблицата с коментари.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Текстово съдържание на коментара, въведено от потребителя.
    /// Това поле се анализира от ML модела за класификация на токсичност.
    /// Максималната допустима дължина е 1000 символа.
    /// </summary>
    [Required(ErrorMessage = "Текстът на коментара е задължителен.")]
    [StringLength(1000, ErrorMessage = "Коментарът е твърде дълъг.")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Дата и час на създаване на коментара.
    /// Стойността се задава автоматично при създаване на обекта в UTC формат.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Текущ статус на коментара в системата за модериране.
    /// Възможните стойности са дефинирани в изброения тип <see cref="CommentStatus"/>.
    /// По подразбиране коментарите се одобряват автоматично, освен ако ML моделът не ги маркира като токсични.
    /// </summary>
    public CommentStatus Status { get; set; } = CommentStatus.Approved;

    /// <summary>
    /// Идентификатор на потребителя, който е автор на коментара.
    /// Представлява външен ключ към таблицата AspNetUsers.
    /// </summary>
    [Required]
    public string AuthorId { get; set; } = string.Empty;

    /// <summary>
    /// Навигационно свойство към потребителя, създал коментара.
    /// Позволява достъп до пълната информация за автора чрез Entity Framework.
    /// </summary>
    public ApplicationUser? Author { get; set; }

    /// <summary>
    /// Идентификатор на публикацията, към която принадлежи коментарът.
    /// Представлява външен ключ към таблицата Posts.
    /// </summary>
    [Required]
    public int PostId { get; set; }

    /// <summary>
    /// Навигационно свойство към публикацията, към която е добавен коментарът.
    /// Използва се за навигация между свързаните обекти в Entity Framework.
    /// </summary>
    public Post? Post { get; set; }
}
