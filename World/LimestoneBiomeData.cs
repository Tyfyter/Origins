using AltLibrary.Common.Systems;
using Origins.Items.Accessories;
using Origins.Items.Armor.Other;
using Origins.Reflection;
using Origins.Tiles.Limestone;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Biomes;
using Terraria.GameContent.Biomes.Desert;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Origins.World {
	public class Limestone_Cave : ModBiome {
		public override int Music => -1;
		public override void Load() {
			On_DesertBiome.Place += On_DesertBiome_Place;
			On_AnthillEntrance.PlaceAt += On_AnthillEntrance_PlaceAt;
			On_ChambersEntrance.PlaceAt += On_ChambersEntrance_PlaceAt;
			On_LarvaHoleEntrance.PlaceAt += On_LarvaHoleEntrance_PlaceAt;
			On_PitEntrance.PlaceAt += On_PitEntrance_PlaceAt;

			On_WorldGen.Pyramid += On_WorldGen_Pyramid;
			WorldGen.DetourPass((PassLegacy)WorldGen.VanillaGenPasses["Shinies"], Detour_Shinies);
			WorldGen.DetourPass((PassLegacy)WorldGen.VanillaGenPasses["Mountain Caves"], Detour_MountainCaves);
		}
		static void Detour_Shinies(WorldGen.orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration) {
			bool rope = TileID.Sets.CanBeClearedDuringGeneration[TileID.Rope];
			bool fire = TileID.Sets.CanBeClearedDuringGeneration[TileID.Campfire];

			TileID.Sets.CanBeClearedDuringGeneration[TileID.Rope] = false;
			TileID.Sets.CanBeClearedDuringGeneration[TileID.Campfire] = false;
			TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Pile_Medium>()] = false;
			TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Stalactite>()] = false;
			TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Stalagmite>()] = false;
			try {
				orig(self, progress, configuration);
			} catch (Exception) {
				TileID.Sets.CanBeClearedDuringGeneration[TileID.Rope] = rope;
				TileID.Sets.CanBeClearedDuringGeneration[TileID.Campfire] = fire;
				TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Pile_Medium>()] = true;
				TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Stalactite>()] = true;
				TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Stalagmite>()] = true;
				throw;
			}
			TileID.Sets.CanBeClearedDuringGeneration[TileID.Rope] = rope;
			TileID.Sets.CanBeClearedDuringGeneration[TileID.Campfire] = fire;
			TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Pile_Medium>()] = true;
			TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Stalactite>()] = true;
			TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Stalagmite>()] = true;
		}
		static void Detour_MountainCaves(WorldGen.orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration) {
			TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Pile_Medium>()] = false;
			TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Stalactite>()] = false;
			TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Stalagmite>()] = false;
			try {
				orig(self, progress, configuration);
			} catch (Exception) {
				TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Pile_Medium>()] = true;
				TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Stalactite>()] = true;
				TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Stalagmite>()] = true;
				throw;
			}
			TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Pile_Medium>()] = true;
			TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Stalactite>()] = true;
			TileID.Sets.CanBeClearedDuringGeneration[ModContent.TileType<Limestone_Stalagmite>()] = true;
		}
		static int hatX, hatY;
		bool On_WorldGen_Pyramid(On_WorldGen.orig_Pyramid orig, int i, int j) {
			if (orig(i, j)) {
				static bool CheckBarrelPlacement(int x, int y) {
					for (int i = 0; i < 2; i++) {
						for (int j = 0; j < 3; j++) {
							if (Framing.GetTileSafely(x + i, y - j).HasTile) return false;
						}
						if (!Framing.GetTileSafely(x + i, y + 1).TileIsType(TileID.SandstoneBrick)) return false;
					}
					return true;
				}
				for (int k = 0; k < 50; k++) {
					for (int l = 0; l <= 50; l++) {
						if (CheckBarrelPlacement(i + l, j + k)) {
							Gen.PlaceBarrel(i + l, j + k, -1);
							goto placedBarrel;
						}
						if (CheckBarrelPlacement(i - l, j + k)) {
							Gen.PlaceBarrel(i - l, j + k, 1);
							goto placedBarrel;
						}
					}
				}
				placedBarrel:;
				return true;
			}
			return false;
		}

		void On_AnthillEntrance_PlaceAt(On_AnthillEntrance.orig_PlaceAt orig, DesertDescription description, Point position, int holeRadius) {
			orig(description, position, holeRadius);
			desertEntrances.Add((position.X - 30, position.X + 30));
		}
		void On_ChambersEntrance_PlaceAt(On_ChambersEntrance.orig_PlaceAt orig, DesertDescription description, Point position) {
			orig(description, position);
			desertEntrances.Add((position.X - 70, position.X + 70));
		}
		void On_LarvaHoleEntrance_PlaceAt(On_LarvaHoleEntrance.orig_PlaceAt orig, DesertDescription description, Point position, int holeRadius) {
			orig(description, position, holeRadius);
			desertEntrances.Add((position.X - 30, position.X + 30));
		}
		void On_PitEntrance_PlaceAt(On_PitEntrance.orig_PlaceAt orig, DesertDescription description, Point position, int holeRadius) {
			orig(description, position, holeRadius);
			desertEntrances.Add((position.X - holeRadius, position.X + holeRadius));
		}

		readonly List<(int start, int end)> desertEntrances = [];
		bool On_DesertBiome_Place(On_DesertBiome.orig_Place orig, DesertBiome self, Point origin, StructureMap structures) {
			desertEntrances.Clear();
			bool placed = orig(self, origin, structures);
			if (placed && (Main.tenthAnniversaryWorld || !Framing.GetTileSafely(hatX, hatY).TileIsType(ModContent.TileType<Lucky_Hat_Tile>()))) {
				RangeRandom rand = new(WorldGen.genRand, GenVars.UndergroundDesertLocation.Left + 100, GenVars.UndergroundDesertLocation.Right - 100);
				for (int i = 0; i < desertEntrances.Count; i++) {
					rand.Multiply(desertEntrances[i].start, desertEntrances[i].end, 1.4e-42f);
				}
				while (rand.AnyWeight) {
					int X = rand.Get();
					int Y;
					for (Y = (int)GenVars.worldSurfaceLow; !Main.tile[X, Y].HasFullSolidTile(); Y++) ;
					if (TileID.Sets.isDesertBiomeSand[Main.tile[X, Y].TileType]) {
						Gen.StartLimestone(X, Y);
						break;
					}
					rand.Multiply(X, X, 0);
				}
			}
			return placed;
		}

		public override bool IsBiomeActive(Player player) {
			return OriginSystem.limestoneTiles > NeededTiles;
		}
		public const int NeededTiles = 400;
		public static class Gen {
			public static void StartLimestone(int i, int j) {
				WorldBiomeGeneration.ChangeRange.ResetRange();
				ushort limestoneTile = (ushort)ModContent.TileType<Limestone>();
				int openingHeight = WorldGen.genRand.Next(25, 40);
				int openingSize = WorldGen.genRand.Next(9 + openingHeight / 5, 20);
				for (int x = -openingSize; x <= openingSize; x++) {
					for (int y = 0; y < 10; y++) {
						Tile tile = Framing.GetTileSafely(i + x, j - y);
						if (!tile.HasTile || !TileID.Sets.Conversion.Sand[tile.TileType]) break;
						tile.HasTile = false;
					}
				}
				float openingSlopeLeft = WorldGen.genRand.NextFloat(0.5f, 0.6f);
				float openingSlopeRight = WorldGen.genRand.NextFloat(0.5f, 0.6f);
				if (openingSlopeLeft * openingHeight >= openingSize - 1) openingSlopeLeft = (openingSize - 1) / (float)openingHeight;
				if (openingSlopeRight * openingHeight >= openingSize - 1) openingSlopeRight = (openingSize - 1) / (float)openingHeight;
				int minOpeningSizeLeft = 0;
				int minOpeningSizeRight = 0;
				for (int y = 0; y < openingHeight; y++) {
					minOpeningSizeLeft = (int)(openingSize - y * openingSlopeLeft);
					minOpeningSizeRight = (int)(openingSize - y * openingSlopeRight);
					for (int x = -minOpeningSizeLeft; x <= minOpeningSizeRight; x++) {
						Tile tile = Framing.GetTileSafely(i + x, j + y);
						tile.HasTile = false;
					}
				}
				int sizeLeft = minOpeningSizeLeft + WorldGen.genRand.Next(30, 50);
				int sizeRight = minOpeningSizeRight + WorldGen.genRand.Next(30, 50);
				j += openingHeight;
				for (int y = 0; y < 4; y++) {
					for (int x = -minOpeningSizeLeft; x <= minOpeningSizeRight; x++) {
						Tile tile = Framing.GetTileSafely(i + x, j + y);
						tile.HasTile = false;
					}
				}
				for (int x = -sizeLeft; x <= sizeRight; x++) {
					int dist = 0;
					if (x < -minOpeningSizeLeft) {
						dist = minOpeningSizeLeft - x;
					} else if (x > minOpeningSizeRight) {
						dist = x - minOpeningSizeRight;
					}
					int limeHeightHere = (int)Math.Max(GenRunners.GetWallDistOffset(i + x) + 2, 0) + Math.Min(dist, openingHeight / 2);
					int startY = j - 1;
					if (x < 10 - sizeLeft) {
						float curveFactor = sizeLeft + x - 10;
						startY += (int)(curveFactor * curveFactor / 10);
					} else if (x > sizeRight - 10) {
						float curveFactor = sizeRight - x - 10;
						startY += (int)(curveFactor * curveFactor / 10);
					}
					for (int y = 0; y < limeHeightHere; y++) {
						Tile tile = Framing.GetTileSafely(i + x, startY - y);
						if (!tile.HasTile || !TileID.Sets.Conversion.Sand[tile.TileType]) break;
						tile.TileType = limestoneTile;
						WorldBiomeGeneration.ChangeRange.AddChangeToRange(i + x, startY - y);
					}
				}
				static int Squirclish(float x, float y, float radius1, float radius2) {
					float squareDist = Math.Max(Math.Abs(x), Math.Abs(y));
					float circleDist = MathF.Sqrt(x * x + y * y);
					if (MathHelper.Lerp(squareDist, circleDist, MathF.Pow(circleDist / (radius1 * 1.4f), 2)) < radius1) return 0;
					if (MathHelper.Lerp(squareDist, circleDist, MathF.Pow(circleDist / (radius2 * 1.4f), 2)) < radius2) return 1;
					return 2;
				}
				int caveHeight = (sizeLeft + sizeRight) - WorldGen.genRand.Next(0, 20);
				float halfCaveHeight = caveHeight * 0.5f;
				float halfCaveWidth = (sizeRight - sizeLeft) * 0.5f;
				float radius1 = (sizeLeft + sizeRight) * 0.5f - 3;
				float radius2 = radius1 + 10;
				for (int x = -(sizeLeft + 10); x <= sizeRight + 10; x++) {
					int side = Math.Sign(x);
					for (int y = 0; y < caveHeight + 20; y++) {
						switch (Squirclish(x - halfCaveWidth + (int)(GenRunners.GetWallDistOffset(j + y) * 1 * side), y - halfCaveHeight, radius1, radius2)) {
							case 0: {
								Tile tile = Framing.GetTileSafely(i + x, j + y);
								tile.HasTile = false;
								if (TileID.Sets.Falling[tile.TileType]) tile.TileType = limestoneTile;
								if (tile.WallType == WallID.DirtUnsafe) tile.WallType = WallID.HardenedSand;
								WorldBiomeGeneration.ChangeRange.AddChangeToRange(i + x, j + y);
								break;
							}
							case 1: {
								Tile tile = Framing.GetTileSafely(i + x, j + y);
								tile.TileType = limestoneTile;
								if (tile.WallType == WallID.DirtUnsafe) tile.WallType = WallID.HardenedSand;
								WorldBiomeGeneration.ChangeRange.AddChangeToRange(i + x, j + y);
								break;
							}
						}
					}
				}
				WeightedRandom<(Vector2 startPos, Vector2 direction, float length)> rayRand = new(WorldGen.genRand);
				for (float a = 0; a < MathHelper.Pi * 0.75f; a += MathHelper.Pi * 0.05f) {
					Vector2 rayStart = (new Vector2(i + halfCaveWidth, j + halfCaveHeight) + WorldGen.genRand.NextVector2Circular(10, 10)) * 16;
					Vector2 direction = (a + MathHelper.PiOver4 * 0.5f).ToRotationVector2();
					float distToWall = CollisionExt.Raymarch(rayStart, direction, 200 * 16);
					if (distToWall < 200 * 16) {
						float distToAir = CollisionExtensions.Raymarch(rayStart + direction * (distToWall + 4), direction, tile => tile.HasTile, 20 * 16);
						if (distToAir > 0 && distToAir < 20 * 16) {
							rayRand.Add((rayStart + direction * distToWall, direction, distToAir), (1 - distToAir / (50 * 16)) * (1 - direction.Y));
						}
					}
				}
				try {
					TileID.Sets.CanBeClearedDuringGeneration[limestoneTile] = true;
					for (int k = Math.Min(WorldGen.genRand.Next(1, 4), rayRand.elements.Count); k > 0; k--) {
						if (k >= 3 && !WorldGen.genRand.NextBool()) k--;
						(Vector2 startPos, Vector2 direction, float length) = rayRand.Pop();
						GenRunners.VeinRunner((int)startPos.X / 16, (int)startPos.Y / 16, WorldGen.genRand.NextFloat(3, 5), direction, length / 16);
					}
				} finally {
					TileID.Sets.CanBeClearedDuringGeneration[limestoneTile] = false;
				}
				static int DoSpike(int i, float j, int direction, ushort tileType, float size, float decay, float decayExp, float cutoff) {
					for (int x = 0; x < 10; x++) {
						for (int k = 1 - (int)size; k < size; k++) {
							Tile tile = Framing.GetTileSafely(i - x * direction, (int)(j + k));
							tile.TileType = tileType;
							tile.HasTile = true;
						}
					}
					int loopCount = 0;
					float currentSize = size;
					while (currentSize >= cutoff && loopCount < 100) {
						for (int k = 1 - (int)currentSize; k < currentSize; k++) {
							Tile tile = Framing.GetTileSafely(i, (int)(j + k));
							tile.TileType = tileType;
							tile.HasTile = true;
						}
						i += direction;
						currentSize = size - decay * MathF.Pow(loopCount, decayExp);
						loopCount++;
					}
					currentSize = size;
					i -= direction * loopCount;
					for (int k = 0; k < loopCount; k++) {
						for (int l = 1 - (int)currentSize; l < currentSize; l++) {
							GenRunners.AutoSlope(i + k, (int)(j + l), true);
						}
						i += direction;
						currentSize = size - decay * MathF.Pow(k, decayExp);
					}
					return loopCount;
				}
				int infLoopGuard = 0;
				List<Vector2> spikePositions = [];
				for (int k = Math.Min(WorldGen.genRand.Next(4, 11), WorldGen.genRand.Next(4, 11)); k > 0; k--) {
					Vector2 rayStart = (new Vector2(i, j + WorldGen.genRand.NextFloat(20, caveHeight - 20))) * 16;
					Vector2 direction = WorldGen.genRand.NextBool().ToDirectionInt() * Vector2.UnitX;
					float distToWall = CollisionExt.Raymarch(rayStart, direction, 200 * 16);
					Vector2 pos = rayStart + direction * (distToWall + 1);
					if (distToWall < 200 * 16 && Framing.GetTileSafely(pos).TileIsType(limestoneTile)) {
						spikePositions.Add(pos);
					} else if (++infLoopGuard < 1000) {
						k++;
					}
				}
				int directions = -1;
				for (int k = 0; k < spikePositions.Count; k++) {
					directions &= spikePositions[k].X > 0 ? -2 : 1;
					DoSpike(
						(int)spikePositions[k].X / 16, spikePositions[k].Y / 16,
						spikePositions[k].X > 0 ? -1 : 1,
						limestoneTile,
						WorldGen.genRand.NextFloat(3, 4.5f),
						WorldGen.genRand.NextFloat(0.1f, 0.2f),
						WorldGen.genRand.NextFloat(1.2f, 1.5f),
						WorldGen.genRand.NextFloat(1, 4)
					);
				}
				if (directions != 0) {
					Vector2 direction;
					switch (directions) {
						default:
						if (WorldGen.genRand.NextBool()) goto case 1;
						goto case -2;

						case 1: // all spikes on left side, put one on the right
						direction = Vector2.UnitX;
						break;
						case -2: // all spikes on right side, put one on the left
						direction = -Vector2.UnitX;
						break;
					}
					int dirInt = (int)direction.X;
					infLoopGuard = 0;
					loop:
					Vector2 rayStart = (new Vector2(i, j + WorldGen.genRand.NextFloat(20, caveHeight - 20))) * 16;
					float distToWall = CollisionExt.Raymarch(rayStart, direction, 100 * 16);
					Vector2 pos = rayStart + direction * (distToWall + 16 * 5);
					if (distToWall < 100 * 16 && Framing.GetTileSafely(pos).TileIsType(limestoneTile)) {
						int length = DoSpike(
							(int)pos.X / 16, pos.Y / 16,
							direction.X > 0 ? -1 : 1,
							limestoneTile,
							WorldGen.genRand.NextFloat(5, 8f),
							0.15f,
							1,
							WorldGen.genRand.NextFloat(2, 3)
						);
						List<Point> validPoints = [];
						for (int k = 2; k < length - 2; k++) {
							for (int l = 0; l < length; l++) {
								Point chestPos = new((int)pos.X / 16 - k * dirInt, (int)pos.Y / 16 - l);
								if (!Framing.GetTileSafely(chestPos).HasTile) {
									validPoints.Add(chestPos);
									break;
								}
							}
						}
						while (validPoints.Count > 0) {
							int index = WorldGen.genRand.Next(validPoints.Count);
							Point chestPos = validPoints[index];
							validPoints.RemoveAt(index);
							if (PlaceBarrel(chestPos.X, chestPos.Y, direction.X > 0 ? 1 : -1)) {
								directions = 0;
								break;
							}
						}
						if (directions == 0) {
							Point ropePoint = new((int)pos.X / 16 - length * dirInt, (int)pos.Y / 16);
							for (int k = 0; k < 6; k++) {
								if (Framing.GetTileSafely(ropePoint.X + dirInt, ropePoint.Y - 1).HasTile) ropePoint.Y--;
							}
							for (int k = 0; k < 100; k++) {
								if (Framing.GetTileSafely(ropePoint.X, ropePoint.Y).HasTile) break;
								if (!WorldGen.PlaceTile(ropePoint.X, ropePoint.Y, TileID.Rope)) break;
								ropePoint.Y++;
							}
						}
					} else if (++infLoopGuard < 1000) goto loop;
				}
				if (directions != 0) {
					infLoopGuard = 0;
					while (++infLoopGuard < 10000) {
						Point chestPos = new(i + WorldGen.genRand.Next(-sizeLeft, sizeRight), j + (int)WorldGen.genRand.NextFloat(halfCaveHeight * 1.5f * (1 - infLoopGuard / 10000f), caveHeight));
						if (PlaceBarrel(chestPos.X, chestPos.Y, WorldGen.genRand.NextBool().ToDirectionInt())) {
							break;
						}
					}
				}
				ushort[] piles = [
					(ushort)ModContent.TileType<Limestone_Pile_Medium>(),
					(ushort)ModContent.TileType<Limestone_Stalactite>(),
					(ushort)ModContent.TileType<Limestone_Stalagmite>()
				];
				for (int k = 0; k < piles.Length; k++) {
					for (int i0 = WorldGen.genRand.Next(1000, 1500); i0-- > 0;) {
						int tries = 18;
						int x = i + WorldGen.genRand.Next(-sizeLeft, sizeRight);
						int y = j + WorldGen.genRand.Next(0, caveHeight);
						while (WorldGen.PlaceObject(x, y, piles[k], random: WorldGen.genRand.Next(3))) {
							if (k == 1) y--;
							else y++;
							if (tries-- > 0) break;
						}
					}
				}
				Rectangle genRange = WorldBiomeGeneration.ChangeRange.GetRange();
				for (int x = genRange.Left; x < genRange.Right; x++) {
					for (int y = genRange.Top; y < genRange.Bottom; y++) {
						GenRunners.AutoSlope(x, y, true);
					}
				}
			}
			public static bool PlaceBarrel(int x, int y, int direction) {
				for (int i = 0; i < 2; i++) {
					for (int j = 0; j < 3; j++) {
						if (Framing.GetTileSafely(x + i, y - j).HasTile) return false;
					}
				}
				int chestIndex = WorldGen.PlaceChest(x, y, style: ChestID.Barrel);
				if (chestIndex != -1) {
					int luckyHat = ModContent.TileType<Lucky_Hat_Tile>();
					bool placedHat = false;
					if (TileExtenstions.CanActuallyPlace(x, y - 2, luckyHat, 0, direction, out TileObject objectData, onlyCheck: false)) {
						if (TileObject.Place(objectData)) {
							placedHat = true;
							WorldGen.SquareTileFrame(x, y - 2);
						}
					}
					Chest chest = Main.chest[chestIndex];
					if (!placedHat) {
						Chest.DestroyChest(chest.x, chest.y);
						for (int i = 0; i < 2; i++) {
							for (int j = 0; j < 2; j++) {
								Tile tile = Framing.GetTileSafely(x + i, y - j);
								tile.HasTile = false;
							}
						}
						return false;
					}
					hatX = x;
					hatY = y - 2;
					int itemIndex = 0;
					chest.item[itemIndex].SetDefaults(WorldGen.genRand.NextFromList(
						ItemID.Revolver,
						ItemID.SandstorminaBottle,
						ItemID.FlyingCarpet,
						ModContent.ItemType<Dysfunctional_Endless_Explosives_Bag>())
					);
					if (WorldGen.genRand.NextBool(7, 8)) {
						chest.item[++itemIndex].SetDefaults(ItemID.Rope);
						chest.item[itemIndex].stack = WorldGen.genRand.Next(25, 76);
					}
					if (WorldGen.genRand.NextBool(3, 5)) {
						chest.item[++itemIndex].SetDefaults(WorldGen.genRand.NextFromList(ItemID.Dynamite, ItemID.Explosives));
						chest.item[itemIndex].stack = WorldGen.genRand.Next(1, 4);
					}
					if (WorldGen.genRand.NextBool(4, 5)) {
						chest.item[++itemIndex].SetDefaults(ItemID.BottledWater);
						chest.item[itemIndex].stack = WorldGen.genRand.Next(3, 9);
					}
					chest.item[++itemIndex].SetDefaults(ItemID.GoldCoin);
					chest.item[itemIndex].stack = WorldGen.genRand.Next(4, 8);
					if (WorldGen.genRand.NextBool(4, 5)) {
						chest.item[++itemIndex].SetDefaults(ItemID.Cobweb);
						chest.item[itemIndex].stack = WorldGen.genRand.Next(30, 91);
					}

					int infLoopGuard = 0;
					while (++infLoopGuard < 1000) {
						Point extraPos = new(x + WorldGen.genRand.Next(-20, 21), y + WorldGen.genRand.Next(-20, 21));
						WorldGen.PlaceTile(extraPos.X, extraPos.Y, TileID.Campfire);
						if (Framing.GetTileSafely(extraPos).TileIsType(TileID.Campfire)) break;
					}
					return true;
				}
				return false;
			}
		}
	}
}
