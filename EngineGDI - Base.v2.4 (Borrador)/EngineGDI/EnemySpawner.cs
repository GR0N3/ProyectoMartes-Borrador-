using System;

namespace EngineGDI
{
    // Spawner (carril) de enemigos.
    // Define una posición fija de spawn (X,Y), un rango de velocidad y un cooldown para no spawnear seguido.
    internal class EnemySpawner
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
        // - spawnX/spawnY: punto donde aparecerán los enemigos de este carril.
        // - velocidadMin/velocidadMax: rango de velocidades (se elige una random al spawnear).
        // - cooldownSegundos: tiempo mínimo entre spawns desde este mismo carril.
        public EnemySpawner(float spawnX, float spawnY, float velocidadMin, float velocidadMax, float cooldownSegundos = 0.6f)
        {
            this.spawnX = spawnX;
            this.spawnY = spawnY;
            this.velocidadMin = velocidadMin;
            this.velocidadMax = velocidadMax;
            this.cooldownSegundos = cooldownSegundos;
            cooldownRestante = 0f;
        }

        // Actualiza el cooldown del spawner.
        public void Update(float deltaTime)
        {
            if (cooldownRestante > 0f)
                cooldownRestante -= deltaTime;
        }

        // Intenta spawnear un enemigo en este carril:
        // 1) si está en cooldown, devuelve false
        // 2) si puede, elige una velocidad aleatoria del rango
        // 3) llama a enemy.Respawn(...) para reusar el objeto
        // 4) reinicia el cooldown y devuelve true
        public bool TrySpawn(EnemyEntity enemy, Random random)
        {
            if (!PuedeSpawnear) return false;

            float velocidad = Lerp(velocidadMin, velocidadMax, (float)random.NextDouble());
            enemy.Respawn(spawnX, spawnY, velocidad);
            cooldownRestante = cooldownSegundos;
            return true;
        }

        // Interpolación lineal (a..b) usada para elegir velocidades random.
        private static float Lerp(float a, float b, float t) => a + (b - a) * t;
    }
}
