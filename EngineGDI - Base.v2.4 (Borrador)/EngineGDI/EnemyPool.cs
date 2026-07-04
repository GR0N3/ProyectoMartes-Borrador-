using System;
using System.Collections.Generic;

namespace EngineGDI
{
    internal class EnemyPool
    {
        private readonly Dictionary<EnemyType, ObjectPool<EnemyEntity>> pools;
        private readonly List<EnemyEntity> activeEnemies;
        private readonly EnemySpawner[] spawners;
        private readonly Random random;
        private readonly float limiteDespawnX;
        private readonly int enemigosEnPantalla;
        private readonly float distanciaMinimaSpawnX;
        private readonly float toleranciaLineaY;
        
        private readonly string spriteAsteroide;
        private readonly string spriteNaveRoja;
        private readonly string spriteNaveAzul;
        private readonly Action<float, float, BulletType> onEnemyShoot;
        private readonly Action<EnemyEntity> onEnemyDestroyed;
        private readonly float asteroidScaleMin;
        private readonly float asteroidScaleMax;

        public IReadOnlyList<EnemyEntity> Enemies => activeEnemies;

        public EnemyPool(
            int capacity,
            int targetFlying,
            string spriteAsteroide,
            string spriteNaveRoja,
            string spriteNaveAzul,
            EnemySpawner[] spawners,
            Action<float, float, BulletType> onEnemyShoot,
            Action<EnemyEntity> onEnemyDestroyed = null,
            float despawnX = -50f,
            float asteroidScaleMin = 0.08f,
            float asteroidScaleMax = 0.14f)
        {
            limiteDespawnX = despawnX;
            this.spawners = spawners;
            this.random = new Random();
            this.enemigosEnPantalla = targetFlying;
            this.distanciaMinimaSpawnX = 150f;
            this.toleranciaLineaY = 1.0f;
            this.spriteAsteroide = spriteAsteroide;
            this.spriteNaveRoja = spriteNaveRoja;
            this.spriteNaveAzul = spriteNaveAzul;
            this.onEnemyShoot = onEnemyShoot;
            this.onEnemyDestroyed = onEnemyDestroyed;
            this.asteroidScaleMin = asteroidScaleMin;
            this.asteroidScaleMax = asteroidScaleMax;

            activeEnemies = new List<EnemyEntity>();
            pools = new Dictionary<EnemyType, ObjectPool<EnemyEntity>>();

            // Inicializar pools genéricas
            pools[EnemyType.Asteroid] = new ObjectPool<EnemyEntity>(
                capacity,
                () => CreatePooledEnemy(EnemyType.Asteroid),
                e => e.IsAlive,
                e => e.Deactivate()
            );

            pools[EnemyType.NaveEnemigaRoja] = new ObjectPool<EnemyEntity>(
                capacity,
                () => CreatePooledEnemy(EnemyType.NaveEnemigaRoja),
                e => e.IsAlive,
                e => e.Deactivate()
            );

            pools[EnemyType.NaveEnemigaAzul] = new ObjectPool<EnemyEntity>(
                capacity,
                () => CreatePooledEnemy(EnemyType.NaveEnemigaAzul),
                e => e.IsAlive,
                e => e.Deactivate()
            );

            SpawnHastaCantidadObjetivo();
        }

        private EnemyEntity CreatePooledEnemy(EnemyType type)
        {
            string sprite = GetSpriteForType(type);
            Action<float, float, BulletType> shootCallback = type == EnemyType.Asteroid ? null : onEnemyShoot;
            var enemy = EnemyFactory.CreateEnemy(type, sprite, 0f, 0f, 0f, shootCallback);

            if (onEnemyDestroyed != null)
                enemy.OnDestroyed += onEnemyDestroyed;

            return enemy;
        }

        public void Update(float deltaTime)
        {
            for (int i = 0; i < spawners.Length; i++)
                spawners[i].Update(deltaTime);

            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemigo = activeEnemies[i];

                enemigo.Update(deltaTime);

                // Si se sale de la pantalla por la izquierda (despawn)
                if (!enemigo.IsDestroying && enemigo.posX <= limiteDespawnX)
                {
                    enemigo.Deactivate();
                    activeEnemies.RemoveAt(i);
                }
                // Si la animación de explosión terminó (IsAlive = false)
                else if (!enemigo.IsAlive)
                {
                    activeEnemies.RemoveAt(i);
                }
            }

            SpawnHastaCantidadObjetivo();
        }

