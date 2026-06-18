using ForumApp.Data;
using ForumApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForumApp.Pages.Moderator;

/// <summary>
/// Модел на страницата за модериране на коментари.
/// Предоставя интерфейс за преглед и обработка на коментари,
/// които са маркирани от ML модела като потенциално токсични.
/// </summary>
/// <remarks>
/// Тази страница е ключова част от системата за хибридно модериране,
/// която комбинира автоматична ML класификация с човешка преценка.
/// Достъпът е ограничен само до потребители с роля "Moderator"
/// чрез атрибута <see cref="AuthorizeAttribute"/>.
/// Модераторите преглеждат коментарите в изчакване и вземат окончателно
/// решение за тяхното одобряване или отхвърляне.
/// </remarks>
[Authorize(Roles = "Moderator")]
public class IndexModel : PageModel
{
    /// <summary>
    /// Контекст на базата данни за достъп до коментарите.
    /// </summary>
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Инициализира нова инстанция на <see cref="IndexModel"/> с посочения контекст на базата данни.
    /// </summary>
    /// <param name="context">Контекст на базата данни, инжектиран чрез Dependency Injection.</param>
    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Списък с коментари, чакащи за преглед от модератор.
    /// Съдържа само коментари със статус <see cref="CommentStatus.Pending"/>,
    /// които са били маркирани от ML модела като потенциално токсични.
    /// </summary>
    public IList<Comment> PendingComments { get; set; } = new List<Comment>();

    /// <summary>
    /// Обработва HTTP GET заявка за страницата за модериране.
    /// Зарежда всички коментари в изчакване заедно с информация за техните автори и публикации.
    /// </summary>
    /// <returns>Задача, представляваща асинхронната операция по зареждане на данните.</returns>
    /// <remarks>
    /// Коментарите се подреждат хронологично (най-старите първи),
    /// за да се осигури справедлива обработка по реда на постъпване.
    /// </remarks>
    public async Task OnGetAsync()
    {
        PendingComments = await _context.Comments
            .Include(c => c.Author)
            .Include(c => c.Post)
            .Where(c => c.Status == CommentStatus.Pending)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Обработва HTTP POST заявка за одобряване на коментар.
    /// Променя статуса на посочения коментар на <see cref="CommentStatus.Approved"/>,
    /// което го прави видим за всички потребители на форума.
    /// </summary>
    /// <param name="id">Уникален идентификатор на коментара, който ще бъде одобрен.</param>
    /// <returns>Пренасочване към същата страница след извършване на операцията.</returns>
    /// <remarks>
    /// След одобрение коментарът се показва в съответната публикация
    /// и авторът му получава възможност за активно участие в дискусията.
    /// </remarks>
    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment != null)
        {
            comment.Status = CommentStatus.Approved;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    /// <summary>
    /// Обработва HTTP POST заявка за отхвърляне на коментар.
    /// Променя статуса на посочения коментар на <see cref="CommentStatus.Rejected"/>,
    /// което го скрива от обикновените потребители.
    /// </summary>
    /// <param name="id">Уникален идентификатор на коментара, който ще бъде отхвърлен.</param>
    /// <returns>Пренасочване към същата страница след извършване на операцията.</returns>
    /// <remarks>
    /// Отхвърлените коментари не се изтриват от базата данни,
    /// а остават със статус Rejected за евентуален бъдещ анализ
    /// или за подобряване на ML модела чрез допълнително обучение.
    /// </remarks>
    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment != null)
        {
            comment.Status = CommentStatus.Rejected;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
