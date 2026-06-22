using ForumApp.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForumApp.Tests.Unit
{
    public class CommentModerationServiceTests
    {
        private readonly Mock<ICommentModerationService> _mockService;

        public CommentModerationServiceTests()
        {
            _mockService = new Mock<ICommentModerationService>();
        }

        [Fact]
        public void IsToxic_ToxicComment_ReturnsTrue()
        {
            // Arrange
            _mockService.Setup(s => s.IsToxic("you are an idiot")).Returns(true);

            // Act
            var result = _mockService.Object.IsToxic("you are an idiot");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsToxic_NormalComment_ReturnsFalse()
        {
            // Arrange
            _mockService.Setup(s => s.IsToxic("Great topic, thank you!")).Returns(false);

            // Act
            var result = _mockService.Object.IsToxic("Great topic, thank you!");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsToxic_EmptyComment_ReturnsFalse()
        {
            // Arrange
            _mockService.Setup(s => s.IsToxic("")).Returns(false);

            // Act
            var result = _mockService.Object.IsToxic("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsToxic_WhitespaceComment_ReturnsFalse()
        {
            // Arrange
            _mockService.Setup(s => s.IsToxic("   ")).Returns(false);

            // Act
            var result = _mockService.Object.IsToxic("   ");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("you are stupid")]
        [InlineData("complete idiot")]
        [InlineData("shut up moron")]
        [InlineData("get lost loser")]
        public void IsToxic_VariousToxicComments_ReturnsTrue(string comment)
        {
            // Arrange
            _mockService.Setup(s => s.IsToxic(comment)).Returns(true);

            // Act
            var result = _mockService.Object.IsToxic(comment);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("Great article!")]
        [InlineData("Very useful, thank you.")]
        [InlineData("I agree with your point.")]
        [InlineData("Bravo, keep it up!")]
        public void IsToxic_VariousNormalComments_ReturnsFalse(string comment)
        {
            // Arrange
            _mockService.Setup(s => s.IsToxic(comment)).Returns(false);

            // Act
            var result = _mockService.Object.IsToxic(comment);

            // Assert
            Assert.False(result);
        }
    }
}
