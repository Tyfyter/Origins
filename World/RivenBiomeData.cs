using AltLibrary.Common.AltBiomes;
using AltLibrary.Common.Systems;
using AltLibrary.Core.Generation;
using Origins.Backgrounds;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Pets;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.NPCs.Riven;
using Origins.NPCs.Riven.World_Cracker;
using Origins.Reflection;
using Origins.Tiles;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Origins.Walls;
using Origins.Water;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Personalities;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using static Origins.OriginExtensions;
using static Terraria.WorldGen;

namespace Origins.World.BiomeData {
	public class Riven_Hive : ModBiome {
		public static IItemDropRule FirstLesionDropRule;
		public static IItemDropRule LesionDropRule;
		public override int Music => Origins.Music.Riven;
		public override ModWaterStyle WaterStyle => ModContent.GetInstance<Riven_Water_Style>();
		public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<Riven_Surface_Background>();
		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => BiomeUGBackground<Riven_Underground_Background>();
		public override int BiomeTorchItemType => ModContent.ItemType<Riven_Torch>();
		public override int BiomeCampfireItemType => ModContent.ItemType<Riven_Campfire_Item>();
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BestiaryIcon => "Origins/UI/WorldGen/IconEvilRiven";
		public override string BackgroundPath => "Origins/UI/MapBGs/Riven_Surface";
		public override string MapBackground => BackgroundPath;
		public static ModBiomeBestiaryInfoElement BestiaryInfoElement => ModContent.GetInstance<Riven_Hive>().ModBiomeBestiaryInfoElement;
		public static FrameCachedValue<float> NormalGlowValue { get; private set; } = new(() => (float)(Math.Sin(Main.GlobalTimeWrappedHourly) + 2) * 0.5f);
		public static bool forcedBiomeActive;
		public static Vector3 ColoredGlow(float intensity) => new Vector3(0.394f, 0.879f, 0.912f) * intensity * NormalGlowValue.GetValue();
		public override bool IsBiomeActive(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.ZoneRivenProgress = Math.Min(OriginSystem.rivenTiles - (NeededTiles - ShaderTileCount), ShaderTileCount) / ShaderTileCount;
			LinearSmoothing(ref originPlayer.ZoneRivenProgressSmoothed, originPlayer.ZoneRivenProgress, OriginSystem.biomeShaderSmoothing * 0.1f);

			return IsActive;
		}
		public static bool IsActive => OriginSystem.rivenTiles > NeededTiles;
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
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Splitsplash>()),
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Riverang>()),
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Amoeba_Toy>()),
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Primordial_Soup>())
			);
		}
		public override void Unload() {
			FirstLesionDropRule = null;
			LesionDropRule = null;
			NormalGlowValue = null;
		}

		public static Color GetGlowAlpha(Color lightColor) {
			return new Color((lightColor.R + 255) / 510f, (lightColor.G + 255) / 510f, (lightColor.B + 255) / 510f, 0.5f);
		}
		public const int NeededTiles = 200;
		public const int ShaderTileCount = 25;
		public const short DefaultTileDust = DustID.BlueMoss;
		public class SpawnRates : SpawnPool {
			public const float AmebSlime = 0.65f;
			public const float Fighter = 1;
			public const float AncientFighter = 0.03f;
			public const float Flajelly = 0.37f;
			public const float BarnBack = 0.3f;
			public const float Amoebeye = 0.25f;
			public const float BlisterBoi = 0.75f;
			public const float Seashell = 0.6f;
			public const float Aqueoua = 0.5f;
			public const float Spighter = 1;
			public const float Wall = 1.5f;
			public const float Mummy = 1;
			public const float Ghoul = 2;
			public const float Cleaver = 0.5f;
			public const float Barnacle = 0.5f;
			public const float Shark1 = 0.4f;
			public const float Worm = 0.4f;
			public const float Crawler = 0.8f;
			public const float Mimic = 0.01f;
			public const float Whip = 0.01f;
			public override string Name => $"{nameof(Riven_Hive)}_{base.Name}";
			public override void SetStaticDefaults() {
				Priority = SpawnPoolPriority.BiomeHigh;
				static float DesertCave(NPCSpawnInfo spawnInfo) => spawnInfo.DesertCave && Main.hardMode ? 1 : 0;
				AddSpawn(NPCID.DesertDjinn, DesertCave);
				AddSpawn(NPCID.DesertLamiaDark, DesertCave);
				AddSpawn(NPCID.DesertBeast, DesertCave);
				static Func<NPCSpawnInfo, float> MimicRate(float rate) => (spawnInfo) => {
					if (Main.hardMode && !spawnInfo.PlayerSafe && spawnInfo.SpawnTileY > Main.rockLayer && !spawnInfo.DesertCave) return rate;
					return 0;
				};
				AddSpawn(ModContent.NPCType<Riven_Mimic>(), MimicRate(Mimic));
				AddSpawn(ModContent.NPCType<Savage_Whip>(), MimicRate(Whip));
			}
			public static float LandEnemyRate(NPCSpawnInfo spawnInfo, bool hardmode = false) {
				if (hardmode && !Main.hardMode) return 0f;
				return 1f;
			}
			public static float FlyingEnemyRate(NPCSpawnInfo spawnInfo, bool hardmode = false) {
				return LandEnemyRate(spawnInfo, hardmode);
			}
			public override bool IsActive(NPCSpawnInfo spawnInfo) {
				return TileLoader.GetTile(spawnInfo.SpawnTileType) is IRivenTile || (spawnInfo.Player.InModBiome<Riven_Hive>() && spawnInfo.SpawnTileType == ModContent.TileType<Encrusted_Ore>()) || forcedBiomeActive;
			}
		}
		public static class Gen {
			public enum FeatureType {
				CHUNK,
				CUSP,
				CAVE,
				CLEAVE,
			}
			public static void StartHive(int i, int j) {
				Vector2 pos = new Vector2(i, j);
				ushort fleshBlockType = (ushort)ModContent.TileType<Spug_Flesh>();
				ushort fleshWallType = (ushort)ModContent.WallType<Riven_Flesh_Wall>();
				ushort gooBlockType = (ushort)ModContent.TileType<Amoeba_Fluid>();
				int oreID = ModContent.TileType<Encrusted_Ore>();
				HashSet<ushort> cleaveReplacables = [fleshBlockType];
				Tile tile;
				int X0 = int.MaxValue;
				int X1 = 0;
				int Y0 = int.MaxValue;
				int Y1 = 0;
				List<(int x, int y, FeatureType type, bool rightSide)> features = [];

				float targetTwist = genRand.NextFloat(-0.5f, 0.5f);
				PolarVec2 speed = new PolarVec2(8, genRand.NextFloat(-0.5f, 0.5f) + MathHelper.PiOver2);
				double strength = 32;
				double baseStrength = strength;
				strength = Math.Pow(strength, 2);
				const double wall_thickness = 24;
				float decay = 1;
				while (strength > 16) {
					int minX = (int)(pos.X - (strength + wall_thickness) * 0.5);
					int maxX = (int)(pos.X + (strength + wall_thickness) * 0.5);
					int minY = (int)(pos.Y - (strength + wall_thickness) * 0.5);
					int maxY = (int)(pos.Y + (strength + wall_thickness) * 0.5);
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
								if (d < baseStrength + wall_thickness && OriginExtensions.IsTileReplacable(x, y)) {
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
									if (compY > j && d < baseStrength + 8 && Math.Abs(x) > Math.Abs(y) && genRand.Next(1500000 + (int)strength) < strength + 424) {
										features.Add((x, y, FeatureType.CLEAVE, x > i));
									}
									//WorldGen.SquareTileFrame(l, k);
								}
								continue;
							}
							if (TileID.Sets.CanBeClearedDuringGeneration[tile.TileType]) {
								if (TileID.Sets.Falling[tile.TileType]) {
									Vector2 posMin = new(float.PositiveInfinity);
									Vector2 posMax = new(float.NegativeInfinity);
									Carver.DoCarve(
										Carver.Climb(new(x, y), pos => {
											if (!OriginExtensions.IsTileReplacable((int)pos.X, (int)pos.Y)) return false;
											Tile tile = Framing.GetTileSafely(pos.ToPoint());
											return tile.HasTile && TileID.Sets.Falling[tile.TileType];
										}, ref posMin, ref posMax),
										pos => {
											Tile tile = Framing.GetTileSafely(pos.ToPoint());
											tile.HasTile = false;
											return 0;
										},
										posMin, posMax
									);
								}
								if (OriginExtensions.IsTileReplacable(x, y)) {
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
					speed.R = Math.Min(speed.R, (float)Math.Sqrt(strength));
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
				strength = Math.Pow(baseStrength, 2);
				WorldBiomeGeneration.ChangeRange.AddChangeToRange(X0, Y0);
				WorldBiomeGeneration.ChangeRange.AddChangeToRange(X1, Y1);
				Point[] directions = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];
				for (int index = 0; index < features.Count; index++) {
					(int x, int y, FeatureType type, bool rightSide) = features[index];
					tile = Main.tile[x, y];
					switch (type) {
						case FeatureType.CHUNK:
						GenRunners.SpikeRunner(x, y,
							fleshBlockType,
							Vec2FromPolar(genRand.NextFloat(MathHelper.TwoPi), genRand.NextFloat(4, 8)),
							8,
							twist: genRand.NextFloat(-2, 2) + MathHelper.Pi,
							oreType: oreID,
							oreRarity: 50
						);
						/*if (AreaAnalysis.March(x, y, directions, pos => Math.Abs(pos.Y - y) < 20 && Framing.GetTileSafely(pos).TileIsType(fleshBlockType), a => a.MaxX - a.MinX >= 100).Broke) {
							Framing.GetTileSafely(x, y).TileType = TileID.AmberGemspark;
						}*/
						break;
						case FeatureType.CUSP:
						GenRunners.SpikeRunner(x, y,
							fleshBlockType,
							Vec2FromPolar(genRand.NextFloat(-0.1f, 0.1f) + (rightSide ? MathHelper.Pi : 0), genRand.NextFloat(2, 4)),
							8,
							twist: genRand.NextFloat(-0.2f, 0.2f),
							oreType: oreID,
							oreRarity: 50
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
							wallType: fleshWallType,
							oreType: oreID,
							oreRarity: 25
						);
						pos = Defiled_Wastelands.Gen.DefiledVeinRunner(
							x, y,
							5,
							Vec2FromPolar(genRand.NextFloat(-0.1f, 0.1f) + (rightSide ? 0 : MathHelper.Pi), genRand.NextFloat(4, 8)),
							genRand.NextFloat(12, 24),
							fleshBlockType,//fleshBlockType
							6,
							wallType: fleshWallType,
							oreType: oreID,
							oreRarity: 25
						).position;
						HiveCave_Old((int)pos.X, (int)pos.Y, 0.5f);
						break;
						case FeatureType.CLEAVE:
						DoScar(
							new(x, y),
							genRand.NextFloat(-0.1f, 0.1f) + (rightSide ? MathHelper.Pi : 0),
							gooBlockType,
							genRand.NextFloat(4, 8),
							genRand.NextFloat(3, 5)
						);
						/*Vector2 cleaveDir = Vec2FromPolar(genRand.NextFloat(-0.1f, 0.1f) + (rightSide ? MathHelper.Pi : 0), genRand.NextFloat(3, 4));
						int step = rightSide ? 1 : -1;
						int steps = 14;
						while (Main.tile[x - step, y].HasSolidTile() && --steps > 0) x -= step;
						GenRunners.SpikeVeinRunner(x, y,
							genRand.NextFloat(3f, 4f),
							cleaveReplacables,
							gooBlockType,
							cleaveDir,
							genRand.NextFloat(0.75f, 1f),
							twist: 0
						);
						GenRunners.SpikeVeinRunner(x, y,
							genRand.NextFloat(3f, 4f),
							cleaveReplacables,
							gooBlockType,
							-cleaveDir,
							genRand.NextFloat(0.75f, 1f),
							twist: 0
						);*/
						break;
					}
					tile.HasTile = true;
				}
				Rectangle genRange = WorldBiomeGeneration.ChangeRange.GetRange();
				ushort barnacleWall = (ushort)OriginsWall.GetWallID<Barnacle_Wall>(WallVersion.Natural);
				{// speckle
					const float barnacling_scale = 0.8f;
					for (int k = genRand.Next(1, 4); k-- > 0;) {
						int x = genRange.X + genRand.Next(genRange.Width);
						int y = genRange.Y + genRand.Next(genRange.Height);
						Vector2 barnPos = new(x, y);
						const float smoothness = 16;
						for (int l = 0; l < smoothness; l++) {
							Vector2 posMin = new(float.PositiveInfinity);
							Vector2 posMax = new(float.NegativeInfinity);
							float size = genRand.NextFloat(10, 30);
							Vector2 offset = genRand.NextFloat(0, MathHelper.TwoPi).ToRotationVector2() * (size + genRand.NextFloat(5, 10));
							Carver.DoCarve(
								Carver.TileFilter(tile => tile.WallType == fleshWallType)
								+ Carver.Circle(// tweak to change the shape and size of the barnacled areas
									barnPos + offset * barnacling_scale,
									scale: size * barnacling_scale,
									rotation: genRand.NextFloat(0, MathHelper.TwoPi),
									aspectRatio: genRand.NextFloat(1, 1.4f),
									ref posMin,
									ref posMax
								),
								pos => {
									Framing.GetTileSafely(pos.ToPoint()).WallType = barnacleWall;
									return 1;
								},
								posMin,
								posMax
							);
						}
					}

					// tweak this to make mottling closer
					const float spread = 7;
					bool[] replaceableTiles = TileID.Sets.Factory.CreateBoolSet(fleshBlockType, gooBlockType);
					Tuple<Vector2, double>[] poss;
					int tries = 100;
					do {
						poss = genRand.PoissonDiskSampling(genRange, v => {
							Tile tile = Framing.GetTileSafely(v.ToPoint());
							return tile.WallType == fleshWallType || tile.WallType == barnacleWall || (tile.HasTile && replaceableTiles[tile.TileType]);
						}, spread).Select(v => new Tuple<Vector2, double>(v, 1)).ToArray();
					} while (poss.Length <= 1 && --tries > 0);
					WeightedRandom<Vector2> positions = new(genRand, poss);
					// tweak this to make more barnicled areas
					int count = (int)(Main.maxTilesX * 5E-03 * genRand.NextFloat(0.9f, 1f));
					while (count-- > 0 && positions.elements.Count > 0) {
						Vector2 specklePos = positions.Pop();
						Tile startTile = Framing.GetTileSafely(specklePos.ToPoint());
						if (startTile.WallType != fleshWallType && startTile.WallType != barnacleWall) {
							count++;
							continue;
						}
						ushort startWallType = startTile.WallType;
						ushort wallType = startWallType;
						switch (genRand.Next(1)) { // add possibilities to add possibilities
							case 0:
							if (startWallType == fleshWallType) {
								wallType = barnacleWall;
							} else {
								wallType = fleshWallType;
							}
							break;
						}
						Vector2 posMin = new(float.PositiveInfinity);
						Vector2 posMax = new(float.NegativeInfinity);
						if (Carver.DoCarve(
							Carver.TileFilter(tile => tile.WallType == startWallType)
							+ Carver.PointyLemon(// tweak to change the shape and size of the barnacled areas
								specklePos,
								scale: genRand.NextFloat(6, 10),
								rotation: genRand.NextFloat(0, MathHelper.TwoPi),
								aspectRatio: genRand.NextFloat(1.1f, 2),
								roundness: genRand.NextFloat(1, 2),
								ref posMin,
								ref posMax
							),
							pos => {
								Framing.GetTileSafely(pos.ToPoint()).WallType = wallType;
								return 1;
							},
							posMin,
							posMax,
							8
						) <= 0) {
							count++;
							continue;
						}
					}
					positions = new(genRand, poss);
					// tweak this to make more calcified areas
					count = (int)(Main.maxTilesX * 6E-03 * genRand.NextFloat(0.9f, 1f));

					ushort calcifiedTile = (ushort)ModContent.TileType<Calcified_Riven_Flesh>();
					ushort calcifiedWall = (ushort)OriginsWall.GetWallID<Calcified_Riven_Flesh_Wall>(WallVersion.Natural);
					while (count-- > 0 && positions.elements.Count > 0) {
						Vector2 specklePos = positions.Pop();
						Tile startTile = Framing.GetTileSafely(specklePos.ToPoint());
						bool foreground = true;
						if (!startTile.TileIsType(fleshBlockType)) {
							if (startTile.WallType != fleshWallType) {
								count++;
								continue;
							}
							foreground = false;
						}
						ushort tileType = fleshBlockType;
						ushort wallType = fleshWallType;
						Vector2 posMin = new(float.PositiveInfinity);
						Vector2 posMax = new(float.NegativeInfinity);
						Carver.Filter shape;
						switch (genRand.Next(1)) { // add possibilities to add possibilities
							default:
							tileType = calcifiedTile;
							wallType = calcifiedWall;
							// tweak to change the shape and size of the calcified areas
							shape = Carver.PointyLemon(
								specklePos,
								scale: genRand.NextFloat(6, 10),
								rotation: genRand.NextFloat(0, MathHelper.TwoPi),
								aspectRatio: genRand.NextFloat(3, 6),
								roundness: genRand.NextFloat(1, 2),
								ref posMin,
								ref posMax
							);
							break;

							case 4: { // fanana 
								tileType = calcifiedTile;
								wallType = calcifiedWall;
								// tweak to change the shape and size of the calcified areas
								float rotation = genRand.NextFloat(0, MathHelper.TwoPi);

								float range = genRand.NextFloat(0.2f, 0.6f); // fan/banana width
								Vector2 turningPoint = specklePos + rotation.ToRotationVector2() * genRand.NextFloat(5, 10);
								if (!genRand.NextBool(8)) rotation += MathHelper.PiOver2; // fan/banana selection
								Carver.Filter[] lemons = new Carver.Filter[genRand.Next(3, 7)];
								for (int k = 0; k < lemons.Length; k++) {
									lemons[k] = Carver.PointyLemon(
										specklePos.RotatedBy(k * range, turningPoint),
										scale: genRand.NextFloat(6, 10),
										rotation: rotation + k * range,
										aspectRatio: genRand.NextFloat(3, 6),
										roundness: genRand.NextFloat(1, 2),
										ref posMin,
										ref posMax
									);
								}
								shape = Carver.Or(lemons);
								break;
							}
						}
						Carver.Filter filter = Carver.TileFilter(tile => (tile.HasTile && replaceableTiles[tile.TileType]) || (!foreground && tile.WallType == fleshWallType));
						if (foreground) filter += Carver.ActiveTileInSet(replaceableTiles);
						filter += shape;

						if (Carver.DoCarve(
							filter,
							pos => {
								Tile tile = Framing.GetTileSafely(pos.ToPoint());
								if (tile.HasTile && replaceableTiles[tile.TileType]) tile.TileType = tileType;
								if (!foreground && tile.WallType == fleshWallType) tile.WallType = wallType;
								return 1;
							},
							posMin,
							posMax,
							8
						) <= 0) {
							count++;
							continue;
						}
					}
					positions = new(genRand, poss);
					// tweak this to make more calcified areas
					count = (int)(Main.maxTilesX * 4E-03 * genRand.NextFloat(0.9f, 1f));
					while (count-- > 0 && positions.elements.Count > 0) {
						Vector2 specklePos = positions.Pop();
						Tile startTile = Framing.GetTileSafely(specklePos.ToPoint());
						if (!replaceableTiles[startTile.TileType]) {
							count++;
							continue;
						}
						ushort tileType = fleshBlockType;
						switch (genRand.Next(1)) { // add possibilities to add possibilities
							case 0:
							tileType = calcifiedTile;
							break;
						}
						Vector2 posMin = new(float.PositiveInfinity);
						Vector2 posMax = new(float.NegativeInfinity);
						Carver.Filter filter = 
							Carver.ActiveTileInSet(replaceableTiles)
							+ Carver.PointyLemon( // tweak to change the shape and size of the calcified areas
								specklePos,
								scale: genRand.NextFloat(6, 10),
								rotation: genRand.NextFloat(0, MathHelper.TwoPi),
								aspectRatio: genRand.NextFloat(3, 6),
								roundness: genRand.NextFloat(1, 2),
								ref posMin,
								ref posMax
							);

						if (Carver.DoCarve(
							filter,
							pos => {
								Framing.GetTileSafely(pos.ToPoint()).TileType = tileType;
								return 1;
							},
							posMin,
							posMax,
							8
						) <= 0) {
							count++;
							continue;
						}
					}
				}

				/*for (int i0 = 0; i0 < genRange.Width; i0++) {
					int x = genRange.X + i0;
					int y = genRange.Y;
					while (!Framing.GetTileSafely(x, y).HasSolidTile()) y++;
				}*/
				ushort flange = (ushort)ModContent.TileType<Marrowick_Flange>();
				ushort largeFlange = (ushort)ModContent.TileType<Large_Marrowick_Flange>();
				for (int i0 = genRand.Next(100, 150); i0-- > 0;) {
					int tries = 18;
					int x = genRange.X + genRand.Next(0, genRange.Width);
					int y = genRand.Next(Math.Min((int)Main.worldSurface - 60, genRange.Y - 1), Math.Min((int)Main.worldSurface, genRange.Y + genRange.Height)) - 1;
					while (!Framing.GetTileSafely(x, y + 1).HasSolidTile()) y++;
					while (Framing.GetTileSafely(x, y).HasSolidTile()) y--;
					while (!TileExtenstions.TryPlace(x, y, genRand.NextBool() ? largeFlange : flange)) {
						y--;
						if (tries-- > 0) break;
					}
				}
				ushort rivenAltar = (ushort)ModContent.TileType<Riven_Altar>();
				for (int i0 = genRand.Next(10, 15); i0-- > 0;) {
					int tries = 0;
					bool placed = false;
					while (!placed && ++tries < 10000) {
						int x = genRange.X + genRand.Next(0, genRange.Width);
						int y = genRange.Y + genRand.Next(0, genRange.Height);
						if (!Framing.GetTileSafely(x, y).HasTile) {
							for (; !Framing.GetTileSafely(x, y).HasTile; y++) {
								if (y > Main.maxTilesY) break;
							}
							y--;
						} else {
							while (Framing.GetTileSafely(x, y).HasTile && y > Main.worldSurface) {
								y--;
							}
						}
						bool tooHigh = true;
						int y2 = y;
						while (y2 > 0) {
							y2--;
							Tile _tile = Framing.GetTileSafely(x, y2);
							if (_tile.HasTile) {
								tooHigh = TileLoader.GetTile(_tile.TileType) is not IRivenTile;
								break;
							}
						}
						if (tooHigh) continue;
						Place3x2(x, y, rivenAltar);
						placed = Framing.GetTileSafely(x, y).TileIsType(rivenAltar);
					}
				}
				ushort rivenLargePile = (ushort)ModContent.TileType<Riven_Large_Foliage>();
				ushort coralPile = (ushort)ModContent.TileType<Marrowick_Coral>();
				for (int i0 = genRand.Next(100, 150); i0-- > 0;) {
					int tries = 18;
					int x = genRange.X + genRand.Next(0, genRange.Width);
					int y = genRange.Y + genRand.Next(0, genRange.Height) - 1;
					ushort type = genRand.NextBool(3) ? coralPile : rivenLargePile;
					while (!PlaceObject(x, y, type, random: TileObjectData.GetTileData(type, 0).RandomStyleRange)) {
						y--;
						if (tries-- > 0) break;
					}
				}
				ushort rivenMediumPile = (ushort)ModContent.TileType<Riven_Medium_Foliage>();
				for (int i0 = genRand.Next(100, 150); i0-- > 0;) {
					int tries = 18;
					int x = genRange.X + genRand.Next(0, genRange.Width);
					int y = genRange.Y + genRand.Next(0, genRange.Height) - 1;
					while (!PlaceObject(x, y, rivenMediumPile, random: TileObjectData.GetTileData(rivenMediumPile, 0).RandomStyleRange)) {
						y--;
						if (tries-- > 0) break;
					}
				}
				for (int i0 = 0; i0 < genRange.Width; i0++) {
					int i1 = i0 + genRange.X;
					for (int j0 = genRange.Height; j0 >= 0; j0--) {
						int j1 = j0 + genRange.Y;
						if (Main.tile[i1, j1].LiquidAmount != 0)
							LiquidMethods.SettleWaterAt(i1, j1);
					}
				}
				WeightedRandom<Point> coralSpots = new(genRand);
				Shelf_Coral shelfCoral = ModContent.GetInstance<Shelf_Coral>();
				for (int i0 = 0; i0 < genRange.Width; i0++) {
					int i1 = i0 + genRange.X;
					for (int j0 = 0; j0 < genRange.Height; j0++) {
						int j1 = j0 + genRange.Y;
						if (shelfCoral.CanGenerate(i1, j1, out double weight)) {
							coralSpots.Add(new(i1, j1), weight);
						}
					}
				}
				int maxCoral = 20;
				while (coralSpots.elements.Count > 0 && maxCoral-->0) {
					Point coralPos = coralSpots.Pop();
					coralSpots.Pop();
					if (shelfCoral.CanGenerate(coralPos.X, coralPos.Y, out _) && TileExtenstions.CanActuallyPlace(coralPos.X, coralPos.Y, shelfCoral.Type, 0, 0, out TileObject objectData, onlyCheck: false) && TileObject.Place(objectData)) {
						TileObjectData.CallPostPlacementPlayerHook(coralPos.X, coralPos.Y, shelfCoral.Type, objectData.style, 0, objectData.alternate, objectData);
					} else {
						maxCoral++;
					}
				}
				NetMessage.SendTileSquare(-1, X0, Y0, X1, Y1);
			}
			public static void StartHive_Old(int i, int j) {
				const float strength = 2.4f;
				const float wallThickness = 4f;
				ushort fleshID = (ushort)ModContent.TileType<Spug_Flesh>();
				ushort weakFleshID = TileID.CrackedBlueDungeonBrick;
				ushort fleshWallID = (ushort)ModContent.WallType<Riven_Flesh_Wall>();
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
				ushort fleshID = (ushort)ModContent.TileType<Spug_Flesh>();
				ushort fleshWallID = (ushort)ModContent.WallType<Riven_Flesh_Wall>();
				ushort blisterID = (ushort)ModContent.TileType<Gel_Blister>();
				int i2 = i + (int)(genRand.Next(-26, 26) * sizeMult);
				int j2 = j + (int)(genRand.Next(-2, 22) * sizeMult);
				Queue<Point> lesionPlacementSpots = new Queue<Point>();
				for (int x = i2 - (int)(33 * sizeMult + 5); x < i2 + (int)(33 * sizeMult + 5); x++) {
					for (int y = j2 + (int)(28 * sizeMult + 4); y >= j2 - (int)(28 * sizeMult + 4); y--) {
						float sq = Math.Max(Math.Abs(y - j2) * 1.5f, Math.Abs(x - i2));
						float diff = (sq * sq + (((y - j2) * (y - j2) * 1.5f) + (x - i2) * (x - i2))) * 0.5f;
						if (diff * 0.9f > 35 * sizeMult * 35 * sizeMult) {
							continue;
						}
						if (diff * 0.9f > (20 * sizeMult - 5) * (20 * sizeMult - 5)) {
							diff = MathF.Sqrt(diff * (GenRunners.GetWallDistOffset(x) * 0.0316076058772687986171132238548f + 1));
						} else {
							diff = 0;
						}
						if (diff > 35 * sizeMult) {
							continue;
						}
						if (OriginExtensions.IsTileReplacable(x, y)) {
							Tile tile = Main.tile[x, y];
							if (!OriginsSets.Walls.RivenWalls[tile.WallType]) {
								tile.ResetToType(fleshID);
								tile.WallType = fleshWallID;
							}
							if ((diff < 35 * sizeMult - 5 || ((y - j) * (y - j)) + (x - i) * (x - i) < 25 * sizeMult * sizeMult)) {
								tile.SetActive(false);
								if (diff > 34 * sizeMult - 5 && Main.tile[x, y + 1].TileIsType(fleshID)) {
									lesionPlacementSpots.Enqueue(new Point(x, y));
								}
							}
							WorldBiomeGeneration.ChangeRange.AddChangeToRange(x, y);
						}
					}
				}
				List<Point> validLesionPlacementSpots = [];
				static bool CheckPos(int x, int y) {
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
				bool placedBlister = false;
				while (!placedBlister) {
					placedBlister = PlaceTile(i2 + genRand.Next(-2, 3), j2 + genRand.Next(-2, 3), blisterID);
				}
				DoScar(
					new(i2, j2),
					genRand.NextFloat(0, MathHelper.TwoPi),
					(ushort)ModContent.TileType<Amoeba_Fluid>(),
					genRand.NextFloat(6, 10),
					genRand.NextFloat(3, 6)
				);
				return new Point(i2, j2);
			}
			public static void DoScar(Vector2 start, float angle, ushort scarBlockID, float scale, float aspectRatio, float roundness = 1) {
				Vector2 posMin = new(float.PositiveInfinity);
				Vector2 posMax = new(float.NegativeInfinity);
				PlayerInput.SetZoom_MouseInWorld();
				start *= 16;
				Vector2 direction = angle.ToRotationVector2();
				float dist = CollisionExt.Raymarch(start, direction, 16 * 50);
				Carver.Filter filter = Carver.ActiveTileInSet(TileID.Sets.Factory.CreateBoolSet(ModContent.TileType<Spug_Flesh>()))
				+ Carver.PointyLemon((start + direction * dist) / 16, scale, direction.ToRotation() + MathHelper.PiOver2, aspectRatio, roundness, ref posMin, ref posMax);

				Carver.DoCarve(
					filter,
					pos => {
						Framing.GetTileSafely(pos.ToPoint()).TileType = scarBlockID;
						return 1;
					},
					posMin,
					posMax,
					8
				);
				posMin = new(MathF.Floor(posMin.X), MathF.Floor(posMin.Y));
				posMax = new(MathF.Ceiling(posMax.X), MathF.Ceiling(posMax.Y));
				WorldGen.RangeFrame((int)posMin.X, (int)posMin.Y, (int)posMax.X, (int)posMax.Y);
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
				int bossHeadType = ModContent.NPCType<World_Cracker_Head>();
				if (shadowOrbCount >= 3) {
					if (!NPC.AnyNPCs(bossHeadType)) {
						shadowOrbCount = 0;
						Main.LocalPlayer.SpawnBossOn(bossHeadType);
					}
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
	public class Riven_Hive_Water_Control : ModSceneEffect {
		public override ModWaterStyle WaterStyle => ModContent.GetInstance<Riven_Water_Style>();
		public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
		public override bool IsSceneEffectActive(Player player) => OriginSystem.rivenTiles > Riven_Hive.NeededTiles;
		public override float GetWeight(Player player) => 1f;
	}
	#region variations
	public class Underground_Riven_Hive_Biome : ModBiome {
		public override int Music => Origins.Music.UndergroundRiven;
		public override string BackgroundPath => "Origins/UI/MapBGs/Riven_Hive_Caverns";
		public override string BestiaryIcon => "Origins/UI/IconStonerRiven";
		public override string MapBackground => BackgroundPath;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneRockLayerHeight && Riven_Hive.IsActive;
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneRivenProgress * 0.99f;
		}
	}
	public class Riven_Hive_Desert : ModBiome {
		public override int Music => Origins.Music.Riven;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BackgroundPath => "Origins/UI/MapBGs/Riven_Desert";
		public override string BestiaryIcon => "Origins/UI/IconDesertRiven";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneDesert && Riven_Hive.IsActive;
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneRivenProgress * 0.99f;
		}
	}
	public class Riven_Hive_Underground_Desert : ModBiome {
		public override int Music => Origins.Music.UndergroundRiven;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BackgroundPath => "Origins/UI/MapBGs/Riven_Desert";
		public override string BestiaryIcon => "Origins/UI/IconCatacombsRiven";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneRockLayerHeight && player.ZoneDesert && Riven_Hive.IsActive;
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneRivenProgress;
		}
	}
	public class Riven_Hive_Ice_Biome : ModBiome {
		public override int Music => Origins.Music.UndergroundRiven;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BackgroundPath => "Origins/UI/MapBGs/Riven_Snow";
		public override string BestiaryIcon => "Origins/UI/IconSnowRiven";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneRockLayerHeight && player.ZoneSnow && Riven_Hive.IsActive;
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneRivenProgress;
		}
	}
	public class Riven_Hive_Ocean : ModBiome {
		public override int Music => Origins.Music.RivenOcean;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BestiaryIcon => "Origins/UI/IconEutrophicSea";
		public override string BackgroundPath => "Origins/UI/MapBGs/Eutrophic_Sea";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneBeach && Riven_Hive.IsActive;
		}
		public override float GetWeight(Player player) {
			return player.OriginPlayer().ZoneRivenProgress * 1f;
		}
	}
	#endregion variations
	public class Riven_Hive_Alt_Biome : AltBiome, IItemObtainabilityProvider {
		public override string WorldIcon => "Origins/UI/WorldGen/IconRiven";
		public override string OuterTexture => "Origins/UI/WorldGen/Outer_Riven";
		public override string IconSmall => "Origins/UI/WorldGen/IconEvilRiven";
		public override Color OuterColor => new(30, 176, 255);
		public override Color NameColor => new(25, 151, 230);
		public override IShoppingBiome Biome => ModContent.GetInstance<Riven_Hive>();
		public override void SetStaticDefaults() {
			BiomeType = AltLibrary.BiomeType.Evil;
			//DisplayName.SetDefault(Language.GetTextValue("Mods.Origins.Generic.Riven_Hive"));
			//Description.SetDefault(Language.GetTextValue("A flourishing and beautiful ecosystem soon to replace the status quo dominated by a now sentient amoeba."));
			//GenPassName.SetDefault(Language.GetTextValue("{$Mods.Origins.Generic.Riven_Hive}"));

			AddTileConversion(ModContent.TileType<Riven_Grass>(), TileID.Grass);
			AddTileConversion(ModContent.TileType<Riven_Jungle_Grass>(), TileID.JungleGrass);
			AddTileConversion(ModContent.TileType<Spug_Flesh>(), TileID.Stone);
			AddTileConversion(ModContent.TileType<Calcified_Riven_Flesh>(), ModContent.TileType<Calcified_Riven_Flesh>());
			AddTileConversion(ModContent.TileType<Silica>(), TileID.Sand);
			AddTileConversion(ModContent.TileType<Quartz>(), TileID.Sandstone);
			AddTileConversion(ModContent.TileType<Brittle_Quartz>(), TileID.HardenedSand);
			AddTileConversion(ModContent.TileType<Primordial_Permafrost>(), TileID.IceBlock);

			AddTileConversion(ModContent.TileType<Amoeba_Fluid>(), TileID.ClayBlock);

			GERunnerConversion.Add(TileID.Silt, ModContent.TileType<Silica>());

			BiomeFlesh = ModContent.TileType<Amoeba_Fluid>(); // temp?
			BiomeFleshWall = ModContent.WallType<Amebic_Gel_Wall>();

			SeedType = ModContent.ItemType<Riven_Grass_Seeds>();
			BiomeOre = ModContent.TileType<Encrusted_Ore>();
			BiomeOreItem = ModContent.ItemType<Encrusted_Ore_Item>();
			BiomeOreBrick = ModContent.TileType<Encrusted_Brick>();
			AltarTile = ModContent.TileType<Riven_Altar>();

			BiomeChestItem = ModContent.ItemType<Plasma_Cutter>();
			BiomeChestTile = ModContent.TileType<Riven_Dungeon_Chest>();
			BiomeChestTileStyle = 1;
			BiomeKeyItem = ModContent.ItemType<Riven_Key>();

			MimicType = ModContent.NPCType<Riven_Mimic>();

			BloodBunny = ModContent.NPCType<Barnacle_Bunny>();
			BloodPenguin = ModContent.NPCType<Riven_Penguin>();
			BloodGoldfish = ModContent.NPCType<Bottomfeeder>();

			AddWallConversions(OriginsWall.GetWallID<Calcified_Riven_Flesh_Wall>(WallVersion.Natural), OriginsWall.GetWallID<Calcified_Riven_Flesh_Wall>(WallVersion.Natural));
			
			AddWallConversions(OriginsWall.GetWallID<Barnacle_Wall>(WallVersion.Natural),
				WallID.RocksUnsafe1,
				WallID.Rocks1Echo
			);

			AddWallConversions<Riven_Flesh_Wall>(
				WallID.Cave7Unsafe,
				WallID.CaveUnsafe,
				WallID.Cave2Unsafe,
				WallID.Cave3Unsafe,
				WallID.Cave4Unsafe,
				WallID.Cave5Unsafe,
				WallID.Cave6Unsafe,
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
				WallID.CrimsonUnsafe4,
				WallID.Stone
			);
			AddWallConversions<Quartz_Wall>(
				WallID.Sandstone
			);
			AddWallConversions<Brittle_Quartz_Wall>(
				WallID.HardenedSand
			);
			AddWallConversions(OriginsWall.GetWallID<Riven_Grass_Wall>(WallVersion.Natural),
				WallID.GrassUnsafe,
				WallID.Grass
			);
			this.AddChambersiteConversions(ModContent.TileType<Chambersite_Ore_Riven_Flesh>(), ModContent.WallType<Chambersite_Riven_Flesh_Wall>());
			EvilBiomeGenerationPass = new Riven_Hive_Generation_Pass();
		}
		public IEnumerable<int> ProvideItemObtainability() {
			yield return BiomeChestItem.Value;
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
		public class Riven_Hive_Generation_Pass : EvilBiomeGenerationPass {
			public override string ProgressMessage => Language.GetTextValue("Mods.Origins.AltBiomes.Riven_Hive_Alt_Biome.GenPassName");
			public override void GenerateEvil(int evilBiomePosition, int evilBiomePositionWestBound, int evilBiomePositionEastBound) {
				WorldBiomeGeneration.ChangeRange.ResetRange();
				int y = (int)GenVars.worldSurface - 50;
				for (; !Main.tile[evilBiomePosition, y].HasSolidTile(); y++) ;

				Riven_Hive.Gen.StartHive(evilBiomePosition, y);

				int minY = WorldBiomeGeneration.ChangeRange.GetRange().Top;
				WorldBiomeGeneration.ChangeRange.AddChangeToRange(evilBiomePositionWestBound, minY);
				WorldBiomeGeneration.ChangeRange.AddChangeToRange(evilBiomePositionEastBound, minY);
				Rectangle range = WorldBiomeGeneration.ChangeRange.GetRange();
				bool anyTiles;
				int extendedMinY = minY;
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
				AltBiome biome = ModContent.GetInstance<Riven_Hive_Alt_Biome>();
				ushort grass = (ushort)ModContent.TileType<Riven_Grass>();
				for (int i = range.Left; i < range.Right; i++) {
					int slopeFactor = Math.Min(Math.Min(i - range.Left, range.Right - i), 99);
					for (int j = range.Top - 10; j < range.Bottom; j++) {
						if (genRand.NextBool(5) && genRand.Next(slopeFactor, 100) < 20) continue;
						if (range.Bottom - j < 5 && genRand.NextBool(5)) break;
						Tile tile = Framing.GetTileSafely(i, j);
						if (tile.HasTile) {
							AltLibrary.Core.ALConvert.ConvertTile(i, j, biome);
							if (tile.TileType == TileID.Dirt && (!Framing.GetTileSafely(i - 1, j).HasTile || !Framing.GetTileSafely(i + 1, j).HasTile || !Framing.GetTileSafely(i, j - 1).HasTile || !Framing.GetTileSafely(i, j + 1).HasTile)) {
								tile.TileType = grass;
							}
						}
						AltLibrary.Core.ALConvert.ConvertWall(i, j, biome);
					}
				}
				OriginSystem.Instance.hasRiven = true;
			}

			public override void PostGenerateEvil() { }
		}
	}
}
