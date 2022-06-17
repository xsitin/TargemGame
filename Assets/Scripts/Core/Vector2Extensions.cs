using UnityEngine;

namespace Platformer.Core
{
    public static class Vector2Extensions
    {
        public static Vector2 WithX(this Vector2 vector2, float x)
        {
            return new Vector2(x, vector2.y);
        }

        public static Vector2 WithY(this Vector2 vector2, float y)
        {
            return new Vector2(vector2.x, y);
        }
    }
}