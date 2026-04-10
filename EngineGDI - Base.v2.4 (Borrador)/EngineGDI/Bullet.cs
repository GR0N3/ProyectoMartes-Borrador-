using System;
using System.Collections.Generic;
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

        public Bullet(string sprite, float posX, float posY, float velX)
        {
            Transform = new Transform(new Vector2(posX, posY), new Vector2(0.1f, 0.1f));
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
    }
}
