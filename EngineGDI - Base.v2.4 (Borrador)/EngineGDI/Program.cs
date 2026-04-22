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

        public static int SCREEN_WIDTH = 1024;
        public static int SCREEN_HEIGHT = 768;
        private static Player player;
        private static BulletPool bulletPool;
        private static BackgroundManager backgroundManager;
        private static AsteroidPool asteroidPool;
        private static UIManager uiManager;

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

            // Carga una fuente desde archivo para los textos de UI.
            Engine.SetUIFontFromFile("Fonts/pixel_lcd_7.ttf", FontStyle.Regular);

            // Inicializa UI y player. El player se reposiciona para quedar centrado en el área jugable (debajo de la HUD).
            uiManager = new UIManager(SCREEN_WIDTH);
            player = new Player("Textures/Player/Player.png", 20, SCREEN_HEIGHT / 2, 200f);
            CenterPlayerInPlayableArea();

            // Pool de balas:
            // - 40 = cantidad máxima de balas simultáneas
            // - 500 = velocidad en X de cada bala
            bulletPool = new BulletPool("Textures/Objects/Bala/Bullet.png", 10, 500f);
            backgroundManager = new BackgroundManager(SCREEN_WIDTH);

            // Pool de asteroides con 5 spawners a lo largo del eje Y, evitando la zona de HUD.
            asteroidPool = AsteroidSpawner.CrearPoolCon5Spawners(
                anchoPantalla: SCREEN_WIDTH,
                altoPantalla: SCREEN_HEIGHT,
                spriteAsteroide: "Textures/Objects/Asteroide/Asteroid_idle.png",
                yMin: uiManager.HudHeight + 10f);


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
                // Spawnea la bala cercana al player (ajuste manual de offsets).
                float spawnX = player.posX + 100f;
                float spawnY = player.posY + 10f;

                // Dispara usando la pool (reutiliza balas).
                bulletPool.TrySpawn(spawnX, spawnY);
                tiempoUltimoDisparo = cadencia;
            }
        }

        static void Update()
        {
            backgroundManager.Update(deltaTime);
            player.Update(deltaTime, uiManager.HudHeight, SCREEN_HEIGHT);

            // Se actualizan balas antes que colisiones para usar posiciones recientes.
            bulletPool.Update(deltaTime, SCREEN_WIDTH);
            asteroidPool.Update(deltaTime);
            if (bulletPool.TryHitAsteroids(asteroidPool.Asteroids))
                uiManager.AddScore(100);

            // Si el player colisiona con un asteroide:
            // - desactiva el asteroide (desaparece)
            // - deshabilita collider del player por 0.5s (evita colisiones múltiples)
            // - aplica daño/vida (con blink)
            var collidingAsteroid = GetCollidingAsteroid();
            if (collidingAsteroid != null)
            {
                collidingAsteroid.Deactivate();
                player.DisableCollider(0.5f);

                if (player.TryTakeDamage(0.5f))
                    uiManager.RemoveLife(1);
            }
        }

        // Renderiza el frame:
        // orden de capas: fondo → player/objetos → UI encima.
        static void Render()
        {
            backgroundManager.Render();
            player.Render();
            asteroidPool.Render();
            bulletPool.Render();
            uiManager.Render();
        }

        // Detecta colisión player ↔ asteroides:
        // Recorre el pool y devuelve el primer asteroide con el que colisiona (si existe).
        static Asteroid GetCollidingAsteroid()
        {
            RectangleF playerCollider = player.GetCollider();
            if (playerCollider.IsEmpty) return null;

            var asteroids = asteroidPool.Asteroids;
            for (int i = 0; i < asteroids.Count; i++)
            {
                var a = asteroids[i];
                if (a == null || !a.IsAlive || a.IsDestroying) continue;
                if (IsBoxColliding(playerCollider, a.GetCollider()))
                    return a;
            }

            return null;
        }

        // Colisión AABB (Axis-Aligned Bounding Box) entre 2 rectángulos sin rotación.
        static bool IsBoxColliding(RectangleF a, RectangleF b)
        {
            return a.Left < b.Right &&
                   a.Right > b.Left &&
                   a.Top < b.Bottom &&
                   a.Bottom > b.Top;
        }

        // Centra el player en el área jugable (debajo de la HUD):
        // - minY = HudHeight
        // - calcula altura jugable y ubica al player en el centro vertical
        static void CenterPlayerInPlayableArea()
        {
            float minY = uiManager.HudHeight;
            float playerHeight = player.SpriteSize.Y * player.Transform.Scale.Y * player.ColliderScale.Y;
            float playableHeight = SCREEN_HEIGHT - minY;
            player.posY = minY + (playableHeight - playerHeight) / 2f;
        }

    }
}
