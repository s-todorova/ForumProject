using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ForumApp.Pages;

/// <summary>
/// Модел на страницата за грешки в приложението.
/// Показва информация за възникнали грешки по време на изпълнение,
/// включително уникален идентификатор на заявката за целите на диагностиката.
/// </summary>
/// <remarks>
/// Страницата е конфигурирана да не се кешира (<see cref="ResponseCacheAttribute"/>)
/// и да игнорира антифалшификационния токен (<see cref="IgnoreAntiforgeryTokenAttribute"/>),
/// тъй като може да бъде достъпена при различни грешки, включително такива,
/// свързани със сесията на потребителя.
/// </remarks>
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    /// <summary>
    /// Уникален идентификатор на HTTP заявката, предизвикала грешката.
    /// Използва се за проследяване и диагностика на проблеми в системните логове.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Определя дали идентификаторът на заявката трябва да бъде показан на потребителя.
    /// Връща <c>true</c>, ако <see cref="RequestId"/> не е празен или null.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    /// <summary>
    /// Услуга за логване на грешки и диагностична информация.
    /// </summary>
    private readonly ILogger<ErrorModel> _logger;

    /// <summary>
    /// Инициализира нова инстанция на <see cref="ErrorModel"/> с посочената услуга за логване.
    /// </summary>
    /// <param name="logger">
    /// Услуга за логване, инжектирана чрез Dependency Injection.
    /// </param>
    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Обработва HTTP GET заявка за страницата с грешки.
    /// Извлича уникалния идентификатор на заявката от текущата активност
    /// или от контекста на HTTP заявката.
    /// </summary>
    public void OnGet()
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }
}

