using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
			return new Vector2((float)(pv.R * Math.Cos(pv.Theta)), (float)(pv.R * Math.Sin(pv.Theta)));
		}
		public static explicit operator PolarVec2(Vector2 vec) {
			return new PolarVec2(vec.Length(), vec.ToRotation());
		}
		public PolarVec2 RotatedBy(float offset) {
			return new PolarVec2(R, Theta + offset);
		}
		public PolarVec2 WithRotation(float theta) {
			return new PolarVec2(R, theta);
		}
		public PolarVec2 WithLength(float length) {
			return new PolarVec2(length, Theta);
		}

		public static bool operator ==(PolarVec2 a, PolarVec2 b) {
			return a.Theta == b.Theta && a.R == b.R;
		}
		public static bool operator !=(PolarVec2 a, PolarVec2 b) {
			return a.Theta != b.Theta || a.R != b.R;
		}
		public static PolarVec2 operator *(PolarVec2 a, float scalar) {
			return new PolarVec2(a.R * scalar, a.Theta);
		}
		public static PolarVec2 operator *(float scalar, PolarVec2 a) {
			return new PolarVec2(a.R * scalar, a.Theta);
		}
		public static Vector2 operator *(PolarVec2 a, Vector2 scalar) {
			return ((Vector2)a) * scalar;
		}
		public static Vector2 operator *(Vector2 scalar, PolarVec2 a) {
			return ((Vector2)a) * scalar;
		}
		public static PolarVec2 operator /(PolarVec2 a, float scalar) {
			return new PolarVec2(a.R / scalar, a.Theta);
		}
		public override bool Equals(object obj) {
			return (obj is PolarVec2 other) && other == this;
		}
		public override int GetHashCode() {
			unchecked {
				return (R.GetHashCode() * 397) ^ Theta.GetHashCode();
			}
		}
		public override string ToString() {
			return $"{{r = {R}, θ = {Theta}}}";
		}
		public static PolarVec2 Zero => new PolarVec2();
		public static PolarVec2 UnitRight => new PolarVec2(1, 0);
		public static PolarVec2 UnitUp => new PolarVec2(1, MathHelper.PiOver2);
		public static PolarVec2 UnitLeft => new PolarVec2(1, MathHelper.Pi);
		public static PolarVec2 UnitDown => new PolarVec2(1, -MathHelper.PiOver2);
	}
	public struct Triangle {
		public readonly Vector2 a;
		public readonly Vector2 b;
		public readonly Vector2 c;
		public Triangle(Vector2 a, Vector2 b, Vector2 c) {
			this.a = a;
			this.b = b;
			this.c = c;
		}
		public bool Intersects(Rectangle rect) {
			return Collision.CheckAABBvLineCollision2(rect.TopLeft(), rect.Size(), a, b) ||
			Collision.CheckAABBvLineCollision2(rect.TopLeft(), rect.Size(), b, c) ||
			Collision.CheckAABBvLineCollision2(rect.TopLeft(), rect.Size(), c, a) ||
			Contains(rect.TopLeft());
		}
		public bool Contains(Vector2 point) {
			bool b0 = Vector2.Dot(new Vector2(point.X - a.X, point.Y - a.Y), new Vector2(a.Y - b.Y, b.X - a.X)) > 0;
			bool b1 = Vector2.Dot(new Vector2(point.X - b.X, point.Y - b.Y), new Vector2(b.Y - c.Y, c.X - b.X)) > 0;
			bool b2 = Vector2.Dot(new Vector2(point.X - c.X, point.Y - c.Y), new Vector2(c.Y - a.Y, a.X - c.X)) > 0;
			return (b0 == b1 && b1 == b2);
		}
		public (Vector2 min, Vector2 max) GetBounds() {
			float minX = (int)Math.Min(Math.Min(a.X, b.X), c.X);
			float minY = (int)Math.Min(Math.Min(a.Y, b.Y), c.Y);
			float maxX = (int)Math.Max(Math.Max(a.X, b.X), c.X);
			float maxY = (int)Math.Max(Math.Max(a.Y, b.Y), c.Y);
			return (new Vector2(minX, minY), new Vector2(maxX, maxY));
		}
		public bool HasNaNs() {
			return a.HasNaNs() || b.HasNaNs() || c.HasNaNs();
		}
	}
	public struct Line {
		public readonly Vector2 a;
		public readonly Vector2 b;
		public Vector2 this[float pos] => Vector2.Lerp(a, b, pos);
		public Line(Vector2 a, Vector2 b) {
			this.a = a;
			this.b = b;
		}
		public bool Intersects(Rectangle rect) {
			return Collision.CheckAABBvLineCollision2(rect.TopLeft(), rect.Size(), a, b);
		}
		public (Vector2 min, Vector2 max) GetBounds() {
			float minX = (int)Math.Min(a.X, b.X);
			float minY = (int)Math.Min(a.Y, b.Y);
			float maxX = (int)Math.Max(a.X, b.X);
			float maxY = (int)Math.Max(a.Y, b.Y);
			return (new Vector2(minX, minY), new Vector2(maxX, maxY));
		}
	}
	public static class GeometryUtils {
		public static double AngleDif(double alpha, double beta) {
			double TwoPi = (Math.PI * 2);
			double phi = Math.Abs(beta - alpha) % TwoPi;       // This is either the distance or 360 - distance
			double dir = ((phi > Math.PI) ^ (alpha > beta)) ? -1 : 1;
			double distance = phi > Math.PI ? TwoPi - phi : phi;
			return distance * dir;
		}
		public static float AngleDif(float alpha, float beta, out int dir) {
			float phi = Math.Abs(beta - alpha) % MathHelper.TwoPi;       // This is either the distance or 360 - distance
			dir = ((phi > MathHelper.Pi) ^ (alpha > beta)) ? -1 : 1;
			float distance = phi > MathHelper.Pi ? MathHelper.TwoPi - phi : phi;
			return distance;
		}
		public static bool IsWithin(this Vector2 a, Vector2 b, float range) => a.DistanceSQ(b) < range * range;
		public static Vector2 Vec2FromPolar(float r, float theta) {
			return new Vector2((float)(r * Math.Cos(theta)), (float)(r * Math.Sin(theta)));
		}
		public static bool GetIntersectionPoints(Vector2 start, Vector2 end, Rectangle bounds, out List<Vector2> intersections) {
			return GetIntersectionPoints(start, end, bounds.TopLeft(), bounds.BottomRight(), out intersections);
		}
		public static bool GetIntersectionPoints(Vector2 start, Vector2 end, Vector2 topLeft, Vector2 bottomRight, out List<Vector2> intersections) {
			intersections = new();
			Vector2 intersection;
			if (Intersects(start, end, topLeft, new Vector2(bottomRight.X, topLeft.Y), out intersection)) intersections.Add(intersection);
			if (Intersects(start, end, new Vector2(bottomRight.X, topLeft.Y), bottomRight, out intersection)) intersections.Add(intersection);
			if (Intersects(start, end, bottomRight, new Vector2(topLeft.X, bottomRight.Y), out intersection)) intersections.Add(intersection);
			if (Intersects(start, end, new Vector2(topLeft.X, bottomRight.Y), topLeft, out intersection)) intersections.Add(intersection);
			return intersections.Any();
		}
		public static bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection) {
			intersection = a1;
			Vector2 b = a2 - a1;
			Vector2 d = b2 - b1;
			float bDotDPerp = b.X * d.Y - b.Y * d.X;

			// if b dot d == 0, it means the lines are parallel so have infinite intersection points
			if (bDotDPerp == 0) return true;

			Vector2 c = b1 - a1;
			float t = (c.X * d.Y - c.Y * d.X) / bDotDPerp;
			if (t < 0 || t > 1) {
				return false;
			}

			float u = (c.X * b.Y - c.Y * b.X) / bDotDPerp;
			if (u < 0 || u > 1) {
				return false;
			}
			intersection = a1 + t * b;
			return true;
		}
		public static float? AngleToTarget(Vector2 diff, float speed, float grav = 0.04f, bool high = false) {
			float v2 = speed * speed;
			float v4 = speed * speed * speed * speed;
			grav = -grav;
			float sqr = v4 - grav * (grav * diff.X * diff.X + 2 * diff.Y * v2);
			if (sqr >= 0) {
				sqr = MathF.Sqrt(sqr);
				float offset = diff.X > 0 ? 0 : MathHelper.Pi;
				return (high ? MathF.Atan((v2 + sqr) / (grav * diff.X)) : MathF.Atan((v2 - sqr) / (grav * diff.X))) + offset;
			}
			return null;
		}
	}
}