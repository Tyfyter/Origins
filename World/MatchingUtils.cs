using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace Origins.World {
	public class TilePatternMatcher {
		readonly Point origin;
		readonly Predicate<Tile>[,] pattern;
		public TilePatternMatcher(string pattern, IDictionary<char, Predicate<Tile>> types, char origin = 'O') {
			string[] rows = pattern.Split('\n');
			this.pattern = new Predicate<Tile>[rows[0].Length, rows.Length];

			for (int j = 0; j < rows.Length; j++) {
				for (int i = 0; i < rows[j].Length; i++) {
					if (rows[j][i] == origin) {
						this.origin = new(i, j);
					}
					types.TryGetValue(rows[j][i], out this.pattern[i, j]);
				}
			}
			for (int i = 0; i < this.pattern.GetLength(0); i++)
				for (int j = 0; j < this.pattern.GetLength(1); j++)
					this.pattern[i, j] ??= _ => true;
		}
		public bool Matches(Point pos) {
			pos.X -= origin.X;
			pos.Y -= origin.Y;
			for (int i = 0; i < pattern.GetLength(0); i++) {
				int x = i + pos.X;
				for (int j = 0; j < pattern.GetLength(1); j++) {
					int y = j + pos.Y;
					if (!pattern[i, j](Framing.GetTileSafely(x, y))) return false;
				}
			}
			return true;
		}
	}
	public ref struct AreaAnalysis {
		int minX;
		int minY;
		int maxX;
		int maxY;
		bool broke;
		HashSet<Point> walked;
		public readonly int MinX => minX;
		public readonly int MinY => minY;
		public readonly int MaxX => maxX;
		public readonly int MaxY => maxY;
		public readonly bool Broke => broke;
		public readonly IReadOnlySet<Point> Walked => walked;
		public delegate bool Breaker(AreaAnalysis analysis);
		public delegate bool Counter(Point position);
		public static AreaAnalysis March(int i, int j, Point[] directions, Counter shouldCount, Breaker shouldBreak) {
			AreaAnalysis analysis = new() {
				minX = i,
				maxX = i,
				minY = j,
				maxY = j,
				walked = []
			};
			analysis.DoMarch(new(i, j), directions.Reverse().ToArray(), shouldCount, shouldBreak);
			return analysis;
		}
		void DoMarch(Point start, Span<Point> directions, Counter shouldCount, Breaker shouldBreak) {
			Stack<Point> queue = new();
			queue.Push(start);
			walked.Add(start);
			while (queue.TryPop(out Point position)) {
				if (shouldCount(position)) {
					Min(ref minX, position.X);
					Max(ref maxX, position.X);
					Min(ref minY, position.Y);
					Max(ref maxY, position.Y);
					for (int i = 0; i < directions.Length; i++) {
						Point next = directions[i];
						next.X += position.X;
						next.Y += position.Y;
						if (walked.Add(next)) {
							queue.Push(next);
						}
					}
				}
				if (shouldBreak(this)) {
					broke = true;
					break;
				}
			}
		}
		static void Min(ref int current, int @new) {
			if (current > @new) current = @new;
		}
		static void Max(ref int current, int @new) {
			if (current < @new) current = @new;
		}
	}
}
