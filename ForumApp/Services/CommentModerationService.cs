using Microsoft.ML;
using Microsoft.ML.Data;

namespace ForumApp.Services
{
    /// <summary>
    /// Имплементация на услугата за ML-базирано модериране на коментари.
    /// Зарежда предварително трениран NAS-BERT модел и го използва за класификация.
    /// Регистрирана като Singleton, тъй като MLContext е скъп за създаване.
    /// </summary>
    public class CommentModerationService : ICommentModerationService
    {
        private class InputData
        {
            [ColumnName("Text")]
            public string Text { get; set; } = "";

            [ColumnName("label")]
            public float label { get; set; } = 0f;
        }

        private class OutputData
        {
            public float PredictedLabel { get; set; }

            [ColumnName("Score")]
            public float[] Score { get; set; } = Array.Empty<float>();
        }

        private readonly MLContext _mlContext;
        private readonly ITransformer? _model;
        private readonly ILogger<CommentModerationService> _logger;

        /// <summary>
        /// Инициализира услугата и зарежда ML модела от диска.
        /// Ако моделът не е наличен, всички коментари получават статус Pending.
        /// </summary>
        /// <param name="logger">Логър за диагностични съобщения.</param>
        public CommentModerationService(ILogger<CommentModerationService> logger)
        {
            _logger = logger;
            _mlContext = new MLContext(seed: 1);

            var modelPath = Path.Combine("MLModels", "SentimentAnalysis.zip");

            if (!File.Exists(modelPath))
            {
                _logger.LogWarning("ML моделът не е намерен на '{Path}' — всички коментари ще бъдат Pending.", modelPath);
                _model = null;
                return;
            }

            try
            {
                _model = _mlContext.Model.Load(modelPath, out _);
                _logger.LogInformation("ML моделът е зареден от '{Path}'.", modelPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при зареждане на ML модела — всички коментари ще бъдат Pending.");
                _model = null;
            }
        }

        /// <summary>
        /// Класифицира текста на коментара като токсичен или безопасен.
        /// При липсващ модел връща <c>true</c> (безопасен fallback — всичко отива при модератор).
        /// </summary>
        /// <param name="commentText">Текстът за анализ.</param>
        /// <returns><c>true</c> ако е токсичен; <c>false</c> ако е безопасен.</returns>
        public bool IsToxic(string commentText)
        {
            if (_model is null)
                return true;

            if (string.IsNullOrWhiteSpace(commentText))
                return false;

            var input = new List<InputData>
            {
                new InputData { Text = commentText, label = 0f }
            };

            var dataView = _mlContext.Data.LoadFromEnumerable(input);
            var predictions = _model.Transform(dataView);

            var results = _mlContext.Data
                .CreateEnumerable<OutputData>(predictions, reuseRowObject: false)
                .ToList();

            var output = results.FirstOrDefault();
            float score0 = output?.Score?.Length > 0 ? output.Score[0] : 0f;
            float score1 = output?.Score?.Length > 1 ? output.Score[1] : 0f;

            // bool mlToxic = score1 > score0 + 0.3f;
            bool mlToxic = score1 > score0 +0.2f;

            _logger.LogInformation(">>> ML АНАЛИЗ: Класифициран като {Label} (Score[0]={S0:F4}, Score[1]={S1:F4}) <<<",
                mlToxic ? "ТОКСИЧЕН (Чакащ)" : "БЕЗОПАСЕН (Одобрен)", score0, score1);

            return mlToxic;
        }
    }
}