using System;

namespace EngineGDI
{
    public static class EnemyFactory
    {
        public static EnemyEntity CreateEnemy(EnemyType type, string sprite, float posX, float posY, float speed, Action<float, float, BulletType> onShootCallback = null)
        {
            switch (type)
            {
                case EnemyType.Asteroid:
                    return new Asteroid(sprite, posX, posY, speed);
                case EnemyType.NaveEnemigaRoja:
                    var naveRoja = new NaveEnemiga(
                        sprite,
                        "Textures/Anims/EnemyDestroy/00.png",
                        "Textures/Anims/EnemyDestroy/01.png",
                        BulletType.NormalBullet,
                        posX,
                        posY,
                        speed);
                    naveRoja.OnShoot = onShootCallback;
                    return naveRoja;
                case EnemyType.NaveEnemigaAzul:
                    var naveAzul = new NaveEnemiga(
                        sprite,
                        "Textures/Anims/EnemyDestroy/10.png",
                        "Textures/Anims/EnemyDestroy/11.png",
                        BulletType.AngularBullet,
                        posX,
                        posY,
                        speed);
                    naveAzul.OnShoot = onShootCallback;
                    return naveAzul;
                default:
                    throw new ArgumentException("Unknown enemy type");
            }
        }
    }
}
