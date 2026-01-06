using AltLibrary;
using AltLibrary.Common.AltBiomes;
using AltLibrary.Common.Systems;
using AltLibrary.Core.Generation;
using Origins.Backgrounds;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Other.Fish;
using Origins.Items.Pets;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.NPCs.Ashen;
using Origins.NPCs.Defiled;
using Origins.Reflection;
using Origins.Tiles;
using Origins.Tiles.Ashen;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Origins.Walls;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.WorldBuilding;
using static Origins.OriginExtensions;
using static Terraria.WorldGen;
using static Terraria.ModLoader.ModContent;

namespace Origins.World.BiomeData {
	public class Ashen_Biome : ModBiome {
		public static IItemDropRule FirstOrbDropRule;
		public static IItemDropRule OrbDropRule;
		public override int Music => Origins.Music.AshenScrapyard;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BestiaryIcon => "Origins/UI/WorldGen/IconEvilAshen";
		public override string BackgroundPath => "Origins/UI/MapBGs/Defiled_Wastelands_Normal";
		public override string MapBackground => BackgroundPath;
		public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => GetInstance<Defiled_Surface_Background>();
		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => BiomeUGBackground<Riven_Underground_Background>();
		public override int BiomeTorchItemType => ItemType<Ashen_Torch>();
		public override int BiomeCampfireItemType => ItemType<Ashen_Campfire_Item>();
		public static bool forcedBiomeActive;
		public override bool IsBiomeActive(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.ZoneAshenProgress = Math.Min(OriginSystem.ashenTiles - (NeededTiles - ShaderTileCount), ShaderTileCount) / ShaderTileCount;
			LinearSmoothing(ref originPlayer.ZoneAshenProgressSmoothed, originPlayer.ZoneAshenProgress, OriginSystem.biomeShaderSmoothing * 0.1f);

			return IsActive;
		}
		public static bool IsActive => OriginSystem.ashenTiles > NeededTiles;
		public override void SpecialVisuals(Player player, bool isActive) {
			//OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			//Filters.Scene["Origins:ZoneRiven"].GetShader().UseProgress(originPlayer.ZoneRivenProgressSmoothed);
			//player.ManageSpecialBiomeVisuals("Origins:ZoneRiven", originPlayer.ZoneRivenProgressSmoothed > 0, player.Center);
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneAshenProgress * 0.98f;
		}
		public override void Load() {
			FirstOrbDropRule = ItemDropRule.Common(ItemType<Neural_Network>());
			FirstOrbDropRule.OnSuccess(ItemDropRule.Common(ItemID.MusketBall, 1, 100, 100));

			IItemDropRule AceShrapnelRule = ItemDropRule.NotScalingWithLuck(ItemType<Ace_Shrapnel>());
			AceShrapnelRule.OnSuccess(ItemDropRule.Common(ItemType<Scrap>(), 1, 200, 200));

			OrbDropRule = new OneFromRulesRule(1,
				FirstOrbDropRule,
				AceShrapnelRule,
				ItemDropRule.NotScalingWithLuck(ItemType<Area_Denial>()),
				ItemDropRule.NotScalingWithLuck(ItemType<Smiths_Hammer>()),
				ItemDropRule.NotScalingWithLuck(ItemType<Seal_Of_Cinders>())
			);
		}
		public override void Unload() {
			FirstOrbDropRule = null;
			OrbDropRule = null;
		}
		public const int NeededTiles = 200;
		public const int ShaderTileCount = 25;
		public const short DefaultTileDust = DustID.Lihzahrd;
		public class SpawnRates : SpawnPool {
			public const float PowerZombie = 0.8f;
			public const float Mimic = 0.01f;
			public const float CursedWeapon = 0.01f;
			public override string Name => $"{nameof(Ashen_Biome)}_{base.Name}";
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
				AddSpawn(NPCType<Trash_Compactor_Mimic>(), MimicRate(Mimic));
				AddSpawn(NPCType<Enchanted_Trident>(), MimicRate(CursedWeapon));
			}
			public static float LandEnemyRate(NPCSpawnInfo spawnInfo, bool hardmode = false) {
				if (hardmode && !Main.hardMode) return 0f;
				return 1f;
			}
			public static float FlyingEnemyRate(NPCSpawnInfo spawnInfo, bool hardmode = false) {
				return LandEnemyRate(spawnInfo, hardmode);
			}
			public override bool IsActive(NPCSpawnInfo spawnInfo) {
				return TileLoader.GetTile(spawnInfo.SpawnTileType) is IAshenTile || (spawnInfo.Player.InModBiome<Ashen_Biome>() && spawnInfo.SpawnTileType == TileType<Sanguinite_Ore>()) || forcedBiomeActive;
			}
		}
		public static class Gen {
			public enum FeatureType {
				CHUNK,
				CUSP,
				CAVE,
			}
			public static void StartHive(int i, int j) {
				Vector2 pos = new(i, j);
				ushort fleshBlockType = (ushort)TileType<Tainted_Stone>();
				ushort fleshWallType = (ushort)WallType<Tainted_Stone_Wall>();
				int oreID = TileType<Sanguinite_Ore>();
				HashSet<ushort> cleaveReplacables = [fleshBlockType];
				Tile tile;
				int X0 = int.MaxValue;
				int X1 = 0;
				int Y0 = int.MaxValue;
				int Y1 = 0;
				List<(int x, int y, FeatureType type, bool rightSide)> features = [];

				float targetTwist = genRand.NextFloat(-0.5f, 0.5f);
				PolarVec2 speed = new(8, genRand.NextFloat(-0.5f, 0.5f) + MathHelper.PiOver2);
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
					}
					tile.HasTile = true;
				}
				Rectangle genRange = WorldBiomeGeneration.ChangeRange.GetRange();

