using System.Drawing;
using System.Windows.Forms;

namespace EngineGDI
{
    // Escena principal del juego.
    // Maneja: player, balas, asteroides, fondo y HUD; además controla pausa y game over.
    internal class Game : IScene
    {
        private readonly int screenWidth;
        private readonly int screenHeight;

        private Player player;
        private BulletPool bulletPool;
        private BackgroundManager backgroundManager;
        private AsteroidPool asteroidPool;
        private UIManager uiManager;

        private PauseController pauseController;
        private bool isPaused;

        private GameOverController gameOverController;
        private bool isGameOver;

        private float cadencia = 0.3f;
        private float tiempoUltimoDisparo = 0f;

        // Construye la escena del juego e instancia los sistemas principales (player, pools, background, UI).
        public Game(int screenWidth, int screenHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;

            uiManager = new UIManager(screenWidth);
            player = new Player("Textures/Player/Player.png", 20, screenHeight / 2f, 200f);
            CenterPlayerInPlayableArea();

            bulletPool = new BulletPool("Textures/Objects/Bala/Bullet.png", 10, 500f);
            backgroundManager = new BackgroundManager(screenWidth);
            asteroidPool = AsteroidSpawner.CrearPoolCon5Spawners(
                anchoPantalla: screenWidth,
                altoPantalla: screenHeight,
                spriteAsteroide: "Textures/Objects/Asteroide/Asteroid_idle.png",
                yMin: uiManager.HudHeight + 10f);

            // Música del gameplay: se reproduce en loop mientras esta escena esté activa.
            AudioManager.Instance.PlayGameMusic();
        }

        // Procesa input según el estado:
        // - GameOver: input del GameOverController
        // - Pausa: input del PauseController
        // - Jugando: movimiento del player, disparo y apertura de pausa con ESC
        public void Input()
        {
            if (isGameOver)
            {
                gameOverController?.Input();
                return;
            }

            if (isPaused)
            {
                pauseController?.Input();
                return;
            }

            if (Engine.OnKeyDown(Keys.Escape))
            {
                isPaused = true;
                pauseController = new PauseController(screenWidth, screenHeight);
                return;
            }

            player.Input(Program.deltaTime);

            tiempoUltimoDisparo -= Program.deltaTime;
            if (Engine.IsKeyDown(Keys.Space) && tiempoUltimoDisparo <= 0f)
            {
                float spawnX = player.posX + 100f;
                float spawnY = player.posY + 10f;
                bulletPool.TrySpawn(spawnX, spawnY);
                // Feedback al disparar: efecto de láser.
                AudioManager.Instance.PlayLaserEffect();
                tiempoUltimoDisparo = cadencia;
            }
        }

        // Actualiza la escena.
        // Si está pausado o en game over, se actualiza solo el overlay y no avanza la simulación.
        public void Update(float deltaTime)
        {
            float gameDeltaTime = (isPaused || isGameOver) ? 0f : deltaTime;

            if (isPaused)
            {
                pauseController.Update(deltaTime);
                ResolvePauseAction();
                return;
            }

            if (isGameOver)
            {
                gameOverController.Update(deltaTime);
                ResolveGameOverAction();
                return;
            }

            backgroundManager.Update(gameDeltaTime);
            player.Update(gameDeltaTime, uiManager.HudHeight, screenHeight);
            bulletPool.Update(gameDeltaTime, screenWidth);
            asteroidPool.Update(gameDeltaTime);

            if (bulletPool.TryHitAsteroids(asteroidPool.Asteroids))
            {
                // Cuando una bala impacta un asteroide, reproducimos el SFX de hit.
                AudioManager.Instance.PlayHitEffect();
                uiManager.AddScore(100);
            }

            var collidingAsteroid = GetCollidingAsteroid();
            if (collidingAsteroid != null)
            {
                collidingAsteroid.Deactivate();
                player.DisableCollider(0.5f);

                if (player.TryTakeDamage(0.5f))
                {
                    uiManager.RemoveLife(1);
                    if (uiManager.Lives <= 0)
                        TriggerGameOver();
                }
            }
        }

        // Renderiza el juego y, si corresponde, el overlay de pausa o game over.
        public void Render()
        {
            backgroundManager.Render();
            if (!isGameOver)
                player.Render();
            asteroidPool.Render();
            bulletPool.Render();
            uiManager.Render();

            if (isPaused)
                pauseController.Render();

            if (isGameOver)
                gameOverController.Render();
        }

        // Ejecuta la acción solicitada por el menú de pausa (Continue / Quit).
        private void ResolvePauseAction()
        {
            if (pauseController == null) return;

            if (pauseController.RequestedAction == PauseAction.Continue)
            {
                isPaused = false;
                pauseController = null;
            }
            else if (pauseController.RequestedAction == PauseAction.QuitToMainMenu)
            {
                SceneManager.Instance.ChangeScene(new MainMenu(screenWidth, screenHeight));
            }
        }

        // Ejecuta la acción solicitada por la pantalla de game over (Retry / Quit).
        private void ResolveGameOverAction()
        {
            if (gameOverController == null) return;

            if (gameOverController.RequestedAction == GameOverAction.Retry)
            {
                SceneManager.Instance.ChangeScene(new Game(screenWidth, screenHeight));
            }
            else if (gameOverController.RequestedAction == GameOverAction.QuitToMainMenu)
            {
                SceneManager.Instance.ChangeScene(new MainMenu(screenWidth, screenHeight));
            }
        }

        // Activa el estado de game over y crea el controlador de la UI correspondiente.
        private void TriggerGameOver()
        {
            isGameOver = true;
            player.DisableCollider(9999f);
            gameOverController = new GameOverController(screenWidth, screenHeight);
        }

        // Devuelve el primer asteroide con el que el player está colisionando (AABB), o null si no hay colisión.
        private Asteroid GetCollidingAsteroid()
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

        // Chequeo de colisión AABB (rectángulos sin rotación).
        private static bool IsBoxColliding(RectangleF a, RectangleF b)
        {
            return a.Left < b.Right &&
                   a.Right > b.Left &&
                   a.Top < b.Bottom &&
                   a.Bottom > b.Top;
        }

        // Reposiciona el player al centro vertical del área jugable (debajo del HUD).
        private void CenterPlayerInPlayableArea()
        {
            float minY = uiManager.HudHeight;
            float playerHeight = player.SpriteSize.Y * player.Transform.Scale.Y * player.ColliderScale.Y;
            float playableHeight = screenHeight - minY;
            player.posY = minY + (playableHeight - playerHeight) / 2f;
        }
    }
}
