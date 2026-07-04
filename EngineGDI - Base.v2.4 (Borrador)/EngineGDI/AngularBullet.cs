namespace EngineGDI
{
    public class AngularBullet : EnemyBullet
    {
        private static readonly System.Random random = new System.Random();
        private static readonly Vector2 upwardDirection = Normalize(new Vector2(-1f, -1f));
        private static readonly Vector2 downwardDirection = Normalize(new Vector2(-1f, 1f));
        private Vector2 currentDirection = downwardDirection;

        public AngularBullet(string sprite, float posX, float posY, float speed)
            : base(sprite, posX, posY, speed)
        {
            renderAngle = GetAngleDegrees(currentDirection);
        }

        public override void Activate(float posX, float posY, float speed)
        {
            base.Activate(posX, posY, speed);
            currentDirection = random.Next(0, 2) == 0 ? upwardDirection : downwardDirection;
            renderAngle = GetAngleDegrees(currentDirection);
        }

        public override void Update(float deltaTime)
        {
            if (!IsActive) return;

            posX += currentDirection.X * Velocidad * deltaTime;
            posY += currentDirection.Y * Velocidad * deltaTime;
        }
    }
}