				/*for (int i0 = 0; i0 < genRange.Width; i0++) {
					int x = genRange.X + i0;
					int y = genRange.Y;
					while (!Framing.GetTileSafely(x, y).HasSolidTile()) y++;
				}*/
				ushort rivenAltar = (ushort)TileType<Ashen_Altar>();
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
				ushort rivenLargePile = (ushort)TileType<Ashen_Large_Foliage>();
				for (int i0 = genRand.Next(100, 150); i0-- > 0;) {
					int tries = 18;
					int x = genRange.X + genRand.Next(0, genRange.Width);
					int y = genRange.Y + genRand.Next(0, genRange.Height) - 1;
					ushort type = rivenLargePile;
					while (!PlaceObject(x, y, type, random: TileObjectData.GetTileData(type, 0).RandomStyleRange)) {
						y--;
						if (tries-- > 0) break;
					}
				}
				ushort rivenMediumPile = (ushort)TileType<Ashen_Medium_Foliage>();
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
				NetMessage.SendTileSquare(-1, X0, Y0, X1, Y1);
			}
			public static void StartHive_Old(int i, int j) {
				const float strength = 2.4f;
				const float wallThickness = 4f;
				ushort fleshID = (ushort)TileType<Tainted_Stone>();
				ushort weakFleshID = TileID.CrackedBlueDungeonBrick;
				ushort fleshWallID = (ushort)WallType<Tainted_Stone_Wall>();
				int j2 = j;
				if (j2 > Main.worldSurface) {
					j2 = (int)Main.worldSurface;
				}
				for (; !SolidTile(i, j2); j2++) { }
				Vector2 position = new(i, j2);
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
				Vector2 manualVel = new(last.velocity.X, 0.2f);
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
					PolarVec2 vel = new(1, last.velocity.ToRotation());
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
				ushort fleshID = (ushort)TileType<Tainted_Stone>();
				ushort fleshWallID = (ushort)WallType<Tainted_Stone_Wall>();
				ushort blisterID = (ushort)TileType<Gel_Blister>();
				int i2 = i + (int)(genRand.Next(-26, 26) * sizeMult);
				int j2 = j + (int)(genRand.Next(-2, 22) * sizeMult);
				Queue<Point> lesionPlacementSpots = new();
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
				return new Point(i2, j2);
			}
			public static void SpreadRivenGrass(int i, int j) {
				const int maxDepth = 150;
				ushort grassType = (ushort)TileType<Ashen_Grass>();
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
							} else if (Main.tileSolid[tile.TileType]) {
								break;
							}
						}
					}
				}
			}
		}
	}
	#region variations
	public class Underground_Ashen_Biome : ModBiome {
		public override int Music => Origins.Music.AshenMines;
		public override string BackgroundPath => "Origins/UI/MapBGs/Defiled_Wastelands_Normal";
		public override string BestiaryIcon => "Origins/UI/WorldGen/IconEvilAshen";
		public override string MapBackground => BackgroundPath;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneRockLayerHeight && Ashen_Biome.IsActive;
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneAshenProgress * 0.99f;
		}
	}
	public class Ashen_Desert : ModBiome {
		public override int Music => Origins.Music.AshenScrapyard;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BackgroundPath => "Origins/UI/MapBGs/Defiled_Wastelands_Normal";
		public override string BestiaryIcon => "Origins/UI/WorldGen/IconEvilAshen";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneDesert && Ashen_Biome.IsActive;
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneAshenProgress * 0.99f;
		}
	}
	public class Ashen_Underground_Desert : ModBiome {
		public override int Music => Origins.Music.AshenMines;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BackgroundPath => "Origins/UI/MapBGs/Defiled_Wastelands_Normal";
		public override string BestiaryIcon => "Origins/UI/WorldGen/IconEvilAshen";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneRockLayerHeight && player.ZoneDesert && Ashen_Biome.IsActive;
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneAshenProgress;
		}
	}
	public class Ashen_Ice_Biome : ModBiome {
		public override int Music => Origins.Music.AshenMines;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BackgroundPath => "Origins/UI/MapBGs/Defiled_Wastelands_Normal";
		public override string BestiaryIcon => "Origins/UI/WorldGen/IconEvilAshen";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneRockLayerHeight && player.ZoneSnow && Ashen_Biome.IsActive;
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneAshenProgress;
		}
	}
	public class Ashen_Ocean : ModBiome {
		public override int Music => Origins.Music.AshenScrapyard;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BackgroundPath => "Origins/UI/MapBGs/Defiled_Wastelands_Normal";
		public override string BestiaryIcon => "Origins/UI/WorldGen/IconEvilAshen";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneBeach && Ashen_Biome.IsActive;
		}
		public override float GetWeight(Player player) {
			return player.OriginPlayer().ZoneAshenProgress * 1f;
		}
	}
	#endregion variations
	public class Ashen_Alt_Biome : AltBiome {
		public override string WorldIcon => "Origins/UI/WorldGen/IconDefiled";
		public override string OuterTexture => "Origins/UI/WorldGen/Outer_Defiled";
		public override string IconSmall => "Origins/UI/WorldGen/IconEvilAshen";
		public override Color OuterColor => new(255, 170, 170);
		public override Color NameColor => new(255, 100, 100);
		public override Color? BiomeSightColor => Color.OrangeRed;
		public override IShoppingBiome Biome => GetInstance<Ashen_Biome>();
		public override void SetStaticDefaults() {
			BiomeType = BiomeType.Evil;

			AddTileConversion(TileType<Ashen_Grass>(), TileID.Grass);
			AddTileConversion(TileType<Ashen_Jungle_Grass>(), TileID.JungleGrass);
			AddTileConversion(TileType<Tainted_Stone>(), TileID.Stone);
			AddTileConversion(TileType<Sootsand>(), TileID.Sand);
			AddTileConversion(TileType<Soot_Sandstone>(), TileID.Sandstone);
			AddTileConversion(TileType<Hardened_Sootsand>(), TileID.HardenedSand);
			AddTileConversion(TileType<Brown_Ice>(), TileID.IceBlock);

			CreateGrassType(new(true, true),
				(TileID.Dirt, TileType<Ashen_Grass>()),
				(TileID.Mud, TileType<Ashen_Jungle_Grass>()),
				(TileType<Murky_Sludge>(), TileType<Ashen_Murky_Sludge_Grass>())
			);

			BiomeFlesh = TileID.AncientGoldBrick;
			BiomeFleshWall = WallID.AncientGoldBrickWall;

			SeedType = ItemType<Ashen_Grass_Seeds>();
			BiomeOre = TileType<Sanguinite_Ore>();
			BiomeOreItem = ItemType<Sanguinite_Ore_Item>();
			BiomeOreBrick = TileType<Sanguinite_Brick>();
			AltarTile = TileType<Ashen_Altar>();

			BiomeChestItem = ItemType<Ashen_Torch>();
			BiomeChestTile = TileType<Ashen_Dungeon_Chest>();
			BiomeChestTileStyle = 1;
			BiomeKeyItem = ItemType<Ashen_Key>();

			MimicType = NPCType<Trash_Compactor_Mimic>();

			BloodBunny = NPCType<Defiled_Mite>();
			BloodPenguin = NPCType<Bile_Thrower>();
			BloodGoldfish = NPCType<Shattered_Goldfish>();

			this.AddOriginsWallConversions<Tainted_Stone_Wall>(WallVersion.Natural,
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
			AddWallConversions<Soot_Sandstone_Wall>(
				WallID.Sandstone,
				WallID.CorruptSandstone,
				WallID.CrimsonSandstone,
				WallID.HallowSandstone
			);
			AddWallConversions<Hardened_Sootsand_Wall>(
				WallID.HardenedSand,
				WallID.CorruptHardenedSand,
				WallID.CrimsonHardenedSand,
				WallID.HallowHardenedSand
			);
			AddWallConversions(OriginsWall.GetWallID<Ashen_Grass_Wall>(WallVersion.Natural),
				WallID.GrassUnsafe,
				WallID.Grass
			);
			this.AddChambersiteTileConversions(Chambersite_Ore.GetOreID(TileType<Tainted_Stone>()));

			EvilBiomeGenerationPass = new Ashen_Generation_Pass();
		}
		public override AltMaterialContext MaterialContext {
			get {
				AltMaterialContext context = new();
				context.SetEvilSword(ItemType<Switchblade_Broadsword>());
				context.SetEvilOre(ItemType<Sanguinite_Ore_Item>());
				context.SetEvilBar(ItemType<Sanguinite_Bar>());
				context.SetEvilHerb(ItemType<Surveysprout_Item>());
				context.SetVileComponent(ItemType<Phoenum>());
				context.SetVileInnard(ItemType<Biocomponent10>());
				context.SetEvilBossDrop(ItemType<NE8>());
				return context;
			}
		}
		public class Ashen_Generation_Pass : EvilBiomeGenerationPass {
			public override string ProgressMessage => Language.GetTextValue("Mods.Origins.AltBiomes.Ashen_Alt_Biome.GenPassName");
			public override void GenerateEvil(int evilBiomePosition, int evilBiomePositionWestBound, int evilBiomePositionEastBound) {
				WorldBiomeGeneration.ChangeRange.ResetRange();
				int y = (int)GenVars.worldSurface - 50;
				for (; !Main.tile[evilBiomePosition, y].HasSolidTile(); y++) ;

				Ashen_Biome.Gen.StartHive(evilBiomePosition, y);

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
				AltBiome biome = GetInstance<Ashen_Alt_Biome>();
				ushort grass = (ushort)TileType<Ashen_Grass>();
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
				OriginSystem.Instance.hasAshen = true;
			}

			public override void PostGenerateEvil() { }
		}
		public class Ashen_Fishing_Pool : FishingLootPool<Ashen_Alt_Biome> {
			public override void SetStaticDefaults() {
				//AddCrates(ItemType<Crate>(), ItemType<Really_Crate>());
				Legendary.Add(new SequentialCatches(
					FishingCatch.Item(ItemID.ScalyTruffle, (player, attempt) => Main.hardMode && player.ZoneSnow && attempt.heightLevel == 3 && !Main.rand.NextBool(3))
				));
				Rare.Add(FishingCatch.Item(ItemType<Internal_Combustionfish>()));
				Uncommon.Add(new SequentialCatches(
					FishingCatch.QuestFish(ItemType<Scrapfish>()),
					FishingCatch.Item(ItemType<Polyeel>())
				));
			}
		}
	}
}
