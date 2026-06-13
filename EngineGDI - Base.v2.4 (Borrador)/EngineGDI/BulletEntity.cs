using System.Drawing;

namespace EngineGDI
{
    public abstract class BulletEntity : ICollidable
    {
        public Transform Transform;
        protected Renderer renderer;
        public float Dano { get; protected set; }
        public float Velocidad { get; protected set; }
        public bool IsActive { get; protected set; }
        public string Sprite { get; protected set; }
        public Vector2 ColliderSize { get; protected set; }

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

        protected const float ColliderReferenceScaleX = 0.05f;
        protected const float ColliderReferenceScaleY = 0.08f;

        public virtual void Activate(float posX, float posY, float speed)
        {
            Transform.Position = new Vector2(posX, posY);
            this.Velocidad = speed;
            IsActive = true;
        }

        public virtual void Deactivate()
        {
            IsActive = false;
            // Se mueve a una posición no visible para cumplir con la reutilización
            Transform.Position = new Vector2(-9999f, -9999f);
        }

        public abstract void Update(float deltaTime);

        protected void EnsureRenderer()
        {
            if (renderer == null)
                renderer = new Renderer(Sprite, ColliderSize);
            else
                renderer.TexturePath = Sprite;
        }

        public virtual void Render(float scaleX = 0.05f, float scaleY = 0.08f)
        {
            EnsureRenderer();
            Transform.Scale = new Vector2(scaleX, scaleY);
            renderer.Draw(Transform);
        }

        public virtual RectangleF GetCollider()
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
