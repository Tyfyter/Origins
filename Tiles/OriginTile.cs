using AltLibrary.Common.AltBiomes;
using Origins.Tiles.Defiled;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Tiles {
	public abstract class OriginTile : ModTile {
		public static List<OriginTile> IDs { get; internal set; }
		public static List<int> DefiledTiles { get; internal set; }
		public ushort mergeID;
		public override void Load() {
			if (IDs != null) {
				IDs.Add(this);
			} else {
				IDs = new List<OriginTile>() { this };
			}
			mergeID = Type;
		}
		protected void AddDefiledTile() {
			if (this is DefiledTile) {
				if (DefiledTiles != null) {
					DefiledTiles.Add(Type);
				} else {
					DefiledTiles = new List<int>() { Type };
				}
			}
		}
		public override void RandomUpdate(int i, int j) {
			return;
			AltBiome biome = null;
			if (this is DefiledTile) {
				biome = ModContent.GetInstance<Defiled_Wastelands_Alt_Biome>();
			}
			if (this is RivenTile) {
				biome = ModContent.GetInstance<Riven_Hive_Alt_Biome>();
			}
			if (biome is not null) {
				if (TileID.Sets.Conversion.Grass[Type]) {
					int retryCount = 0;
					retry:
					if (retryCount++ > 100) return;
					switch (WorldGen.genRand.Next(Main.hardMode ? 3 : 2)) {
						case 0:
						WeightedRandom<(int, int)> rand = new WeightedRandom<(int, int)>();
						Tile current;
						for (int y = -1; y < 2 && (j + y) < Main.worldSurface; y++) {
							for (int x = -1; x < 2; x++) {
								current = Main.tile[i + x, j + y];
								if (current.TileType == TileID.Grass) {
									if (Main.tile[i + x, j + y - 1].TileType != TileID.Sunflower) rand.Add((i + x, j + y));
								} else if (current.TileType == TileID.Dirt) {
									if (!(Main.tile[i + x - 1, j + y].HasTile && Main.tile[i + x + 1, j + y].HasTile && Main.tile[i + x, j + y - 1].HasTile && Main.tile[i + x, j + y + 1].HasTile)) {
										rand.Add((i + x, j + y));
									}
								}
							}
						}
						if (rand.elements.Count > 0) {
							(int x, int y) = rand.Get();
							AltLibrary.Core.ALConvert.Convert(biome, x, y, 1);
							WorldGen.SquareTileFrame(x, y);
							NetMessage.SendTileSquare(-1, x, y, 1);
						} else {
							goto retry;
						}
						break;
						case 1:
						if (!Main.tile[i, j - 1].HasTile) {
							Main.tile[i, j - 1].ResetToType((ushort)ModContent.TileType<Defiled_Foliage>());
						} else {
							goto retry;
						}
						break;
						case 2:
						base.RandomUpdate(i, j);
						break;
					}
				} else {
					if (!Main.hardMode) return;
					WeightedRandom<(int, int)> rand = new WeightedRandom<(int, int)>();
					Tile current;
					for (int y = -3; y < 4; y++) {
						for (int x = -3; x < 4; x++) {
							if (Main.tile[i + x, j + y - 1].TileType != TileID.Sunflower) rand.Add((i + x, j + y));
						}
					}
					if (rand.elements.Count > 0) {
						(int x, int y) = rand.Get();
						AltLibrary.Core.ALConvert.Convert(biome, x, y, 1);
						WorldGen.SquareTileFrame(x, y);
						NetMessage.SendTileSquare(-1, x, y, 1);
					}
				}
			}
		}
	}
	//temp solution
	public interface DefiledTile { }
	public interface RivenTile { }
	public interface AshenTile { }
}
