namespace EngineGDI
{
    public abstract class EnemyBullet : BulletEntity
    {
        protected float renderAngle = 180f;

        protected EnemyBullet(string sprite, float posX, float posY, float speed)
        {
            this.Sprite = sprite;
            this.Velocidad = speed;
            this.Dano = 1f;
            this.ColliderSize = new Vector2(26f, 12f);
            this.Transform = new Transform(new Vector2(posX, posY), new Vector2(ColliderReferenceScaleX, ColliderReferenceScaleY));
            this.IsActive = true;
        }

        public override void Render(float scaleX = 0.05f, float scaleY = 0.08f)
        {
            EnsureRenderer();
            Transform.Scale = new Vector2(scaleX, scaleY);
            renderer.Draw(Transform, renderAngle, 0.5f, 0.5f);
        }

        protected static Vector2 Normalize(Vector2 vector)
        {
            float length = (float)System.Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y));
            if (length <= 0f)
                return new Vector2(-1f, 0f);

            return new Vector2(vector.X / length, vector.Y / length);
        }

        protected static float GetAngleDegrees(Vector2 vector)
        {
            return (float)(System.Math.Atan2(vector.Y, vector.X) * (180.0 / System.Math.PI));
        }
    }
}
