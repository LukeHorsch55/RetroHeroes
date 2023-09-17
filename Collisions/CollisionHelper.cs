using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollisionExample.Collisions
{
    public static class CollisionHelper
    {
        public static bool Collides(BoundingCircle a, BoundingCircle b)
        {
            return Math.Pow(a.Radius + b.Radius, 2) >= Math.Pow(a.Center.X - b.Center.X, 2) + Math.Pow(a.Center.Y - b.Center.Y, 2);
        }

        public static bool Collides(BoundingRectangle a, BoundingRectangle b)
        {
            return !(a.Right < b.Left || a.Left > b.Right || a.Top > b.Bottom || a.Bottom < b.Top);
        }

        public static bool Collides(BoundingCircle a, BoundingRectangle b)
        {
            float nearestX = MathHelper.Clamp(a.Center.X, b.Left, b.Right);
            float nearestY = MathHelper.Clamp(a.Center.Y, b.Top, b.Bottom);

            return Math.Pow(a.Radius, 2) >= Math.Pow(a.Center.X - nearestX, 2) + Math.Pow(a.Center.Y - nearestY, 2);
        }
    }
}
