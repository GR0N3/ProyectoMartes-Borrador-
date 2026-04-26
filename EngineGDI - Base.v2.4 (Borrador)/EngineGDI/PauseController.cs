using System;
using System.Drawing;
using System.Windows.Forms;

namespace EngineGDI
{
    // Acciones posibles desde el menú de pausa.
    internal enum PauseAction
    {
        None = 0,
        Continue = 1,
        QuitToMainMenu = 2
    }

    // Controla el menú de pausa (tabla + botones + estrella de selección).
    // Este controlador solo administra UI y selección; el pausar/reanudar lo decide la escena Game.
    internal class PauseController
    {
        private readonly int screenWidth;
        private readonly int screenHeight;

        private readonly string tableSprite = "Textures/UI/Pause/PauseTable.png";
        private readonly string continueButtonSprite = "Textures/UI/Pause/ContinueButtom.png";
        private readonly string quitButtonSprite = "Textures/UI/Pause/QUITButtom.png";
        private readonly string starSprite = "Textures/UI/HUD/Estrella.png";

        private float tableScale = 1f;
        private float tableCenterX;
        private float tableCenterY;
        private float tableWidthScaled;
        private float tableHeightScaled;

        private float buttonScale = 1f;
        private float continueButtonCenterX;
        private float continueButtonCenterY;
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
        // La escena que lo usa debe leerla y reaccionar (Continue / Quit).
        public PauseAction RequestedAction { get; private set; } = PauseAction.None;

        // Construye el menú de pausa y precalcula escalas/posiciones basadas en la resolución.
        public PauseController(int screenWidth, int screenHeight)
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

            using (var btnImg = Image.FromFile(continueButtonSprite))
            {
                float desiredButtonWidth = tableWidthScaled * 0.62f;
                buttonScale = desiredButtonWidth / btnImg.Width;
                buttonWidthScaled = btnImg.Width * buttonScale;
                buttonHeightScaled = btnImg.Height * buttonScale;
            }

            float yCenter = tableCenterY;
            float yContinue = yCenter - (buttonHeightScaled * 0.2f);
            float yQuit = yCenter + (buttonHeightScaled * 0.8f);

            continueButtonCenterX = tableCenterX;
            continueButtonCenterY = yContinue;
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
            RequestedAction = PauseAction.None;

            if (Engine.OnKeyDown(Keys.Up) || Engine.OnKeyDown(Keys.W) || Engine.OnKeyDown(Keys.Down) || Engine.OnKeyDown(Keys.S))
                selectedIndex = (selectedIndex + 1) % 2;

            if (Engine.OnKeyDown(Keys.Enter))
            {
                if (selectedIndex == 0)
                    RequestedAction = PauseAction.Continue;
                else
                    RequestedAction = PauseAction.QuitToMainMenu;
            }
        }

        // Actualiza la animación de rotación de la estrella.
        public void Update(float deltaTime)
        {
            starAngle += deltaTime * starRotationSpeed;
            if (starAngle >= 360f) starAngle -= 360f;
        }

        // Dibuja la tabla de pausa, los botones y el indicador de selección.
        public void Render()
        {
            DrawCentered(tableSprite, tableCenterX, tableCenterY, tableScale, tableScale);

            DrawCentered(continueButtonSprite, continueButtonCenterX, continueButtonCenterY, buttonScale, buttonScale);
            DrawCentered(quitButtonSprite, quitButtonCenterX, quitButtonCenterY, buttonScale, buttonScale);

            float selectedY = selectedIndex == 0 ? continueButtonCenterY : quitButtonCenterY;
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
