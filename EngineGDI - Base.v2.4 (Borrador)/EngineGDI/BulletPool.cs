using System.Collections.Generic;
using System.Drawing;

namespace EngineGDI
{
    public class BulletPool
    {
        private readonly List<Bullet> pool;

        private readonly string sprite;
        private readonly float bulletSpeed;

        // Crea el pool de balas preinstanciando "poolSize" balas inactivas.
        // sprite = ruta del sprite de la bala.
        // poolSize = cantidad inicial de balas en memoria.
        // bulletSpeed = velocidad horizontal que tendrán las balas al activarse.
        public BulletPool(string sprite, int poolSize, float bulletSpeed)
        {
            this.sprite = sprite;
            this.bulletSpeed = bulletSpeed;
            pool = new List<Bullet>(poolSize);

            for (int i = 0; i < poolSize; i++)
            {
                var b = new Bullet(sprite, 0f, 0f, 0f);
                b.Deactivate();
                pool.Add(b);
            }
        }

        // Devuelve una bala lista para usar:
        // - Si existe una bala inactiva, la reutiliza.
        // - Si todas están activas, crea una nueva y la agrega al pool (pool dinámico).
        public Bullet GetBullet()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                var b = pool[i];
                if (!b.IsActive)
                    return b;
            }

            var newBullet = new Bullet(sprite, 0f, 0f, 0f);
            newBullet.Deactivate();
            pool.Add(newBullet);
            return newBullet;
        }

        // Intenta spawnear/disparar una bala:
        // Obtiene una bala del pool, la activa en (x,y) y le asigna la velocidad.
        // En este modelo siempre devuelve true porque el pool puede crecer si es necesario.
        public bool TrySpawn(float x, float y)
        {
            var b = GetBullet();
            b.Activate(x, y, bulletSpeed);
            return true;
        }

        // Actualiza el movimiento de todas las balas activas.
        // Si una bala sobrepasa maxX, se desactiva para que vuelva a estar disponible en el pool.
        public void Update(float deltaTime, float maxX)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                var b = pool[i];
                if (!b.IsActive) continue;
                b.Update(deltaTime);
                if (b.posX > maxX)
                    b.Deactivate();
            }
        }

        // Dibuja únicamente las balas que están activas.
        // scaleX/scaleY controlan el tamaño con el que se renderiza el sprite.
        public void Render(float scaleX = 0.05f, float scaleY = 0.08f)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                var b = pool[i];
                if (b.IsActive)
                    b.Render(scaleX, scaleY);
            }
        }

        // Colisión bala vs un solo asteroide.
        // Si una bala impacta:
        // - la bala se desactiva (vuelve al pool)
        // - el asteroide entra en destrucción (animación de explosión)
        // Devuelve true si hubo hit.
        public bool TryHitAsteroid(Asteroid asteroid)
        {
            if (asteroid == null || !asteroid.IsAlive || asteroid.IsDestroying) return false;
            RectangleF asteroidCollider = asteroid.GetCollider();

            for (int i = 0; i < pool.Count; i++)
            {
                var b = pool[i];
                if (!b.IsActive) continue;

                if (IsBoxColliding(b.GetCollider(), asteroidCollider))
                {
                    b.Deactivate();
                    asteroid.Destroy();
                    return true;
                }
            }

            return false;
        }

        // Colisión bala vs lista de asteroides.
        // Recorre todas las balas activas y verifica colisión contra cada asteroide vivo/no-destruyéndose.
        // Si detecta un hit, desactiva la bala, destruye el asteroide y termina (1 hit por frame por simplicidad).
        public bool TryHitAsteroids(IReadOnlyList<Asteroid> asteroids)
        {
            if (asteroids == null || asteroids.Count == 0) return false;

            for (int i = 0; i < pool.Count; i++)
            {
                var b = pool[i];
                if (!b.IsActive) continue;

                RectangleF bulletCollider = b.GetCollider();

                for (int j = 0; j < asteroids.Count; j++)
                {
                    var a = asteroids[j];
                    if (a == null || !a.IsAlive || a.IsDestroying) continue;

                    if (IsBoxColliding(bulletCollider, a.GetCollider()))
                    {
                        b.Deactivate();
                        a.Destroy();
                        return true;
                    }
                }
            }

            return false;
        }

        // Colisión AABB (Axis-Aligned Bounding Box) para rectángulos sin rotación.
        private bool IsBoxColliding(RectangleF a, RectangleF b)
        {
            return a.Left < b.Right &&
                   a.Right > b.Left &&
                   a.Top < b.Bottom &&
                   a.Bottom > b.Top;
        }
    }
}
