namespace EngineGDI
{

    internal sealed class GameManager
    {
        private static readonly GameManager instance = new GameManager();

        public static GameManager Instance => instance;

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }
        public float DeltaTime { get; private set; }
        public int HighScore { get; private set; }

        private GameManager() { }

        public void Initialize(int screenWidth, int screenHeight)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
        }

        public void SetDeltaTime(float deltaTime)
        {
            DeltaTime = deltaTime;
        }

        public void TryUpdateHighScore(int currentScore)
        {
            if (currentScore > HighScore)
                HighScore = currentScore;
        }
    }
}
