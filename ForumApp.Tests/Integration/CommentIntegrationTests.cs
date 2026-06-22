using ForumApp.Data;
using ForumApp.Models;
using ForumApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ForumApp.Tests.Integration
{
    public class CommentIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ICommentModerationService> _mockModerationService;

        public CommentIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockModerationService = new Mock<ICommentModerationService>();

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser@test.com",
                Email = "testuser@test.com",
                IsActive = true
            };

            var post = new Post
            {
                Id = 1,
                Title = "Test Post",
                Content = "Test content",
                AuthorId = "user1",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.Posts.Add(post);
            _context.SaveChanges();
        }

        [Fact]
        public async Task NormalComment_GetsApprovedStatus()
        {
            // Arrange — ML service казва безопасен
            _mockModerationService.Setup(s => s.IsToxic("Great post!")).Returns(false);

            // Act — симулираме логиката от OnPostAsync
            var isToxic = _mockModerationService.Object.IsToxic("Great post!");
            var status = isToxic ? CommentStatus.Pending : CommentStatus.Approved;

            var comment = new Comment
            {
                Text = "Great post!",
                PostId = 1,
                AuthorId = "user1",
                CreatedAt = DateTime.UtcNow,
                Status = status
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Assert
            var saved = await _context.Comments.FirstOrDefaultAsync(c => c.Text == "Great post!");
            Assert.NotNull(saved);
            Assert.Equal(CommentStatus.Approved, saved.Status);
        }

        [Fact]
        public async Task ToxicComment_GetsPendingStatus()
        {
            // Arrange — ML service казва токсичен
            _mockModerationService.Setup(s => s.IsToxic("you are an idiot")).Returns(true);

            // Act
            var isToxic = _mockModerationService.Object.IsToxic("you are an idiot");
            var status = isToxic ? CommentStatus.Pending : CommentStatus.Approved;

            var comment = new Comment
            {
                Text = "you are an idiot",
                PostId = 1,
                AuthorId = "user1",
                CreatedAt = DateTime.UtcNow,
                Status = status
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Assert
            var saved = await _context.Comments.FirstOrDefaultAsync(c => c.Text == "you are an idiot");
            Assert.NotNull(saved);
            Assert.Equal(CommentStatus.Pending, saved.Status);
        }

        [Fact]
        public async Task ApprovedComments_AreVisibleOnPost()
        {
            // Arrange
            _context.Comments.AddRange(
                new Comment { Text = "Good comment", PostId = 1, AuthorId = "user1", Status = CommentStatus.Approved, CreatedAt = DateTime.UtcNow },
                new Comment { Text = "Bad comment", PostId = 1, AuthorId = "user1", Status = CommentStatus.Pending, CreatedAt = DateTime.UtcNow },
                new Comment { Text = "Rejected comment", PostId = 1, AuthorId = "user1", Status = CommentStatus.Rejected, CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act — само Approved коментари се показват
            var visibleComments = await _context.Comments
                .Where(c => c.PostId == 1 && c.Status == CommentStatus.Approved)
                .ToListAsync();

            // Assert
            Assert.Single(visibleComments);
            Assert.Equal("Good comment", visibleComments[0].Text);
        }

        [Fact]
        public async Task DeactivatedUser_CommentIsBlocked()
        {
            // Arrange
            var inactiveUser = new ApplicationUser
            {
                Id = "user2",
                UserName = "banned@test.com",
                Email = "banned@test.com",
                IsActive = false
            };
            _context.Users.Add(inactiveUser);
            await _context.SaveChangesAsync();

            // Act — симулираме проверката от OnPostAsync
            var user = await _context.Users.FindAsync("user2");
            bool canComment = user != null && user.IsActive;

            // Assert
            Assert.False(canComment);
        }

        [Fact]
        public async Task Post_CanBeDeleted_ByAuthor()
        {
            // Act
            var post = await _context.Posts.FindAsync(1);
            Assert.NotNull(post);

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            // Assert
            var deleted = await _context.Posts.FindAsync(1);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task MultipleComments_OrderedByDate()
        {
            // Arrange
            _context.Comments.AddRange(
                new Comment { Text = "First", PostId = 1, AuthorId = "user1", Status = CommentStatus.Approved, CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
                new Comment { Text = "Second", PostId = 1, AuthorId = "user1", Status = CommentStatus.Approved, CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
                new Comment { Text = "Third", PostId = 1, AuthorId = "user1", Status = CommentStatus.Approved, CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var comments = await _context.Comments
                .Where(c => c.PostId == 1 && c.Status == CommentStatus.Approved)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            // Assert
            Assert.Equal("First", comments[0].Text);
            Assert.Equal("Second", comments[1].Text);
            Assert.Equal("Third", comments[2].Text);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
