using System.Security.Claims;
using ForumApp.Data;
using ForumApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ForumApp.Pages.Posts;

/// <summary>
/// Модел на страницата за създаване на нова публикация (дискусионна тема) във форума.
/// Позволява на автентикирани потребители да създават нови теми за дискусия.
/// </summary>
/// <remarks>
/// Достъпът до тази страница изисква автентикация чрез атрибута <see cref="AuthorizeAttribute"/>.
/// Деактивираните потребители не могат да създават нови публикации,
/// дори ако са успешно автентикирани.
/// </remarks>
[Authorize]
public class CreateModel : PageModel
{
    /// <summary>
    /// Контекст на базата данни за достъп до публикациите.
    /// </summary>
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Инициализира нова инстанция на <see cref="CreateModel"/> с посочения контекст на базата данни.
    /// </summary>
    /// <param name="context">Контекст на базата данни, инжектиран чрез Dependency Injection.</param>
    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Обект на новата публикация, свързан с формуляра чрез двупосочно свързване на данни.
    /// Съдържа заглавието и съдържанието, въведени от потребителя.
    /// </summary>
    [BindProperty]
    public Post NewPost { get; set; } = new Post();

    /// <summary>
    /// Обработва HTTP GET заявка за страницата за създаване на публикация.
    /// Показва празен формуляр за въвеждане на нова тема.
    /// </summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// Обработва HTTP POST заявка за създаване на нова публикация.
    /// Валидира входните данни, проверява дали потребителят е активен
    /// и записва новата публикация в базата данни.
    /// </summary>
    /// <returns>
    /// Пренасочване към главната страница при успех,
    /// или връщане на текущата страница с грешки при невалидни данни.
    /// </returns>
    /// <remarks>
    /// Методът извършва следните проверки:
    /// <list type="bullet">
    /// <item><description>Проверка за автентикация на потребителя</description></item>
    /// <item><description>Проверка дали потребителският профил е активен</description></item>
    /// <item><description>Валидация на модела според дефинираните атрибути</description></item>
    /// </list>
    /// Полетата AuthorId и Author се изключват от валидацията на модела,
    /// тъй като се попълват автоматично от системата.
    /// </remarks>
    public async Task<IActionResult> OnPostAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Challenge();

        var currentUser = await _context.Users.FindAsync(userId);
        if (currentUser == null || !currentUser.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Профилът ви е деактивиран от администратор. Нямате право да създавате нови теми.");
            return Page();
        }

        ModelState.Remove("NewPost.AuthorId");
        ModelState.Remove("NewPost.Author");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        NewPost.AuthorId = userId;
        NewPost.CreatedAt = DateTime.UtcNow;

        _context.Posts.Add(NewPost);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Index");
    }
}
