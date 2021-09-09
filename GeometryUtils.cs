using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace Tyfyter.Utils {
    public struct PolarVec2 {
        public float R;
        public float Theta;
        public PolarVec2(float r, float theta) {
            R = r;
            Theta = theta;
        }
        public static explicit operator Vector2(PolarVec2 pv) {
            return new Vector2((float)(pv.R*Math.Cos(pv.Theta)),(float)(pv.R*Math.Sin(pv.Theta)));
        }
        public static explicit operator PolarVec2(Vector2 vec) {
            return new PolarVec2(vec.Length(), vec.ToRotation());
        }
        public static PolarVec2 Zero => new PolarVec2();
        public static PolarVec2 UnitRight => new PolarVec2(1, 0);
        public static PolarVec2 UnitUp => new PolarVec2(1, MathHelper.PiOver2);
        public static PolarVec2 UnitLeft => new PolarVec2(1, MathHelper.Pi);
        public static PolarVec2 UnitDown => new PolarVec2(1, -MathHelper.PiOver2);
    }
}