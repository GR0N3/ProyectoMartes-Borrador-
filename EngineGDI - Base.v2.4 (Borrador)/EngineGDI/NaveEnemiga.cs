using System;
using System.Collections.Generic;
using System.Drawing;

namespace EngineGDI
{
    public class NaveEnemiga : EnemyEntity
    {
        Animation idle;
        Animation explosion;
        Animation currentAnimation;
        
        private float explosionElapsed = 0f;
        private float shootTimer = 0f;
        private float timeElapsed = 0f;
        private float initialY = 0f;

        public override float posX
        {
            get => Transform.Position.X;
            set => Transform.Position = new Vector2(value, Transform.Position.Y);
        }

        public override float posY
        {
            get => Transform.Position.Y;
            set => Transform.Position = new Vector2(Transform.Position.X, value);
        }

        public float velX;
        private string sprite;
        public Vector2 SpriteSize { get; private set; }

        private bool isDestroying = false;
        public override bool IsDestroying => isDestroying;

        private bool isAlive = true;
        public override bool IsAlive => isAlive;

        // Callback para disparar proyectiles
        public Action<float, float> OnShoot;

        public NaveEnemiga(string sprite, float posX, float posY, float velX)
        {
            this.Vida = 1f; // Soporta 1 impacto de bala
            this.Dano = 1f;  // Quita 1 de vida al chocar
            this.AudioHitPath = "Sounds/Hit_Effect.wav";
            this.AudioDeathPath = "Sounds/Hit_Effect.wav"; // SFX de explosión/hit

            this.sprite = sprite;
            this.velX = velX;
            this.initialY = posY;

            idle = new Animation("idle", new List<string> { sprite }, 0.1f, true);
            Transform = new Transform(new Vector2(posX, posY), new Vector2(0.04f, 0.04f)); // Escala pequeña para que quepa bien en pantalla

            using (var img = Image.FromFile(sprite))
                SpriteSize = new Vector2(img.Width, img.Height);

            CreateAnimations();
            currentAnimation = idle;
        }

        private void CreateAnimations()
        {
            var idleFrames = new List<string> { sprite };
            idle = new Animation("idle", idleFrames, 0.1f, true);

            var explosionFrames = new List<string>
            {
                "Textures/Anims/EnemyDestroy/0.png",
                "Textures/Anims/EnemyDestroy/1.png"
            };
            explosion = new Animation("explosion", explosionFrames, 0.15f, false);
        }

        public override void Update(float deltaTime)
        {
            if (!isAlive) return;

            if (!isDestroying)
            {
                timeElapsed += deltaTime;
                posX -= velX * deltaTime;

                // Movimiento en zig-zag (onda senoidal en el eje Y)
                posY = initialY + (float)Math.Sin(timeElapsed * 3f) * 45f;

                // Disparo automático cada 2 segundos
                shootTimer += deltaTime;
                if (shootTimer >= 2.0f)
                {
                    shootTimer = 0f;
                    // Dispara desde el centro del frente izquierdo de la nave enemiga
                    OnShoot?.Invoke(posX, posY + (SpriteSize.Y * Transform.Scale.Y) / 2f);
                }
            }
            else
            {
                explosionElapsed += deltaTime;
                float explosionTotalDuration = 0.15f * 2; // 2 frames
                if (explosionElapsed >= explosionTotalDuration)
                    isAlive = false;
            }

            currentAnimation.Update(deltaTime);
        }

        public override void Render(float scaleX = 0.04f, float scaleY = 0.04f)
        {
            DrawWithRenderer(currentAnimation.currentFrame, scaleX, scaleY, Transform.Rotation.X);
        }

        public override void Destroy()
        {
            if (!isAlive || isDestroying) return;
            isDestroying = true;
            explosionElapsed = 0f;
            explosion.Reset();
            currentAnimation = explosion;
        }

        public override void Respawn(float posX, float posY, float velX)
        {
            Transform.Position = new Vector2(posX, posY);
            this.initialY = posY;
            this.velX = velX;
            isAlive = true;
            isDestroying = false;
            explosionElapsed = 0f;
            shootTimer = 0f;
            timeElapsed = 0f;
            this.Vida = 1f;
            idle.Reset();
            explosion.Reset();
            currentAnimation = idle;
        }

        public override void Deactivate()
        {
            isAlive = false;
            isDestroying = false;
            explosionElapsed = 0f;
            idle.Reset();
            explosion.Reset();
            currentAnimation = idle;
            Transform.Position = new Vector2(-9999f, -9999f);
        }

        public override RectangleF GetCollider()
        {
            float width = SpriteSize.X * Transform.Scale.X;
            float height = SpriteSize.Y * Transform.Scale.Y;

            return new RectangleF(
                Transform.Position.X,
                Transform.Position.Y,
                width,
                height);
        }
    }
}
