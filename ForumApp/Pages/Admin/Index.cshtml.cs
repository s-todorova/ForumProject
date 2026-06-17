using ForumApp.Data;
using ForumApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForumApp.Pages.Admin;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalUsers { get; set; }
    public int TotalPosts { get; set; }
    public int PendingComments { get; set; }

    public IList<Post> AllPosts { get; set; } = new List<Post>();

    public async Task OnGetAsync()
    {
        TotalUsers = await _context.Users.CountAsync();
        TotalPosts = await _context.Posts.CountAsync();
        PendingComments = await _context.Comments.CountAsync(c => c.Status == Models.CommentStatus.Pending);

        AllPosts = await _context.Posts
            .Include(p => p.Author)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeletePostAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);

        if (post != null)
        {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}