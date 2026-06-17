using Microsoft.AspNetCore.Identity;

namespace ForumApp.Models;

/// <summary>
/// Разширява базовия IdentityUser с допълнителни полета за нуждите на форума.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Флаг, показващ дали профилът е активен. Администраторът може да го променя.
    /// </summary>
    public bool IsActive { get; set; } = true;
}