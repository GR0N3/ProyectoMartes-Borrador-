namespace EngineGDI
{
    public class NormalBullet : EnemyBullet
    {
        public NormalBullet(string sprite, float posX, float posY, float speed)
            : base(sprite, posX, posY, speed)
        {
            renderAngle = 180f;
        }

        public override void Update(float deltaTime)
        {
            if (!IsActive) return;

            posX -= Velocidad * deltaTime;
        }
    }
}
