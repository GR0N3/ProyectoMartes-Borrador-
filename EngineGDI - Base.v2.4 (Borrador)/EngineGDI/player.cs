using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EngineGDI
{
    public class Player
    {
        public Transform Transform;

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

        private string sprite;
        private float speed;

        // Velocidad actual en Y (se acumula y amortigua para lograr suavidad)
        private float velY = 0f;

        // Aceleración aplicada al presionar una tecla
        private const float acel = 500f;

        // Factor de fricción (0 = sin fricción, 1 = frena al instante)
        // Valor cercano a 10 da una frenada rápida pero suave
        private const float fricc = 10f;

        // Velocidad máxima en Y
        private const float velMax = 400f;

        public Player(string sprite, float posX, float posY, float speed)
        {
            this.sprite = sprite;
            Transform = new Transform(new Vector2(posX, posY), new Vector2(0.05f, 0.05f));
            this.speed = speed;
        }

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

        public void Update(float deltaTime)
        {
            posY += velY * deltaTime;
        }

        public void Render(float scaleX = 0.05f, float scaleY = 0.05f)
        {
            Transform.Scale = new Vector2(scaleX, scaleY);
            Engine.Draw(sprite, Transform.Position.X, Transform.Position.Y, Transform.Scale.X, Transform.Scale.Y, Transform.Rotation);
        }
    }
}
