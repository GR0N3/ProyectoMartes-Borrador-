using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineGDI
{
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 Zero => new Vector2(0f, 0f);
        public static Vector2 One => new Vector2(1f, 1f);

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 v, float s) => new Vector2(v.X * s, v.Y * s);
        public static Vector2 operator *(float s, Vector2 v) => new Vector2(v.X * s, v.Y * s);
        public static Vector2 operator /(Vector2 v, float s) => new Vector2(v.X / s, v.Y / s);
    }
}

