using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;

namespace EngineGDI
{
    public class Asteroid
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

        public float velX;
        private string sprite;

        public Asteroid(string sprite, float posX, float posY, float velX)
        {
            Transform = new Transform(new Vector2(posX, posY), new Vector2(0.1f, 0.1f));
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
            Transform.Scale = new Vector2(scaleX, scaleY);
            Engine.Draw(sprite, Transform.Position.X, Transform.Position.Y, Transform.Scale.X, Transform.Scale.Y, Transform.Rotation);
        }
    }
}
