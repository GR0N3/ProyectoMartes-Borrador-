using System;
using System.Drawing;
using System.Windows.Forms;

namespace EngineGDI
{
    // Acciones posibles desde la pantalla de game over.
    internal enum GameOverAction
    {
        None = 0,
        Retry = 1,
        QuitToMainMenu = 2
    }

    // Controla la pantalla de game over (tabla + botones + estrella de selección).
    // Este controlador solo administra UI y selección; reiniciar/cambiar escena lo hace la escena Game.
    internal class GameOverController
    {
        private readonly int screenWidth;
        private readonly int screenHeight;

        private readonly string tableSprite = "Textures/UI/GameOver/GameOverTable.png";
        private readonly string retryButtonSprite = "Textures/UI/GameOver/RetryButtom.png";
        private readonly string quitButtonSprite = "Textures/UI/GameOver/QUITButtom.png";
        private readonly string starSprite = "Textures/UI/HUD/Estrella.png";

        private float tableScale = 1f;
        private float tableCenterX;
        private float tableCenterY;
        private float tableWidthScaled;
        private float tableHeightScaled;

        private float buttonScale = 1f;
        private float retryButtonCenterX;
        private float retryButtonCenterY;
        private float quitButtonCenterX;
        private float quitButtonCenterY;
        private float buttonWidthScaled;
        private float buttonHeightScaled;

        private float starScale = 1f;
        private float starOffsetFromButton = 30f;
        private float starAngle = 0f;
        private float starRotationSpeed = 180f;

        private int selectedIndex = 0;

        // Acción solicitada por el jugador al confirmar con Enter.
        // La escena que lo usa debe leerla y reaccionar (Retry / Quit).
        public GameOverAction RequestedAction { get; private set; } = GameOverAction.None;

        // Construye la pantalla de game over y precalcula escalas/posiciones basadas en la resolución.
        public GameOverController(int screenWidth, int screenHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;

            tableCenterX = screenWidth * 0.5f;
            tableCenterY = screenHeight * 0.60f;

            using (var tableImg = Image.FromFile(tableSprite))
            {
                float desiredTableWidth = screenWidth * 0.55f;
                float desiredTableHeight = screenHeight * 0.65f;
                float scaleX = desiredTableWidth / tableImg.Width;
                float scaleY = desiredTableHeight / tableImg.Height;
                tableScale = Math.Min(scaleX, scaleY);

                tableWidthScaled = tableImg.Width * tableScale;
                tableHeightScaled = tableImg.Height * tableScale;
            }

            using (var btnImg = Image.FromFile(retryButtonSprite))
            {
                float desiredButtonWidth = tableWidthScaled * 0.62f;
                buttonScale = desiredButtonWidth / btnImg.Width;
                buttonWidthScaled = btnImg.Width * buttonScale;
                buttonHeightScaled = btnImg.Height * buttonScale;
            }

            float yCenter = tableCenterY;
            float yRetry = yCenter - (buttonHeightScaled * 0.2f);
            float yQuit = yCenter + (buttonHeightScaled * 0.8f);

            retryButtonCenterX = tableCenterX;
            retryButtonCenterY = yRetry;
            quitButtonCenterX = tableCenterX;
            quitButtonCenterY = yQuit;

            using (var starImg = Image.FromFile(starSprite))
            {
                float desiredStarHeight = buttonHeightScaled * 0.25f;
                starScale = desiredStarHeight / starImg.Height;
                starOffsetFromButton = (buttonWidthScaled * 0.5f) + (starImg.Width * starScale * 0.3f);
            }
        }

        // Maneja la navegación (↑/↓ o W/S) y la confirmación (Enter).
        // Al confirmar, setea RequestedAction.
        public void Input()
        {
            RequestedAction = GameOverAction.None;

            if (Engine.OnKeyDown(Keys.Up) || Engine.OnKeyDown(Keys.W) || Engine.OnKeyDown(Keys.Down) || Engine.OnKeyDown(Keys.S))
                selectedIndex = (selectedIndex + 1) % 2;

            if (Engine.OnKeyDown(Keys.Enter))
            {
                // Feedback de UI: la pantalla de game over usa el mismo SFX de botón al confirmar.
                AudioManager.Instance.PlayButtonEffect();
                if (selectedIndex == 0)
                    RequestedAction = GameOverAction.Retry;
                else
                    RequestedAction = GameOverAction.QuitToMainMenu;
            }
        }

        // Actualiza la animación de rotación de la estrella.
        public void Update(float deltaTime)
        {
            starAngle += deltaTime * starRotationSpeed;
            if (starAngle >= 360f) starAngle -= 360f;
        }

        // Dibuja la tabla de game over, los botones y el indicador de selección.
        public void Render()
        {
            DrawCentered(tableSprite, tableCenterX, tableCenterY, tableScale, tableScale);

            DrawCentered(retryButtonSprite, retryButtonCenterX, retryButtonCenterY, buttonScale, buttonScale);
            DrawCentered(quitButtonSprite, quitButtonCenterX, quitButtonCenterY, buttonScale, buttonScale);

            float selectedY = selectedIndex == 0 ? retryButtonCenterY : quitButtonCenterY;
            DrawStar(tableCenterX, selectedY);
        }

        // Dibuja la estrella al costado del botón actualmente seleccionado.
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
