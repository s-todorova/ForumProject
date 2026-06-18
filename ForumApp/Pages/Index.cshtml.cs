using ForumApp.Data;
using ForumApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForumApp.Pages;

/// <summary>
/// Модел на главната страница на форума.
/// Отговаря за зареждането и показването на списък с всички публикации,
/// подредени по дата на създаване в низходящ ред.
/// </summary>
public class IndexModel : PageModel
{
    /// <summary>
    /// Контекст на базата данни за достъп до публикации и коментари.
    /// </summary>
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Инициализира нова инстанция на <see cref="IndexModel"/> с посочения контекст на базата данни.
    /// </summary>
    /// <param name="context">
    /// Контекст на базата данни, инжектиран чрез Dependency Injection.
    /// </param>
    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Списък с публикации, които ще бъдат показани на главната страница.
    /// Съдържа само одобрените коментари за всяка публикация.
    /// </summary>
    public IList<Post> Posts { get; set; } = new List<Post>();

    /// <summary>
    /// Обработва HTTP GET заявка за главната страница.
    /// Зарежда всички публикации от базата данни заедно с техните автори
    /// и одобрени коментари, подредени по дата на създаване (най-новите първи).
    /// </summary>
    /// <returns>
    /// Задача, представляваща асинхронната операция по зареждане на данните.
    /// </returns>
    /// <remarks>
    /// Филтрирането на коментари по статус <see cref="CommentStatus.Approved"/>
    /// гарантира, че само модерирани и одобрени коментари се показват на потребителите.
    /// </remarks>
    public async Task OnGetAsync()
    {
        Posts = await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments.Where(c => c.Status == CommentStatus.Approved))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}
