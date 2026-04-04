using System.Windows.Forms;

namespace EngineGDI
{
    public class player
    {
        public float posX;
        public float posY;

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

        public player(string sprite, float posX, float posY, float speed)
        {
            this.sprite = sprite;
            this.posX      = posX;
            this.posY      = posY;
            this.speed     = speed;
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
            if (velY >  velMax) velY =  velMax;
            if (velY < -velMax) velY = -velMax;
        }

        public void Update(float deltaTime)
        {
            posY += velY * deltaTime;
        }

        public void Render(float scaleX = 0.05f, float scaleY = 0.05f)
        {
            Engine.Draw(sprite, posX, posY, scaleX, scaleY);
        }
    }
}
