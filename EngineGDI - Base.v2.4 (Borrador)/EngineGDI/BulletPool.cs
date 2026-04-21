using System.Collections.Generic;
using System.Drawing;

namespace EngineGDI
{
    public class BulletPool
    {
        // Balas libres para usar (pool). Stack = LIFO, muy barato (Push/Pop).
        private readonly Stack<Bullet> available = new Stack<Bullet>();
        // Balas activas en pantalla (se actualizan y se renderizan).
        private readonly List<Bullet> active = new List<Bullet>();

        private readonly string sprite;
        private readonly float bulletSpeed;

        public BulletPool(string sprite, int poolSize, float bulletSpeed)
        {
            this.sprite = sprite;
            this.bulletSpeed = bulletSpeed;

            // Pre-creación: se instancian N balas una sola vez.
            // Luego se reutilizan (no se crean ni destruyen balas durante el gameplay).
            for (int i = 0; i < poolSize; i++)
            {
                var b = new Bullet(sprite, 0f, 0f, 0f);
                b.Deactivate();
                available.Push(b);
            }
        }

        // Intenta disparar: toma una bala disponible, la activa y la pasa a la lista de activas.
        // Devuelve false si no hay balas libres (pool agotada).
        public bool TrySpawn(float x, float y)
        {
            if (available.Count == 0) return false;

            var b = available.Pop();
            b.Activate(x, y, bulletSpeed);
            active.Add(b);
            return true;
        }

        // Actualiza solo las balas activas.
        // Cuando una bala sale de pantalla (posX > maxX), se desactiva y vuelve a "available".
        public void Update(float deltaTime, float maxX)
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var b = active[i];
                b.Update(deltaTime);
                if (!b.IsActive || b.posX > maxX)
                    ReleaseAt(i);
            }
        }

        // Renderiza únicamente las balas activas.
        public void Render(float scaleX = 0.05f, float scaleY = 0.08f)
        {
            for (int i = 0; i < active.Count; i++)
                active[i].Render(scaleX, scaleY);
        }

        public bool TryHitAsteroid(Asteroid asteroid)
        {
            if (asteroid == null || !asteroid.IsAlive || asteroid.IsDestroying) return false;
            RectangleF asteroidCollider = asteroid.GetCollider();

            for (int i = active.Count - 1; i >= 0; i--)
            {
                var b = active[i];
                if (!b.IsActive)
                {
                    ReleaseAt(i);
                    continue;
                }

                if (IsBoxColliding(b.GetCollider(), asteroidCollider))
                {
                    ReleaseAt(i);
                    asteroid.Destroy();
                    return true;
                }
            }

            return false;
        }

        public bool TryHitAsteroids(IReadOnlyList<Asteroid> asteroids)
        {
            if (asteroids == null || asteroids.Count == 0) return false;

            for (int i = active.Count - 1; i >= 0; i--)
            {
                var b = active[i];
                if (!b.IsActive)
                {
                    ReleaseAt(i);
                    continue;
                }

                RectangleF bulletCollider = b.GetCollider();

                for (int j = 0; j < asteroids.Count; j++)
                {
                    var a = asteroids[j];
                    if (a == null || !a.IsAlive || a.IsDestroying) continue;

                    if (IsBoxColliding(bulletCollider, a.GetCollider()))
                    {
                        ReleaseAt(i);
                        a.Destroy();
                        return true;
                    }
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

        // Devuelve una bala activa al pool:
        // 1) se elimina de la lista de activas
        // 2) se desactiva (para que no procese)
        // 3) vuelve al stack de disponibles
        private void ReleaseAt(int activeIndex)
        {
            var b = active[activeIndex];
            active.RemoveAt(activeIndex);
            b.Deactivate();
            available.Push(b);
        }
    }
}
