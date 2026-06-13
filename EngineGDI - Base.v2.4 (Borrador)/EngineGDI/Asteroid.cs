using System;
using System.Collections.Generic;
using System.Drawing;

namespace EngineGDI
{
    // Asteroide individual.
    // Se mueve hacia la izquierda, puede explotar con una animación y expone un collider para colisiones.
    public class Asteroid : EnemyEntity
    {
        Animation idle;
        Animation explosion;
        Animation currentAnimation;
        private const float ExplosionFrameDuration = 0.1f;
        private const int ExplosionFrameCount = 6;
        private float explosionElapsed = 0f;
        private const float ColliderReferenceScaleX = 0.1f;
        private const float ColliderReferenceScaleY = 0.1f;

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
        public string Sprite => sprite;

        private bool isDestroying = false;
        public override bool IsDestroying => isDestroying;

        private bool isAlive = true;
        public override bool IsAlive => isAlive;

        public Vector2 ColliderSize { get; } = new Vector2(172f, 172f);

        // Crea un asteroide con sprite base (idle), posición inicial y velocidad en X.
        // También prepara la animación de explosión.
        public Asteroid(string sprite, float posX, float posY, float velX)
        {
            // Inicialización de campos de EnemyEntity
            this.Vida = 1f;
            this.Dano = 1f;
            this.AudioHitPath = "Sounds/Hit_Effect.wav";
            this.AudioDeathPath = "Sounds/Hit_Effect.wav";

            idle = new Animation("idle", new List<string> { sprite }, 0.1f, true);
            Transform = new Transform(new Vector2(posX, posY), new Vector2(0.1f, 0.1f));
            this.velX = velX;
            this.sprite = sprite;
            CreateAnimations();
            currentAnimation = idle;
        }

        // Crea/recarga las animaciones:
        // idle: 1 frame (el sprite original)
        // explosion: 6 frames (Textures/Anims/AsteroidDestroy/0..5.png)
        private void CreateAnimations()
        {
            var idleFrames = new List<string>();
            idleFrames.Add(sprite);
            idle = new Animation("idle", idleFrames, 0.1f, true);

            var explosionFrames = new List<string>();
            for (int i = 0; i < 6; i++)
            {
                explosionFrames.Add($"Textures/Anims/AsteroidDestroy/{i}.png");
            }
            explosion = new Animation("explosion", explosionFrames, 0.1f, false);
        }

        // Actualiza el asteroide:
        // Si está vivo y NO está destruyéndose: se mueve a la izquierda.
        // Si está destruyéndose: avanza el tiempo de explosión y al terminar se marca como no-vivo.
        // Siempre actualiza la animación actual (idle o explosion).
        public override void Update(float deltaTime)
        {
            if (!isAlive) return;

            if (!isDestroying)
                posX -= velX * deltaTime;
            else
            {
                explosionElapsed += deltaTime;
                float explosionTotalDuration = ExplosionFrameDuration * ExplosionFrameCount;
                if (explosionElapsed >= explosionTotalDuration)
                    isAlive = false;
            }

            currentAnimation.Update(deltaTime);
        }

        // Dibuja el frame actual de la animación en la posición del Transform.
        public override void Render(float scaleX = 0.05f, float scaleY = 0.05f)
        {
            DrawWithRenderer(currentAnimation.currentFrame, scaleX, scaleY, Transform.Rotation.X);
        }

        // Inicia la destrucción:
        // cambia la animación a explosión y resetea el timer.
        public override void Destroy()
        {
            if (!isAlive || isDestroying) return;
            isDestroying = true;
            explosionElapsed = 0f;
            explosion.Reset();
            currentAnimation = explosion;
        }

        // Reutiliza este asteroide para volver a aparecer (respawn):
        // resetea estado, posición, escala de referencia, velocidad y animaciones.
        public override void Respawn(float posX, float posY, float velX)
        {
            Transform.Position = new Vector2(posX, posY);
            Transform.Scale = new Vector2(ColliderReferenceScaleX, ColliderReferenceScaleY);
            this.velX = velX;
            isAlive = true;
            isDestroying = false;
            explosionElapsed = 0f;
            this.Vida = 1f;
            idle.Reset();
            explosion.Reset();
            currentAnimation = idle;
        }

        // Desactiva el asteroide para devolverlo a la pool:
        // queda no-vivo y resetea animaciones/estado.
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

        // Devuelve el rectángulo de colisión (AABB) escalado según el Transform.
        // Se usa para detectar colisión bala ↔ asteroide y player ↔ asteroide.
        public override RectangleF GetCollider()
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
