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
    public static class Utils
    {
        public static class Vector
        {
            public static Vector2f Onef => new(1, 1);
            public static Vector2i Onei => new(1, 1);

            public static float GetAngle(Vector2f v1, Vector2f v2)
            {
                return MathF.Atan2(v2.Y - v1.Y, v2.X - v1.X);
            }

            public static float Dot(Vector2f v1, Vector2f v2)
            {
                return (v1.X * v2.X) + (v1.Y * v2.Y);
            }

            public static float Cross(Vector2f v1, Vector2f v2)
            {
                return (v1.X * v2.Y) - (v1.Y * v2.X);
            }

            public static Vector2f Clamp(Vector2f v, Vector2f min, Vector2f max)
            {
                return new Vector2f(
                    Math.Clamp(v.X, min.X, max.X),
                    Math.Clamp(v.Y, min.Y, max.Y)
                );
            }

            public static void Clamp(ref Vector2f v, Vector2f min, Vector2f max)
            {
                v.X = Math.Clamp(v.X, min.X, max.X);
                v.Y = Math.Clamp(v.Y, min.Y, max.Y);
            }

            public static float Distance(Vector2f v1, Vector2f v2)
            {
                return MathF.Sqrt(((v2.X - v1.X) * (v2.X - v1.X)) + ((v2.Y - v1.Y) * (v2.Y - v1.Y)));
            }

            public static float DistanceSquared(Vector2f v1, Vector2f v2)
            {
                return ((v2.X - v1.X) * (v2.X - v1.X)) + ((v2.Y - v1.Y) * (v2.Y - v1.Y));
            }

            public static Vector2f Lerp(Vector2f v1, Vector2f v2, float t)
            {
                return (1 - t) * v1 + t * v2;
            }

            public static Vector2f Max(Vector2f v1, Vector2f v2)
            {
                return new Vector2f(
                    v1.X > v2.X ? v1.X : v2.X,
                    v1.Y > v2.Y ? v1.Y : v2.Y
                );
            }

            public static Vector2f Min(Vector2f v1, Vector2f v2)
            {
                return new Vector2f(
                    v1.X < v2.X ? v1.X : v2.X,
                    v1.Y < v2.Y ? v1.Y : v2.Y
                );
            }
        }

        public static class Collision
        {
            // sources: http://jeffreythompson.org/collision-detection/table_of_contents.php

            public static bool PointPoint(Vector2i p1, Vector2i p2)
            {
                return p1.X == p2.X && p1.Y == p2.X;
            }

            public static bool PointPoint(Vector2f p1, Vector2f p2)
            {
                return p1.X == p2.X && p1.Y == p2.X;
            }

            public static bool PointCircle(Vector2i p, CircleShape c)
            {
                float distX = p.X - c.Center().X;
                float distY = p.Y - c.Center().Y;
                float distance = MathF.Sqrt((distX * distX) + (distY * distY));

                return distance <= c.Radius;
            }

            public static bool PointCircle(Vector2f p, CircleShape c)
            {
                float distX = p.X - c.Center().X;
                float distY = p.Y - c.Center().Y;
                float distance = MathF.Sqrt((distX * distX) + (distY * distY));

                return distance <= c.Radius;
            }

            public static bool CircleCircle(CircleShape c1, CircleShape c2)
            {
                float distX = c1.Center().X - c2.Center().X;
                float distY = c1.Center().Y - c2.Center().Y;
                float distance = MathF.Sqrt((distX * distX) + (distY * distY));

                return distance <= c1.Radius + c2.Radius;
            }

            public static bool PointRectangle(Vector2i p, RectangleShape rect)
            {
                return p.X >= rect.Position.X &&    // right of the left edge AND
                        p.X <= rect.Right() &&      // left of the right edge AND
                        p.Y >= rect.Position.Y &&   // below the top AND
                        p.Y <= rect.Bottom();       // above the bottom
            }

            public static bool PointRectangle(Vector2f p, RectangleShape rect)
            {
                return p.X >= rect.Position.X &&    // right of the left edge AND
                        p.X <= rect.Right() &&      // left of the right edge AND
                        p.Y >= rect.Position.Y &&   // below the top AND
                        p.Y <= rect.Bottom();       // above the bottom
            }

            public static bool RectangleRectangle(RectangleShape rect1, RectangleShape rect2)
            {
                return rect1.Right() >= rect2.Position.X &&     // r1 right edge past r2 left
                        rect1.Position.X <= rect2.Right() &&    // r1 left edge past r2 right
                        rect1.Bottom() >= rect2.Position.Y &&   // r1 top edge past r2 bottom
                        rect1.Position.Y <= rect2.Bottom();     // r1 bottom edge past r2 top   
            }

            public static bool CircleRectangle(CircleShape c, RectangleShape r)
            {
                // temporary variables to set edges for testing
                float testX = c.Center().X;
                float testY = c.Center().Y;

                // which edge is closest?
                if (c.Center().X < r.Position.X) testX = r.Position.X;  // test left edge
                else if (c.Center().X > r.Right()) testX = r.Right();   // right edge
                if (c.Center().Y < r.Position.Y) testY = r.Position.Y;  // top edge
                else if (c.Center().Y > r.Bottom()) testY = r.Bottom(); // bottom edge

                // get distance from closest edges
                float distX = c.Center().X - testX;
                float distY = c.Center().Y - testY;
                float distance = MathF.Sqrt((distX * distX) + (distY * distY));

                // if the distance is less than the radius, collision!
                return distance <= c.Radius;
            }

            public static bool PointPolygon(ConvexShape polygon, Vector2i p)
            {
                bool collision = false;
                Vector2f[] vertices = polygon.GetPoints();
                for (int current = 0; current < vertices.Length; current++)
                {

                    // go through each of the vertices, plus
                    // the next vertex in the list
                    // get next vertex in list
                    // if we've hit the end, wrap around to 0
                    int next = current + 1;
                    if (next == vertices.Length) next = 0;

                    // get the PVectors at our current position
                    // this makes our if statement a little cleaner
                    Vector2f vc = vertices[current];    // c for "current"
                    Vector2f vn = vertices[next];       // n for "next"

                    // compare position, flip 'collision' variable
                    // back and forth
                    if (((vc.Y >= p.Y && vn.Y < p.Y) || (vc.Y < p.Y && vn.Y >= p.Y)) &&
                         (p.X < (vn.X - vc.X) * (p.Y - vc.Y) / (vn.Y - vc.Y) + vc.X))
                    {
                        collision = !collision;
                    }
                }
                return collision;
            }
        }
    }
}
