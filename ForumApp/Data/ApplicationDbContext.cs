using ForumApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ForumApp.Data;

/// <summary>
/// Главен контекст на базата данни за форумното приложение.
/// Наследява <see cref="IdentityDbContext{TUser}"/> за интегриране на ASP.NET Core Identity
/// система за автентикация и авторизация на потребители.
/// Управлява достъпа до всички таблици в базата данни, включително публикации и коментари.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// Инициализира нова инстанция на <see cref="ApplicationDbContext"/> с посочените опции.
    /// </summary>
    /// <param name="options">
    /// Конфигурационни опции за контекста на базата данни,
    /// включително връзката към SQLite базата данни.
    /// </param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Колекция от публикации (дискусионни теми) във форума.
    /// Предоставя достъп до таблицата Posts в базата данни чрез Entity Framework Core.
    /// </summary>
    public DbSet<Post> Posts { get; set; }

    /// <summary>
    /// Колекция от коментари към публикациите във форума.
    /// Предоставя достъп до таблицата Comments в базата данни.
    /// Коментарите подлежат на ML-базирано модериране за токсично съдържание.
    /// </summary>
    public DbSet<Comment> Comments { get; set; }

    /// <summary>
    /// Конфигурира модела на базата данни чрез Fluent API.
    /// Дефинира релациите между обектите и поведението при изтриване.
    /// </summary>
    /// <param name="builder">
    /// Обект за конфигуриране на модела, предоставен от Entity Framework Core.
    /// </param>
    /// <remarks>
    /// Релациите са конфигурирани с <see cref="DeleteBehavior.Restrict"/>,
    /// за да се предотврати каскадно изтриване на свързани записи
    /// при изтриване на потребител.
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Post>()
            .HasOne(p => p.Author)
            .WithMany()
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
