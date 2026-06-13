namespace EngineGDI
{
    public class PlayerBullet : BulletEntity
    {
        public PlayerBullet(string sprite, float posX, float posY, float speed)
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
            posX += Velocidad * deltaTime;
        }
    }
}
