using System.ComponentModel.DataAnnotations;

namespace ForumApp.Models;

/// <summary>
/// Представлява дискусионна тема (публикация) във форума.
/// Публикациите са основната структурна единица на форума, към която потребителите
/// могат да добавят коментари. Всяка публикация има автор, заглавие и съдържание.
/// </summary>
public class Post
{
    /// <summary>
    /// Уникален идентификатор на публикацията в базата данни.
    /// Използва се като първичен ключ в таблицата Posts.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Заглавие на публикацията, което се показва в списъка с теми.
    /// Дължината трябва да бъде между 5 и 100 символа.
    /// </summary>
    [Required(ErrorMessage = "Заглавието е задължително.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Заглавието трябва да е между 5 и 100 символа.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Основно текстово съдържание на публикацията.
    /// Съдържа подробното описание или въпрос, който авторът иска да сподели с общността.
    /// </summary>
    [Required(ErrorMessage = "Съдържанието е задължително.")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Дата и час на създаване на публикацията.
    /// Стойността се задава автоматично при създаване на обекта в UTC формат.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Идентификатор на потребителя, който е автор на публикацията.
    /// Представлява външен ключ към таблицата AspNetUsers.
    /// </summary>
    [Required]
    public string AuthorId { get; set; } = string.Empty;

    /// <summary>
    /// Навигационно свойство към потребителя, създал публикацията.
    /// Позволява достъп до пълната информация за автора чрез Entity Framework.
    /// </summary>
    public ApplicationUser? Author { get; set; }

    /// <summary>
    /// Колекция от коментари, добавени към тази публикация.
    /// Представлява релация "един към много" между публикации и коментари.
    /// Коментарите подлежат на ML-базирано модериране за токсично съдържание.
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
