namespace ForumApp.Services
{
    /// <summary>
    /// Договор за услугата за модериране на коментари чрез ML модел.
    /// </summary>
    public interface ICommentModerationService
    {
        /// <summary>
        /// Оценява дали даден текст на коментар е токсичен.
        /// </summary>
        /// <param name="commentText">Текстът на коментара за анализ.</param>
        /// <returns>
        /// <c>true</c> ако моделът класифицира текста като токсичен (статус Pending);
        /// <c>false</c> ако текстът е безопасен (статус Approved).
        /// </returns>
        bool IsToxic(string commentText);
    }
}