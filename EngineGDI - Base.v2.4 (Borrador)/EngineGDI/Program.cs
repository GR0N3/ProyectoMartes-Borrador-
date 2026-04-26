using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;


namespace EngineGDI
{
    static class Program
    {
        // Delta time:
        // tiempo (en segundos) transcurrido entre un frame y el siguiente.
        public static float deltaTime;

        // Tiempo (en segundos) en el que se midió el último frame.
        // Se usa con Stopwatch para calcular deltaTime con más precisión que DateTime.
        static double lastFrameTimeSeconds = 0.0;

        // Resolución de la ventana.
        public static int SCREEN_WIDTH = 1024;
        public static int SCREEN_HEIGHT = 768;

        // Duración objetivo de un frame a 60 FPS (~0.016666... segundos).
        static readonly double targetFrameSeconds = 1.0 / 60.0;

        // Reloj de alta resolución para medir tiempos entre frames.
        static readonly Stopwatch stopwatch = Stopwatch.StartNew();

        // Punto de entrada principal para la aplicación.
        // Inicializa el motor y delega el loop principal al SceneManager.
        [STAThread]
        static void Main()
        {
            Engine.Initialize("IERVA ENGINE", SCREEN_WIDTH, SCREEN_HEIGHT, false);

            // Carga una fuente desde archivo para los textos de UI.
            Engine.SetUIFontFromFile("Fonts/pixel_lcd_7.ttf", FontStyle.Regular);

            // Comienza en el menú principal.
            SceneManager.Instance.ChangeScene(new MainMenu(SCREEN_WIDTH, SCREEN_HEIGHT));

            // Inicializa el "inicio del frame" para la primera medición de deltaTime.
            lastFrameTimeSeconds = stopwatch.Elapsed.TotalSeconds;

            while (Engine.IsWindowOpen)
            {
                #region Engine Window Control
                Engine.UpdateWindow();
                #endregion

                calcDeltatime();

                SceneManager.Instance.Input();
                SceneManager.Instance.Update(deltaTime);
                SceneManager.Instance.Render();


                #region Engine Window Control
                Engine.Window.Invalidate();
                #endregion

                // Limita el loop para no correr más rápido que 60 FPS.
                // Nota: si el render/lógica tardan más que targetFrameSeconds, no se puede sostener 60 y el juego bajará de FPS igual.
                LimitTo60Fps();
            }
        }

        // Calcula el deltaTime (segundos entre frames) usando el reloj del sistema.
        static void calcDeltatime()
        {
            double nowSeconds = stopwatch.Elapsed.TotalSeconds;
            double frameSeconds = nowSeconds - lastFrameTimeSeconds;

            // Protección por si el reloj diera un valor extraño (no debería pasar, pero evita números inválidos).
            if (frameSeconds < 0.0) frameSeconds = 0.0;

            // Evita saltos gigantes de deltaTime (por ejemplo al minimizar la ventana o si el proceso se frena un rato).
            // Con esto, la física/movimiento no "teletransporta" objetos al volver.
            if (frameSeconds > 0.25) frameSeconds = 0.25;

            deltaTime = (float)frameSeconds;
            lastFrameTimeSeconds = nowSeconds;
        }

        static void LimitTo60Fps()
        {
            double nowSeconds = stopwatch.Elapsed.TotalSeconds;

            // Tiempo transcurrido desde el inicio del frame actual.
            double elapsedSinceFrameStart = nowSeconds - lastFrameTimeSeconds;

            // Cuánto falta para completar 1 frame de 60 FPS.
            double remaining = targetFrameSeconds - elapsedSinceFrameStart;

            if (remaining <= 0.0) return;

            // Duerme lo que falta en milisegundos (aprox). Sleep no es exacto al 100%, por eso luego se ajusta con busy-wait.
            int sleepMs = (int)(remaining * 1000.0);
            if (sleepMs > 0)
                Thread.Sleep(sleepMs);

            // Ajuste fino: espera activa hasta que se cumpla exactamente el targetFrameSeconds.
            while (stopwatch.Elapsed.TotalSeconds - lastFrameTimeSeconds < targetFrameSeconds)
            {
            }
        }

    }
}
