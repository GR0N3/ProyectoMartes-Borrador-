namespace EngineGDI
{
    public class EnemyBullet : BulletEntity
    {
        public EnemyBullet(string sprite, float posX, float posY, float speed)
        {
            this.Sprite = sprite;
            this.Velocidad = speed;
            this.Dano = 1f;
            this.ColliderSize = new Vector2(26f, 12f);
            this.Transform = new Transform(new Vector2(posX, posY), new Vector2(ColliderReferenceScaleX, ColliderReferenceScaleY));
            this.IsActive = true;
        }

        public override void Update(float deltaTime)
        {
            if (!IsActive) return;
            // Movimiento hacia la izquierda
            posX -= Velocidad * deltaTime;
        }

        public override void Render(float scaleX = 0.05f, float scaleY = 0.08f)
        {
            EnsureRenderer();
            Transform.Scale = new Vector2(scaleX, scaleY);
            renderer.Draw(Transform, 180f, 0.5f, 0.5f);
        }
    }
}
