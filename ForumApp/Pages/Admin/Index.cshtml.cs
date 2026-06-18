using ForumApp.Data;
using ForumApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForumApp.Pages.Admin;

/// <summary>
/// Модел на административната страница за управление на потребители.
/// Предоставя функционалност за преглед на всички регистрирани потребители,
/// промяна на техния статус (активен/неактивен) и управление на ролята Moderator.
/// </summary>
/// <remarks>
/// Достъпът до тази страница е ограничен само до потребители с роля "Admin"
/// чрез атрибута <see cref="AuthorizeAttribute"/>.
/// Администраторът не може да променя собствения си статус или роля,
/// за да се предотврати случайно заключване от системата.
/// </remarks>
[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    /// <summary>
    /// Контекст на базата данни за достъп до потребителите.
    /// </summary>
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Мениджър за управление на потребители чрез ASP.NET Core Identity.
    /// </summary>
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Инициализира нова инстанция на <see cref="IndexModel"/> с необходимите зависимости.
    /// </summary>
    /// <param name="context">Контекст на базата данни, инжектиран чрез Dependency Injection.</param>
    /// <param name="userManager">Мениджър за потребители, инжектиран чрез Dependency Injection.</param>
    public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Общ брой регистрирани потребители в системата.
    /// Показва се като статистика в административния панел.
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Брой активни потребители в системата.
    /// Активните потребители имат право да влизат и да използват форума.
    /// </summary>
    public int ActiveUsers { get; set; }

    /// <summary>
    /// Брой потребители с роля "Moderator".
    /// Модераторите имат право да преглеждат и одобряват/отхвърлят коментари,
    /// маркирани от ML модела като потенциално токсични.
    /// </summary>
    public int ModeratorsCount { get; set; }

    /// <summary>
    /// Вътрешен клас за представяне на информация за потребител в административния списък.
    /// Служи като DTO (Data Transfer Object) за визуализация в изгледа.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// Уникален идентификатор на потребителя в системата.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Електронна поща на потребителя, служеща и като потребителско име.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Флаг, указващ дали потребителският акаунт е активен.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Списък с ролите, присвоени на потребителя (напр. Admin, Moderator, User).
        /// </summary>
        public IList<string> Roles { get; set; } = new List<string>();
    }

    /// <summary>
    /// Списък с информация за всички потребители, показван в административния панел.
    /// </summary>
    public List<UserInfo> UsersList { get; set; } = new();

    /// <summary>
    /// Обработва HTTP GET заявка за административната страница.
    /// Зарежда статистика за потребителите и пълен списък с техните данни и роли.
    /// </summary>
    /// <returns>Задача, представляваща асинхронната операция по зареждане на данните.</returns>
    public async Task OnGetAsync()
    {
        TotalUsers = await _context.Users.CountAsync();
        ActiveUsers = await _context.Users.CountAsync(u => u.IsActive);

        var users = await _context.Users.ToListAsync();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            UsersList.Add(new UserInfo
            {
                Id = u.Id,
                Email = u.Email ?? "",
                IsActive = u.IsActive,
                Roles = roles
            });

            if (roles.Contains("Moderator")) ModeratorsCount++;
        }
    }

    /// <summary>
    /// Обработва HTTP POST заявка за превключване на статуса на потребител (активен/неактивен).
    /// Администраторът не може да променя собствения си статус.
    /// </summary>
    /// <param name="id">Уникален идентификатор на потребителя, чийто статус ще бъде променен.</param>
    /// <returns>Пренасочване към същата страница след извършване на операцията.</returns>
    public async Task<IActionResult> OnPostToggleStatusAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            if (user.Email != User.Identity?.Name)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
            }
        }
        return RedirectToPage();
    }

    /// <summary>
    /// Обработва HTTP POST заявка за добавяне или премахване на ролята "Moderator" от потребител.
    /// Администраторът не може да променя собствената си роля.
    /// </summary>
    /// <param name="id">Уникален идентификатор на потребителя, чиято роля ще бъде променена.</param>
    /// <returns>Пренасочване към същата страница след извършване на операцията.</returns>
    /// <remarks>
    /// Ролята Moderator дава право на потребителя да преглежда коментари,
    /// които ML моделът е маркирал като потенциално токсични, и да взема
    /// окончателно решение за тяхното одобряване или отхвърляне.
    /// </remarks>
    public async Task<IActionResult> OnPostToggleModeratorAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            if (user.Email != User.Identity?.Name)
            {
                if (await _userManager.IsInRoleAsync(user, "Moderator"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Moderator");
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "Moderator");
                }
            }
        }
        return RedirectToPage();
    }
}
