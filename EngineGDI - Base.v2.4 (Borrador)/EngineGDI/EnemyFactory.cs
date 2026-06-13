using System;

namespace EngineGDI
{
    public static class EnemyFactory
    {
        public static EnemyEntity CreateEnemy(EnemyType type, string sprite, float posX, float posY, float speed, Action<float, float> onShootCallback = null)
        {
            switch (type)
            {
                case EnemyType.Asteroid:
                    return new Asteroid(sprite, posX, posY, speed);
                case EnemyType.NaveEnemiga:
                    var nave = new NaveEnemiga(sprite, posX, posY, speed);
                    nave.OnShoot = onShootCallback;
                    return nave;
                default:
                    throw new ArgumentException("Unknown enemy type");
            }
        }
    }
}
