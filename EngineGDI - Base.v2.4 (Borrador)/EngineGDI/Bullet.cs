using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineGDI
{
    public class Bullet
    {
        public Transform Transform;

        public float posX
        {
            get => Transform.Position.X;
            set => Transform.Position = new Vector2(value, Transform.Position.Y);
        }

        public float posY
        {
            get => Transform.Position.Y;
            set => Transform.Position = new Vector2(Transform.Position.X, value);
        }
        public float velX;

        private string sprite;
        public bool IsActive { get; private set; }
        public Vector2 ColliderSize { get; } = new Vector2(26f, 12f);
        private const float ColliderReferenceScaleX = 0.05f;
        private const float ColliderReferenceScaleY = 0.08f;

        public Bullet(string sprite, float posX, float posY, float velX)
        {
            Transform = new Transform(new Vector2(posX, posY), new Vector2(ColliderReferenceScaleX, ColliderReferenceScaleY));
            this.sprite = sprite;
            this.velX = velX;
            IsActive = true;
        }

        // Reactiva una bala existente de la pool, reseteando su estado.
        public void Activate(float posX, float posY, float velX)
        {
            Transform.Position = new Vector2(posX, posY);
            this.velX = velX;
            IsActive = true;
        }

        // Desactiva la bala para que no se actualice ni se renderice.
        // La pool la marca así cuando sale de la pantalla y la devuelve al stack de disponibles.
        public void Deactivate()
        {
            IsActive = false;
        }

        public void Update(float deltaTime)
        {
            if (!IsActive) return;
            posX += velX * deltaTime;
        }

        public void Render(float scaleX = 0.1f, float scaleY = 0.1f)
        {
            Transform.Scale = new Vector2(scaleX, scaleY);
            Engine.Draw(sprite, Transform.Position.X, Transform.Position.Y, Transform.Scale.X, Transform.Scale.Y);
            if (!IsActive) return;
        }

        public RectangleF GetCollider()
        {
            float width = ColliderSize.X * (Transform.Scale.X / ColliderReferenceScaleX);
            float height = ColliderSize.Y * (Transform.Scale.Y / ColliderReferenceScaleY);

            return new RectangleF(
                Transform.Position.X,
                Transform.Position.Y,
                width,
                height);
        }
    }
}
