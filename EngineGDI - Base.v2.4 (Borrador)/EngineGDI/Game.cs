using System;
using System.Drawing;
using System.Windows.Forms;

namespace EngineGDI
{
    // Escena principal del juego.
    // Maneja: player, balas, enemigos, fondo y HUD; además controla pausa y game over.
    internal class Game : IScene
    {
        private enum NivelJuego
        {
            Nivel1,
            Nivel2
        }

        private const int PuntajeObjetivoNivel2 = 2500;
        private readonly int screenWidth;
        private readonly int screenHeight;
        private readonly NivelJuego nivelActual;

        private Player player;
        private BulletPool bulletPool;
        private BackgroundManager backgroundManager;
        private EnemyPool enemyPool;
        private UIManager uiManager;

        private PauseController pauseController;
        private bool isPaused;

        private GameOverController gameOverController;
        private bool isGameOver;

        private float cadencia = 0.3f;
        private float tiempoUltimoDisparo = 0f;
        private bool shouldAdvanceToLevel2;

        // Construye la escena del juego e instancia los sistemas principales (player, pools, background, UI).
        public Game(int screenWidth, int screenHeight)
            : this(screenWidth, screenHeight, NivelJuego.Nivel1, 10, 0)
        {
        }

        private Game(int screenWidth, int screenHeight, NivelJuego nivelActual, int initialLives, int initialScore)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.nivelActual = nivelActual;

            uiManager = new UIManager(screenWidth, initialLives: initialLives, initialScore: initialScore);
            player = new Player("Textures/Player/Player.png", 20, screenHeight / 2f, 200f);
            CenterPlayerInPlayableArea();

            // Pool unificado de balas (Jugador y Enemigos)
            bulletPool = new BulletPool(
                playerSprite: "Textures/Objects/Bala/Bullet.png",
                enemySprite: "Textures/Objects/Bala/Bullet.png",
                poolSize: 15,
                playerSpeed: 500f,
                enemySpeed: 300f
            );

            player.BindShooting((x, y) => bulletPool.TrySpawn(BulletType.Player, x, y));
            player.OnLifeLost += HandlePlayerLifeLost;

            backgroundManager = new BackgroundManager(screenWidth, fondoSprite: GetBackgroundSpriteForCurrentLevel());

            // Pool unificado de enemigos (Asteroides y Naves Enemigas)
            enemyPool = EnemyPool.CrearPoolCon5Spawners(
                anchoPantalla: screenWidth,
                altoPantalla: screenHeight,
                spriteAsteroide: "Textures/Objects/Asteroide/Asteroid_idle.png",
                spriteNaveRoja: "Textures/Enemies/RedEnemy.png",
                spriteNaveAzul: "Textures/Enemies/BlueEnemy.png",
                onEnemyShoot: (x, y, bulletType) => bulletPool.TrySpawn(bulletType, x, y),
                onEnemyDestroyed: HandleEnemyDestroyed,
                yMin: uiManager.HudHeight + 10f,
                asteroidScaleMin: 0.06f,
                asteroidScaleMax: 0.10f,
                incluirNaveRoja: nivelActual == NivelJuego.Nivel1,
                incluirNaveAzul: nivelActual == NivelJuego.Nivel2
            );

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

            if (CanAdvanceToLevel2() && IsLevel2ShortcutPressed())
            {
                shouldAdvanceToLevel2 = true;
                return;
            }

            player.Input(Program.deltaTime);

            tiempoUltimoDisparo -= Program.deltaTime;
            if (Engine.IsKeyDown(Keys.Space) && tiempoUltimoDisparo <= 0f)
            {
                player.Shoot();
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

            if (shouldAdvanceToLevel2)
            {
                AdvanceToLevel2();
                return;
            }

            backgroundManager.Update(gameDeltaTime);
            player.Update(gameDeltaTime, uiManager.HudHeight, screenHeight);
            bulletPool.Update(gameDeltaTime, screenWidth);
            enemyPool.Update(gameDeltaTime);

            // Colisión: Balas del jugador vs enemigos
            bulletPool.TryHitEnemies(enemyPool.Enemies);

            // Colisión: Balas enemigas vs jugador
            if (bulletPool.TryHitPlayer(player))
            {
                player.DisableCollider(0.5f);
                player.TakeDamage(1f);
            }

            // Colisión física: Enemigo vs jugador
            var collidingEnemy = GetCollidingEnemy();
            if (collidingEnemy != null)
            {
                collidingEnemy.Deactivate();
                player.DisableCollider(0.5f);
                player.TakeDamage(collidingEnemy.Dano);
            }

            if (shouldAdvanceToLevel2)
                AdvanceToLevel2();
        }

        private void HandlePlayerLifeLost(int livesLost)
        {
            uiManager.RemoveLife(livesLost);
            if (uiManager.Lives <= 0)
                TriggerGameOver();
        }

        private void HandleEnemyDestroyed(EnemyEntity enemy)
        {
            AudioManager.Instance.PlayHitEffect();
            uiManager.AddScore(100);
            GameManager.Instance.TryUpdateHighScore(uiManager.Score);

            if (CanAdvanceToLevel2() && uiManager.Score >= PuntajeObjetivoNivel2)
                shouldAdvanceToLevel2 = true;
        }

        // Renderiza el juego y, si corresponde, el overlay de pausa o game over.
        public void Render()
        {
            backgroundManager.Render();
            if (!isGameOver)
                player.Render();
            enemyPool.Render();
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
                SceneManager.Instance.ChangeScene(new Game(screenWidth, screenHeight, nivelActual, 10, 0));
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

        // Devuelve el primer enemigo con el que el player está colisionando (AABB), o null si no hay colisión.
        private EnemyEntity GetCollidingEnemy()
        {
            RectangleF playerCollider = player.GetCollider();
            if (playerCollider.IsEmpty) return null;

            var enemies = enemyPool.Enemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                var e = enemies[i];
                if (e == null || !e.IsAlive || e.IsDestroying) continue;
                if (IsBoxColliding(playerCollider, e.GetCollider()))
                    return e;
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

        private bool CanAdvanceToLevel2()
        {
            return nivelActual == NivelJuego.Nivel1;
        }

        private void AdvanceToLevel2()
        {
            shouldAdvanceToLevel2 = false;
            SceneManager.Instance.ChangeScene(new Game(screenWidth, screenHeight, NivelJuego.Nivel2, uiManager.Lives, uiManager.Score));
        }

        private bool IsLevel2ShortcutPressed()
        {
            bool controlDown = Engine.IsKeyDown(Keys.ControlKey) ||
                               Engine.IsKeyDown(Keys.LControlKey) ||
                               Engine.IsKeyDown(Keys.RControlKey);

            return controlDown && Engine.OnKeyDown(Keys.F);
        }

        private string GetBackgroundSpriteForCurrentLevel()
        {
            return nivelActual == NivelJuego.Nivel2
                ? "Textures/BackGrounds/BackGround2.png"
                : "Textures/BackGrounds/Background.png";
        }
    }
}
