using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace Origins.World {
	public class Carver {
		public delegate void Filter(Vector2 pos, ref bool output);
		public static Filter PointyLemon(Vector2 center, float scale, float rotation, float aspectRatio, float roundness, ref Vector2 posMin, ref Vector2 posMax) {
			float sin = MathF.Sin(rotation);
			float cos = MathF.Cos(rotation);
			Vector2 a = new Vector2(1, aspectRatio).RotatedBy(rotation) * scale * 0.5f;
			Vector2 b = new Vector2(-1, aspectRatio).RotatedBy(rotation) * scale * 0.5f;
			posMin = Vector2.Min(posMin, center + Vector2.Min(Vector2.Min(a, b), Vector2.Min(-a, -b)));
			posMax = Vector2.Max(posMax, center + Vector2.Max(Vector2.Max(a, b), Vector2.Max(-a, -b)));
			void DoFilter(Vector2 pos, ref bool output) {
				if (!output) return;
				Vector2 offset = (pos - center) / scale;
				if (pos == center) return;
				offset = new(offset.X * cos + offset.Y * sin, offset.X * sin - offset.Y * cos);
				float dist = offset.Y * offset.Y + MathF.Pow(Math.Abs(offset.X * aspectRatio), Math.Abs(offset.Y * roundness));
				output = dist <= 1;
			}
			return DoFilter;
		}
		public static Filter TileFilter(Predicate<Tile> predicate) {
			void DoFilter(Vector2 pos, ref bool output) {
				if (!output) return;
				output = predicate(Framing.GetTileSafely(pos.ToPoint()));
			}
			return DoFilter;
		}
		public static void CanSetTile(Vector2 pos, ref bool output) {
			if (!output) return;
			Tile tile = Framing.GetTileSafely(pos.ToPoint());
			output = !tile.HasTile || TileID.Sets.CanBeClearedDuringGeneration[tile.TileType];
		}
		public static Filter CanBeReplaced => ActiveTileInSet(TileID.Sets.CanBeClearedDuringGeneration);
		public static Filter CanBeReplacedWithOre => ActiveTileInSet(TileID.Sets.CanBeClearedDuringOreRunner);
		public static Filter ActiveTileInSet(bool[] set) {
			void DoFilter(Vector2 pos, ref bool output) {
				if (!output) return;
				Tile tile = Framing.GetTileSafely(pos.ToPoint());
				output = tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType] && set[tile.TileType];
			}
			return DoFilter;
		}
		public static int DoCarve(Filter filter, Func<Vector2, int> action, Vector2 posMin, Vector2 posMax, int matchThreshold = 0) {
			posMin = new(MathF.Floor(posMin.X), MathF.Floor(posMin.Y));
			posMax = new(MathF.Ceiling(posMax.X), MathF.Ceiling(posMax.Y));
			List<Vector2> positions = [];
			for (float i = posMin.X; i <= posMax.X; i++) {
				for (float j = posMin.Y; j <= posMax.Y; j++) {
					bool match = true;
					filter(new(i, j), ref match);
					if (match) positions.Add(new(i, j));
				}
			}
			if (positions.Count < matchThreshold) return 0;
			int count = 0;
			for (int i = 0; i < positions.Count; i++) {
				count += action(positions[i]);
			}
			return count;
		}
	}
}
