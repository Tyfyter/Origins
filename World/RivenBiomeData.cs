using AltLibrary.Common.AltBiomes;
using AltLibrary.Core.Generation;
using Microsoft.Xna.Framework;
using Origins.Backgrounds;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Other.Testing;
using Origins.Items.Pets;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.Misc;
using Origins.NPCs.Riven;
using Origins.Tiles;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using Origins.Walls;
using Origins.Water;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Tyfyter.Utils;
using static Origins.OriginExtensions;
using static Terraria.WorldGen;

namespace Origins.World.BiomeData {
	public class Riven_Hive : ModBiome {
		public static IItemDropRule FirstLesionDropRule;
		public static IItemDropRule LesionDropRule;
		public override int Music => Origins.Music.Riven;
		public override ModWaterStyle WaterStyle => ModContent.GetInstance<Riven_Water_Style>();
		public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<Riven_Surface_Background>();
		public override int BiomeTorchItemType => ModContent.ItemType<Riven_Torch>();
		public override int BiomeCampfireItemType => ModContent.ItemType<Riven_Campfire_Item>();
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BestiaryIcon => "Origins/UI/Cleaver_Preview";
		public override string BackgroundPath => "Origins/UI/MapBGs/Riven_Hive_Caverns";
		public override string MapBackground => BackgroundPath;
		public static ModBiomeBestiaryInfoElement BestiaryInfoElement => ModContent.GetInstance<Riven_Hive>().ModBiomeBestiaryInfoElement;
		public static FrameCachedValue<float> NormalGlowValue { get; private set; }  = new(() => (float)(Math.Sin(Main.GlobalTimeWrappedHourly) + 2) * 0.5f);
		public override bool IsBiomeActive(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.ZoneRivenProgress = Math.Min(OriginSystem.rivenTiles - (NeededTiles - ShaderTileCount), ShaderTileCount) / ShaderTileCount;
			LinearSmoothing(ref originPlayer.ZoneRivenProgressSmoothed, originPlayer.ZoneRivenProgress, OriginSystem.biomeShaderSmoothing * 0.1f);

			return OriginSystem.rivenTiles > NeededTiles;
		}
		public override void SpecialVisuals(Player player, bool isActive) {
			//OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			//Filters.Scene["Origins:ZoneRiven"].GetShader().UseProgress(originPlayer.ZoneRivenProgressSmoothed);
			//player.ManageSpecialBiomeVisuals("Origins:ZoneRiven", originPlayer.ZoneRivenProgressSmoothed > 0, player.Center);
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneRivenProgress * 0.98f;
		}
		public override void Load() {
			FirstLesionDropRule = ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Riven_Splitter>())
				.WithOnSuccess(ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Harpoon>(), 1, 99, 99));

