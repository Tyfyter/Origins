using AltLibrary.Common.Systems;
using ModLiquidLib.ModLoader;
using Origins.Items.Accessories;
using Origins.Liquids;
using Origins.Tiles.Brine;
using Origins.Tiles.Defiled;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Origins.Walls;
using Origins.World;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.WorldBuilding;
using static Terraria.ModLoader.ModContent;
using static Terraria.WorldGen;

namespace Origins {
	public partial class OriginSystem : ModSystem {
		public static List<Vector2> EvilSpikeAvoidancePoints = [];
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
			const double max_defiled_spike_size = 6.0;
			Defiled_Wastelands_Alt_Biome.defiledWastelandsWestEdge = new();
			Defiled_Wastelands_Alt_Biome.defiledWastelandsEastEdge = new();
			EvilSpikeAvoidancePoints.Clear();
			#region _
			/*genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
			if (genIndex == -1) {
				return;
			}
			tasks.Insert(genIndex + 1, new PassLegacy("TEST Biome", delegate (GenerationProgress progress) {
			progress.Message = "Generating TEST Biome";
			for(int i = 0; i < Main.maxTilesX / 900; i++) {       //900 is how many biomes. the bigger is the number = less biomes
				int X = (int)(Main.maxTilesX*0.4);//WorldGen.genRand.Next(1, Main.maxTilesX - 300);
				int Y = (int)WorldGen.worldSurfaceLow;//WorldGen.genRand.Next((int)WorldGen.worldSurfaceLow, Main.maxTilesY - 200);
				int TileType = 56;     //this is the tile u want to use for the biome , if u want to use a vanilla tile then its int TileType = 56; 56 is obsidian block

				WorldGen.TileRunner(X, Y, 350, WorldGen.genRand.Next(100, 200), TileType, false, 0f, 0f, true, true);  //350 is how big is the biome     100, 200 this changes how random it looks.
				WorldGen.CloudLake(X, (int)WorldGen.worldSurfaceHigh);
				WorldGen.AddShadowOrb(X, (int) WorldGen.worldSurfaceHigh-1);
				//Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh-1).type = TileID.AmberGemspark;
				//Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh+1).type = TileID.AmberGemspark;
				//Framing.GetTileSafely(X-1, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
				//Framing.GetTileSafely(X+1, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
				//Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
			}
			}));*/
			#endregion _
			int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Jungle"));
			if (genIndex != -1) {
				tasks.Insert(++genIndex, new PassLegacy("Find Brine Pool Spot", delegate (GenerationProgress progress, GameConfiguration _) {
					progress.Message = Mod.GetLocalization("GenPass.PickPrinePoolPos.DisplayName", () => "Finding a nice spot to pool brine").Value;
					int tries = 0;
					retry:
					int X = WorldGen.genRand.Next(GenVars.JungleX - 100, GenVars.JungleX + 100);
					int Y;
					if (WorldGen.remixWorldGen) {
						Y = Main.UnderworldLayer - WorldGen.genRand.Next(100, 125);
					} else {
						for (Y = (int)GenVars.worldSurfaceLow; !Main.tile[X, Y].HasTile; Y++) ;
						Y += WorldGen.genRand.Next(100, 125);
					}
					if (++tries < 1000 && (!GenVars.structures.CanPlace(new Rectangle(X, Y, 1, 1), 48) || WorldBiomeGeneration.EvilBiomeGenRanges.Any(r => r.Contains(X, Y)))) goto retry;
					Mod.Logger.Info("BrineGen:" + X + ", " + Y);
					brineCenter = new(X, Y);
				}));
			}
			genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Larva"));
			if (genIndex != -1) {
				tasks.Insert(++genIndex, new PassLegacy("Brine Pool", delegate (GenerationProgress progress, GameConfiguration _) {
					progress.Message = Mod.GetLocalization("GenPass.PrinePool.DisplayName", () => "Pooling Brine").Value;
					Brine_Pool.Gen.BrineStart(brineCenter.X, brineCenter.Y);
				}));
				tasks.Insert(++genIndex, new PassLegacy("Fiberglass Undergrowth", delegate (GenerationProgress progress, GameConfiguration __) {
					Mod.Logger.Info("Fiberglass Undergrowth");
					progress.Message = "Undergrowing Fiberglass";
					_ = OriginSystem.Instance.brinePoolRange;
					//for (int i = 0; i < Main.maxTilesX / 5000; i++) {
					bool placed = false;
					int tries = 0;
					RangeRandom rangeRand = new(WorldGen.genRand, GenVars.jungleMinX + 100, GenVars.jungleMaxX - 10);
					rangeRand.Multiply(Instance.brinePoolRange.Left, Instance.brinePoolRange.Right, 0.1f);
					while (!placed && rangeRand.AnyWeight) {
						int X = rangeRand.Get();
						rangeRand.Multiply(X, X, 0);
						int Y;
						for (Y = (int)GenVars.worldSurfaceLow; !Main.tile[X, Y].HasTile; Y++) ;
						Y += WorldGen.genRand.Next(350, 450);
						int templeTop = GenVars.tTop - 100;
						int templeBottom = GenVars.tBottom + 100;
						if (Y > templeTop && Y < templeBottom) {
							Y = Math.Abs(Y - templeTop) < Math.Abs(Y - templeBottom) ? templeTop : templeBottom;
						}
						if (GenVars.structures.CanPlace(new Rectangle(X - 32 - (32 + 16), Y - (32 + 16), 64 + 16, 64 + 16)) || ++tries > 1000) {
							Mod.Logger.Info("FiberglassGen:" + X + ", " + Y);
							Fiberglass_Undergrowth.Gen.FiberglassStart(X, Y);
							placed = true;
						}
					}
					//}
				}));

			}
			List<(Point, int)> EvilSpikes = [];
			genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Weeds"));
			getEvilTileConversionTypes(evil_wastelands, out ushort defiledStoneType, out ushort defiledGrassType, out ushort defiledPlantType, out ushort defiledSandType, out ushort _, out ushort _, out ushort defilediceType);
			tasks.Insert(genIndex + 1, new PassLegacy("Finding Spots For Spikes", (progress, _) => {
				for (int index = 0; index < Defiled_Wastelands_Alt_Biome.defiledWastelandsWestEdge.Count; index++) {
					int minX = Defiled_Wastelands_Alt_Biome.defiledWastelandsWestEdge[index];
					int maxX = Defiled_Wastelands_Alt_Biome.defiledWastelandsEastEdge[index];
					HashSet<int> spanchors = [
						defiledStoneType,
						defiledGrassType,
						defiledSandType,
						defilediceType
					];
					int tilesSinceSpike = 0;
					for (int i = minX; i <= maxX; i++) {
						int heightSinceSurface = 0;
						bool canSpike = true;
						for (int j = (int)GenVars.worldSurfaceLow - 25; j < Main.maxTilesY; j++) {
							Tile tile = Main.tile[i, j];
							if (tile.HasTile && !(tile.IsHalfBlock || tile.Slope != SlopeID.None)) {
								if (tile.TileType == TileID.Plants) tile.TileType = defiledPlantType;
								if (tile.TileType == TileID.Grass) tile.TileType = defiledGrassType;

								if (canSpike && (spanchors.Contains(tile.TileType) || (tile.TileType == TileID.SnowBlock && genRand.NextBool(8)))) {
									bool hasOpenSide = genRand.NextBool(17 + EvilSpikes.Count / 3);
									for (int k = -1; k <= 1 && !hasOpenSide; k++) {
										for (int l = -1; l <= 1 && !hasOpenSide; l++) {
											if (!(k == 0 && l == 0) && !Main.tile[i + k, j + l].HasSolidTile() && Main.tile[i + k, j + l].WallType == WallID.None) {
												hasOpenSide = true;
											}
										}
									}
									if (hasOpenSide && genRand.Next(0, 17 + EvilSpikes.Count / 3) <= tilesSinceSpike / 3) {
										EvilSpikes.Add((new Point(i, j), Math.Min(genRand.Next(9, 18) + tilesSinceSpike / 5, 35)));
										tilesSinceSpike = -15;
										canSpike = false;
									} else if (tilesSinceSpike < 30 * 15) {
										tilesSinceSpike++;
									}
								}
								if (++heightSinceSurface > 30) break;
							}
						}
					}
				}
				if (EvilSpikes.Count > 0) {
					Mod.Logger.Info($"Adding {EvilSpikes.Count} Evil Spikes");
				}
			}));
			genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
			tasks.Insert(genIndex + 1, new PassLegacy("Placing Spikes", (progress, _) => {
				int skipped = 0;
				while (EvilSpikes.Count > 0) {
					const float hole_avoidance = 12.5f;
					(Point pos, int size) i = EvilSpikes[0];
					Point p = i.pos;
					EvilSpikes.RemoveAt(0);
					bool tooClose = false;
					Vector2 posVec = p.ToVector2();
					for (int n = 0; !tooClose && n < EvilSpikeAvoidancePoints.Count; n++) {
						if (posVec.DistanceSQ(EvilSpikeAvoidancePoints[n]) < hole_avoidance * hole_avoidance) {
							tooClose = true;
						}
					}
					if (tooClose) continue;
					Vector2 vel = Vector2.Zero;
					int wallType = OriginsWall.GetWallID<Defiled_Stone_Wall>(WallVersion.Natural);
					for (int k = 1; k <= 10 && vel == Vector2.Zero; k = k > 0 ? -k : -(k - 1)) {
						for (int l = 1; l <= 10 && vel == Vector2.Zero; l = l > 0 ? -l : -(l - 1)) {
							Tile tile = Main.tile[p.X + k, p.Y + l];
							if (!tile.HasSolidTile() && tile.WallType != wallType) {
								vel = new Vector2(k, l).SafeNormalize(-Vector2.UnitY);
							}
						}
					}
					if (vel == Vector2.Zero) {
						skipped++;
						continue;
					}
					p = p.OffsetBy((int)(vel.X * -3), (int)(vel.Y * -3));
					//TestRunners.SpikeRunner(p.X, p.Y, duskStoneID, vel, i.Item2, randomtwist: true);
					double size = i.size * 0.25;
					if (size > max_defiled_spike_size) size = max_defiled_spike_size;
					if (genRand.NextBool(5)) {
						size += 6;
						Vector2 tempPos = new(p.X, p.Y);
						while (Main.tile[(int)tempPos.X, (int)tempPos.Y].HasTile && Main.tileSolid[Main.tile[(int)tempPos.X, (int)tempPos.Y].TileType]) {
							tempPos += vel;
						}
						tempPos -= vel * 3;
						p = tempPos.ToPoint();
						//p = new Point(p.X+(int)(vel.X*18),p.Y+(int)(vel.Y*18));
					}
					bool twist = genRand.NextBool(4, 5);
					GenRunners.DefiledSpikeRunner(
						p.X,
						p.Y,
						size,
						defiledStoneType,
						wallType,
						vel,
						decay: genRand.NextFloat(0.12f, 0.30f),
						twist: twist ? genRand.NextFloat(-0.04f, 0.04f) : 1f,
						randomtwist: !twist,
						cutoffStrength: 1.5
					);
				}
				if (skipped > 0) {
					Mod.Logger.Info($"Skipped {skipped} Evil Spikes");
				}
				for (int index = 0; index < Defiled_Wastelands_Alt_Biome.defiledWastelandsWestEdge.Count; index++) {
					int minX = Defiled_Wastelands_Alt_Biome.defiledWastelandsWestEdge[index];
					int maxX = Defiled_Wastelands_Alt_Biome.defiledWastelandsEastEdge[index];
					ushort bramble = (ushort)ModContent.TileType<Tangela_Bramble>();
					for (int i0 = genRand.Next(50, 100); i0-- > 0;) {
						int tries = 20;
						int x = genRand.Next(minX, maxX);
						int y = genRand.Next((int)GenVars.worldSurfaceLow, (int)GenVars.worldSurfaceHigh);
						while (tries-- > 0) {
							if (TileObject.CanPlace(x, y, bramble, 0, 1, out TileObject objectData, onlyCheck: false)) {
								TileObjectData tileData = TileObjectData.GetTileData(bramble, objectData.style);
								int left = x - tileData.Origin.X;
								int top = y - tileData.Origin.Y;
								for (int y0 = 0; y0 < tileData.Height; y0++) {
									for (int x0 = 0; x0 < tileData.Width; x0++) {
										Tile tileSafely = Framing.GetTileSafely(left + x, top + y);
										if (tileSafely.HasTile && !Main.tileCut[tileSafely.TileType]) goto fail;
										//tileSafely.HasTile = false;
									}
								}
								if (TileObject.Place(objectData)) WorldGen.SquareTileFrame(x, y);
								break;
								fail:;
							}
							y++;
						}
					}
				}
			}));
			tasks.Add(new PassLegacy("Stone Mask", (progress, _) => {
				int i = 0;
				bool leavesSolid = Main.tileSolid[TileID.LeafBlock];
				try {
					Main.tileSolid[TileID.LeafBlock] = false;
					while (i < 100) {
						int x = genRand.Next(oceanDistance, Main.maxTilesX - oceanDistance);
						int y = 0;
						for (; !Main.tile[x, y + 1].HasTile; y++) ;
						if (Main.tileSolid[Main.tile[x, y + 1].TileType] && Main.tileSolid[Main.tile[x + 1, y + 1].TileType]) {
							if (PlaceTile(x, y, TileType<Stone_Mask_Tile>())) {
								break;
							}
						}
					}
				} finally {
					Main.tileSolid[TileID.LeafBlock] = leavesSolid;
				}
			}));
			genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
			tasks.Insert(genIndex + 1, new PassLegacy("Shinies (Singular no longer)", (progress, _) => {
				ushort type = (ushort)TileType<Carburite>();
				for (int i = 0; i < (int)(Main.maxTilesX * Main.maxTilesY * 3.75E-05); i++) {
					WorldGen.OreRunner(
						genRand.Next(0, Main.maxTilesX),
						genRand.Next((int)Main.worldSurface, (int)Main.rockLayer),
						genRand.Next(3, 6),
						genRand.Next(4, 8),
						type
					);
				}
				type = (ushort)TileType<Silicon_Ore>();
				for (int i = 0; i < (int)(Main.maxTilesX * Main.maxTilesY * 3.75E-05); i++) {
					WorldGen.OreRunner(
						genRand.Next(0, Main.maxTilesX),
						genRand.Next((int)Main.worldSurface, Main.UnderworldLayer),
						genRand.Next(3, 8),
						genRand.Next(4, 6),
						type
					);
				}
			}));
			genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Remove Broken Traps"));
			tasks.Insert(genIndex, new PassLegacy("Boom", (progress, _) => {
				ushort type = (ushort)TileType<Bomb_Trap>();
				for (int i = 0; i < Main.maxTilesX; i++) {
					for (int j = (int)GenVars.worldSurfaceHigh; j < Main.maxTilesY; j++) {
						Tile tile = Framing.GetTileSafely(i, j);
						if (tile.TileType == TileID.Traps && tile.TileFrameY == 0 && genRand.NextBool(10) && HasTriggerWithinRange(i, j, 10)) {
							tile.TileType = type;
						}
					}
				}
			}));
			tasks.Add(new PassLegacy("Oil", (progress, _) => {
				int oilCountTarget = (int)(Main.maxTilesX * Main.maxTilesY * 0.1f * genRand.NextFloat(0.9f, 1f));
				int oil = Oil.ID;
				int burningOil = Burning_Oil.ID;
				int tries = 0;
				int oilCount = 0;
				const int max_tries = 10000;
				while (oilCount < oilCountTarget) {
					int i = genRand.Next(0, Main.maxTilesX);
					int j = genRand.Next((int)GenVars.rockLayer + 40, Main.UnderworldLayer);
					Tile tile = Main.tile[i, j];
					if (tile.LiquidAmount > 0 && tile.LiquidType is LiquidID.Water or LiquidID.Lava) {
						tries = 0;
						int liquidType = tile.LiquidType;
						bool ignite = false;
						bool Counter(Point pos) {
							if (pos.X < 0 || pos.Y < 0 || pos.X >= Main.maxTilesX || pos.Y >= Main.maxTilesY) return false;
							Tile tile = Main.tile[pos];
							if (tile.HasTile && Main.tileSolid[tile.TileType]) return false;
							return tile.LiquidAmount > 0 && tile.LiquidType == liquidType;
						}
						bool Breaker(AreaAnalysis analysis) {
							if (analysis.Counted.Count > 50 * 50) return true;
							bool IsSnowBiome(Point pos) {
								if (pos.X < 0 || pos.Y < 0 || pos.X >= Main.maxTilesX || pos.Y >= Main.maxTilesY) return false;
								Tile tile = Main.tile[pos];
								if (tile.HasTile) return false;
								if (tile.TileType == TileID.Spikes) ignite = true;
								return tile.TileType == TileID.BreakableIce || TileID.Sets.IcesSnow[tile.TileType];
							}
							Point pos = analysis.Counted[^1];
							if (pos.Y < GenVars.rockLayer + 40) return true;
							return IsSnowBiome(pos + UnitX)
							|| IsSnowBiome(pos - UnitX)
							|| IsSnowBiome(pos + UnitY)
							|| IsSnowBiome(pos - UnitY);
						}
						AreaAnalysis analysis = AreaAnalysis.March(i, j, AreaAnalysis.Orthogonals, Counter, Breaker);
						if (analysis.Broke) continue;
						int multiPoolCounter = 0;
						foreach (Point pos in analysis.Counted) {
							tile = Main.tile[pos];
							oilCount += tile.LiquidAmount;
							multiPoolCounter += tile.LiquidAmount;
							if (multiPoolCounter > 64 * 255 && oilCountTarget < Main.maxTilesX * Main.maxTilesY * 0.2f) oilCountTarget += tile.LiquidAmount;
							tile.LiquidType = ignite ? burningOil : oil;
						}
					} else if (++tries > max_tries) break;
				}
				Mod.Logger.Info($"Generated {oilCount / 255f:0.##} blocks of oil, finished because {(tries > max_tries ? "more liquid could not be found, " : "")}target was {oilCountTarget / 255f:0.##}");
			}));
			genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Tile Cleanup"));
			tasks.Insert(genIndex + 1, new PassLegacy("Brine Droplets", (progress, _) => {
				ushort stoneID = (ushort)ModContent.TileType<Baryte>();
				ushort dropletID = (ushort)ModContent.TileType<Magic_Dropper_Brine>();
				for (int j = 0; j < Main.maxTilesY - 1; j++) {
					for (int i = 0; i < Main.maxTilesX; i++) {
						if (Main.tile[i, j].TileType == stoneID) {
							Tile tile = Main.tile[i, j + 1];
							if (tile.TileIsType(TileID.WaterDrip)) tile.TileType = dropletID;
						}
					}
				}
			}));

