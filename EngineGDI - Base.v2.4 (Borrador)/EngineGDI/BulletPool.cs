using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
