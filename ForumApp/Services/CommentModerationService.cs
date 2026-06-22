namespace ForumApp.Services
{
    public class CommentModerationService : ICommentModerationService
    {
        private readonly ILogger<CommentModerationService> _logger;

        public CommentModerationService(ILogger<CommentModerationService> logger)
        {
            _logger = logger;
        }


        public bool IsToxic(string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText))
                return false;

            bool toxicEN = false;
            bool toxicBG = false;

            // english model
            try
            {
                var inputEN = new MLModel.ModelInput { Text = commentText };
                var resultEN = MLModel.Predict(inputEN);

                float scoreEN = resultEN.Score?.Length > 1 ? resultEN.Score[1] : 0f;
                toxicEN = scoreEN > 0.5f;

                _logger.LogInformation(
                    ">>> EN model: '{Text}' → Score[0]={S0:F4} Score[1]={S1:F4} → {Label} <<<",
                    commentText,
                    resultEN.Score?.Length > 0 ? resultEN.Score[0] : 0f,
                    scoreEN,
                    toxicEN ? "toxic" : "good");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ENG");
            }

            // bulgarain model
            try
            {
                var inputBG = new MLModel1.ModelInput { Text = commentText };
                var resultBG = MLModel1.Predict(inputBG);

                float scoreBG = resultBG.Score?.Length > 1 ? resultBG.Score[1] : 0f;
                toxicBG = scoreBG > 0.5f;

                _logger.LogInformation(
                    ">>> BG model: '{Text}' → Score[0]={S0:F4} Score[1]={S1:F4} → {Label} <<<",
                    commentText,
                    resultBG.Score?.Length > 0 ? resultBG.Score[0] : 0f,
                    scoreBG,
                    toxicBG ? "toxic" : "good");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error in BG.");
            }

            bool toxic = toxicEN || toxicBG;

            return toxic;
        }
    }
}