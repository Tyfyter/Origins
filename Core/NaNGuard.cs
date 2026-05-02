using System;
using System.Collections.Generic;
using Terraria;

namespace Origins.Core {
	public readonly ref struct NaNGuard(Entity entity) : IDisposable {
		readonly Vector2 position = entity.position;
		readonly Vector2 velocity = entity.velocity;
		public void Dispose() {
			bool positionNaN = entity.position.HasNaNs();
			bool velocityNaN = entity.velocity.HasNaNs();
			if (positionNaN) entity.position = position;
			if (velocityNaN) entity.velocity = velocity;
#if DEBUG
			if (positionNaN || velocityNaN) throw new Exception($"Set {string.Join(", ", [
				..nameof(position).If(positionNaN),
				..nameof(velocity).If(velocityNaN),
			])} to NaN");
#endif
		}
	}
	public readonly ref struct SingleNaNGuard(ref float variable) : IDisposable {
		readonly ref float variable = ref variable;
		readonly float value = variable;
		public void Dispose() {
			if (float.IsNaN(variable)) {
				variable = value;
#if DEBUG
				throw new Exception($"Value was set to NaN");
#endif
			}
		}
	}
	public static class NaNGuardExtensions {
		public static NaNGuard NaNGuard(this Entity entity) => new(entity);
		public static SingleNaNGuard NaNGuard(ref this float variable) => new(ref variable);
	}
	file static class Extensions {
		public static IEnumerable<T> If<T>(this T value, bool condition) => condition ? [value] : [];
	}
}
