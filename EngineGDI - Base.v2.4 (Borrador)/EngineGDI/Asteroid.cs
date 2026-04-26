using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace EngineGDI
{
    // Asteroide individual.
    // Se mueve hacia la izquierda, puede explotar con una animación y expone un collider para colisiones.
    public class Asteroid
    {
        public Transform Transform;
        Animation idle;
        Animation explosion;
        Animation currentAnimation;
        private const float ExplosionFrameDuration = 0.1f;
        private const int ExplosionFrameCount = 6;
        private float explosionElapsed = 0f;
        private const float ColliderReferenceScaleX = 0.1f;
        private const float ColliderReferenceScaleY = 0.1f;

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

        public float velX;
        private string sprite;
        public string Sprite => sprite;
        public bool IsDestroying { get; private set; } = false;
        public bool IsAlive { get; private set; } = true;
        public Vector2 ColliderSize { get; } = new Vector2(172f, 172f);

        // Crea un asteroide con sprite base (idle), posición inicial y velocidad en X.
        // También prepara la animación de explosión.
        public Asteroid(string sprite, float posX, float posY, float velX)
        {
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
        private void CreateAnimations ()
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
            

        public void Input()
        {

        }

        // Actualiza el asteroide:
        // Si está vivo y NO está destruyéndose: se mueve a la izquierda.
        // Si está destruyéndose: avanza el tiempo de explosión y al terminar se marca como no-vivo.
        // Siempre actualiza la animación actual (idle o explosion).
        public void Update(float deltaTime)
        {
            if (!IsAlive) return;

            if (!IsDestroying)
                posX -= velX * deltaTime;
            else
            {
                explosionElapsed += deltaTime;
                float explosionTotalDuration = ExplosionFrameDuration * ExplosionFrameCount;
                if (explosionElapsed >= explosionTotalDuration)
                    IsAlive = false;
            }

            currentAnimation.Update(deltaTime);
        }

        // Dibuja el frame actual de la animación en la posición del Transform.
        public void Render(float scaleX = 0.1f, float scaleY = 0.1f)
        {
            Transform.Scale = new Vector2(scaleX, scaleY);
            Engine.Draw(currentAnimation.currentFrame, Transform.Position.X, Transform.Position.Y, Transform.Scale.X, Transform.Scale.Y, Transform.Rotation);
        }

        // Inicia la destrucción:
        // cambia la animación a explosión y resetea el timer.
        public void Destroy()
        {
            if (!IsAlive || IsDestroying) return;
            IsDestroying = true;
            explosionElapsed = 0f;
            explosion.Reset();
            currentAnimation = explosion;
        }

        // Reutiliza este asteroide para volver a aparecer (respawn):
        // resetea estado, posición, escala de referencia, velocidad y animaciones.
        public void Respawn(float posX, float posY, float velX)
        {
            Transform.Position = new Vector2(posX, posY);
            Transform.Scale = new Vector2(ColliderReferenceScaleX, ColliderReferenceScaleY);
            this.velX = velX;
            IsAlive = true;
            IsDestroying = false;
            explosionElapsed = 0f;
            idle.Reset();
            explosion.Reset();
            currentAnimation = idle;
        }

        // Desactiva el asteroide para devolverlo a la pool:
        // queda no-vivo y resetea animaciones/estado.
        public void Deactivate()
        {
            IsAlive = false;
            IsDestroying = false;
            explosionElapsed = 0f;
            idle.Reset();
            explosion.Reset();
            currentAnimation = idle;
        }

        // Devuelve el rectángulo de colisión (AABB) escalado según el Transform.
        // Se usa para detectar colisión bala ↔ asteroide y player ↔ asteroide.
        public RectangleF GetCollider()
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
