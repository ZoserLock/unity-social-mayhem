using System.Numerics;

namespace Zen.Core
{
    public static class VectorExtensionMethods
    {
        public static bool EpsilonEqual(this Vector3 vector, Vector3 other, float epsilon = 0.001f)
        {
            return Vector3.DistanceSquared(vector, other) < epsilon * epsilon;
        }

        public static Vector3 ToVector3(this System.Numerics.Vector2 vector)
        {
            return new Vector3(vector.X, vector.Y, 0);
        }

        public static Vector2 ToVector2(this System.Numerics.Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
    }
}