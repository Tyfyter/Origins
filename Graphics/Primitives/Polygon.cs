using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;

namespace Origins.Graphics.Primitives;
public class Polygon : IMoveToPegFlag {
	public readonly VertexPositionColorTexture[] vertices;
	public readonly Vector2[] uvPositions;
	public readonly Vector2 baseSize;
	public Vector2 ScaledSize { get; private set; }
	readonly Vector2[] vertexPositions;
	readonly short[] indices;
	readonly short[] outlineIndices;
	readonly short[] wireframeIndices;
	public Polygon(params Span<Vector2> vertexPositions) {
		vertices = new VertexPositionColorTexture[vertexPositions.Length];
		uvPositions = new Vector2[vertexPositions.Length];
		this.vertexPositions = new Vector2[vertexPositions.Length];
		vertexPositions.CopyTo(this.vertexPositions);
		Vector2 lowerBounds = new(float.PositiveInfinity);
		Vector2 upperBounds = new(float.NegativeInfinity);
		for (int i = 0; i < vertexPositions.Length; i++) {
			Max(ref upperBounds.X, vertexPositions[i].X);
			Max(ref upperBounds.Y, vertexPositions[i].Y);

			Min(ref lowerBounds.X, vertexPositions[i].X);
			Min(ref lowerBounds.Y, vertexPositions[i].Y);
		}
		for (int i = 0; i < vertexPositions.Length; i++) {
			uvPositions[i] = vertexPositions[i].GetLerpValue(lowerBounds, upperBounds);
		}
		baseSize = upperBounds - lowerBounds;
		#region Find winding direction of a known triangle
		float minY = float.PositiveInfinity;
		int minIndex = -1;
		for (int i = 0; i < vertexPositions.Length; i++) {
			if (Minimize(ref minY, vertexPositions[i].Y)) minIndex = i;
		}
		int _a = this.vertexPositions.Modulo(minIndex - 1);
		int _c = this.vertexPositions.Modulo(minIndex + 1);
		Triangle nextT = new(vertexPositions[_a], vertexPositions[minIndex], vertexPositions[_c]);
		int direction = double.Sign(Winding(nextT));
		#endregion

		List<short> remainingIndices = new(Enumerable.Range(0, vertexPositions.Length).Select(v => (short)v));
		List<Triangle> addedTris = [];
		List<short> indices = new();
		List<TriOption> options = [];
		for (int i = 0; i < remainingIndices.Count; i++) {
			if (VertexTri(remainingIndices, (short)i, direction) is TriOption option) options.InsertOrdered(option);
		}
		doIt:
		while (options.Count > 0) {
			for (int i = options.Count - 1; i >= 0; i--) {
				(Triangle tri, short a, short b, short c) = options[i];
				options.RemoveAt(i);
				if (!remainingIndices.Contains(b)) continue;
				if (!addedTris.Any(o => tri.Intersects(o))) {
					indices.Add(a);
					indices.Add(b);
					indices.Add(c);
					addedTris.Add(tri);
					remainingIndices.Remove(b);
					if (remainingIndices.Count <= 0) break;
					if (VertexTri(remainingIndices, a, direction) is TriOption optA) options.InsertOrdered(optA);
					if (VertexTri(remainingIndices, b, direction) is TriOption optB) options.InsertOrdered(optB);
					i = options.Count - 1;
				}
			}
		}
		if (remainingIndices.Count > 0) {
			for (int i = 0; i < remainingIndices.Count; i++) {
				if (VertexTri(remainingIndices, remainingIndices[i], direction) is TriOption option && !addedTris.Any(o => option.Tri.Intersects(o))) {
					options.InsertOrdered(option);
				}
			}
			if (options.Count > 0) goto doIt;
		}
		/*while (remainingIndices.Count > 2) {
			bool any = false;
			for (int i = vertexPositions.Length - 1; i >= 0; i--) {
				if (i >= remainingIndices.Count) continue;
				short a = remainingIndices[i];
				short b = remainingIndices[(i + 1) % remainingIndices.Count];
				short c = remainingIndices[(i + 2) % remainingIndices.Count];
				Triangle tri = new(vertexPositions[a], vertexPositions[b], vertexPositions[c]);
				if (IsDegenerate(tri)) continue;
				_ = GeometryUtils.AngleDif((tri.b - tri.a).ToRotation(), (tri.c - tri.b).ToRotation(), out int dir);
				if (tri.b == default && dir != direction) ;
				if (dir != -direction) continue;
				for (int j = 0; j < vertexPositions.Length; j++) {
					int cur = (i + j) % vertexPositions.Length;
					if (a == cur) continue;
					if (b == cur) continue;
					if (c == cur) continue;
					if (tri.Contains(vertexPositions[cur])) goto fail;
				}
				indices.Add(a);
				indices.Add(b);
				indices.Add(c);
				remainingIndices.RemoveAt((i + 1) % remainingIndices.Count);
				any = true;
				fail:;
			}
			if (!any) break;
		}*/
		this.indices = indices.ToArray();

		outlineIndices = new short[vertices.Length + 1];
		for (short i = 0; i < outlineIndices.Length; i++) outlineIndices[i] = i;
		outlineIndices[^1] = 0;

		HashSet<UnorderedTuple<short>> lines = [];
		for (int i = 0; i < this.indices.Length / 3; i++) {
			int baseInd = i * 3;
			int wfInd = i * 6;
			lines.Add(new(this.indices[baseInd], this.indices[baseInd + 1]));
			lines.Add(new(this.indices[baseInd + 1], this.indices[baseInd + 2]));
			lines.Add(new(this.indices[baseInd + 2], this.indices[baseInd]));
		}
		wireframeIndices = lines.SelectMany<UnorderedTuple<short>, short>(l => [l.a, l.b]).ToArray();
		ResetVertices();
	}
	public static Polygon Import(float size, Alignment alignment, params Span<Vector2> vertexPositions) {
		switch (alignment) {
			case Alignment.None:
			Vector2 lowerBounds = new(float.PositiveInfinity);
			Vector2 upperBounds = new(float.NegativeInfinity);
			for (int i = 0; i < vertexPositions.Length; i++) {
				Max(ref upperBounds.X, vertexPositions[i].X);
				Max(ref upperBounds.Y, vertexPositions[i].Y);

				Min(ref lowerBounds.X, vertexPositions[i].X);
				Min(ref lowerBounds.Y, vertexPositions[i].Y);
			}
			for (int i = 0; i < vertexPositions.Length; i++) vertexPositions[i] *= size / upperBounds.Max();
			return new Polygon(vertexPositions);

			default:
			return Import(size, Vector2.Zero, vertexPositions);

			case Alignment.TopRight:
			return Import(size, Vector2.UnitX, vertexPositions);

			case Alignment.BottomLeft:
			return Import(size, Vector2.UnitY, vertexPositions);

			case Alignment.BottomRight:
			return Import(size, Vector2.One, vertexPositions);

			case Alignment.Center:
			return Import(size, new Vector2(0.5f), vertexPositions);
		}
	}
	public static Polygon Import(float size, Vector2 origin, params Span<Vector2> vertexPositions) {
		Vector2 lowerBounds = new(float.PositiveInfinity);
		Vector2 upperBounds = new(float.NegativeInfinity);
		for (int i = 0; i < vertexPositions.Length; i++) {
			Max(ref upperBounds.X, vertexPositions[i].X);
			Max(ref upperBounds.Y, vertexPositions[i].Y);

			Min(ref lowerBounds.X, vertexPositions[i].X);
			Min(ref lowerBounds.Y, vertexPositions[i].Y);
		}
		origin *= upperBounds - lowerBounds;
		for (int i = 0; i < vertexPositions.Length; i++) vertexPositions[i] = (vertexPositions[i] - lowerBounds - origin) * size / (upperBounds - lowerBounds).Max();
		return new Polygon(vertexPositions);
	}
	public enum Alignment {
		/// <summary>
		/// Padding will affect size and UV coordinates
		/// </summary>
		None,
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
		Center
	}
	public Polygon ResetVertices() {
		for (int i = 0; i < vertices.Length; i++) {
			vertices[i] = new(
				new(vertexPositions[i], 0),
				Color.White,
				uvPositions[i]
			);
		}
		ScaledSize = baseSize;
		return this;
	}
	public Polygon ResetPositions() {
		for (int i = 0; i < vertices.Length; i++) Pos(i) = vertexPositions[i];
		ScaledSize = baseSize;
		return this;
	}
	public Polygon ResetColors() {
		for (int i = 0; i < vertices.Length; i++) vertices[i].Color = Color.White;
		return this;
	}
	public Polygon ResetUVs() {
		for (int i = 0; i < vertices.Length; i++) vertices[i].TextureCoordinate = uvPositions[i];
		return this;
	}
	public Polygon Scale(float scale) => Scale(new Vector2(scale));
	public Polygon Scale(Vector2 scale) {
		for (int i = 0; i < vertices.Length; i++) {
			Pos(i) *= scale;
		}
		ScaledSize *= scale;
		return this;
	}
	public Polygon Translate(Vector2 offset) {
		for (int i = 0; i < vertices.Length; i++) {
			Pos(i) += offset;
		}
		return this;
	}
	public Polygon Rotate(float angle) {
		Vector2 x = new(MathF.Cos(angle), -MathF.Sin(angle));
		Vector2 y = new(-x.Y, x.X);
		for (int i = 0; i < vertices.Length; i++) {
			OriginExtensions.MatrixMult(ref Pos(i), x, y);
		}
		return this;
	}
	public bool Contains(Vector2 point) {
		for (int i = 0; i < indices.Length; i += 3) if (new Triangle(
			Pos(indices[i]),
			Pos(indices[i + 1]),
			Pos(indices[i + 2])
		).Contains(point)) return true;
		return false;
	}
	public void Draw() {
		if (indices.Length <= 0) return;
		Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
		Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
		/*for (int i = 0; i < indices.Length / 3; i++) {
			Color color = Main.hslToRgb(i / (float)(indices.Length / 3), 1f, 0.4f);
			using ScopedOverride<Color> a = vertices[indices[i * 3]].Color.ScopedOverride(color);
			using ScopedOverride<Color> b = vertices[indices[i * 3 + 1]].Color.ScopedOverride(color.MultiplyRGB(Color.DarkGray));
			using ScopedOverride<Color> c = vertices[indices[i * 3 + 2]].Color.ScopedOverride(color.MultiplyRGB(new(50, 50, 50)));
			Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, i * 3, 1);
		}
		Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineStrip, vertices, 0, vertices.Length, Enumerable.Range(0, vertices.Length).ToArray(), 0, vertices.Length);
		for (int i = 0; i < indices.Length / 3; i++) {
			Triangle trangle = new(vertices[indices[i * 3]].Position.XY(), vertices[indices[i * 3 + 1]].Position.XY(), vertices[indices[i * 3 + 2]].Position.XY());
			Main.spriteBatch.DrawString(
				Terraria.GameContent.FontAssets.ItemStack.Value,
				i.ToString(),
				(trangle.a + trangle.b + trangle.c) / 3,
				Color.White
			);
			if (trangle.Contains(Main.MouseScreen)) {
				Main.spriteBatch.DrawString(
					Terraria.GameContent.FontAssets.ItemStack.Value,
					indices[(i * 3)].ToString(),
					trangle.a,
					Color.White
				);
				Main.spriteBatch.DrawString(
					Terraria.GameContent.FontAssets.ItemStack.Value,
					indices[(i * 3 + 1)].ToString(),
					trangle.b,
					Color.White
				);
				Main.spriteBatch.DrawString(
					Terraria.GameContent.FontAssets.ItemStack.Value,
					indices[(i * 3 + 2)].ToString(),
					trangle.c,
					Color.White
				);
			}
		}
		Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(
			PrimitiveType.LineStrip,
			vertices,
			0,
			vertices.Length,
			Enumerable.Range(0, vertexPositions.Length).SelectMany<int, short>(v => [(short)v, (short)((v + 1) % vertexPositions.Length)]).ToArray(),
			0,
			vertices.Length
		);*/
	}
	public void DrawOutline() {
		Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
		Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineStrip, vertices, 0, vertices.Length, outlineIndices, 0, vertices.Length);
	}
	public void DrawWireframe() {
		Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
		Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, vertices, 0, vertices.Length, wireframeIndices, 0, wireframeIndices.Length / 2);
	}
	ref Vector2 Pos(int i) => ref Unsafe.As<Vector3, Vector2>(ref vertices[i].Position);
	TriOption? VertexTri(List<short> remainingIndices, short around, int windingOrder = 1) {
		int b = remainingIndices.IndexOf(around);
		int a = remainingIndices[remainingIndices.Modulo(b - 1)];
		int c = remainingIndices[remainingIndices.Modulo(b + 1)];
		Triangle nextT = new(vertexPositions[a], vertexPositions[around], vertexPositions[c]);
		if (Winding(nextT) * windingOrder <= 0) return null;
		if (vertexPositions.Any(p => nextT.a != p && nextT.b != p && nextT.c != p && nextT.Contains(p))) return null;
		return new(nextT, (short)a, around, (short)c);
	}
	readonly record struct TriOption(Triangle Tri, short A, short B, short C) : IComparable<TriOption> {
		readonly float sortPriority = -GeometryUtils.AngleDif((Tri.a - Tri.b).ToRotation(), (Tri.c - Tri.b).ToRotation(), out _);//Area(Tri);
		public readonly int CompareTo(TriOption other) => sortPriority.CompareTo(other.sortPriority);
	}
	struct Flag : IMovedToPegFlag;
	static float Winding(Triangle tri) {
		return tri.a.X * (tri.b.Y - tri.c.Y)
			 + tri.b.X * (tri.c.Y - tri.a.Y)
			 + tri.c.X * (tri.a.Y - tri.b.Y);
	}
	public VertexCache ModificationContext() => new(this);
	List<VertexPositionColorTexture[]> modificationCache;
	int modificationCount;
	public readonly ref struct VertexCache {
		readonly Polygon polygon;
		readonly VertexPositionColorTexture[] pool;
		public VertexCache(Polygon polygon) {
			this.polygon = polygon;
			polygon.modificationCache ??= [];
			if (polygon.modificationCount >= polygon.modificationCache.Count) polygon.modificationCache.Add(new VertexPositionColorTexture[polygon.vertices.Length]);
			pool = polygon.modificationCache[polygon.modificationCount];
			Array.Copy(polygon.vertices, pool, polygon.vertices.Length);
			polygon.modificationCount++;
		}
		public readonly void Dispose() {
			Array.Copy(pool, polygon.vertices, polygon.vertices.Length);
			polygon.modificationCount--;
		}
	}
}
