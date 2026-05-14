using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;
using V3 = System.Numerics.Vector3;

namespace Origins.Core {
	public abstract class Curve<T>(CurveInterpolation.LoopData? loopData = null) : IEnumerable<Curve<T>.CurveNode> {
		List<CurveNode> nodes = [];
		public Curve<T> Clone() {
			Curve<T> clone = (Curve<T>)MemberwiseClone();
			clone.nodes = [.. nodes];
			return clone;
		}
		public T this[float position] {
			get {
				CurveNode prevNode = default;
				CurveNode currentNode = default;
				if (loopData is CurveInterpolation.LoopData loop) {
					prevNode = nodes[^1];
					if (position < nodes[0].x || position > prevNode.x) {
						currentNode = nodes[0];
						float low = prevNode.x;
						float high = currentNode.x;
						if (position < nodes[0].x) low -= loop.High - loop.Low;
						else high += loop.High - loop.Low;
						return Interpolate(prevNode.value, currentNode.value, currentNode.interpolation.GetProgress(position, low, high));
					}
				}
				for (int i = 0; i < nodes.Count; i++) {
					if (nodes[i].x == position) return nodes[i].value;
					prevNode = currentNode;
					currentNode = nodes[i];
					if (currentNode.x > position) break;
				}
				if (currentNode == null) return default;
				if (position > currentNode.x || prevNode is null) return currentNode.value;
				return Interpolate(prevNode.value, currentNode.value, currentNode.interpolation.GetProgress(position, prevNode.x, currentNode.x));
			}
		}
		public CurveInterpolation.LoopData? loopData = loopData;
		public void Add(CurveNode node) {
			nodes.InsertOrdered(node);
		}
		public void Reorder(CurveNode node) {
			nodes.Remove(node);
			nodes.InsertOrdered(node);
		}
		public abstract T Interpolate(T a, T b, float progress);
		public class CurveNode(float x, T value) : IComparable<CurveNode> {
			public bool Locked { get; init; } = false;
			public float x = x;
			public T value = value;
			public CurveInterpolation interpolation = new CurveInterpolation.Linear();
			public int CompareTo(CurveNode other) => x.CompareTo(other.x);
		}

		IEnumerator<CurveNode> IEnumerable<CurveNode>.GetEnumerator() => ((IEnumerable<CurveNode>)nodes).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)nodes).GetEnumerator();
	}
	public abstract class CurveInterpolation {
		public record struct LoopData(float Low, float High);
		public abstract float GetProgress(float aX, float bX, float x);
		public class Linear : CurveInterpolation {
			public override float GetProgress(float x, float aX, float bX) => Utils.Remap(x, aX, bX, 0, 1);
		}
	}
	public class HSLCurve(CurveInterpolation.LoopData? loopData = null) : Curve<HSLColor>(loopData) {
		public override HSLColor Interpolate(HSLColor a, HSLColor b, float progress) => HSLColor.Lerp(a, b, progress);
	}
	public struct HSLColor(float H, float S, float L) {
		public float H = WrapHue(H), S = S, L = L;
		public float Hue {
			readonly get => WrapHue(H);
			set => H = WrapHue(value);
		}
		public float Saturation {
			readonly get => Math.Clamp(S, 0, 1);
			set => S = Math.Clamp(value, 0, 1);
		}
		public float Lightness {
			readonly get => Math.Clamp(L, 0, 1);
			set => L = Math.Clamp(value, 0, 1);
		}
		public Color RGB {
			readonly get => Main.hslToRgb(WrapHue(H), S, L);
			set => Main.rgbToHsl(value).Deconstruct(out H, out S, out L);
		}
		public Vector3 RGBV {
			readonly get {
				if (S == 0f) {
					return new(L);
				} else {
					float H = WrapHue(this.H);
					float t2 = L >= 0.5f ? L + S - L * S : (L * (1 + S));
					float t = 2f * L - t2;
					float r = H + 1f / 3f;
					float g = H;
					float b = H - 1f / 3f;
					return new(
						hue2rgb(r, t, t2),
						hue2rgb(g, t, t2),
						hue2rgb(b, t, t2)
					);
				}
				static float hue2rgb(float c, float t1, float t2) {
					if (c < 0.0)
						c += 1;

					if (c > 1.0)
						c -= 1;

					if (6 * c < 1)
						return t1 + (t2 - t1) * 6f * c;

					if (2 * c < 1)
						return t2;

					if (3 * c < 2)
						return t1 + (t2 - t1) * (2f / 3f - c) * 6;

					return t1;
				}
			}
			set {
				float max = Math.Max(Math.Max(value.X, value.Y), value.Z);
				float min = Math.Min(Math.Min(value.X, value.Y), value.Z);
				L = (max + min) / 2f;
				if (max == min) {
					S = 0;
				} else {
					float diff = max - min;
					S = diff / (L > 0.5f ? (2f - max - min) : (max + min));
					if (max == value.X)
						H = (value.Y - value.Z) / diff + ((value.Y < value.Z) ? 6 : 0);

					if (max == value.Y)
						H = (value.Z - value.X) / diff + 2f;

					if (max == value.Z)
						H = (value.X - value.Y) / diff + 4f;

					H /= 6f;
				}
			}
		}
		public static HSLColor Lerp(HSLColor a, HSLColor b, float progress) => new(
				a.H + GeometryUtils.AngleDif(a.H * hToAngle, b.H * hToAngle, out int dir) * angleToH * dir * progress,
				float.Lerp(a.S, b.S, progress),
				float.Lerp(a.L, b.L, progress)
			);
		const float hToAngle = MathHelper.TwoPi / 1f;
		const float angleToH = 1f / MathHelper.TwoPi;
		public static float WrapHue(float hue) {
			if (hue >= 0 && hue < 1) return hue;
			hue %= 1;
			if (hue < 0) return hue + 1;
			return hue;
		}
		public static implicit operator HSLColor(Color value) {
			HSLColor output = default;
			output.RGB = value;
			return output;
		}
	}
}
