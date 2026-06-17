using System.Security.Claims;
using ForumApp.Data;
using ForumApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ForumApp.Pages.Posts;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Post NewPost { get; set; } = new Post();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userId == null)
        {
            return Challenge();
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