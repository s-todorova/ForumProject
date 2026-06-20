using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using ForumApp.Data;
using ForumApp.Models;
using ForumApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForumApp.Pages.Posts;

/// <summary>
/// Модел на страницата за детайли на публикация.
/// Показва пълното съдържание на публикацията, нейните одобрени коментари
/// и предоставя възможност за добавяне на нови коментари от автентикирани потребители.
/// </summary>
/// <remarks>
/// Тази страница е ключова за системата за ML-базирано модериране на съдържание.
/// При добавяне на нов коментар, текстът се анализира за наличие на токсично съдържание
/// и статусът на коментара се определя автоматично преди записването в базата данни.
/// </remarks>
public class DetailsModel : PageModel
{
    /// <summary>
    /// Контекст на базата данни за достъп до публикации и коментари.
    /// </summary>
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Услуга за ML-базирано модериране на коментари чрез NAS-BERT модел.
    /// </summary>
    private readonly ICommentModerationService _moderationService;

    /// <summary>
    /// Инициализира нова инстанция на <see cref="DetailsModel"/>.
    /// </summary>
    /// <param name="context">Контекст на базата данни.</param>
    /// <param name="moderationService">ML услуга за модериране на коментари.</param>
    public DetailsModel(ApplicationDbContext context, ICommentModerationService moderationService)
    {
        _context = context;
        _moderationService = moderationService;
    }

    /// <summary>
    /// Публикацията, която се показва на страницата.
    /// Включва информация за автора чрез навигационното свойство.
    /// </summary>
    public Post Post { get; set; } = default!;

    /// <summary>
    /// Списък с одобрени коментари към публикацията.
    /// Показват се само коментари със статус <see cref="CommentStatus.Approved"/>,
    /// подредени хронологично по дата на създаване.
    /// </summary>
    public IList<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Текст на новия коментар, въведен от потребителя.
    /// Свързан с формуляра чрез двупосочно свързване на данни.
    /// Максималната допустима дължина е 1000 символа.
    /// </summary>
    [BindProperty]
    [StringLength(1000, ErrorMessage = "Коментарът не може да надвишава 1000 символа.")]
    public string NewCommentText { get; set; } = string.Empty;

    /// <summary>
    /// Обработва HTTP GET заявка за страницата с детайли на публикация.
    /// Зарежда публикацията и нейните одобрени коментари от базата данни.
    /// </summary>
    /// <param name="id">Уникален идентификатор на публикацията.</param>
    /// <returns>
    /// Страницата с детайли при успех, или NotFound резултат ако публикацията не съществува.
    /// </returns>
    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        Post = await _context.Posts
            .Include(p => p.Author)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (Post == null) return NotFound();

        Comments = await _context.Comments
            .Include(c => c.Author)
            .Where(c => c.PostId == id && c.Status == CommentStatus.Approved)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return Page();
    }

    /// <summary>
    /// Обработва HTTP POST заявка за добавяне на нов коментар към публикацията.
    /// Извършва валидация на входните данни, проверка на статуса на потребителя
    /// и ML-базирана класификация на съдържанието за токсичност.
    /// </summary>
    /// <param name="id">Уникален идентификатор на публикацията, към която се добавя коментарът.</param>
    /// <returns>
    /// Пренасочване към същата страница при успех,
    /// или връщане на страницата с грешки при невалидни данни.
    /// </returns>
    /// <remarks>
    /// Процесът на модериране включва следните стъпки:
    /// <list type="number">
    /// <item><description>Проверка за автентикация на потребителя</description></item>
    /// <item><description>Проверка дали потребителският профил е активен</description></item>
    /// <item><description>Валидация на текста на коментара</description></item>
    /// <item><description>ML-базирана класификация за токсично съдържание</description></item>
    /// <item><description>Присвояване на съответния статус и записване в базата данни</description></item>
    /// </list>
    /// Текущата имплементация използва опростена логика за откриване на токсични думи.
    /// В бъдеще тук ще бъде интегриран NAS-BERT модел за по-точна класификация.
    /// </remarks>
    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!User.Identity?.IsAuthenticated ?? true) return Challenge();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var currentUser = await _context.Users.FindAsync(userId);
        if (currentUser == null || !currentUser.IsActive)
        {
            ModelState.AddModelError("NewCommentText", "Профилът ви е деактивиран. Нямате право да публикувате коментари.");
            return await OnGetAsync(id);
        }

        if (string.IsNullOrWhiteSpace(NewCommentText))
        {
            ModelState.AddModelError("NewCommentText", "Коментарът не може да бъде празен.");
            return await OnGetAsync(id);
        }

        // ML класификация чрез NAS-BERT модел
        var calculatedStatus = _moderationService.IsToxic(NewCommentText)
            ? CommentStatus.Pending
            : CommentStatus.Approved;

        var comment = new Comment
        {
            Text = NewCommentText,
            PostId = id,
            AuthorId = userId ?? "",
            CreatedAt = DateTime.UtcNow,
            Status = calculatedStatus
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return RedirectToPage(new { id = id });
    }
}