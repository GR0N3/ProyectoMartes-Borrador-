using System;
using System.Collections.Generic;

namespace EngineGDI
{
    // Pool de asteroides:
    // Pre-crea N asteroides (capacity) para no instanciar durante el juego.
    // Mantiene una cantidad fija en pantalla (targetFlying).
    // Cuando un asteroide muere (termina explosión) o sale por la izquierda, vuelve al pool y se respawnea otro.
    internal class AsteroidPool
    {
        private readonly List<Asteroid> pool;
        private readonly AsteroidSpawner[] spawners;
        private readonly Random random;
        private readonly float limiteDespawnX;
        private readonly int asteroidesEnPantalla;
        private readonly float distanciaMinimaSpawnX;
        private readonly float toleranciaLineaY;
        private readonly string sprite;

        // Expone el pool completo para que otros sistemas (por ejemplo colisiones) puedan iterarlo.
        // Nota: la lista incluye activos e inactivos; para lógica de gameplay se filtra con IsAlive / IsDestroying.
        public IReadOnlyList<Asteroid> Asteroids => pool;


        // Crea el pool:
        // - capacity: cantidad inicial de asteroides en memoria.
        // - targetFlying: cuántos asteroides se intenta mantener "volando" simultáneamente (vivos y no explotando).
        // - sprite: sprite idle usado para construir cada asteroide.
        // - spawners: carriles/posiciones donde pueden aparecer.
        // - despawnX: límite en X para reciclar (cuando el asteroide llega a esa X por izquierda).
        public AsteroidPool(int capacity, int targetFlying, string sprite, AsteroidSpawner[] spawners, float despawnX = -50f)
        {
            limiteDespawnX = despawnX;
            this.spawners = spawners;
            random = new Random();
            asteroidesEnPantalla = targetFlying;
            pool = new List<Asteroid>(capacity);
            distanciaMinimaSpawnX = 120f;
            toleranciaLineaY = 0.01f;
            this.sprite = sprite;

            for (int i = 0; i < capacity; i++)
            {
                var asteroide = new Asteroid(sprite, 0f, 0f, 0f);
                asteroide.Deactivate();
                pool.Add(asteroide);
            }

            SpawnHastaCantidadObjetivo();
        }


        // Loop de actualización del pool:
        // 1) actualiza el cooldown de cada spawner
        // 2) actualiza cada asteroide activo (movimiento/animación)
        // 3) si un asteroide sale del área (despawnX), se desactiva para que pueda ser reutilizado
        // 4) repone la cantidad objetivo en pantalla (targetFlying)
        public void Update(float deltaTime)
        {
            for (int i = 0; i < spawners.Length; i++)
                spawners[i].Update(deltaTime);

            for (int i = 0; i < pool.Count; i++)
            {
                var asteroide = pool[i];
                if (!asteroide.IsAlive) continue;

                asteroide.Update(deltaTime);

                if (!asteroide.IsDestroying && asteroide.posX <= limiteDespawnX)
                    asteroide.Deactivate();
            }

            SpawnHastaCantidadObjetivo();
        }


        // Renderiza únicamente los asteroides vivos.
        // scaleX/scaleY controlan el tamaño con el que se dibuja el sprite del asteroide.
        public void Render(float scaleX = 0.035f, float scaleY = 0.035f)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                var asteroide = pool[i];
                if (asteroide.IsAlive)
                    asteroide.Render(scaleX, scaleY);
            }
        }

        // Mantiene la cantidad objetivo de asteroides en pantalla:
        // - cuenta cuántos están "volando" (IsAlive && !IsDestroying)
        // - mientras falten, pide un spawner disponible y activa (respawn) un asteroide del pool
        private void SpawnHastaCantidadObjetivo()
        {
            int asteroidesVolando = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                var asteroide = pool[i];
                if (asteroide.IsAlive && !asteroide.IsDestroying)
                    asteroidesVolando++;
            }

            while (asteroidesVolando < asteroidesEnPantalla)
            {
                var spawner = ElegirSpawnerDisponible();
                if (spawner == null) break;

                var asteroide = GetAsteroideDisponible();
                if (!spawner.TrySpawn(asteroide, random))
                {
                    asteroide.Deactivate();
                    break;
                }

                asteroidesVolando++;
            }
        }

        // Devuelve un asteroide reutilizable:
        // - si hay uno inactivo en el pool, lo devuelve
        // - si todos están activos, crea uno nuevo, lo agrega y lo devuelve (pool dinámico)
        private Asteroid GetAsteroideDisponible()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                var a = pool[i];
                if (!a.IsAlive)
                    return a;
            }

            var nuevo = new Asteroid(sprite, 0f, 0f, 0f);
            nuevo.Deactivate();
            pool.Add(nuevo);
            return nuevo;
        }


        // Elige un spawner válido para generar un nuevo asteroide:
        // - no debe estar en cooldown
        // - no debe existir otro asteroide activo en ese mismo carril (mismo Y) demasiado cerca del punto de spawn
        // Devuelve null si ninguno cumple, para evitar spawns injustos (encima de otro).
        private AsteroidSpawner ElegirSpawnerDisponible()
        {
            int inicio = random.Next(spawners.Length);
            for (int offset = 0; offset < spawners.Length; offset++)
            {
                int i = (inicio + offset) % spawners.Length;
                var spawner = spawners[i];

                if (!spawner.PuedeSpawnear) continue;
                if (HayAsteroideEnLinea(spawner)) continue;

                return spawner;
            }

            return null;
        }

        // Devuelve true si ya existe un asteroide en el mismo carril (mismo Y del spawner)
        // y todavía está lo suficientemente cerca del punto de spawn como para que el nuevo aparezca encima.
        private bool HayAsteroideEnLinea(AsteroidSpawner spawner)
        {
            float spawnX = spawner.SpawnX;
            float spawnY = spawner.SpawnY;
            float limiteX = spawnX - distanciaMinimaSpawnX;

            for (int i = 0; i < pool.Count; i++)
            {
                var a = pool[i];
                if (!a.IsAlive) continue;

                float dy = a.posY - spawnY;
                if (dy < 0f) dy = -dy;
                if (dy > toleranciaLineaY) continue;

                if (a.posX > limiteX)
                    return true;
            }

            return false;
        }
    }
}
