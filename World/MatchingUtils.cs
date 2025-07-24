using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

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
}
