using Xunit;
using EngineGDI;

namespace UnitTestProject
{
    public class Vector2Tests
    {
        [Fact]
        public void Constructor_SetsValues()
        {
            // Arrange
            float x = 2f;
            float y = 5f;

            // Act
            var v = new Vector2(x, y);

            // Assert
            Assert.Equal(x, v.X);
            Assert.Equal(y, v.Y);
        }

        [Fact]
        public void ToString_ReturnsExpectedFormat()
        {
            // Arrange
            float x = 2f;
            float y = 5f;
            var v = new Vector2(x, y);
            var expected = $"X : {x} / Y : {y}";

            // Act
            var actual = v.ToString();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
