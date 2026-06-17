using System.Security.Claims;
using ForumApp.Data;
using ForumApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ForumApp.Pages.Posts;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Post Post { get; set; } = default!;
    public IList<Comment> Comments { get; set; } = new List<Comment>();

    // ПРОМЯНА: Преименувахме го на NewCommentText, за да съвпада с модела
    [BindProperty]
    public string NewCommentText { get; set; } = string.Empty; 

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var post = await _context.Posts
            .Include(p => p.Author)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (post == null) return NotFound();

        Post = post;

        Comments = await _context.Comments
            .Include(c => c.Author)
            .Where(c => c.PostId == id)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!User.Identity?.IsAuthenticated ?? true) return Challenge();

        if (string.IsNullOrWhiteSpace(NewCommentText))
        {
            ModelState.AddModelError("NewCommentText", "Коментарът не може да бъде празен.");
            return await OnGetAsync(id);
        }

        var comment = new Comment
        {
            Text = NewCommentText,
            PostId = id,
            AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
            CreatedAt = DateTime.UtcNow,
            Status = CommentStatus.Pending
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return RedirectToPage(new { id = id });
    }
}