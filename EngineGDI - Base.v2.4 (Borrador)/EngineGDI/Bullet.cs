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

        // Constructor:
        // Inicializa transform, sprite y velocidad.
        // Nota: en este proyecto las instancias se crean mayormente al inicializar la pool.
        public Bullet(string sprite, float posX, float posY, float velX)
        {
            Transform = new Transform(new Vector2(posX, posY), new Vector2(ColliderReferenceScaleX, ColliderReferenceScaleY));
            this.sprite = sprite;
            this.velX = velX;
            IsActive = true;
        }

        // Reactiva una bala existente del pool:
        // setea posición, velocidad y marca IsActive=true para que vuelva a actualizarse/renderizarse.
        public void Activate(float posX, float posY, float velX)
        {
            Transform.Position = new Vector2(posX, posY);
            this.velX = velX;
            IsActive = true;
        }

        // Desactiva la bala:
        // al estar inactiva no se mueve ni se dibuja, y queda lista para ser reutilizada por la pool.
        public void Deactivate()
        {
            IsActive = false;
        }

        // Movimiento de la bala:
        // si está activa, avanza en X usando velX y deltaTime.
        public void Update(float deltaTime)
        {
            if (!IsActive) return;
            posX += velX * deltaTime;
        }

        // Render de la bala:
        // aplica escala y dibuja el sprite en la posición actual.
        public void Render(float scaleX = 0.1f, float scaleY = 0.1f)
        {
            Transform.Scale = new Vector2(scaleX, scaleY);
            Engine.Draw(sprite, Transform.Position.X, Transform.Position.Y, Transform.Scale.X, Transform.Scale.Y);
            if (!IsActive) return;
        }

        // Collider AABB de la bala (sin rotación):
        // Usa un tamaño base (ColliderSize) y lo ajusta respecto a la escala actual comparándola con la escala de referencia.
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
