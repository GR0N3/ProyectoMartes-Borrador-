using System;
using System.Drawing;

namespace EngineGDI
{
    public abstract class EnemyEntity : IDamageable, ICollidable
    {
        public float Vida { get; protected set; }
        public float Dano { get; protected set; }
        public string AudioHitPath { get; protected set; }
        public string AudioDeathPath { get; protected set; }

        public Transform Transform;
        protected readonly Renderer renderer;

        public event Action<EnemyEntity> OnDestroyed;

        protected EnemyEntity()
        {
            renderer = new Renderer(string.Empty, Vector2.Zero);
        }

        public abstract bool IsAlive { get; }
        public abstract bool IsDestroying { get; }

        public abstract float posX { get; set; }
        public abstract float posY { get; set; }

        public abstract void Update(float deltaTime);
        public abstract void Render(float scaleX, float scaleY);
        public abstract RectangleF GetCollider();
        public abstract void Destroy();
        public abstract void Respawn(float posX, float posY, float speed);
        public abstract void Deactivate();

        public virtual void TakeDamage(float amount)
        {
            if (!IsAlive || IsDestroying) return;

            Vida -= amount;
            if (Vida <= 0)
            {
                Destroy();
                PlaySound(AudioDeathPath);
                NotifyDestroyed();
            }
            else
            {
                PlaySound(AudioHitPath);
            }
        }

        protected void DrawWithRenderer(string texturePath, float scaleX, float scaleY, float angle = 0f, float offsetX = 0f, float offsetY = 0f)
        {
            renderer.TexturePath = texturePath;
            Transform.Scale = new Vector2(scaleX, scaleY);
            renderer.Draw(Transform, angle, offsetX, offsetY);
        }

        protected void NotifyDestroyed()
        {
            OnDestroyed?.Invoke(this);
        }

        protected void PlaySound(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                
                try
                {
                    Engine.PlaySound(path);
                }
                catch { }
            }
        }
    }
}
