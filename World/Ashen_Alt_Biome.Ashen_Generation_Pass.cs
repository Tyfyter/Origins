using AltLibrary.Common.AltBiomes;
using AltLibrary.Common.Systems;
using AltLibrary.Core.Generation;
using ModLiquidLib.ModLoader;
using Origins.Core;
using Origins.Core.Structures;
using Origins.Liquids;
using Origins.Tiles.Ashen;
using Origins.Tiles.Ashen.Hanging_Scrap;
using Origins.Walls;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.Localization;
using Terraria.WorldBuilding;
using static Origins.OriginExtensions;
using static Terraria.ModLoader.ModContent;
using static Terraria.WorldGen;

namespace Origins.World.BiomeData; 
public partial class Ashen_Alt_Biome {
	Structure heatVents;
	Structure incineratorPits;
	Structure oilRigs;
	Structure radioTowers;
	Structure replenishmentFacilities;
	public class Ashen_Generation_Pass : EvilBiomeGenerationPass {
		public static List<Rectangle> scrapyards = [];
		public override string ProgressMessage => Language.GetTextValue("Mods.Origins.AltBiomes.Ashen_Alt_Biome.GenPassName");
		public override void GenerateEvil(int evilBiomePosition, int evilBiomePositionWestBound, int evilBiomePositionEastBound) {
			WorldBiomeGeneration.ChangeRange.ResetRange();

			ushort stoneType = (ushort)TileType<Tainted_Stone>();
			ushort stoneWallType = (ushort)OriginsWall.GetWallID<Tainted_Stone_Wall>(WallVersion.Natural);

			ushort sludgeType = (ushort)TileType<Murky_Sludge>();
			for (int i = evilBiomePositionWestBound; i < evilBiomePositionEastBound; i++) {
				int top = 0;
				bool setTop = false;
				int sludgeDepth = genRand.Next(7, 16);
				int stoneDepth = genRand.Next(29, 32);
				for (int j = (int)GenVars.worldSurfaceLow; j < GenVars.worldSurfaceHigh + 32; j++) {
					Tile tile = Main.tile[i, j];
					if (tile.HasTile && !(TileID.Sets.BreakableWhenPlacing[tile.TileType] || Main.tileCut[tile.TileType])) {
						if (tile.TileType == TileID.Trees) {
							OriginSystem.RemoveTree(i, j);
							continue;
						}
						AreaAnalysis analysis = AreaAnalysis.March(i, j, AreaAnalysis.Orthogonals, AreaAnalysis.HasFullSolidTile, analysis => analysis.Counted.Count > 200);
						if (!analysis.Broke) {
							for (int k = 0; k < analysis.Counted.Count; k++) {
								tile = Main.tile[analysis.Counted[k]];
								tile.HasTile = false;
							}
							continue;
						}
						if (setTop.TrySet(true)) top = j;
						else if (j >= top + sludgeDepth + stoneDepth) break;
						tile.TileType = j < top + sludgeDepth ? sludgeType : stoneType;
						WorldBiomeGeneration.ChangeRange.AddChangeToRange(i, j);
					} else {
						if (setTop && j > top + sludgeDepth) break;
						setTop = false;
					}
				}
				for (int j = (int)GenVars.worldSurfaceLow; j < GenVars.worldSurfaceHigh + 32; j++) {
					Tile tile = Main.tile[i, j];
					tile.WallType = WallID.None;
					if (tile.HasTile && !(TileID.Sets.BreakableWhenPlacing[tile.TileType] || Main.tileCut[tile.TileType])) break;
				}
			}

			Rectangle range = WorldBiomeGeneration.ChangeRange.GetRange();
			bool anyTiles;
			int extendedMinY = range.Top;
			do {
				anyTiles = false;
				extendedMinY--;
				for (int i = range.Left; i < range.Right && !anyTiles; i++) {
					anyTiles = Framing.GetTileSafely(i, extendedMinY).HasTile;
				}
			} while (anyTiles);
			extendedMinY++;
			WorldBiomeGeneration.ChangeRange.AddChangeToRange(evilBiomePosition, extendedMinY);
			range = WorldBiomeGeneration.ChangeRange.GetRange();

			WorldBiomeGeneration.EvilBiomeGenRanges.Add(range);
			AltBiome biome = GetInstance<Ashen_Alt_Biome>();
			ushort grass = (ushort)TileType<Ashen_Grass>();
			for (int i = range.Left; i < range.Right; i++) {
				int slopeFactor = Math.Min(Math.Min(i - range.Left, range.Right - i), 99);
				for (int j = range.Top - 10; j < range.Bottom; j++) {
					if (genRand.NextBool(5) && genRand.Next(slopeFactor, 100) < 20) continue;
					if (range.Bottom - j < 5 && genRand.NextBool(5)) break;
					Tile tile = Framing.GetTileSafely(i, j);
					AltLibrary.Core.ALConvert.Convert(biome, i, j, 0);
					if (tile.TileType == TileID.Dirt && (!Framing.GetTileSafely(i - 1, j).HasTile || !Framing.GetTileSafely(i + 1, j).HasTile || !Framing.GetTileSafely(i, j - 1).HasTile || !Framing.GetTileSafely(i, j + 1).HasTile)) {
						tile.TileType = grass;
					}
				}
			}
			OriginSystem.Instance.hasAshen = true;
			scrapyards.Add(range);
		}

		public override void PostGenerateEvil() { }
	}

