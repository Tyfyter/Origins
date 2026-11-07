using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Terraria;

namespace Origins.Core; 
public static class Iterators {
	public static RectangleIterator IterateOver(this Rectangle rectangle, TileOrder order = default, bool clampToWorld = true) {
		int minX = rectangle.X;
		int minY = rectangle.Y;
		int maxX = rectangle.Right;
		int maxY = rectangle.Bottom;
		if (clampToWorld) {
			minX = Utils.Clamp(minX, 0, Main.maxTilesX - 1);
			minY = Utils.Clamp(minY, 0, Main.maxTilesY - 1);
			maxX = Utils.Clamp(maxX, 0, Main.maxTilesX - 1);
			maxY = Utils.Clamp(maxY, 0, Main.maxTilesY - 1);
		}
		return new(minX, minY, maxX, maxY, order);
	}
}
[Flags]
[SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification = "<Pending>")]
public enum TileOrder {
	RowMajor = 0b000,
	AscX = 0b000,
	AscY = 0b000,
	ColMajor = 0b001,
	DescX = 0b010,
	DescY = 0b100,
}
public struct RectangleIterator : IEnumerable<Point>, IEnumerator<Point> {
	SubIterator major;
	SubIterator minor;
	bool breakMajor;
	readonly bool rowMajor;
	public RectangleIterator(int minX, int minY, int maxX, int maxY, TileOrder order) {
		SubIterator x = new(minX, maxX, order.HasFlag(TileOrder.DescX));
		SubIterator y = new(minY, maxY, order.HasFlag(TileOrder.DescY));
		rowMajor = !order.HasFlag(TileOrder.ColMajor);
		if (rowMajor) (major, minor) = (x, y);
		else (major, minor) = (y, x);
		Reset();
	}
	readonly Point CurrentPoint => rowMajor ? new(major.current, minor.current) : new(minor.current, major.current);
	[UnscopedRef, DebuggerHidden]
	public Value Current => new(CurrentPoint, ref breakMajor);
	public readonly RectangleIterator GetEnumerator() => this;
	readonly Point IEnumerator<Point>.Current => CurrentPoint;
	readonly object IEnumerator.Current => CurrentPoint;
	readonly void IDisposable.Dispose() { }
	readonly IEnumerator<Point> IEnumerable<Point>.GetEnumerator() => this;
	readonly IEnumerator IEnumerable.GetEnumerator() => this;
	void IEnumerator.Reset() => Reset();
	public bool MoveNext() {
		if (breakMajor || !major.MoveNext()) {
			breakMajor = false;
			major.Reset();
			return major.MoveNext() && minor.MoveNext();
		}
		return true;
	}
	void Reset() {
		major.Reset();
		minor.Reset();
		minor.MoveNext();
	}
	struct SubIterator(int min, int max, bool descend) {
		public int current;
		public void Reset() => current = descend ? (max + 1) : (min - 1);
		public bool MoveNext() {
			if (descend) return --current >= min;
			return ++current <= max;
		}
	}
	[StructLayout(LayoutKind.Explicit)]
	public readonly ref struct Value(Point point, ref bool breakMajor) {
		[Obsolete("Exists to overload with an error", true)]
		public Value(Value recursor, ref bool breakMajor) : this(Point.Zero, ref breakMajor) => throw new InvalidOperationException();
		[FieldOffset(0)]
		readonly public Point point = point;
		[FieldOffset(0)]
		readonly public int X;
		[FieldOffset(sizeof(int))]
		readonly public int Y;
		[FieldOffset(sizeof(int) * 2)]
		readonly ref bool breakMajor = ref breakMajor;
		public void BreakMajor() => breakMajor = true;
		public static implicit operator Point(Value value) => value.point;
	}
}