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
    public static class GeometryUtils {
        public static double AngleDif(double alpha, double beta) {
            double phi = Math.Abs(beta - alpha) % (Math.PI * 2);       // This is either the distance or 360 - distance
            double distance = ((phi > Math.PI) ^ (alpha > beta)) ? (Math.PI * 2) - phi : phi;
            return distance;
        }
        public static float AngleDif(float alpha, float beta, out int dir) {
            float phi = Math.Abs(beta - alpha) % MathHelper.TwoPi;       // This is either the distance or 360 - distance
            dir = ((phi > MathHelper.Pi) ^ (alpha > beta)) ? -1 : 1;
            float distance = phi > MathHelper.Pi ? MathHelper.TwoPi - phi : phi;
            return distance;
        }
    }
}