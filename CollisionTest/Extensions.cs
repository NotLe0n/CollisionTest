using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace CollisionTest
{
    public static class Extensions
    {
        #region vector2f
        public static void Deconstruct(this Vector2f vec, out float x, out float y)
        {
            x = vec.X;
            y = vec.Y;
        }

        public static float Length(this Vector2f vec)
        {
            return MathF.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
        }

        public static float LengthSquared(this Vector2f vec)
        {
            return vec.X * vec.X + vec.Y * vec.Y;
        }

        public static Vector2f Normalize(this Vector2f vec)
        {
            return vec / vec.Length();
        }

        public static Vector2f Floor(this ref Vector2f vec)
        {
            vec.X = MathF.Floor(vec.X);
            vec.Y = MathF.Floor(vec.Y);
            return vec;
        }

        public static Vector2f Ceiling(this ref Vector2f vec)
        {
            vec.X = MathF.Ceiling(vec.X);
            vec.Y = MathF.Ceiling(vec.Y);
            return vec;
        }

        public static Vector2f Round(this ref Vector2f vec)
        {
            vec.X = MathF.Round(vec.X);
            vec.Y = MathF.Round(vec.Y);
            return vec;
        }
        #endregion
        #region vector2i
        public static void Deconstruct(this Vector2i vec, out float x, out float y)
        {
            x = vec.X;
            y = vec.Y;
        }

        public static float Length(this Vector2i vec)
        {
            return MathF.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
        }

        public static float LengthSquared<T>(this Vector2i vec)
        {
            return vec.X * vec.X + vec.Y * vec.Y;
        }

        public static Vector2f Normalize(this Vector2i vec)
        {
            return (Vector2f)vec / vec.Length();
        }
        #endregion
        #region Shapes
        public static float Right(this RectangleShape rect)
        {
            return rect.Position.X + rect.Size.X;
        }

        public static float Bottom(this RectangleShape rect)
        {
            return rect.Position.Y + rect.Size.Y;
        }

        public static Vector2f Center(this RectangleShape rect)
        {
            return rect.Position + rect.Size / 2;
        }

        public static Vector2f Center(this CircleShape circle)
        {
            return new Vector2f(circle.Position.X + circle.Radius, circle.Position.Y + circle.Radius);
        }

        public static Vector2f[] GetPoints(this ConvexShape polygon)
        {
            Vector2f[] points = new Vector2f[polygon.GetPointCount()];
            for (uint i = 0; i < polygon.GetPointCount(); i++)
            {
                points[i] = polygon.GetPoint(i) + polygon.Position;
            }

            return points;
        }

        public static void SetPoints(this ConvexShape polygon, Vector2f[] points)
        {
            if (points.Length > polygon.GetPointCount())
                throw new Exception("Array too long");

            for (uint i = 0; i < polygon.GetPointCount(); i++)
            {
                polygon.SetPoint(i, points[i]);
            }
        }
        #endregion
    }
}
