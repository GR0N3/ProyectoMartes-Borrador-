using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace EngineGDI
{
    // Player:
    // - Lee input para moverse en Y
    // - Se mantiene dentro del área jugable (no entra en la HUD)
    // - Tiene un estado de "parpadeo" (invulnerabilidad visual) al recibir daño
    // - Puede desactivar temporalmente su collider para evitar colisiones repetidas
    public class Player
    {
        public Transform Transform;
        public Vector2 SpriteSize { get; private set; }
        public Vector2 ColliderScale { get; set; } = new Vector2(1.2f, 1.2f);
        private float colliderDisabledTimeRemaining = 0f;
        private float blinkTimeRemaining = 0f;
        private float blinkToggleTimer = 0f;
        private bool isVisible = true;

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

        private float speed;
        private string sprite;

        // Velocidad actual en Y (se acumula y amortigua para lograr suavidad)
        private float velY = 0f;

        // Aceleración aplicada al presionar una tecla
        private const float acel = 500f;

        // Factor de fricción (0 = sin fricción, 1 = frena al instante)
        // Valor cercano a 10 da una frenada rápida pero suave
        private const float fricc = 10f;

        // Velocidad máxima en Y
        private const float velMax = 400f;

        private const float DefaultScaleX = 0.05f;
        private const float DefaultScaleY = 0.05f;

        // Constructor:
        // - Carga tamaño del sprite (SpriteSize) leyendo la imagen
        // - Inicializa el Transform con una escala por defecto consistente con el render
        // - Guarda la velocidad (en este script se usa el movimiento suave con velY)
        public Player(string sprite, float posX, float posY, float speed)
        {
            this.sprite = sprite;
            Transform = new Transform(new Vector2(posX, posY), new Vector2(DefaultScaleX, DefaultScaleY));
            this.speed = speed;

            using (var img = Image.FromFile(sprite))
                SpriteSize = new Vector2(img.Width, img.Height);
        }

        // Input del player:
        // - Up/W acelera hacia arriba (velY negativa)
        // - Down/S acelera hacia abajo (velY positiva)
        // - si no hay input aplica fricción para frenar suavemente
        // - clampa la velocidad máxima
        public void Input(float deltaTime)
        {
            if (Engine.IsKeyDown(Keys.Up) || Engine.IsKeyDown(Keys.W))
            {
                velY -= acel * deltaTime;
            }
            else if (Engine.IsKeyDown(Keys.Down) || Engine.IsKeyDown(Keys.S))
            {
                velY += acel * deltaTime;
            }
            else
            {
                // Fricción: reduce la velocidad progresivamente cuando no hay input
                velY -= velY * fricc * deltaTime;
            }

            // Clamp de velocidad máxima
            if (velY > velMax) velY = velMax;
            if (velY < -velMax) velY = -velMax;
        }

        // Update del player:
        // 1) aplica movimiento en Y usando velY
        // 2) clampa el Y para que no salga del área jugable
        // 3) actualiza el timer de desactivación del collider (si está activo)
        // 4) actualiza el parpadeo (blink) cuando está recibiendo daño
        public void Update(float deltaTime, float minY, float screenHeight)
        {
            posY += velY * deltaTime;
            ClampToScreenY(minY, screenHeight);

            if (colliderDisabledTimeRemaining > 0f)
            {
                colliderDisabledTimeRemaining -= deltaTime;
                if (colliderDisabledTimeRemaining < 0f) colliderDisabledTimeRemaining = 0f;
            }

            if (blinkTimeRemaining > 0f)
            {
                blinkTimeRemaining -= deltaTime;
                blinkToggleTimer -= deltaTime;

                if (blinkToggleTimer <= 0f)
                {
                    blinkToggleTimer = 0.1f;
                    isVisible = !isVisible;
                }

                if (blinkTimeRemaining <= 0f)
                    isVisible = true;
            }
        }

        // Render del player:
        // - si está "invisible" por el blink, no dibuja
        // - aplica escala y dibuja el sprite en la posición del Transform
        public void Render(float scaleX = 0.05f, float scaleY = 0.05f)
        {
            if (!isVisible) return;
            Transform.Scale = new Vector2(scaleX, scaleY);
            Engine.Draw(sprite, Transform.Position.X, Transform.Position.Y, Transform.Scale.X, Transform.Scale.Y, Transform.Rotation);
        }

        // Collider del player:
        // - si el collider está deshabilitado temporalmente, devuelve RectangleF.Empty (sin colisión)
        // - si está habilitado, calcula un AABB escalado según el sprite y la escala actual
        public RectangleF GetCollider()
        {
            if (colliderDisabledTimeRemaining > 0f)
                return RectangleF.Empty;

            float width = SpriteSize.X * Transform.Scale.X * ColliderScale.X;
            float height = SpriteSize.Y * Transform.Scale.Y * ColliderScale.Y;

            return new RectangleF(
                Transform.Position.X,
                Transform.Position.Y,
                width,
                height);
        }

        // Deshabilita el collider por "seconds" segundos.
        // Si ya está deshabilitado por más tiempo, mantiene el valor mayor.
        public void DisableCollider(float seconds)
        {
            if (seconds <= 0f) return;
            if (seconds > colliderDisabledTimeRemaining)
                colliderDisabledTimeRemaining = seconds;
        }

        // Intenta aplicar daño:
        // - si ya está en blink, no permite volver a aplicar daño (evita spam)
        // - si puede, inicia el blink por blinkSeconds y devuelve true
        public bool TryTakeDamage(float blinkSeconds = 1f)
        {
            if (blinkTimeRemaining > 0f) return false;
            blinkTimeRemaining = blinkSeconds;
            blinkToggleTimer = 0.1f;
            isVisible = false;
            return true;
        }

        // Mantiene al player dentro del rango vertical permitido:
        // minY suele ser el alto de la HUD para que el player no se meta detrás de la UI.
        private void ClampToScreenY(float minY, float screenHeight)
        {
            float height = SpriteSize.Y * Transform.Scale.Y * ColliderScale.Y;

            if (posY < minY) posY = minY;
            if (posY > screenHeight - height) posY = screenHeight - height;
        }
    }
}
