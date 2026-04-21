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
        private readonly Stack<Asteroid> asteroidesDisponibles;
        private readonly List<Asteroid> asteroidesActivos;
        private readonly AsteroidSpawner[] spawners;
        private readonly Random random;
        private readonly float limiteDespawnX;
        private readonly int asteroidesEnPantalla;
        private readonly float distanciaMinimaSpawnX;
        private readonly float toleranciaLineaY;

        public IReadOnlyList<Asteroid> Asteroids => asteroidesActivos;


        // Crea la pool:
        // capacity = cantidad total de objetos que existen (memoria)
        // targetFlying = cantidad simultánea que se intenta mantener "volando" (no destruyéndose)
        // spawners = carriles/posiciones posibles de spawn
        // despawnX = cuando el asteroide llega a X <= despawnX se recicla
        
        public AsteroidPool(int capacity, int targetFlying, string sprite, AsteroidSpawner[] spawners, float despawnX = -50f)
        {
            limiteDespawnX = despawnX;
            this.spawners = spawners;
            random = new Random();
            asteroidesEnPantalla = targetFlying;
            asteroidesDisponibles = new Stack<Asteroid>(capacity);
            asteroidesActivos = new List<Asteroid>(capacity);
            distanciaMinimaSpawnX = 120f;
            toleranciaLineaY = 0.01f;

            for (int i = 0; i < capacity; i++)
            {
                var asteroide = new Asteroid(sprite, 0f, 0f, 0f);
                asteroide.Deactivate();
                asteroidesDisponibles.Push(asteroide);
            }

            SpawnHastaCantidadObjetivo();
        }


        // Actualiza:
        // cooldowns de spawners
        // estado/movimiento/animación de asteroides activos
        // recicla los que murieron o salieron de pantalla y luego repone hasta llegar a la cantidad objetivo en pantalla
 
        public void Update(float deltaTime)
        {
            for (int i = 0; i < spawners.Length; i++)
                spawners[i].Update(deltaTime);

            for (int i = asteroidesActivos.Count - 1; i >= 0; i--)
            {
                var asteroide = asteroidesActivos[i];

                asteroide.Update(deltaTime);

                if (!asteroide.IsAlive)
                {
                    DevolverAlPool(i);
                    continue;
                }

                if (!asteroide.IsDestroying && asteroide.posX <= limiteDespawnX)
                {
                    DevolverAlPool(i);
                    continue;
                }
            }

            SpawnHastaCantidadObjetivo();
        }


        // Renderiza todos los asteroides vivos de la lista activa.
        public void Render(float scaleX = 0.035f, float scaleY = 0.035f)
        {
            for (int i = 0; i < asteroidesActivos.Count; i++)
            {
                var asteroide = asteroidesActivos[i];
                if (asteroide.IsAlive)
                    asteroide.Render(scaleX, scaleY);
            }
        }

        // Quita un asteroide de la lista activa y lo devuelve al stack de disponibles.
        // Se usa cuando termina su explosión o cuando se va por fuera de pantalla.
        private void DevolverAlPool(int indiceActivo)
        {
            var asteroide = asteroidesActivos[indiceActivo];
            asteroidesActivos.RemoveAt(indiceActivo);
            asteroide.Deactivate();
            asteroidesDisponibles.Push(asteroide);
        }

        // Mantiene la cantidad objetivo en pantalla:
        // cuenta cuántos están "volando" (vivos y no destruyéndose) y spawnea desde disponibles hasta completar el objetivo
        private void SpawnHastaCantidadObjetivo()
        {
            int asteroidesVolando = 0;
            for (int i = 0; i < asteroidesActivos.Count; i++)
            {
                var asteroide = asteroidesActivos[i];
                if (asteroide.IsAlive && !asteroide.IsDestroying)
                    asteroidesVolando++;
            }

            while (asteroidesVolando < asteroidesEnPantalla && asteroidesDisponibles.Count > 0)
            {
                var spawner = ElegirSpawnerDisponible();
                if (spawner == null) break;

                var asteroide = asteroidesDisponibles.Pop();
                if (!spawner.TrySpawn(asteroide, random))
                {
                    asteroide.Deactivate();
                    asteroidesDisponibles.Push(asteroide);
                    break;
                }

                asteroidesActivos.Add(asteroide);
                asteroidesVolando++;
            }
        }


        // Elige un spawner que:
        // No esté en cooldown y no tenga otro asteroide demasiado cerca en su misma línea (Y)
        // Si ninguno sirve, devuelve null.
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

        // Devuelve true si ya existe un asteroide en el mismo carril (mismo Y del spawner) y todavía está cerca del punto de spawn (para evitar spawnear uno encima del otro).
        private bool HayAsteroideEnLinea(AsteroidSpawner spawner)
        {
            float spawnX = spawner.SpawnX;
            float spawnY = spawner.SpawnY;
            float limiteX = spawnX - distanciaMinimaSpawnX;

            for (int i = 0; i < asteroidesActivos.Count; i++)
            {
                var a = asteroidesActivos[i];
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
