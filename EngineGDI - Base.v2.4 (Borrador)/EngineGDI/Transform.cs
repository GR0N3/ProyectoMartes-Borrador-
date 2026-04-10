using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineGDI
{
    public class Transform
    {
        public Vector2 Position;
        public Vector2 Scale;
        public float Rotation;

        public Transform()
            : this(Vector2.Zero, Vector2.One)
        {
        }

        public Transform(Vector2 position)
            : this(position, Vector2.One)
        {
        }

        public Transform(Vector2 position, Vector2 scale)
        {
            Position = position;
            Scale = scale;
            Rotation = 0f;
        }
    }
}