	public override void ModifyGenPass(List<GenPass> passes, GenPass originalPass) {
		if (originalPass.Name == "Settle Liquids") {
			Ashen_Generation_Pass.scrapyards.Clear();
			passes.Add(new PassLegacy("Scrap and Oil", (_, _) => {
				for (int i = 0; i < Ashen_Generation_Pass.scrapyards.Count; i++) ScrapyardPass2(Ashen_Generation_Pass.scrapyards[i]);
				ScrapyardPass3(genRand.Next(Ashen_Generation_Pass.scrapyards));
			}));
		}
		static void ScrapyardPass2(Rectangle scrapyard) {
			ushort sludgeType = (ushort)TileType<Murky_Sludge>();
			ushort scrapType = (ushort)TileType<Scrap_Heap>();
			for (int j = 0; j < scrapyard.Height; j++) {
				for (int i = 0; i < scrapyard.Width; i++) {
					Tile tile = Main.tile[scrapyard.X + i, scrapyard.Y + j];
					if (tile.LiquidAmount > 0) {
						tile.LiquidType = LiquidLoader.LiquidType<Oil>();
						tile = Main.tile[scrapyard.X + i, scrapyard.Y + j + 1];
						if (tile.HasFullSolidTile()) {
							int sludgeDepth = genRand.Next(7, 16);
							for (int k = 1; k < sludgeDepth; k++) {
								tile = Main.tile[scrapyard.X + i, scrapyard.Y + j + k];
								if (!tile.HasFullSolidTile()) break;
								tile.TileType = sludgeType;
							}
						}
					}
				}
			}
			int tries = 1000;
			for (int i = 0; i < scrapyard.Width / 50; i++) {
				int x = scrapyard.X + genRand.Next(scrapyard.Width);
				int y = scrapyard.Y + genRand.Next(scrapyard.Height);
				Tile tile = Main.tile[x, y];
				if (!tile.HasTile) {
					if (tries > 0) {
						i--;
						tries--;
					}
					continue;
				}
				while (tile.HasTile) tile = Main.tile[x, --y];
				Vector2 posMin = new(float.PositiveInfinity);
				Vector2 posMax = new(float.NegativeInfinity);
				Carver.Filter[] lemons = new Carver.Filter[genRand.Next(2, 5)];
				for (int k = 0; k < lemons.Length; k++) {
					lemons[k] = Carver.PointyLemon(
						new(x, y),
						scale: genRand.NextFloat(4, 8),
						rotation: genRand.NextFloat(0, MathHelper.Pi),
						aspectRatio: genRand.NextFloat(2, 3),
						roundness: genRand.NextFloat(1, 2),
						ref posMin,
						ref posMax
					);
				}
				Carver.DoCarve(
					Carver.EmptyTile + Carver.Or(lemons),
					pos => {
						Tile tile = Framing.GetTileSafely(pos.ToPoint());
						tile.HasTile = true;
						tile.TileType = scrapType;
						return 0;
					},
					posMin, posMax
				);
			}
			tries = 1000;
			for (int i = 0; i < scrapyard.Width / 10; i++) {
				int x = scrapyard.X + genRand.Next(scrapyard.Width);
				int y = scrapyard.Y + genRand.Next(scrapyard.Height);
				Tile tile = Main.tile[x, y];
				if (!tile.HasTile) {
					if (tries > 0) {
						i--;
						tries--;
					}
					continue;
				}
				while (tile.HasTile) tile = Main.tile[x, --y];
				y++;
				new Hanging_Scrap_Action(new(x, y), new(HangingScrap.GetRandom(genRand), (Half)Main.rand.NextFloat(float.Tau))).Perform();
			}
		}
		void ScrapyardPass3(Rectangle scrapyard) {
			RangeRandom rand = new(genRand, scrapyard.Left + 7, scrapyard.Right - 6);
			Span<int> heights = stackalloc int[rand.End - rand.Start];
			this.ScrapyardPass3(scrapyard, rand, heights);
		}
	}
	void ScrapyardPass3(Rectangle scrapyard, RangeRandom rand, Span<int> heights) {
		foreach (RectangleIterator.Value pos in scrapyard.IterateOver(TileOrder.ColMajor)) {
			if (pos.X < rand.Start || pos.X >= rand.End) {
				pos.BreakMajor();
				continue;
			}
			Tile tile = Main.tile[pos];
			if (tile.LiquidAmount > 0) {
				pos.BreakMajor();
				rand.Multiply(pos.X, pos.X + 1, 0);
				continue;
			}
			if (tile.HasTile) {
				heights[pos.X - rand.Start] = pos.Y;
				pos.BreakMajor();
			}
		}
		for (int i = 3; i < heights.Length - 3; i++) {
			for (int j = -3; j <= 3; j++) {
				if (Math.Abs(heights[i] - heights[i + j]) > 2) rand.Multiply(i + rand.Start - 1, i + rand.Start + 2, 0.5f);
			}
		}
		GenerateStructure(replenishmentFacilities, rand.Get(), rand, heights);
		for (int i = 0; i < 6; i++) if (rand.AnyWeight) GenerateStructure(heatVents, rand.Get(), rand, heights);
		for (int i = 0; i < 4; i++) if (rand.AnyWeight) GenerateStructure(incineratorPits, rand.Get(), rand, heights);
		for (int i = 0; i < 4; i++) if (rand.AnyWeight) GenerateStructure(oilRigs, rand.Get(), rand, heights);
		for (int i = 0; i < 4; i++) if (rand.AnyWeight) GenerateStructure(radioTowers, rand.Get(), rand, heights);
		static void GenerateStructure(Structure structure, int x, RangeRandom rand, Span<int> heights) {
			foreach (Rectangle item in structure.Generate(x, heights[x - rand.Start]) ?? []) {
				rand.Multiply(item.X, item.Right + 1, 0);
			}
		}
	}
}
