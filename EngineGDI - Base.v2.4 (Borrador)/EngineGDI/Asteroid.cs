using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineGDI
{
    public class Asteroid
    {
        public float posX;
        public float posY;

        public float velX;
        private string sprite;

        public Asteroid(string sprite,float posX, float posY, float velX) 
        {
            this.posX = posX;
            this.posY = posY;
            this.velX = velX;
            this.sprite = sprite;
        }

        public void Input() 
        {

        }

        public void Update(float deltaTime) 
        {
            posX -= velX * deltaTime;
        }

        public void Render(float scaleX = 0.1f, float scaleY = 0.1f) 
        {
            Engine.Draw(sprite, posX, posY, scaleX, scaleY);
        }

    }
}
