using System;

namespace EngineGDI
{
    // Spawner (carril) de asteroides.
    // Define una posición fija de spawn (X,Y), un rango de velocidad y un cooldown para no spawnear seguido.
    internal class AsteroidSpawner
    {
        private readonly float spawnX;
        private readonly float spawnY;
        private readonly float velocidadMin;
        private readonly float velocidadMax;
        private readonly float cooldownSegundos;
        private float cooldownRestante;

        public float SpawnX => spawnX;
        public float SpawnY => spawnY;
        public bool PuedeSpawnear => cooldownRestante <= 0f;

        // Constructor del spawner:
        // - spawnX/spawnY: punto donde aparecerán los asteroides de este carril.
        // - velocidadMin/velocidadMax: rango de velocidades (se elige una random al spawnear).
        // - cooldownSegundos: tiempo mínimo entre spawns desde este mismo carril.
        public AsteroidSpawner(float spawnX, float spawnY, float velocidadMin, float velocidadMax, float cooldownSegundos = 0.6f)
        {
            this.spawnX = spawnX;
            this.spawnY = spawnY;
            this.velocidadMin = velocidadMin;
            this.velocidadMax = velocidadMax;
            this.cooldownSegundos = cooldownSegundos;
            cooldownRestante = 0f;
        }

        /// Actualiza el cooldown del spawner.
        public void Update(float deltaTime)
        {
            if (cooldownRestante > 0f)
                cooldownRestante -= deltaTime;
        }

        // Intenta spawnear un asteroide en este carril:
        // 1) si está en cooldown, devuelve false
        // 2) si puede, elige una velocidad aleatoria del rango
        // 3) llama a asteroid.Respawn(...) para reusar el objeto
        // 4) reinicia el cooldown y devuelve true
        public bool TrySpawn(Asteroid asteroid, Random random)
        {
            if (!PuedeSpawnear) return false;

            float velocidad = Lerp(velocidadMin, velocidadMax, (float)random.NextDouble());
            asteroid.Respawn(spawnX, spawnY, velocidad);
            cooldownRestante = cooldownSegundos;
            return true;
        }


        // Interpolación lineal (a..b) usada para elegir velocidades random.
        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

        // Helper de fábrica para crear una pool con 5 spawners distribuidos a lo largo del eje Y.
        // Esto evita tener que instanciar spawners y pool manualmente desde Program.cs.
        public static AsteroidPool CrearPoolCon5Spawners(
            int anchoPantalla,
            int altoPantalla,
            string spriteAsteroide,
            int cantidadEnPool = 8,
            int asteroidesSimultaneos = 5,
            float velocidadMin = 80f,
            float velocidadMax = 180f,
            float spawnX = 1350f,
            float despawnX = -100f,
            float yMin = 50f,
            float yMaxOffset = 200f,
            float cooldownSpawner = 0.8f)
        {
            // yMaxOffset se usa para no spawnear demasiado abajo (ej: dejar espacio para UI inferior si existiera).
            float yMax = altoPantalla - yMaxOffset;

            // Crea 5 carriles (spawners) equiespaciados en Y entre yMin y yMax.
            var spawners = new AsteroidSpawner[5];
            for (int i = 0; i < spawners.Length; i++)
            {
                float t = spawners.Length == 1 ? 0f : i / (float)(spawners.Length - 1);
                float y = yMin + (yMax - yMin) * t;
                spawners[i] = new AsteroidSpawner(spawnX, y, velocidadMin, velocidadMax, cooldownSpawner);
            }

            // Construye y devuelve la pool ya configurada.
            return new AsteroidPool(
                capacity: cantidadEnPool,
                targetFlying: asteroidesSimultaneos,
                sprite: spriteAsteroide,
                spawners: spawners,
                despawnX: despawnX);
        }
    }
}
