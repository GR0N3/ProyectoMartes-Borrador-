using System;
using System.Collections.Generic;
using System.Drawing;

namespace EngineGDI
{
    public enum BulletType
    {
        Player,
        Enemy
    }

    public class BulletPool
    {
        private readonly Dictionary<BulletType, ObjectPool<BulletEntity>> pools;
        private readonly List<BulletEntity> activePlayerBullets;
        private readonly List<BulletEntity> activeEnemyBullets;

        private readonly string playerBulletSprite;
        private readonly string enemyBulletSprite;
        private readonly float playerBulletSpeed;
        private readonly float enemyBulletSpeed;

        public IReadOnlyList<BulletEntity> ActivePlayerBullets => activePlayerBullets;
        public IReadOnlyList<BulletEntity> ActiveEnemyBullets => activeEnemyBullets;

        public BulletPool(string playerSprite, string enemySprite, int poolSize, float playerSpeed, float enemySpeed)
        {
            this.playerBulletSprite = playerSprite;
            this.enemyBulletSprite = enemySprite;
            this.playerBulletSpeed = playerSpeed;
            this.enemyBulletSpeed = enemySpeed;

            pools = new Dictionary<BulletType, ObjectPool<BulletEntity>>();
            activePlayerBullets = new List<BulletEntity>();
            activeEnemyBullets = new List<BulletEntity>();

            // Inicializar pools genéricas
            pools[BulletType.Player] = new ObjectPool<BulletEntity>(
                poolSize,
                () => new PlayerBullet(playerSprite, 0f, 0f, 0f),
                b => b.IsActive,
                b => b.Deactivate()
            );

            pools[BulletType.Enemy] = new ObjectPool<BulletEntity>(
                poolSize,
                () => new EnemyBullet(enemySprite, 0f, 0f, 0f),
                b => b.IsActive,
                b => b.Deactivate()
            );
        }

        // Obtiene o crea una bala del pool correspondiente
        private BulletEntity GetBullet(BulletType type)
        {
            return pools[type].Get();
        }

        // Intenta spawnear/activar una bala
        public bool TrySpawn(BulletType type, float x, float y)
        {
            var b = GetBullet(type);
            float speed = (type == BulletType.Player) ? playerBulletSpeed : enemyBulletSpeed;
            b.Activate(x, y, speed);

            if (type == BulletType.Player)
                activePlayerBullets.Add(b);
            else
                activeEnemyBullets.Add(b);

            return true;
        }

        // Actualiza las posiciones y limpia balas inactivas o fuera de la pantalla
        public void Update(float deltaTime, float screenWidth)
        {
            // Balas del jugador
            for (int i = activePlayerBullets.Count - 1; i >= 0; i--)
            {
                var b = activePlayerBullets[i];
                b.Update(deltaTime);

                if (b.posX > screenWidth)
                {
                    b.Deactivate();
                    activePlayerBullets.RemoveAt(i);
                }
                else if (!b.IsActive)
                {
                    activePlayerBullets.RemoveAt(i);
                }
            }

            // Balas de enemigos
            for (int i = activeEnemyBullets.Count - 1; i >= 0; i--)
            {
                var b = activeEnemyBullets[i];
                b.Update(deltaTime);

                // Si se sale de la pantalla por la izquierda (despawn)
                if (b.posX < -100f)
                {
                    b.Deactivate();
                    activeEnemyBullets.RemoveAt(i);
                }
                else if (!b.IsActive)
                {
                    activeEnemyBullets.RemoveAt(i);
                }
            }
        }

        // Renderiza las balas activas en pantalla
        public void Render()
        {
            for (int i = 0; i < activePlayerBullets.Count; i++)
            {
                activePlayerBullets[i].Render(0.05f, 0.08f);
            }

            for (int i = 0; i < activeEnemyBullets.Count; i++)
            {
                activeEnemyBullets[i].Render(0.05f, 0.08f);
            }
        }

        // Colisión bala jugador vs lista de enemigos
        public bool TryHitEnemies(IReadOnlyList<EnemyEntity> enemies)
        {
            if (enemies == null || enemies.Count == 0) return false;

            for (int i = activePlayerBullets.Count - 1; i >= 0; i--)
            {
                var b = activePlayerBullets[i];
                RectangleF bulletCollider = b.GetCollider();

                for (int j = 0; j < enemies.Count; j++)
                {
                    var e = enemies[j];
                    if (e == null || !e.IsAlive || e.IsDestroying) continue;

                    if (IsBoxColliding(bulletCollider, e.GetCollider()))
                    {
                        b.Deactivate();
                        activePlayerBullets.RemoveAt(i);

                        e.TakeDamage(b.Dano);
                        return true;
                    }
                }
            }

            return false;
        }

        // Colisión bala enemiga vs jugador
        public bool TryHitPlayer(Player player)
        {
            if (player == null) return false;
            RectangleF playerCollider = player.GetCollider();
            if (playerCollider.IsEmpty) return false;

            for (int i = activeEnemyBullets.Count - 1; i >= 0; i--)
            {
                var b = activeEnemyBullets[i];
                if (IsBoxColliding(b.GetCollider(), playerCollider))
                {
                    b.Deactivate();
                    activeEnemyBullets.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        private bool IsBoxColliding(RectangleF a, RectangleF b)
        {
            return a.Left < b.Right &&
                   a.Right > b.Left &&
                   a.Top < b.Bottom &&
                   a.Bottom > b.Top;
        }
    }
}