        public void Render(float scaleX = 0.035f, float scaleY = 0.035f)
        {
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                var enemigo = activeEnemies[i];
                if (enemigo.IsAlive)
                {
                    if (enemigo is Asteroid)
                        enemigo.Render(0.05f, 0.05f);
                    else
                        enemigo.Render(0.04f, 0.04f);
                }
            }
        }

        private void SpawnHastaCantidadObjetivo()
        {
            while (activeEnemies.Count < enemigosEnPantalla)
            {
                var spawner = ElegirSpawnerDisponible();
                if (spawner == null) break;

                EnemyType tipoElegido = ElegirTipoEnemigo();

                var enemigo = GetEnemigoDisponible(tipoElegido);
                if (!spawner.TrySpawn(enemigo, random))
                {
                    enemigo.Deactivate();
                    break;
                }

                if (enemigo is Asteroid asteroid)
                    asteroid.SetUniformScale(GetRandomAsteroidScale());

                activeEnemies.Add(enemigo);
            }
        }

        private EnemyEntity GetEnemigoDisponible(EnemyType type)
        {
            return pools[type].Get();
        }

        private EnemySpawner ElegirSpawnerDisponible()
        {
            int inicio = random.Next(spawners.Length);
            for (int offset = 0; offset < spawners.Length; offset++)
            {
                int i = (inicio + offset) % spawners.Length;
                var spawner = spawners[i];

                if (!spawner.PuedeSpawnear) continue;
                if (HayEnemigoEnLinea(spawner)) continue;

                return spawner;
            }

            return null;
        }

        private bool HayEnemigoEnLinea(EnemySpawner spawner)
        {
            float spawnX = spawner.SpawnX;
            float spawnY = spawner.SpawnY;
            float limiteX = spawnX - distanciaMinimaSpawnX;

            for (int i = 0; i < activeEnemies.Count; i++)
            {
                var e = activeEnemies[i];
                if (!e.IsAlive) continue;

                float dy = Math.Abs(e.posY - spawnY);
                if (dy > toleranciaLineaY) continue;

                if (e.posX > limiteX)
                    return true;
            }

            return false;
        }

        public static EnemyPool CrearPoolCon5Spawners(
            int anchoPantalla,
            int altoPantalla,
            string spriteAsteroide,
            string spriteNaveRoja,
            string spriteNaveAzul,
            Action<float, float, BulletType> onEnemyShoot,
            Action<EnemyEntity> onEnemyDestroyed = null,
            int cantidadEnPool = 8,
            int enemigosSimultaneos = 5,
            float velocidadMin = 80f,
            float velocidadMax = 180f,
            float spawnX = 1350f,
            float despawnX = -100f,
            float yMin = 50f,
            float yMaxOffset = 200f,
            float cooldownSpawner = 0.8f,
            float asteroidScaleMin = 0.08f,
            float asteroidScaleMax = 0.14f)
        {
            float yMax = altoPantalla - yMaxOffset;

            var spawners = new EnemySpawner[5];
            for (int i = 0; i < spawners.Length; i++)
            {
                float t = spawners.Length == 1 ? 0f : i / (float)(spawners.Length - 1);
                float y = yMin + (yMax - yMin) * t;
                spawners[i] = new EnemySpawner(spawnX, y, velocidadMin, velocidadMax, cooldownSpawner);
            }

            return new EnemyPool(
                capacity: cantidadEnPool,
                targetFlying: enemigosSimultaneos,
                spriteAsteroide: spriteAsteroide,
                spriteNaveRoja: spriteNaveRoja,
                spriteNaveAzul: spriteNaveAzul,
                spawners: spawners,
                onEnemyShoot: onEnemyShoot,
                onEnemyDestroyed: onEnemyDestroyed,
                despawnX: despawnX,
                asteroidScaleMin: asteroidScaleMin,
                asteroidScaleMax: asteroidScaleMax);
        }

        private string GetSpriteForType(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Asteroid:
                    return spriteAsteroide;
                case EnemyType.NaveEnemigaRoja:
                    return spriteNaveRoja;
                case EnemyType.NaveEnemigaAzul:
                    return spriteNaveAzul;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private EnemyType ElegirTipoEnemigo()
        {
            double roll = random.NextDouble();
            if (roll < 0.60)
                return EnemyType.Asteroid;
            if (roll < 0.80)
                return EnemyType.NaveEnemigaRoja;

            return EnemyType.NaveEnemigaAzul;
        }

        private float GetRandomAsteroidScale()
        {
            return asteroidScaleMin + ((asteroidScaleMax - asteroidScaleMin) * (float)random.NextDouble());
        }
    }
}