			LesionDropRule = new OneFromRulesRule(1,
				FirstLesionDropRule,
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Amebolize_Incantation>()),
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Slashswarm>()),
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Riverang>()),
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Amoeba_Toy>()),
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Parasitic_Influence>())
			);
		}
		public override void Unload() {
			FirstLesionDropRule = null;
			LesionDropRule = null;
			NormalGlowValue = null;
		}
		public const int NeededTiles = 200;
		public const int ShaderTileCount = 25;
		public const short DefaultTileDust = DustID.BlueMoss;
		public static class SpawnRates {
			public const float AmebSlime = 0.65f;
			public const float Fighter = 1;
			public const float Flajelly = 0.37f;
			public const float BBack = 0.45f;
			public const float BlisterBoi = 0.75f;
			public const float Seashell = 0.6f;
			public const float Spighter = 1;
			public const float Mummy = 1;
			public const float Cleaver = 0.7f;
			public const float Barnacle = 0.5f;
			public const float Moeba = 0.8f;
			public const float Shark1 = 0.4f;
			public const float Worm = 0.6f;
			public const float Crawler = 0.8f;
			public static float LandEnemyRate(NPCSpawnInfo spawnInfo, bool hardmode = false) {
				if (hardmode && !Main.hardMode) return 0f;
				if (TileLoader.GetTile(spawnInfo.SpawnTileType) is RivenTile || (spawnInfo.Player.InModBiome<Riven_Hive>() && spawnInfo.SpawnTileType == ModContent.TileType<Encrusted_Ore>())) {
					return 1f;
				}
				return 0f;
			}
			public static float FlyingEnemyRate(NPCSpawnInfo spawnInfo, bool hardmode = false) {
				if (hardmode && !Main.hardMode) return 0f;
				return spawnInfo.Player.InModBiome<Riven_Hive>() ? 1f : 0f;
			}
		}
		public static class Gen {
			static int lesionCount = 0;
			public enum FeatureType {
				CHUNK,
				CUSP,
				CAVE
			}
			public static void StartHive(int i, int j) {
				lesionCount = 0;
				Vector2 pos = new Vector2(i, j);
				ushort fleshBlockType = (ushort)ModContent.TileType<Riven_Flesh>();
				ushort fleshWallType = (ushort)ModContent.WallType<Riven_Flesh_Wall>();
				Tile tile;
				int X0 = int.MaxValue;
				int X1 = 0;
				int Y0 = int.MaxValue;
				int Y1 = 0;
				List<(int x, int y, FeatureType type, bool rightSide)> features = new();

				float targetTwist = genRand.NextFloat(-0.5f, 0.5f);
				PolarVec2 speed = new PolarVec2(8, genRand.NextFloat(-0.5f, 0.5f) + MathHelper.PiOver2);
				double strength = 32;
				double baseStrength = strength;
				strength = Math.Pow(strength, 2);
				double wallThickness = 24;
				float decay = 1;
				while (strength > 16) {
					int minX = (int)(pos.X - (strength + wallThickness) * 0.5);
					int maxX = (int)(pos.X + (strength + wallThickness) * 0.5);
					int minY = (int)(pos.Y - (strength + wallThickness) * 0.5);
					int maxY = (int)(pos.Y + (strength + wallThickness) * 0.5);
					if (minX < 1) {
						minX = 1;
					}
					if (maxX > Main.maxTilesX - 1) {
						maxX = Main.maxTilesX - 1;
					}
					if (minY < 1) {
						minY = 1;
					}
					if (maxY > Main.maxTilesY - 1) {
						maxY = Main.maxTilesY - 1;
					}
					bool generatedChunk = false;
					for (int x = minX; x < maxX; x++) {
						for (int y = minY; y < maxY; y++) {
							double wallOffset = (GenRunners.GetWallDistOffset((float)Math.Atan2(y - j, x - i) * 4 + x + y) * 0.0316076058772687986171132238548f + 1);
							double dist = (Math.Pow(Math.Abs(x - pos.X), 2) + Math.Pow(Math.Abs(y - pos.Y), 2)) * wallOffset;
							tile = Main.tile[x, y];
							int compY = (int)(y + wallOffset);
							if (dist > strength) {
								double d = Math.Sqrt(dist);
								if (d < baseStrength + wallThickness && OriginExtensions.IsTileReplacable(x, y)) {
									if (tile.WallType != fleshWallType) {
										if (compY > j || (tile.HasTile && Main.tileSolid[tile.TileType])) {
											SpreadRivenGrass(x, y);
											tile.HasTile = true;
											tile.TileType = fleshBlockType;
											tile.WallType = fleshWallType;
										} else if (tile.WallType != WallID.None) {
											tile.WallType = fleshWallType;
										}
									}

									if (compY > j && d < baseStrength + 8 && genRand.Next(1500000 + (int)strength) < strength + 424) {
										features.Add((x, y, genRand.NextBool() ? FeatureType.CUSP : FeatureType.CAVE, x > i));
									}
									//WorldGen.SquareTileFrame(l, k);
								}
								continue;
							}
							if (TileID.Sets.CanBeClearedDuringGeneration[tile.TileType]) {
								if (compY > j && OriginExtensions.IsTileReplacable(x, y)) {
									tile.HasTile = false;
								} else if (tile.HasTile && Main.tileSolid[tile.TileType]) {
									tile.TileType = fleshBlockType;
								}
								//WorldGen.SquareTileFrame(l, k);
								if (compY > j || tile.WallType != WallID.None) tile.WallType = fleshWallType;
								if (x > X1) {
									X1 = x;
								} else if (x < X0) {
									X0 = x;
								}
								if (y > Y1) {
									Y1 = y;
								} else if (y < Y0) {
									Y0 = y;
								}
								if (!generatedChunk && genRand.Next(3000000) < strength + 924) {
									features.Add((x, y, FeatureType.CHUNK, false));
									//generatedChunk = true;
								}
							}
						}
					}
					pos += (Vector2)speed;
					if (OriginExtensions.LinearSmoothing(ref speed.Theta, targetTwist + MathHelper.PiOver2, 0.05f)) targetTwist = genRand.NextFloat(-0.5f, 0.5f);
					strength *= decay;
					speed.R = (float)Math.Min(speed.R, (float)Math.Sqrt(strength));
					decay -= 0.01f;
				}
				if (X0 < 1) {
					X0 = 1;
				}
				if (Y0 > Main.maxTilesX - 1) {
					Y0 = Main.maxTilesX - 1;
				}
				if (X1 < 1) {
					X1 = 1;
				}
				if (Y1 > Main.maxTilesY - 1) {
					Y1 = Main.maxTilesY - 1;
				}
				for (int index = 0; index < features.Count; index++) {
					(int x, int y, FeatureType type, bool rightSide) = features[index];
					tile = Main.tile[x, y];
					switch (type) {
						case FeatureType.CHUNK:
						GenRunners.SpikeRunner(x, y,
							fleshBlockType,
							Vec2FromPolar(genRand.NextFloat(MathHelper.TwoPi), genRand.NextFloat(4, 8)),
							8,
							twist: genRand.NextFloat(-2, 2) + MathHelper.Pi
						);
						break;
						case FeatureType.CUSP:
						GenRunners.SpikeRunner(x, y,
							fleshBlockType,
							Vec2FromPolar(genRand.NextFloat(-0.1f, 0.1f) + (rightSide ? MathHelper.Pi : 0), genRand.NextFloat(2, 4)),
							8,
							twist: genRand.NextFloat(-0.2f, 0.2f)
						);
						break;
						case FeatureType.CAVE:
						Defiled_Wastelands.Gen.DefiledVeinRunner(
							x, y,
							5,
							Vec2FromPolar(genRand.NextFloat(-0.1f, 0.1f) + (rightSide ? MathHelper.Pi : 0), genRand.NextFloat(4, 8)),
							12,
							fleshBlockType,//fleshBlockType
							6,
							wallType: fleshWallType
						);
						pos = Defiled_Wastelands.Gen.DefiledVeinRunner(
							x, y,
							5,
							Vec2FromPolar(genRand.NextFloat(-0.1f, 0.1f) + (rightSide ? 0 : MathHelper.Pi), genRand.NextFloat(4, 8)),
							genRand.NextFloat(12, 24),
							fleshBlockType,//fleshBlockType
							6,
							wallType: fleshWallType
						).position;
						HiveCave_Old((int)pos.X, (int)pos.Y, 0.5f);
						break;
					}
					tile.HasTile = true;
				}
				WorldGen.RangeFrame(X0, Y0, X1, Y1);
				NetMessage.SendTileSquare(Main.myPlayer, X0, Y0, X1 - X0, Y1 - Y1);
			}
			public static void StartHive_Old(int i, int j) {
				const float strength = 2.4f;
				const float wallThickness = 4f;
				ushort fleshID = (ushort)ModContent.TileType<Riven_Flesh>();
				ushort weakFleshID = TileID.CrackedBlueDungeonBrick;
				ushort fleshWallID = (ushort)ModContent.WallType<Riven_Flesh_Wall>();
				lesionCount = 0;
				int j2 = j;
				if (j2 > Main.worldSurface) {
					j2 = (int)Main.worldSurface;
				}
				for (; !SolidTile(i, j2); j2++) { }
				Vector2 position = new Vector2(i, j2);
				for (int x = i - 30; x < i + 30; x++) {
					for (int y = j2 - 25; y < j2 + 15; y++) {
						float diff = (((y - j2) * (y - j2) * 1.5f) + (x - i) * (x - i));
						if (diff > 800) {
							continue;
						}
						Main.tile[x, y].ResetToType(fleshID);
						if (diff < 750) {
							Main.tile[x, y].WallType = fleshWallID;
						}
					}
				}
				Vector2 vector = new Vector2(0, -1).RotatedByRandom(1.6f, genRand);
				int distance = 0;
				while (Main.tile[(int)position.X, (int)position.Y].HasTile && Main.tileSolid[Main.tile[(int)position.X, (int)position.Y].TileType]) {
					//Main.tile[(int)position.X, (int)position.Y].ResetToType(TileID.EmeraldGemspark);
					//SquareTileFrame((int)position.X, (int)position.Y);
					position += vector;
					if (++distance >= 160) {
						break;
					}
				}
				vector = -vector;
				(Vector2 position, Vector2 velocity) last = (position, vector);
				//Tile t = Main.tile[(int)last.position.X, (int)last.position.Y];
				(int x, int y, Vector2 direction, double length) startValues = ((int)last.position.X, (int)last.position.Y, last.velocity.RotatedByRandom(0.5f, genRand), distance * genRand.NextFloat(0.4f, 0.6f));
				last = GenRunners.WalledVeinRunner(startValues.x, startValues.y, strength * genRand.NextFloat(0.9f, 1.1f), startValues.direction, startValues.length, weakFleshID, wallThickness);
				//t.ResetToType(TileID.AmethystGemspark);
				Vector2 manualVel = new Vector2(last.velocity.X, 0.2f);
				//t = Main.tile[(int)last.position.X, (int)last.position.Y];
				GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength * genRand.NextFloat(0.9f, 1.1f), Vector2.Normalize(new Vector2(-manualVel.X, 0.2f)), genRand.NextFloat(distance * 0.4f, distance * 0.6f) * (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
				last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength * genRand.NextFloat(0.9f, 1.1f), Vector2.Normalize(manualVel), genRand.NextFloat(distance * 0.4f, distance * 0.6f) / (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
				GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength * genRand.NextFloat(0.9f, 1.1f), Vector2.Normalize(manualVel), genRand.NextFloat(distance * 0.4f, distance * 0.6f) / (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
				last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength * genRand.NextFloat(0.9f, 1.1f), new Vector2(0, 1).RotatedByRandom(0.2f, genRand), genRand.NextFloat(distance * 0.4f, distance * 0.6f), weakFleshID, wallThickness, wallType: fleshWallID);
				//t.ResetToType(TileID.AmethystGemspark);
				manualVel.X = -manualVel.X;
				//t = Main.tile[(int)last.position.X, (int)last.position.Y];
				GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength * genRand.NextFloat(0.9f, 1.1f), Vector2.Normalize(new Vector2(-manualVel.X, 0.2f)), genRand.NextFloat(distance * 0.4f, distance * 0.6f) * (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
				last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength * genRand.NextFloat(0.9f, 1.1f), Vector2.Normalize(manualVel), genRand.NextFloat(distance * 0.4f, distance * 0.6f) / (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
				GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength * genRand.NextFloat(0.9f, 1.1f), Vector2.Normalize(manualVel), genRand.NextFloat(distance * 0.4f, distance * 0.6f) / (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
				last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength * genRand.NextFloat(0.9f, 1.1f), new Vector2(0, 1).RotatedByRandom(0.2f, genRand), genRand.NextFloat(distance * 0.4f, distance * 0.6f), weakFleshID, wallThickness, wallType: fleshWallID);
				//t.ResetToType(TileID.AmethystGemspark);
				for (int index = 0; index < 10; index++) {
					//t = Main.tile[(int)last.position.X, (int)last.position.Y];
					last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength * genRand.NextFloat(0.9f, 1.1f), last.velocity.RotatedByRandom(0.8f, genRand), genRand.NextFloat(distance * 0.2f, distance * 0.3f), weakFleshID, wallThickness, wallType: fleshWallID);
					if (index < 8) {
						GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength * genRand.NextFloat(0.9f, 1.1f), last.velocity.RotatedBy(genRand.Next(2) * 2 - 1).RotatedByRandom(0.8f, genRand), genRand.NextFloat(distance * 0.4f, distance * 0.6f), weakFleshID, wallThickness, wallType: fleshWallID);
					}
					PolarVec2 vel = new PolarVec2(1, last.velocity.ToRotation());
					OriginExtensions.AngularSmoothing(ref vel.Theta, MathHelper.PiOver2, 0.7f);
					//t.ResetToType(TileID.AmethystGemspark);
					last = (last.position, (Vector2)vel);
				}
				//t = Main.tile[(int)last.position.X, (int)last.position.Y];
				//t.ResetToType(TileID.AmethystGemspark);
				Point caveCenter = HiveCave_Old((int)last.position.X, (int)last.position.Y);
				Vector2 cavernOpening = last.position - caveCenter.ToVector2();
				GenRunners.VeinRunner((int)last.position.X, (int)last.position.Y, strength, cavernOpening.SafeNormalize(Vector2.Zero), cavernOpening.Length());
				GenRunners.VeinRunner(startValues.x, startValues.y, strength, startValues.direction, startValues.length);
				(Vector2 position, Vector2 velocity)[] arms = new (Vector2 position, Vector2 velocity)[4];
				arms[0] = last = GenRunners.WalledVeinRunner(caveCenter.X, caveCenter.Y, strength * genRand.NextFloat(0.9f, 1.1f), new Vector2(1, -0.25f).RotatedByRandom(0.2f, genRand), genRand.NextFloat(32, 64), weakFleshID, wallThickness, wallType: fleshWallID);
				HiveCave_Old((int)last.position.X, (int)last.position.Y, genRand.NextFloat(0.3f, 0.5f));
				arms[1] = last = GenRunners.WalledVeinRunner(caveCenter.X, caveCenter.Y, strength * genRand.NextFloat(0.9f, 1.1f), new Vector2(1, 0.25f).RotatedByRandom(0.2f, genRand), genRand.NextFloat(32, 64), weakFleshID, wallThickness, wallType: fleshWallID);
				HiveCave_Old((int)last.position.X, (int)last.position.Y, genRand.NextFloat(0.3f, 0.5f));
				arms[2] = last = GenRunners.WalledVeinRunner(caveCenter.X, caveCenter.Y, strength * genRand.NextFloat(0.9f, 1.1f), new Vector2(-1, -0.25f).RotatedByRandom(0.2f, genRand), genRand.NextFloat(32, 64), weakFleshID, wallThickness, wallType: fleshWallID);
				HiveCave_Old((int)last.position.X, (int)last.position.Y, genRand.NextFloat(0.3f, 0.5f));
				arms[3] = last = GenRunners.WalledVeinRunner(caveCenter.X, caveCenter.Y, strength * genRand.NextFloat(0.9f, 1.1f), new Vector2(-1, 0.25f).RotatedByRandom(0.2f, genRand), genRand.NextFloat(32, 64), weakFleshID, wallThickness, wallType: fleshWallID);
				HiveCave_Old((int)last.position.X, (int)last.position.Y, genRand.NextFloat(0.3f, 0.5f));
			}
			public static Point HiveCave_Old(int i, int j, float sizeMult = 1f) {
				ushort fleshID = (ushort)ModContent.TileType<Riven_Flesh>();
				ushort fleshWallID = (ushort)ModContent.WallType<Riven_Flesh_Wall>();
				ushort lesionID = (ushort)ModContent.TileType<Riven_Lesion>();
				ushort blisterID = (ushort)ModContent.TileType<Gel_Blister>();
				int i2 = i + (int)(genRand.Next(-26, 26) * sizeMult);
				int j2 = j + (int)(genRand.Next(-2, 22) * sizeMult);
				Queue<Point> lesionPlacementSpots = new Queue<Point>();
				for (int x = i2 - (int)(33 * sizeMult + 5); x < i2 + (int)(33 * sizeMult + 5); x++) {
					for (int y = j2 + (int)(28 * sizeMult + 4); y >= j2 - (int)(28 * sizeMult + 4); y--) {
						float sq = Math.Max(Math.Abs(y - j2) * 1.5f, Math.Abs(x - i2));
						float diff = (float)Math.Sqrt((sq * sq + (((y - j2) * (y - j2) * 1.5f) + (x - i2) * (x - i2))) * 0.5f * (GenRunners.GetWallDistOffset(x) * 0.0316076058772687986171132238548f + 1));
						if (diff > 35 * sizeMult) {
							continue;
						}
						if (OriginExtensions.IsTileReplacable(x, y)) {
							if (Main.tile[x, y].WallType != fleshWallID) {
								Main.tile[x, y].ResetToType(fleshID);
							}
							Main.tile[x, y].WallType = fleshWallID;
							if ((diff < 35 * sizeMult - 5 || ((y - j) * (y - j)) + (x - i) * (x - i) < 25 * sizeMult * sizeMult)) {
								Main.tile[x, y].SetActive(false);
								if (diff > 34 * sizeMult - 5 && Main.tile[x, y + 1].TileIsType(fleshID)) {
									lesionPlacementSpots.Enqueue(new Point(x, y));
								}
							}
						}
					}
				}
				List<Point> validLesionPlacementSpots = new List<Point>();
				bool CheckPos(int x, int y) {
					return !Main.tile[x, y].HasTile && !Main.tile[x, y - 1].HasTile && Main.tile[x, y + 1].HasTile && Main.tile[x, y + 1].Slope == SlopeType.Solid;
				}
				while (lesionPlacementSpots.Count > 0) {
					Point current = lesionPlacementSpots.Dequeue();
					if (validLesionPlacementSpots.Contains(current)) continue;
					if (CheckPos(current.X, current.Y) && CheckPos(current.X - 1, current.Y) && CheckPos(current.X + 1, current.Y) && CheckPos(current.X + 2, current.Y)) {
						int minX = current.X;
						int maxX = current.X;
						for (int x = -1; x > -3; x--) {
							if (CheckPos(current.X + x, current.Y)) {
								if (x <= -2) minX--;
							} else {
								break;
							}
						}
						for (int x = 1; x < 4; x++) {
							if (CheckPos(current.X + x, current.Y)) {
								if (x >= 2) maxX++;
							} else {
								break;
							}
						}
						for (int x = minX; x < maxX; x++) {
							validLesionPlacementSpots.Add(new(x, current.Y + 4));
						}
					}
				}
				bool placedLesion = false;
				for (int index = 0; index < 4; index++) {
					if (validLesionPlacementSpots.Count < 1 || lesionCount > 18) {
						break;
					}
					Point current = genRand.Next(validLesionPlacementSpots);

					placedLesion |= PlaceTile(current.X, current.Y, lesionID);

					lesionCount++;
					validLesionPlacementSpots.Remove(current.OffsetBy(-1));
					validLesionPlacementSpots.Remove(current);
					validLesionPlacementSpots.Remove(current.OffsetBy(1));
					validLesionPlacementSpots.Remove(current.OffsetBy(2));
				}
				while (!placedLesion) {
					placedLesion |= PlaceTile(i2 + genRand.Next(-2, 3), j2 + genRand.Next(-2, 3), blisterID);
				}
				return new Point(i2, j2);
			}
			public static void SpreadRivenGrass(int i, int j) {
				const int maxDepth = 150;
				ushort grassType = (ushort)ModContent.TileType<Riven_Grass>();
				Stack<(int x, int y, int depth)> stack = new();
				stack.Push((i, j, 0));
				while (stack.Count > 0) {
					(int x, int y, int depth) = stack.Pop();
					Tile tile = Framing.GetTileSafely(x, y);
					if (tile.HasTile && tile.TileType == TileID.Grass) {
						tile.TileType = grassType;
						if (depth < maxDepth) {
							stack.Push((x + 1, y, depth + 1));
							stack.Push((x - 1, y, depth + 1));
							stack.Push((x, y + 1, depth + 1));
							stack.Push((x, y - 1, depth + 1));
						}
					}
					
					for (int c = 0; c < 7; c++) {
						tile = Framing.GetTileSafely(x, --y);
						if (tile.HasTile) {
							if (tile.TileType == TileID.Grass) {
								if (depth < maxDepth) {
									stack.Push((x, y, depth + 1));
								}
								break;
							} else if(Main.tileSolid[tile.TileType]) {
								break;
							}
						}
					}
				}
			}
		}
		public static void CheckLesion(int i, int j, int type) {
			if (WorldGen.generatingWorld) {
				return;
			}
			int x = Main.tile[i, j].TileFrameX != 0 ? i - 1 : i;
			int y = Main.tile[i, j].TileFrameY != 0 && Main.tile[i, j].TileFrameY != 36 ? j - 1 : j;
			for (int k = 0; k < 2; k++) {
				for (int l = 0; l < 2; l++) {
					Tile tile = Main.tile[x + k, y + l];
					if (tile != null && (!tile.HasUnactuatedTile || tile.TileType != type)) {
						destroyObject = true;
						break;
					}
				}
				if (destroyObject) {
					break;
				}
			}
			if (!destroyObject) {
				return;
			}
			for (int m = x; m < x + 2; m++) {
				for (int n = y; n < y + 2; n++) {
					if (Main.tile[m, n].TileType == type) {
						KillTile(m, n);
					}
				}
			}
			if (Main.netMode != NetmodeID.MultiplayerClient && !noTileActions) {
				if (genRand.NextBool(2)) {
					spawnMeteor = true;
				}
				/*int num3 = Main.rand.Next(5);
				if (!shadowOrbSmashed) {
					num3 = 0;
				}
				switch (num3) {
					case 0: {
						Item.NewItem(GetItemSource_FromTileBreak(i, j), x * 16, y * 16, 32, 32, 800, 1, pfix:-1);
						int stack = genRand.Next(100, 101);
						Item.NewItem(GetItemSource_FromTileBreak(i, j), x * 16, y * 16, 32, 32, 97, stack);
						break;
					}
					case 1:
					Item.NewItem(GetItemSource_FromTileBreak(i, j), x * 16, y * 16, 32, 32, 1256, 1, pfix: -1);
					break;
					case 2:
					Item.NewItem(GetItemSource_FromTileBreak(i, j), x * 16, y * 16, 32, 32, 802, 1, pfix: -1);
					break;
					case 3:
					Item.NewItem(GetItemSource_FromTileBreak(i, j), x * 16, y * 16, 32, 32, 3062, 1, pfix: -1);
					break;
					case 4:
					Item.NewItem(GetItemSource_FromTileBreak(i, j), x * 16, y * 16, 32, 32, 1290, 1, pfix: -1);
					break;
				}*/

				float fx = x * 16;
				float fy = y * 16;
				float distance = -1f;
				int plr = 0;
				for (int pindex = 0; pindex < 255; pindex++) {
					float currentDist = Math.Abs(Main.player[pindex].position.X - fx) + Math.Abs(Main.player[pindex].position.Y - fy);
					if (currentDist < distance || distance == -1f) {
						plr = pindex;
						distance = currentDist;
					}
				}

				DropAttemptInfo dropInfo = default(DropAttemptInfo);
				dropInfo.player = Main.player[plr];
				dropInfo.IsExpertMode = Main.expertMode;
				dropInfo.IsMasterMode = Main.masterMode;
				dropInfo.IsInSimulation = false;
				dropInfo.rng = Main.rand;
				Origins.ResolveRuleWithHandler(shadowOrbSmashed ? LesionDropRule : FirstLesionDropRule, dropInfo, (DropAttemptInfo info, int item, int stack, bool _) => {
					Item.NewItem(GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 32, 32, item, stack, pfix: -1);
				});
				shadowOrbSmashed = true;
				shadowOrbCount++;
				if (shadowOrbCount >= 3) {
					shadowOrbCount = 0;
					NPC.SpawnOnPlayer(plr, ModContent.NPCType<NPCs.Riven.World_Cracker.World_Cracker_Head>());
				} else {
					LocalizedText localizedText = Lang.misc[10];
					if (shadowOrbCount == 2) {
						localizedText = Lang.misc[11];
					}
					if (Main.netMode == NetmodeID.SinglePlayer) {
						Main.NewText(localizedText.ToString(), 50, byte.MaxValue, 130);
					} else if (Main.netMode == NetmodeID.Server) {
						ChatHelper.BroadcastChatMessage(NetworkText.FromKey(localizedText.Key), new Color(50, 255, 130));
					}
					AchievementsHelper.NotifyProgressionEvent(7);
				}
			}
			SoundEngine.PlaySound(SoundID.NPCDeath1, new Vector2(i * 16, j * 16));
			destroyObject = false;
		}
	}
	public class Underground_Riven_Hive_Biome : ModBiome {
		public override int Music => Origins.Music.UndergroundRiven;
		public override string BackgroundPath => "Origins/UI/MapBGs/Riven_Hive_Caverns";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneRockLayerHeight && player.InModBiome<Riven_Hive>();
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneRivenProgress;
		}
	}
	public class Riven_Hive_Desert : ModBiome {
		public override int Music => Origins.Music.Riven;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BestiaryIcon => "Origins/UI/Defiled_Wastelands_Bestiary_Icon";
		public override string BackgroundPath => "Origins/UI/MapBGs/Riven_Desert";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneDesert && player.InModBiome<Riven_Hive>();
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneRivenProgress * 0.99f;
		}
	}
	public class Riven_Hive_Ocean : ModBiome {
		public override int Music => Origins.Music.RivenOcean;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BestiaryIcon => "Origins/UI/Defiled_Wastelands_Bestiary_Icon";
		public override string BackgroundPath => "Origins/UI/MapBGs/Eutrophic_Sea";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneBeach && player.InModBiome<Riven_Hive>();
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneRivenProgress * 0.99f;
		}
	}
	public class Riven_Hive_Alt_Biome : AltBiome {
		public override string WorldIcon => "";//TODO: Redo tree icons for AltLib
		public override string OuterTexture => "Origins/UI/WorldGen/Outer_Riven";
		public override string IconSmall => "Origins/UI/WorldGen/IconEvilRiven";
		public override Color OuterColor => new(30, 176, 255);
		public override IShoppingBiome Biome => ModContent.GetInstance<Riven_Hive>();
		public override void SetStaticDefaults() {
			BiomeType = AltLibrary.BiomeType.Evil;
			//DisplayName.SetDefault(Language.GetTextValue("Mods.Origins.Generic.Riven_Hive"));
			//Description.SetDefault(Language.GetTextValue("A flourishing and beautiful ecosystem soon to replace the status quo dominated by a now sentient amoeba."));
			//GenPassName.SetDefault(Language.GetTextValue("{$Mods.Origins.Generic.Riven_Hive}"));

			AddTileConversion(ModContent.TileType<Riven_Grass>(), TileID.Grass);
			AddTileConversion(ModContent.TileType<Riven_Jungle_Grass>(), TileID.JungleGrass);
			AddTileConversion(ModContent.TileType<Riven_Flesh>(), TileID.Stone);
			AddTileConversion(ModContent.TileType<Silica>(), TileID.Sand);
			AddTileConversion(ModContent.TileType<Defiled_Sandstone>(), TileID.Sandstone);
			AddTileConversion(ModContent.TileType<Hardened_Defiled_Sand>(), TileID.HardenedSand);
			AddTileConversion(ModContent.TileType<Primordial_Permafrost>(), TileID.IceBlock);

			SeedType = ModContent.ItemType<Riven_Grass_Seeds>();
			BiomeOre = ModContent.TileType<Encrusted_Ore>();
			BiomeOreItem = ModContent.ItemType<Encrusted_Ore_Item>();
			AltarTile = ModContent.TileType<Riven_Altar>();

			BiomeChestItem = ModContent.ItemType<Riven_Dungeon_Chest_Placeholder_Item>();
			BiomeChestTile = ModContent.TileType<Riven_Dungeon_Chest>();
			BiomeChestTileStyle = 1;
			BiomeKeyItem = ModContent.ItemType<Riven_Key>();

			MimicType = ModContent.NPCType<Riven_Mimic>();

			BloodBunny = ModContent.NPCType<Barnacle_Bunny>();
			BloodPenguin = ModContent.NPCType<Riven_Penguin>();

			AddWallConversions<Riven_Flesh_Wall>(
				WallID.Stone,
				WallID.CaveUnsafe,
				WallID.Cave2Unsafe,
				WallID.Cave3Unsafe,
				WallID.Cave4Unsafe,
				WallID.Cave5Unsafe,
				WallID.Cave6Unsafe,
				WallID.Cave7Unsafe,
				WallID.Cave8Unsafe,
				WallID.EbonstoneUnsafe,
				WallID.CorruptionUnsafe1,
				WallID.CorruptionUnsafe2,
				WallID.CorruptionUnsafe3,
				WallID.CorruptionUnsafe4,
				WallID.CrimstoneUnsafe,
				WallID.CrimsonUnsafe1,
				WallID.CrimsonUnsafe2,
				WallID.CrimsonUnsafe3,
				WallID.CrimsonUnsafe4
			);
			AddWallConversions<Chambersite_Riven_Flesh_Wall>((ushort)ModContent.WallType<Chambersite_Stone_Wall>());
		}
		public override AltMaterialContext MaterialContext {
			get {
				AltMaterialContext context = new AltMaterialContext();
				context.SetEvilHerb(ModContent.ItemType<Wrycoral_Item>());
				context.SetEvilBar(ModContent.ItemType<Encrusted_Bar>());
				context.SetEvilOre(ModContent.ItemType<Encrusted_Ore_Item>());
				context.SetVileInnard(ModContent.ItemType<Bud_Barnacle>());
				context.SetVileComponent(ModContent.ItemType<Alkahest>());
				context.SetEvilBossDrop(ModContent.ItemType<Riven_Carapace>());
				context.SetEvilSword(ModContent.ItemType<Vorpal_Sword>());
				return context;
			}
		}
		public override EvilBiomeGenerationPass GetEvilBiomeGenerationPass() {
			return new Riven_Hive_Generation_Pass();
		}
		public class Riven_Hive_Generation_Pass : EvilBiomeGenerationPass {
			public override void GenerateEvil(int evilBiomePosition, int evilBiomePositionWestBound, int evilBiomePositionEastBound) {
				int y = (int)GenVars.worldSurface - 50;
				for (; !Main.tile[evilBiomePosition, y].HasSolidTile(); y++) ;
				Riven_Hive.Gen.StartHive(evilBiomePosition, y);
				OriginSystem.Instance.hasDefiled = true;
			}

			public override void PostGenerateEvil() { }
		}
	}
}
