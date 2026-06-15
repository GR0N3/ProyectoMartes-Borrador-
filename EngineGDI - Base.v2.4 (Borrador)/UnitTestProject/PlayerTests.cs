using Xunit;
using EngineGDI;

namespace UnitTestProject
{
    public class PlayerTests
    {
        [Fact]
        public void TakeDamage_RaisesOnLifeLost_WithExpectedAmount()
        {
            // Arrange
            var player = new Player("Textures/Player/Player.png", 20f, 20f, 200f);
            int received = 0;
            player.OnLifeLost += (amount) => received = amount;

            // Act
            player.TakeDamage(10f);

            // Assert
            Assert.Equal(10, received);
        }

        [Fact]
        public void TakeDamage_WhileBlink_DoesNotRaiseAgain()
        {
            // Arrange
            var player = new Player("Textures/Player/Player.png", 20f, 20f, 200f);
            int callCount = 0;
            int lastAmount = 0;
            player.OnLifeLost += (amount) => { callCount++; lastAmount = amount; };

            // Act
            player.TakeDamage(10f);
            // Immediate second damage should be ignored because of blink
            player.TakeDamage(5f);

            // Assert
            Assert.Equal(1, callCount);
            Assert.Equal(10, lastAmount);
        }
    }
}
