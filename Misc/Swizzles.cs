using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origins.Misc {
	public static class Swizzles {
		public static Vector2 XX(this Vector3 v) => new(v.X, v.X);
		public static Vector2 XY(this Vector3 v) => new(v.X, v.Y);
		public static Vector2 XZ(this Vector3 v) => new(v.X, v.Z);
		public static Vector2 YX(this Vector3 v) => new(v.Y, v.X);
		public static Vector2 YY(this Vector3 v) => new(v.Y, v.Y);
		public static Vector2 YZ(this Vector3 v) => new(v.Y, v.Z);
		public static Vector2 ZX(this Vector3 v) => new(v.Z, v.X);
		public static Vector2 ZY(this Vector3 v) => new(v.Z, v.Y);
		public static Vector2 ZZ(this Vector3 v) => new(v.Z, v.Z);
	}
}