			if (remixWorldGen) {
				genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Corruption"));
				tasks.Insert(genIndex + 1, new PassLegacy("Gem (Singular)", (progress, _) => {
					Dictionary<ushort, ushort> types = Chambersite_Stone_Wall.AddChambersite;
					int totalCount = 0;
					float tryCount = 0;
					for (float i = 0; i < ((Main.maxTilesX * Main.maxTilesY) * 0.5E-05f); i++) {
						Vector2 pos = genRand.NextVector2FromRectangle(genRand.Next(WorldBiomeGeneration.EvilBiomeGenRanges));
						int count = GenRunners.DictionaryWallRunner(
							(int)pos.X,
							(int)pos.Y,
							genRand.Next(2, 4),
							genRand.Next(3, 6),
							types
						);
						totalCount += count;
						if (count == 0) i -= 0.9f;
						tryCount++;
					}
					Origins.instance.Logger.Info($"Generated {totalCount} chambersite walls over {tryCount} tries");
				}));
			}
		}
		static Point UnitX = new(1, 0);
		static Point UnitY = new(0, 1);
		public static bool HasTriggerWithinRange(int i, int j, int range) {
			List<Point> currentPoints = [];
			List<Point> nextPoints = [];
			HashSet<Point> prevPoints = [];
			nextPoints.Add(new(i, j));
			while (nextPoints.Count > 0) {
				Utils.Swap(ref currentPoints, ref nextPoints);
				while (currentPoints.Count > 0) {
					Point item = currentPoints[0];
					currentPoints.RemoveAt(0);
					if (!InWorld(item.X, item.Y, 5)) {
						continue;
					}
					Tile tile = Main.tile[item.X, item.Y];
					if (tile.RedWire) {
						prevPoints.Add(item);
						if (IsItATrigger(tile) && Math.Abs(item.X - i) <= range) {
							return true;
						}
						Point item2 = new(item.X - 1, item.Y);
						if (!prevPoints.Contains(item2)) {
							nextPoints.Add(item2);
						}
						item2 = new(item.X + 1, item.Y);
						if (!prevPoints.Contains(item2)) {
							nextPoints.Add(item2);
						}
						item2 = new(item.X, item.Y - 1);
						if (!prevPoints.Contains(item2)) {
							nextPoints.Add(item2);
						}
						item2 = new(item.X, item.Y + 1);
						if (!prevPoints.Contains(item2)) {
							nextPoints.Add(item2);
						}
					}
				}
			}
			return false;
		}
		public static void RemoveTree(int i, int j) {
			Tile tile = Main.tile[i, j];

			while (tile.TileIsType(TileID.Trees)) {
				if (Main.tile[i - 1, j].TileIsType(TileID.Trees)) {
					Main.tile[i - 1, j].ClearTile();
				}
				if (Main.tile[i + 1, j].TileIsType(TileID.Trees)) {
					Main.tile[i + 1, j].ClearTile();
				}
				tile.HasTile = false;
				tile = Main.tile[i, --j];
			}
		}
		public static void getEvilTileConversionTypes(byte evilType, out ushort stoneType, out ushort grassType, out ushort plantType, out ushort sandType, out ushort sandstoneType, out ushort hardenedSandType, out ushort iceType) {
			switch (evilType) {
				case evil_wastelands:
				stoneType = (ushort)TileType<Defiled_Stone>();
				grassType = (ushort)TileType<Defiled_Grass>();
				plantType = (ushort)TileType<Defiled_Foliage>();
				sandType = (ushort)TileType<Defiled_Sand>();
				sandstoneType = (ushort)TileType<Defiled_Sandstone>();
				hardenedSandType = (ushort)TileType<Hardened_Defiled_Sand>();
				iceType = (ushort)TileType<Defiled_Ice>();
				break;
				case evil_riven:
				stoneType = (ushort)TileType<Spug_Flesh>();
				grassType = stoneType;
				plantType = (ushort)TileType<Riven_Foliage>();
				sandType = stoneType;
				sandstoneType = stoneType;
				hardenedSandType = stoneType;
				iceType = stoneType;
				break;
				case evil_crimson:
				stoneType = TileID.Crimstone;
				grassType = TileID.CrimsonGrass;
				plantType = TileID.CrimsonPlants;
				sandType = TileID.Crimsand;
				sandstoneType = TileID.CrimsonSandstone;
				hardenedSandType = TileID.CrimsonHardenedSand;
				iceType = TileID.FleshIce;
				break;
				default:
				stoneType = TileID.Ebonstone;
				grassType = TileID.CorruptGrass;
				plantType = TileID.CorruptPlants;
				sandType = TileID.Ebonsand;
				sandstoneType = TileID.CorruptSandstone;
				hardenedSandType = TileID.CorruptHardenedSand;
				iceType = TileID.CorruptIce;
				break;
			}
		}
	}
}