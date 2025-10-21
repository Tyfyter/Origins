using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.World;
using Origins.World.BiomeData;
using System;
using System.Numerics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Projectiles.Pets;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Rusty_Piping : OriginTile, IAshenTile {
		public override void Load() {
			Mod.AddContent(new TileItem(this));
		}
		public override void SetStaticDefaults() {
			Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Ashen_Foliage>(), 0, 6));
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = false;
			TileID.Sets.Stone[Type] = false;
			TileID.Sets.Conversion.Stone[Type] = false;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
			AddMapEntry(FromHexRGB(0x6c452c));

			MinPick = 65;
			MineResist = 2;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public AreaAnalysis GetConnections(int i, int j, Point ignorePos, out bool powered) {
			bool _powered = false;
			bool Counter(Point position) {
				if (position == ignorePos) return false;
				Tile tile = Framing.GetTileSafely(position);
				if (tile.TileIsType(Type)) {
					_powered |= tile.WallType == WallID.AmberGemspark;
					return true;
				}
				return false;
			}
			bool Breaker(AreaAnalysis analysis) => false;
			AreaAnalysis analysis = AreaAnalysis.March(i + 1, j, AreaAnalysis.Orthogonals, Counter, Breaker);
			powered = _powered;
			return analysis;
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			Point placedPos = new(i, j);
			bool Counter(Point position) {
				if (position == placedPos) return false;
				return Framing.GetTileSafely(position).TileIsType(Type);
			}
			bool Breaker(AreaAnalysis analysis) {
				return Framing.GetTileSafely(analysis.Counted[^1]).WallType == WallID.AmberGemspark;
			}
			AreaAnalysis right = AreaAnalysis.March(i + 1, j, AreaAnalysis.Orthogonals, Counter, Breaker);
			AreaAnalysis left = AreaAnalysis.March(i - 1, j, AreaAnalysis.Orthogonals, Counter, Breaker);
			AreaAnalysis up = AreaAnalysis.March(i, j + 1, AreaAnalysis.Orthogonals, Counter, Breaker);
			AreaAnalysis down = AreaAnalysis.March(i, j - 1, AreaAnalysis.Orthogonals, Counter, Breaker);
			bool powered = right.Broke || left.Broke || up.Broke || down.Broke;
			int minX = i;
			int minY = j;
			int maxX = i;
			int maxY = j;
			Min(ref minX, right.MinX);
			Min(ref minY, right.MinY);
			Max(ref maxX, right.MaxX);
			Max(ref maxY, right.MaxY);
			
			Min(ref minX, left.MinX);
			Min(ref minY, left.MinY);
			Max(ref maxX, left.MaxX);
			Max(ref maxY, left.MaxY);
			
			Min(ref minX, up.MinX);
			Min(ref minY, up.MinY);
			Max(ref maxX, up.MaxX);
			Max(ref maxY, up.MaxY);
			
			Min(ref minX, down.MinX);
			Min(ref minY, down.MinY);
			Max(ref maxX, down.MaxX);
			Max(ref maxY, down.MaxY);

			bool[,] depoweredMap = new bool[maxX + 1 - minX, maxY + 1 - minY];
			if (!right.Broke) right.OrCountedMap(depoweredMap, minX, minY);
			if (!left.Broke) left.OrCountedMap(depoweredMap, minX, minY);
			if (!up.Broke) up.OrCountedMap(depoweredMap, minX, minY);
			if (!down.Broke) down.OrCountedMap(depoweredMap, minX, minY);

			for (int x = minX; x <= maxX; x++) {
				for (int y = minY; y <= maxY; y++) {
					Tile tile = Framing.GetTileSafely(x, y);
					if (tile.TileType == Type) {
						tile.TileColor = depoweredMap[x - minX, y - minY] ? PaintID.DeepRedPaint : PaintID.DeepLimePaint;
					}
				}
			}
			for (int k = 0; k < right.Counted.Count; k++) {
			}
		}
		static void Min<T>(ref T current, T @new) where T : IComparisonOperators<T, T, bool> {
			if (current > @new) current = @new;
		}
		static void Max<T>(ref T current, T @new) where T : IComparisonOperators<T, T, bool> {
			if (current < @new) current = @new;
		}
	}
}
