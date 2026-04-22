using System.Drawing;

namespace EngineGDI
{
    // UIManager:
    // Dibuja la HUD superior (barra negra), el texto (PLAYER/SCORE), las vidas (rectángulos azules)
    // y mantiene el estado de Score y Lives.
    public class UIManager
    {
        private readonly string blackRectSprite;
        private readonly string blueRectSprite;
        private readonly float screenWidth;

        private readonly float hudScale;
        private readonly float hudHeight;
        private readonly float blueScale;
        private readonly float blueWidth;

        private readonly int maxLives;

        public int Lives { get; private set; }
        public int Score { get; private set; }
        public float HudHeight => hudHeight;

        // Constructor:
        // - Calcula el escalado de la barra de HUD (Black_Rect) para que ocupe todo el ancho de pantalla.
        // - Calcula el escalado del rectángulo azul de vida en base a la altura de la HUD.
        // - Inicializa vidas y score.
        public UIManager(
            float screenWidth,
            string blackRectSprite = "Textures/UI/HUD/Black_Rect.png",
            string blueRectSprite = "Textures/UI/HUD/Blue_Rect.png",
            int maxLives = 10)
        {
            this.screenWidth = screenWidth;
            this.blackRectSprite = blackRectSprite;
            this.blueRectSprite = blueRectSprite;
            this.maxLives = maxLives;
            Lives = maxLives;
            Score = 0;

            using (var hudImg = Image.FromFile(blackRectSprite))
            {
                hudScale = screenWidth / hudImg.Width;
                hudHeight = hudImg.Height * hudScale;
            }

            using (var blueImg = Image.FromFile(blueRectSprite))
            {
                // Altura deseada del rectángulo azul (vida) en relación a la altura de la HUD.
                // Este valor controla el tamaño de los "cuadraditos" azules.
                float desiredBlueHeight = hudHeight * 0.30f;
                blueScale = desiredBlueHeight / blueImg.Height;
                blueWidth = blueImg.Width * blueScale;
            }
        }

        // Suma puntos al score.
        public void AddScore(int amount)
        {
            Score += amount;
        }

        // Resta vidas.
        // Se clampa a 0 para que no sea negativo.
        public void RemoveLife(int amount = 1)
        {
            Lives -= amount;
            if (Lives < 0) Lives = 0;
        }

        // Renderiza la HUD:
        // 1) dibuja la barra negra superior
        // 2) dibuja labels (PLAYER / SCORE)
        // 3) dibuja las vidas como rectángulos azules
        // 4) dibuja el score formateado en 6 dígitos (ej: 000300)
        public void Render()
        {
            Engine.Draw(blackRectSprite, 0f, 0f, hudScale, hudScale);

            float paddingX = 20f * hudScale;
            float labelX = paddingX;
            float playerLabelY = 20f * hudScale;
            float scoreLabelY = 70f * hudScale;

            int fontSize = (int)(32f * hudScale);
            if (fontSize < 10) fontSize = 10;

            Engine.DrawText("PLAYER :", labelX, playerLabelY, fontSize, Color.White);
            Engine.DrawText("SCORE  :", labelX, scoreLabelY, fontSize, Color.White);

            float barsX = 260f * hudScale;
            float barsY = 10f * hudScale;
            float spacing = 10f * hudScale;

            int barsToDraw = Lives;
            if (barsToDraw > maxLives) barsToDraw = maxLives;

            for (int i = 0; i < barsToDraw; i++)
            {
                float x = barsX + i * (blueWidth + spacing);
                Engine.Draw(blueRectSprite, x, barsY, blueScale, blueScale);
            }

            string scoreText = Score.ToString("D10");
            float scoreX = 260f * hudScale;
            Engine.DrawText(scoreText, scoreX, scoreLabelY, fontSize, Color.White);
        }
    }
}
