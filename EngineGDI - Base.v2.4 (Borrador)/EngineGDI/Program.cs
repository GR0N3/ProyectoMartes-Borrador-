
using System;
using System.Drawing;
using System.Windows.Forms;


namespace EngineGDI
{
    static class Program
    {
        // Delta time
        public static float deltaTime;
        static DateTime lastFrameTime = DateTime.Now;

        // mostrar debug
        public static bool showDebug = true;
        public static string currentMsg = "";

        public static int SCREEN_WIDTH = 1024;
        public static int SCREEN_HEIGHT = 768;
        private static Player player;
        private static Asteroid asteroid;
        private static BulletPool bulletPool;

        // Tiempo mínimo entre disparos (en segundos)
        private static float cadencia = 0.3f;
        private static float tiempoUltimoDisparo = 0f;

        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Engine.Initialize("IERVA ENGINE", SCREEN_WIDTH, SCREEN_HEIGHT, false);

            player = new Player("Textures/Player/Player.png", 20, SCREEN_HEIGHT / 2, 200f);

            asteroid = new Asteroid("Textures/Objects/Asteroide/Asteroid_idle.png", 800, 100, 100);

            // Pool de balas:
            // - 40 = cantidad máxima de balas simultáneas
            // - 500 = velocidad en X de cada bala
            bulletPool = new BulletPool("Textures/Objects/Bala/Bullet.png", 10, 500f);


            while (Engine.IsWindowOpen)
            {
                #region Engine Window Control
                Engine.UpdateWindow();
                #endregion

                calcDeltatime();

                Input();
                Update();
                Render();


                #region Engine Window Control
                //Engine.Clear(Color.Black);
                currentMsg = deltaTime.ToString();
                // mensajes de debug
                if (showDebug)
                {
                    Engine.ClearDebug();
                    Engine.DebugLog(currentMsg);

                }
                Engine.Window.Invalidate();
                #endregion
            }
        }

        //Calculo de DeltaTime
        static void calcDeltatime()
        {
            TimeSpan deltaSpan = DateTime.Now - lastFrameTime;
            deltaTime = (float)deltaSpan.TotalSeconds;
            lastFrameTime = DateTime.Now;
        }


        static void Input()
        {
            player.Input(deltaTime);

            tiempoUltimoDisparo -= deltaTime;

            if (Engine.IsKeyDown(Keys.Space) && tiempoUltimoDisparo <= 0f)
            {
                // Instancia la bala en el borde derecho del sprite del player, mitad del Y
                float spawnX = player.posX + 100f;
                float spawnY = player.posY + 10f;
                // Si la pool está agotada, TrySpawn devuelve false y no dispara (sin crear objetos nuevos).
                bulletPool.TrySpawn(spawnX, spawnY);
                tiempoUltimoDisparo = cadencia;
            }
        }

        static void Update()
        {
            player.Update(deltaTime);
            asteroid.Update(deltaTime);

            // Mueve balas activas y devuelve al pool las que salen de pantalla.
            bulletPool.Update(deltaTime, SCREEN_WIDTH);
        }

        static void Render()
        {
            Engine.Draw("Textures/BackGrounds/BackGround.png", 0, 0);
            player.Render();
            asteroid.Render();
            bulletPool.Render();
        }

    }
}
