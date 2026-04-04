
using System;
using System.Drawing;
using System.Windows.Forms;


namespace EngineGDI
{
    static class Program
    {
        // mostrar debug
        public static bool showDebug = true;
        public static string currentMsg = "";
        
        public static float deltaTime = 0f;
        public static DateTime startTime;
        private static float lastFrameTime = 0f;


        public static int SCREEN_WIDTH = 1024;
        public static int SCREEN_HEIGHT = 780;
        private static player player;
        private static Asteroid asteroid;

        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Engine.Initialize("IERVA ENGINE", SCREEN_WIDTH, SCREEN_HEIGHT, false);

            player = new player("Textures/Player/Player.png", 20, SCREEN_HEIGHT / 2, 200f);

            asteroid = new Asteroid("Textures/Objetos/Asteroide/Asteroid_idle.png", 800, 100, 100);

            startTime = DateTime.Now;


            while (Engine.IsWindowOpen)
            {
                //Tiempo actual en segundos desde el inicio del programa
                var currentTime = (float)(DateTime.Now - startTime).TotalSeconds;
                //Tiempo que tardó el último frame
                deltaTime = currentTime - lastFrameTime;
                //Tiempo de ultimo frame actualizado al tiempo actual
                lastFrameTime = currentTime;

                #region Engine Window Control
                Engine.UpdateWindow();
                #endregion
                Input();
                Update();
                Render();
                #region Engine Window Control
                Engine.Clear(Color.Black);
                // mensajes de debug
                if (showDebug)
                {
                    Engine.ClearDebug();
                    Engine.DebugLog(currentMsg);

                }
                Engine.Window.Invalidate();
                #endregion
            }

            // aqui se pueden agregar controles de input
            void Input()
            {
                player.Input(deltaTime);
            }

            void Update()
            {
                player.Update(deltaTime);
                asteroid.Update(deltaTime);
            }

            void Render()
            {
                Engine.Draw("Textures/Fondo/BackGround.png", 0, 0, 0.5f, 0.5f);
                player.Render();
                asteroid.Render();
            }
        }

       
    }
}
