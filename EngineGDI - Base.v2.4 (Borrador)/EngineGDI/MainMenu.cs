using System.Drawing;
using System.Windows.Forms;

namespace EngineGDI
{
    // Escena del menú principal.
    // Permite iniciar el juego o cerrar el programa.
    internal class MainMenu : IScene
    {
        private readonly int screenWidth;
        private readonly int screenHeight;

        private readonly string backgroundSprite = "Textures/UI/Menu/BackGround/FondoMenu.png";
        private readonly string startButtonSprite = "Textures/UI/Menu/Buttons/STARTButtom.png";
        private readonly string quitButtonSprite = "Textures/UI/Menu/Buttons/QUITButtom.png";
        private readonly string starSprite = "Textures/UI/HUD/Estrella.png";

        private float backgroundScaleX = 1f;
        private float backgroundScaleY = 1f;

        private float buttonScale = 1f;
        private float startButtonCenterX;
        private float startButtonCenterY;
        private float quitButtonCenterX;
        private float quitButtonCenterY;

        private float starScale = 1f;
        private float starOffsetFromButton = 30f;
        private float starAngle = 0f;
        private float starRotationSpeed = 180f;

        private int selectedIndex = 0;

        // Crea el menú principal y calcula escalas/posiciones basadas en la resolución.
        public MainMenu(int screenWidth, int screenHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;

            using (var bg = Image.FromFile(backgroundSprite))
            {
                backgroundScaleX = screenWidth / (float)bg.Width;
                backgroundScaleY = screenHeight / (float)bg.Height;
            }

            float desiredButtonWidth = screenWidth * 0.35f;
            float buttonHeight = 0f;
            using (var startImg = Image.FromFile(startButtonSprite))
            {
                buttonScale = desiredButtonWidth / startImg.Width;
                buttonHeight = startImg.Height * buttonScale;
            }

            startButtonCenterX = screenWidth * 0.5f;
            startButtonCenterY = screenHeight * 0.45f;
            quitButtonCenterX = screenWidth * 0.5f;
            quitButtonCenterY = startButtonCenterY + buttonHeight * 1.6f;

            using (var starImg = Image.FromFile(starSprite))
            {
                float desiredStarHeight = buttonHeight * 0.55f;
                starScale = desiredStarHeight / starImg.Height;
                starOffsetFromButton = (desiredButtonWidth * 0.5f) + (starImg.Width * starScale * 0.7f);
            }

            // Música del menú: se reproduce en loop mientras esta escena esté activa.
            AudioManager.Instance.PlayMenuMusic();
        }

        // Maneja la navegación (↑/↓ o W/S) y la confirmación (Enter).
        public void Input()
        {
            if (Engine.OnKeyDown(Keys.Up) || Engine.OnKeyDown(Keys.W))
                selectedIndex = (selectedIndex - 1 + 2) % 2;
            else if (Engine.OnKeyDown(Keys.Down) || Engine.OnKeyDown(Keys.S))
                selectedIndex = (selectedIndex + 1) % 2;

            if (Engine.OnKeyDown(Keys.Enter))
            {
                // Feedback de UI: al confirmar con Enter se reproduce el efecto de botón.
                AudioManager.Instance.PlayButtonEffect();
                if (selectedIndex == 0)
                    SceneManager.Instance.ChangeScene(new Game(screenWidth, screenHeight));
                else
                    Engine.Window.Close();
            }
        }

        // Actualiza la animación de rotación de la estrella.
        public void Update(float deltaTime)
        {
            starAngle += deltaTime * starRotationSpeed;
            if (starAngle >= 360f) starAngle -= 360f;
        }

        // Dibuja el fondo, botones y el indicador de selección.
        public void Render()
        {
            Engine.Draw(backgroundSprite, 0f, 0f, backgroundScaleX, backgroundScaleY);

            DrawCentered(startButtonSprite, startButtonCenterX, startButtonCenterY, buttonScale, buttonScale);
            DrawCentered(quitButtonSprite, quitButtonCenterX, quitButtonCenterY, buttonScale, buttonScale);

            float selectedY = selectedIndex == 0 ? startButtonCenterY : quitButtonCenterY;
            DrawStar(startButtonCenterX, selectedY);
        }

        // Dibuja la estrella de selección al costado del botón seleccionado.
        private void DrawStar(float buttonCenterX, float buttonCenterY)
        {
            float leftX = buttonCenterX - starOffsetFromButton;
            float rightX = buttonCenterX + starOffsetFromButton;

            Engine.Draw(starSprite, leftX, buttonCenterY, starScale, starScale, starAngle, 0.5f, 0.5f);
            Engine.Draw(starSprite, rightX, buttonCenterY, starScale, starScale, starAngle, 0.5f, 0.5f);
        }

        // Dibuja un sprite centrado usando pivote (0.5, 0.5).
        private static void DrawCentered(string sprite, float centerX, float centerY, float scaleX, float scaleY)
        {
            Engine.Draw(sprite, centerX, centerY, scaleX, scaleY, 0f, 0.5f, 0.5f);
        }
    }
}
