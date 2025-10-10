using AltLibrary;
using AltLibrary.Common.AltBiomes;
using AltLibrary.Common.Systems;
using AltLibrary.Core;
using AltLibrary.Core.Generation;
using Origins.Backgrounds;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Other.Fish;
using Origins.Items.Pets;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.NPCs.Defiled;
using Origins.NPCs.Defiled.Boss;
using Origins.Projectiles.Misc;
using Origins.Tiles;
using Origins.Tiles.Defiled;
using Origins.Tiles.Other;
using Origins.Walls;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Personalities;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.WorldBuilding;
using static Origins.OriginExtensions;
using static Terraria.WorldGen;
using static Terraria.ModLoader.ModContent;

namespace Origins.World.BiomeData {
	public class Defiled_Wastelands : ModBiome {
		public static IItemDropRule FirstFissureDropRule;
		public static IItemDropRule FissureDropRule;
		public override int Music => Main.swapMusic ? Origins.Music.OtherworldlyDefiled : Origins.Music.Defiled;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BestiaryIcon => "Origins/UI/WorldGen/IconEvilDefiled";
		public override string BackgroundPath => "Origins/UI/MapBGs/Defiled_Wastelands_Normal";
		public override string MapBackground => BackgroundPath;
		public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<Defiled_Surface_Background>();
		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => BiomeUGBackground<Defiled_Underground_Background>();
		public override int BiomeTorchItemType => ModContent.ItemType<Defiled_Torch>();
		public override int BiomeCampfireItemType => ModContent.ItemType<Defiled_Campfire_Item>();
		public static bool forcedBiomeActive;
		public static bool monolithActive;
		public override bool IsBiomeActive(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			int defiledTiles = OriginSystem.defiledTiles;
			int defiledAmalgamation = ModContent.NPCType<Defiled_Amalgamation>();
			Rectangle screenRect = new Rectangle(0, 0, NPC.sWidth, NPC.sHeight).Recentered(player.Center);
			Rectangle npcRect = new(0, 0, 5000 * 2, 5000 * 2);
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.type == defiledAmalgamation) {
					if (screenRect.Intersects(npcRect.Recentered(npc.Center))) defiledTiles += 100;
					if (npc.target == player.whoAmI) defiledTiles += 100;
				} else if (npc.ModNPC is IDefiledEnemy && screenRect.Intersects(npcRect.Recentered(npc.Center))) {
					defiledTiles += 5;
				}
			}
			originPlayer.ZoneDefiledProgress = (Math.Min(
				defiledTiles - (NeededTiles - ShaderTileCount),
				ShaderTileCount
			) / ShaderTileCount) * 0.9f;

			LinearSmoothing(ref originPlayer.ZoneDefiledProgressSmoothed, monolithActive ? 1 : originPlayer.ZoneDefiledProgress, OriginSystem.biomeShaderSmoothing);

			return defiledTiles > NeededTiles;
		}
		public override void SpecialVisuals(Player player, bool isActive) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			const float heart_range = 16 * 50;
			ReadOnlyCollection<Point16> heartLocations = TESystem.GetLocations<Defiled_Heart_TE_System>();
			float heartProximity = heartLocations.Count > 0 ? 
				Math.Max(0, heart_range - heartLocations.Select(heart => player.Distance(heart.ToWorldCoordinates())).Min()) / heart_range
				: 0;
			Filters.Scene["Origins:ZoneDefiled"].GetShader()
				.UseProgress(originPlayer.ZoneDefiledProgressSmoothed * (MathF.Pow(heartProximity, 4) + 1))
				.UseIntensity(OriginClientConfig.Instance.DefiledShaderJitter * 0.0035f * (MathF.Pow(heartProximity, 2) * 1.25f + 1))
				.UseOpacity(Math.Max(OriginClientConfig.Instance.DefiledShaderNoise * (MathF.Pow(heartProximity, 2) * 1.25f + 1), float.Epsilon))
				.Shader.Parameters["uTimeScale"].SetValue(OriginClientConfig.Instance.DefiledShaderSpeed);
			player.ManageSpecialBiomeVisuals("Origins:ZoneDefiled", originPlayer.ZoneDefiledProgressSmoothed > 0 && !OriginAccessibilityConfig.Instance.DisableDefiledWastelandsShader, player.Center);
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneDefiledProgress * 0.98f;
		}
		public override void Load() {
			FirstFissureDropRule = ItemDropRule.Common(ModContent.ItemType<Kruncher>());
			FirstFissureDropRule.OnSuccess(ItemDropRule.Common(ItemID.MusketBall, 1, 100, 100));

			FissureDropRule = new OneFromRulesRule(1,
				FirstFissureDropRule,
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Dim_Starlight>()),
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Monolith_Rod>()),
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Krakram>()),
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Suspicious_Looking_Pebble>())
			);
		}
		public override void Unload() {
			FirstFissureDropRule = null;
			FissureDropRule = null;
		}
		public const int NeededTiles = 200;
		public const int ShaderTileCount = 75;
		public const short DefaultTileDust = DustID.Titanium;
		//public static SpawnConditionBestiaryInfoElement BestiaryIcon = new SpawnConditionBestiaryInfoElement("Bestiary_Biomes.Ocean", 28, "Images/MapBG11");
		public class SpawnRates : SpawnPool {
			public const float Cyclops = 1;
			public const float Mite = 1;
			public const float Mummy = 1;
			public const float Ghoul = 2;
			public const float Brute = 0.6f;
			public const float Flyer = 0.6f;
			public const float Worm = 0.6f;
			public const float Mimic = 0.01f;
			public const float Bident = 0.01f;
			public const float Tripod = 0.3f;
			public const float Sqid = 0.09f;
			public const float AncientCyclops = 0.03f;
			public const float Asphyxiator = 0.5f;
			public const float AncientFlyer = 0.04f;
			public const float Nearby = 0.5f;
			public const float Broadcaster = 0.3f;
			public override string Name => $"{nameof(Defiled_Wastelands)}_{base.Name}";
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
				AddSpawn(ModContent.NPCType<Defiled_Mimic>(), MimicRate(Mimic));
				AddSpawn(ModContent.NPCType<Enchanted_Trident>(), MimicRate(Bident));
			}
			public static float LandEnemyRate(NPCSpawnInfo spawnInfo, bool hardmode = false) {
				if (hardmode && !Main.hardMode) return 0f;
				return 1f;
			}
			public static float FlyingEnemyRate(NPCSpawnInfo spawnInfo, bool hardmode = false) {
				return LandEnemyRate(spawnInfo, hardmode);
			}

			public override bool IsActive(NPCSpawnInfo spawnInfo) {
				return TileLoader.GetTile(spawnInfo.SpawnTileType) is IDefiledTile || (spawnInfo.Player.InModBiome<Defiled_Wastelands>() && spawnInfo.SpawnTileType == ModContent.TileType<Lost_Ore>()) || forcedBiomeActive;
			}
		}
		public static class Gen {
			public static void StartDefiled(float i, float j) {
				const float strength = 3.3f; //width of tunnels
				const float wallThickness = 3.1f;
				const float distance = 48; //tunnel length
				const float caveSize = 30;

				ushort stoneID = (ushort)ModContent.TileType<Defiled_Stone>();
				ushort stoneWallID = (ushort)ModContent.WallType<Defiled_Stone_Wall>();
				int oreID = ModContent.TileType<Lost_Ore>();
				Vector2 startVec = new Vector2(i, j);
				int fisureCount = 0;
				DefiledCave(i, j);
				Queue<(int generation, (Vector2 position, Vector2 velocity))> veins = new Queue<(int generation, (Vector2 position, Vector2 velocity))>();
				int startCount = genRand.Next(4, 9);
				float maxSpread = 3f / startCount;
				Vector2 vel;
				for (int count = startCount; count > 0; count--) {
					vel = Vector2.UnitX.RotatedBy((MathHelper.TwoPi * (count / (float)startCount)) + genRand.NextFloat(-maxSpread, maxSpread));
					veins.Enqueue((0, (startVec + vel * 16, vel)));
				}
				(int generation, (Vector2 position, Vector2 velocity) data) current;
				(int generation, (Vector2 position, Vector2 velocity) data) next;
				List<Vector2> fissureCheckSpots = new List<Vector2>();
				List<Vector2> nodes = [];
				Vector2 airCheckVec;
				List<Vector2> ends = [];
				while (veins.Count > 0) {
					current = veins.Dequeue();
					int endChance = genRand.Next(1, 5) + genRand.Next(0, 4) + genRand.Next(0, 4);
					int selector = genRand.Next(4);
					if (endChance <= current.generation) {
						if (genRand.Next(veins.Count) < 6 - fissureCheckSpots.Count) {
							selector = 3;
						}
					} else if (selector == 3 && genRand.Next(veins.Count) > 6 - fissureCheckSpots.Count) {
						selector = genRand.Next(3);
					}
					switch (selector) {
						case 0:
						case 1: {//single vein
							next = (current.generation + 1,
								DefiledVeinRunner(
									(int)current.data.position.X,
									(int)current.data.position.Y,
									strength * genRand.NextFloat(0.9f, 1.1f), //tunnel width randomization
									current.data.velocity.RotatedBy(genRand.NextBool() ? genRand.NextFloat(-0.6f, -0.1f) : genRand.NextFloat(0.2f, 0.6f)), //random rotation
									genRand.NextFloat(distance * 0.8f, distance * 1.2f), //tunnel length
									stoneID,
									wallThickness,
									wallType: stoneWallID,
									oreType: oreID
								));
							airCheckVec = next.data.position;
							if (airCheckVec.Y < Main.worldSurface && Main.tile[(int)airCheckVec.X, (int)airCheckVec.Y].WallType == WallID.None) {
								OriginSystem.EvilSpikeAvoidancePoints.Add(new((int)airCheckVec.X, (int)airCheckVec.Y));
								break;
							}
							if (endChance > current.generation) {
								veins.Enqueue(next);
							} else {
								ends.Add(next.data.position);
							}
							nodes.Add(next.data.position);
							break;
						}
						case 2: {//split vein
							next = (current.generation + 2,
								DefiledVeinRunner(
									(int)current.data.position.X,
									(int)current.data.position.Y,
									strength * genRand.NextFloat(0.9f, 1.1f),
									current.data.velocity.RotatedBy(-0.4f + genRand.NextFloat(-1, 0.2f)),
									genRand.NextFloat(distance * 0.8f, distance * 1.2f),
									stoneID,
									wallThickness,
									wallType: stoneWallID,
									oreType: oreID
								));
							airCheckVec = next.data.position;
							if (airCheckVec.Y < Main.worldSurface && Main.tile[(int)airCheckVec.X, (int)airCheckVec.Y].WallType == WallID.None) {
								OriginSystem.EvilSpikeAvoidancePoints.Add(new((int)airCheckVec.X, (int)airCheckVec.Y));
								break;
							}
							if (endChance > current.generation) {
								veins.Enqueue(next);
							}
							next = (current.generation + 2,
								DefiledVeinRunner(
									(int)current.data.position.X,
									(int)current.data.position.Y,
									strength * genRand.NextFloat(0.9f, 1.1f),
									current.data.velocity.RotatedBy(0.4f + genRand.NextFloat(-0.2f, 1)),
									genRand.NextFloat(distance * 0.8f, distance * 1.2f),
									stoneID,
									wallThickness,
									wallType: stoneWallID,
									oreType: oreID
								));
							airCheckVec = next.data.position;
							if (airCheckVec.Y < Main.worldSurface && Main.tile[(int)airCheckVec.X, (int)airCheckVec.Y].WallType == WallID.None) {
								OriginSystem.EvilSpikeAvoidancePoints.Add(new((int)airCheckVec.X, (int)airCheckVec.Y));
								break;
							}
							if (endChance > current.generation) {
								veins.Enqueue(next);
							}
							nodes.Add(next.data.position);
							break;
						}
						case 3: {//vein & cave
							next = (current.generation + 2,
								DefiledVeinRunner(
									(int)current.data.position.X,
									(int)current.data.position.Y,
									strength * genRand.NextFloat(0.9f, 1.1f),
									current.data.velocity.RotatedBy(genRand.NextBool() ? genRand.NextFloat(-0.4f, -0.2f) : genRand.NextFloat(0.2f, 0.4f)),
									genRand.NextFloat(distance * 0.8f, distance * 1.2f),
									stoneID,
									wallThickness,
									wallType: stoneWallID,
									oreType: oreID
								));
							airCheckVec = next.data.position;
							if (airCheckVec.Y < Main.worldSurface && Main.tile[(int)airCheckVec.X, (int)airCheckVec.Y].WallType == WallID.None) {
								OriginSystem.EvilSpikeAvoidancePoints.Add(new(airCheckVec.X, airCheckVec.Y));
								break;
							}
							if (endChance > next.generation) {
								veins.Enqueue(next);
							}
							float size = genRand.NextFloat(0.3f, 0.4f);
							if (airCheckVec.Y >= Main.worldSurface) {
								DefiledCave(next.data.position.X, next.data.position.Y, size);
							}
							DefiledRib(next.data.position.X, next.data.position.Y, size * caveSize, 0.75f);
							fissureCheckSpots.Add(next.data.position);
							nodes.Add(next.data.position);
							break;
						}
					}
				}
				for (int k = 0; k < ends.Count; k++) {
					bool canOpen = false;
					int checkX = (int)ends[k].X;
					int checkY = (int)ends[k].Y;
					int l = 0;
					for (; l < 20; l++) {
						if (Framing.GetTileSafely(checkX, --checkY).HasFullSolidTile()) break;
					}
					if (l >= 20) continue;
					l = 0;
					for (; l < 40 && !canOpen; l++) canOpen = Framing.GetTileSafely(checkX, --checkY).HasFullSolidTile();
					if (canOpen) {
						DefiledVeinRunner(
							checkX,
							checkY,
							strength * genRand.NextFloat(0.9f, 1.1f),
							-Vector2.UnitY.RotatedBy(genRand.NextFloat(0.1f, 0.2f) * genRand.NextBool().ToDirectionInt()),
							Math.Max(l, genRand.NextFloat(distance * 0.8f, distance * 1.2f)),
							stoneID,
							wallThickness,
							wallType: stoneWallID
						);
					}
				}
				ushort fissureID = (ushort)ModContent.TileType<Defiled_Relay>();
				while (fisureCount < 6 && fissureCheckSpots.Count > 0) {
					int ch = genRand.Next(fissureCheckSpots.Count);
					for (int o = 0; o > -5; o = o > 0 ? -o : -o + 1) {
						Point p = fissureCheckSpots[ch].ToPoint();
						int loop = 0;
						for (; !Main.tile[p.X + o - 1, p.Y + 1].HasTile || !Main.tile[p.X + o, p.Y + 1].HasTile; p.Y++) {
							if (++loop > 10) {
								break;
							}
						}
						WorldGen.KillTile(p.X + o - 1, p.Y - 1);
						WorldGen.KillTile(p.X + o, p.Y - 1);
						WorldGen.KillTile(p.X + o - 1, p.Y);
						WorldGen.KillTile(p.X + o, p.Y);
						WorldGen.PlaceTile(p.X + o - 1, p.Y + 1, stoneID);
						WorldGen.PlaceTile(p.X + o, p.Y + 1, stoneID);
						WorldGen.SlopeTile(p.X + o - 1, p.Y + 1, SlopeID.None);
						WorldGen.SlopeTile(p.X + o, p.Y + 1, SlopeID.None);
						if (TileExtenstions.CanActuallyPlace(p.X + o, p.Y, fissureID, 0, 0, out TileObject to, checkStay: true)) {
							TileObject.Place(to);
							//WorldGen.Place2x2(p.X + o, p.Y, fissureID, 0);
							fisureCount++;
							break;
						}
					}
					fissureCheckSpots.RemoveAt(ch);
				}
				const float max_ravel_dist = 150;
				Vector2 center = new(i * 16 + 8, j * 16 + 8);
				for (int k = nodes.Count; k-->0;) {
					Vector2 node = nodes[k].ToWorldCoordinates();
					nodes.RemoveAt(k);
					if (nodes.Count == 0) continue;
					Vector2 dirToCenter = node.DirectionTo(center);
					int tries = 10;
					int target;
					Vector2 targetNode;
					do {
						target = genRand.Next(nodes.Count);
						targetNode = nodes[target].ToWorldCoordinates();
					} while ((!node.IsWithin(targetNode, max_ravel_dist * 16) || Vector2.Dot(dirToCenter, targetNode.DirectionTo(center)) < 0) && tries-- > 0);
					nodes.RemoveAt(target);
					k--;
					if (tries > 0) RavelConnection(node, targetNode);
				}
				Rectangle genRange = WorldBiomeGeneration.ChangeRange.GetRange();
				ushort defiledAltar = (ushort)ModContent.TileType<Defiled_Altar>();
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
								tooHigh = TileLoader.GetTile(_tile.TileType) is not IDefiledTile;
								break;
							}
						}
						if (tooHigh) continue;
						Place3x2(x, y, defiledAltar);
						placed = Framing.GetTileSafely(x, y).TileIsType(defiledAltar);
					}
				}
				ushort defiledLargePile = (ushort)ModContent.TileType<Defiled_Large_Foliage>();
				for (int i0 = genRand.Next(100, 150); i0-- > 0;) {
					int tries = 18;
					int x = genRange.X + genRand.Next(0, genRange.Width);
					int y = genRange.Y + genRand.Next(0, genRange.Height) - 1;
					while (!PlaceObject(x, y, defiledLargePile)) {
						y--;
						if (tries-->0) break;
					}
				}
				ushort defiledMediumPile = (ushort)ModContent.TileType<Defiled_Medium_Foliage>();
				for (int i0 = genRand.Next(100, 150); i0-- > 0;) {
					int tries = 18;
					int x = genRange.X + genRand.Next(0, genRange.Width);
					int y = genRange.Y + genRand.Next(0, genRange.Height) - 1;
					while (!PlaceObject(x, y, defiledMediumPile)) {
						y--;
						if (tries-->0) break;
					}
				}
				/*ushort defiledPot = (ushort)ModContent.TileType<Defiled_Pot>();
				int placedPots = 0;
				for (int i0 = genRand.Next(100, 150); i0-- > 0;) {
					int x = (int)i + genRand.Next(-100, 101);
					int y = (int)j + genRand.Next(-80, 81);
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
					Place3x2(x, y, defiledPot);
					if (Framing.GetTileSafely(x, y).TileIsType(defiledPot)) placedPots++;
				}
				Origins.instance.Logger.Info($"Placed {placedPots} defiled pots");
				Origins.instance.Logger.Info($"Generated {{$Defiled_Wastelands}} with {fisureCount} fissures");*/
				//Main.NewText($"Generated Defiled Wastelands with {fisureCount} fissures");
			}
			public static void DefiledCave(float i, float j, float sizeMult = 1f) {
				ushort stoneID = (ushort)ModContent.TileType<Defiled_Stone>();
				ushort stoneWallID = (ushort)ModContent.WallType<Defiled_Stone_Wall>();
				for (int x = (int)Math.Floor(i - (28 * sizeMult + 5)); x < (int)Math.Ceiling(i + (28 * sizeMult + 5)); x++) {
					for (int y = (int)Math.Ceiling(j + (28 * sizeMult + 4)); y >= (int)Math.Floor(j - (28 * sizeMult + 4)); y--) {
						if (Main.tile[x, y].HasTile && !WorldGen.CanKillTile(x, y)) continue;
						float diff = (float)Math.Sqrt((((y - j) * (y - j)) + (x - i) * (x - i)) * (GenRunners.GetWallDistOffset((float)Math.Atan2(y - j, x - i) * 4 + x + y) * 0.0316076058772687986171132238548f + 1));
						if (diff > 35 * sizeMult) {
							continue;
						}
						if (Main.tile[x, y].WallType != stoneWallID) {
							Main.tile[x, y].ResetToType(stoneID);
						}
						Main.tile[x, y].WallType = stoneWallID;
						if (diff < 35 * sizeMult - 5) {
							Tile tile0 = Main.tile[x, y];
							tile0.HasTile = false;
						}
						WorldBiomeGeneration.ChangeRange.AddChangeToRange(x, y);
					}
				}
			}
			public static void DefiledRibs(float i, float j, float sizeMult = 1f) {
				ushort stoneID = (ushort)ModContent.TileType<Defiled_Regolith>();
				for (int x = (int)Math.Floor(i - (28 * sizeMult + 5)); x < (int)Math.Ceiling(i + (28 * sizeMult + 5)); x++) {
					for (int y = (int)Math.Ceiling(j + (28 * sizeMult + 4)); y >= (int)Math.Floor(j - (28 * sizeMult + 4)); y--) {
						float diff = (float)Math.Sqrt((((y - j) * (y - j)) + (x - i) * (x - i)) * (GenRunners.GetWallDistOffset((float)Math.Atan2(y - j, x - i) * 4 + x + y) * 0.0316076058772687986171132238548f + 1));
						if (diff > 16 * sizeMult) {
							continue;
						}
						if (Main.tile[x, y].HasTile && !WorldGen.CanKillTile(x, y)) continue;
						if (Math.Cos(diff * 0.7f) <= 0.1f) {
							Main.tile[x, y].ResetToType(stoneID);
						} else {
							Tile tile0 = Main.tile[x, y];
							tile0.HasTile = false;
						}
					}
				}
			}
			public static void DefiledRib(float i, float j, float size = 16f, float thickness = 1) {
				ushort stoneID = (ushort)ModContent.TileType<Defiled_Stone>();
				for (int x = (int)Math.Floor(i - size); x < (int)Math.Ceiling(i + size); x++) {
					for (int y = (int)Math.Ceiling(j + size); y >= (int)Math.Floor(j - size); y--) {
						if (Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType]) {
							continue;
						}
						if (Main.tile[x, y].HasTile && !WorldGen.CanKillTile(x, y)) continue;
						float diff = (float)Math.Sqrt((((y - j) * (y - j)) + (x - i) * (x - i)) * (GenRunners.GetWallDistOffset((float)Math.Atan2(y - j, x - i) * 4 + x + y) * 0.0316076058772687986171132238548f + 1));
						if (diff > size + thickness || diff < size - thickness) {
							continue;
						}
						Main.tile[x, y].ResetToType(stoneID);
					}
				}
			}
			public static (Vector2 position, Vector2 velocity) DefiledVeinRunner(int i, int j, double strength, Vector2 speed, double length, ushort wallBlockType, float wallThickness, float twist = 0, bool randomtwist = false, int wallType = -1, int oreType = -1, int oreRarity = 500) {
				Vector2 pos = new Vector2(i, j);
				Tile tile;
				if (randomtwist) twist = Math.Abs(twist);
				int X0 = int.MaxValue;
				int X1 = 0;
				int Y0 = int.MaxValue;
				int Y1 = 0;
				double baseStrength = strength;
				strength = Math.Pow(strength, 2);
				float basewallThickness = wallThickness;
				wallThickness *= wallThickness;
				double decay = speed.Length();
				Vector2 direction = speed / (float)decay;
				bool hasWall = wallType != -1;
				ushort _wallType = hasWall ? (ushort)wallType : (ushort)0;
				Dictionary<ushort, bool> dirtWalls = new() {
					[WallID.DirtUnsafe] = true,
					[WallID.MudUnsafe] = true
				};
				while (length > 0) {
					length -= decay;
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
					for (int l = minX; l < maxX; l++) {
						for (int k = minY; k < maxY; k++) {
							float el = l + (GenRunners.GetWallDistOffset((float)length + k) + 0.5f) / 2.5f;
							float ek = k + (GenRunners.GetWallDistOffset((float)length + l) + 0.5f) / 2.5f;
							double dist = Math.Pow(Math.Abs(el - pos.X), 2) + Math.Pow(Math.Abs(ek - pos.Y), 2);
							tile = Main.tile[l, k];
							if (Main.tileDungeon[tile.TileType]) {
								return (pos, speed);
							}
							bool openAir = (k < Main.worldSurface && tile.WallType == WallID.None);
							if (dist > strength) {
								double d = Math.Sqrt(dist);
								if (!openAir && d < baseStrength + basewallThickness && OriginExtensions.IsTileReplacable(l, k) && tile.WallType != _wallType) {

									if (!WorldGen.IsAContainer(tile)) {
										tile.HasTile = true;
										tile.ResetToType(wallBlockType);
										if (oreType != -1 && genRand.NextBool(oreRarity)) OreRunner(l, k, genRand.Next(2, 6), genRand.Next(3, 7), (ushort)oreType);
									}
									//WorldGen.SquareTileFrame(l, k);
									if (hasWall) {
										if (tile.WallType == WallID.DirtUnsafe || tile.WallType == WallID.MudUnsafe) {
											OriginExtensions.SpreadWall(l, k, _wallType, dirtWalls);
										}
										tile.WallType = _wallType;
									}
								}
								continue;
							}
							if (TileID.Sets.CanBeClearedDuringGeneration[tile.TileType]) {
								if (!Main.tileContainer[tile.TileType] && !Main.tileContainer[Main.tile[l, k - 1].TileType]) {
									Tile tile0 = Main.tile[l, k];
									tile0.HasTile = false;
								}
								//WorldGen.SquareTileFrame(l, k);
								if (hasWall && !openAir) {
									tile.WallType = _wallType;
								}
								if (l > X1) X1 = l;
								if (l < X0) X0 = l;
								if (k > Y1) Y1 = k;
								if (k < Y0) Y0 = k;
							}
						}
					}
					pos += speed;
					if (randomtwist || twist != 0.0) {
						speed = randomtwist ? speed.RotatedBy(genRand.NextFloat(-twist, twist)) : speed.RotatedBy(twist);
					}
					if (direction.X > 0) {

					}
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
				RangeFrame(X0, Y0, X1, Y1);
				NetMessage.SendTileSquare(Main.myPlayer, X0, Y0, X1 - X0, Y1 - Y0);
				WorldBiomeGeneration.ChangeRange.AddChangeToRange(X0, Y0);
				WorldBiomeGeneration.ChangeRange.AddChangeToRange(X1, Y1);
				return (pos, speed);
			}
			static int index = 0;
			public static void RavelConnection(Vector2 start, Vector2 end) {
				Vector2 direction = end - start;
				float distance = direction.Length();
				direction /= distance;
				float wallDistA = CollisionExt.Raymarch(start, direction, distance);
				if (wallDistA == distance) return;
				start += (wallDistA - 1) * direction;
				end -= CollisionExt.Raymarch(end, -direction, distance) * direction;
				Point tilePos = start.ToTileCoordinates();
				Vector2 tileSubPos = (start - tilePos.ToWorldCoordinates(0, 0)) / 16;
				static void DoLoopyThing(float currentSubPos, out float newSubPos, int currentTilePos, out int newTilePos, double direction) {
					newTilePos = currentTilePos;
					if (currentSubPos == 0 && direction < 0) {
						newSubPos = 1;
						newTilePos--;
					} else if (currentSubPos == 1 && direction > 0) {
						newSubPos = 0;
						newTilePos++;
					} else {
						newSubPos = currentSubPos;
					}
				}
				ushort stoneID = (ushort)ModContent.TileType<Defiled_Stone>();
				ushort stoneWallID = (ushort)ModContent.WallType<Defiled_Stone_Wall>();
				bool removedAnyTiles;
				void Stamp() {
					for (int x = -2; x <= 2; x++) {
						for (int y = -2; y <= 2; y++) {
							Tile tile = Framing.GetTileSafely(tilePos.X + x, tilePos.Y + y);
							if (x != 0 || y != 0) {
								if (tile.WallType != stoneWallID) tile.ResetToType(stoneID);
							} else if (CanKillTile(tilePos.X + x, tilePos.Y + y)) {
								tile.HasTile = false;
								removedAnyTiles = true;
								GenRunners.AutoSlope(tilePos.X + x + 1, tilePos.Y + y, true);
								GenRunners.AutoSlope(tilePos.X + x - 1, tilePos.Y + y, true);
								GenRunners.AutoSlope(tilePos.X + x, tilePos.Y + y + 1, true);
								GenRunners.AutoSlope(tilePos.X + x, tilePos.Y + y - 1, true);
							}
							tile.WallType = stoneWallID;
						}
					}
				}
				if (RavelStep(tileSubPos, direction) == tileSubPos) {
					DoLoopyThing(tileSubPos.X, out tileSubPos.X, tilePos.X, out tilePos.X, direction.X);
					DoLoopyThing(tileSubPos.Y, out tileSubPos.Y, tilePos.Y, out tilePos.Y, direction.Y);
				}
				while (tilePos.ToWorldCoordinates().DistanceSQ(end) > 16 * 16) {
					Vector2 next = RavelStep(tileSubPos, direction);
					if (next == tileSubPos) break;
					removedAnyTiles = false;
					DoLoopyThing(next.Y, out next.Y, tilePos.Y, out tilePos.Y, direction.Y);
					Stamp();
					DoLoopyThing(next.X, out next.X, tilePos.X, out tilePos.X, direction.X);
					Stamp();
					Tile? obstructionTileX = null;
					Tile? obstructionTileY = null;
					switch (Math.Sign(direction.X)) {
						case -1:
						obstructionTileX = Framing.GetTileSafely(tilePos.X + 1, tilePos.Y);
						break;
						case 1:
						obstructionTileX = Framing.GetTileSafely(tilePos.X - 1, tilePos.Y);
						break;
					}
					switch (Math.Sign(direction.Y)) {
						case -1:
						obstructionTileY = Framing.GetTileSafely(tilePos.X, tilePos.Y + 1);
						break;
						case 1:
						obstructionTileY = Framing.GetTileSafely(tilePos.X, tilePos.Y - 1);
						break;
					}
					if ((obstructionTileX?.HasTile ?? false) && (obstructionTileY?.HasTile ?? false)) {
						(WorldGen.genRand.NextBool() ? obstructionTileX : obstructionTileY).Value.ClearTile();
					}
					if (!removedAnyTiles || !InWorld(tilePos.X, tilePos.Y)) break;
					tileSubPos = next;
				}
			}
			static Vector2 RavelStep(Vector2 pos, Vector2 direction) {
				if (direction.X == 0) return new(pos.X, direction.Y > 0 ? 1 : 0);
				if (direction.Y == 0) return new(direction.X > 0 ? 1 : 0, pos.Y);
				double slope = direction.Y / direction.X;
				int xVlaue = direction.X > 0 ? 1 : 0;
				double yIntercept = pos.Y - slope * (pos.X - xVlaue);
				if (yIntercept >= 0 && yIntercept <= 1) return new Vector2(xVlaue, (float)yIntercept);
				int yVlaue = direction.Y > 0 ? 1 : 0;
				double xIntercept = (pos.Y - yVlaue) / -slope + pos.X;
				return new Vector2((float)xIntercept, yVlaue);
			}
		}

		public static void CheckFissure(int i, int j, int type) {
			if (Main.netMode != NetmodeID.MultiplayerClient && !WorldGen.noTileActions) {
				float fx = i * 16;
				float fy = j * 16;

				float distance = -1f;
				int player = 0;
				for (int playerIndex = 0; playerIndex < 255; playerIndex++) {
					float currentDist = Math.Abs(Main.player[playerIndex].position.X - fx) + Math.Abs(Main.player[playerIndex].position.Y - fy);
					if (currentDist < distance || distance == -1f) {
						player = playerIndex;
						distance = currentDist;
					}
				}

				DropAttemptInfo dropInfo = default(DropAttemptInfo);
				dropInfo.player = Main.player[player];
				dropInfo.IsExpertMode = Main.expertMode;
				dropInfo.IsMasterMode = Main.masterMode;
				dropInfo.IsInSimulation = false;
				dropInfo.rng = Main.rand;
				Origins.ResolveRuleWithHandler(shadowOrbSmashed ? FissureDropRule : FirstFissureDropRule, dropInfo, (DropAttemptInfo info, int item, int stack, bool _) => {
					Item.NewItem(GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 32, 32, item, stack, pfix: -1);
				});
				shadowOrbSmashed = true;

				//this projectile handles the rest
				Projectile.NewProjectile(GetItemSource_FromTileBreak(i, j), new Vector2((i + 1) * 16, (j + 1) * 16), Vector2.Zero, ModContent.ProjectileType<Defiled_Wastelands_Signal>(), 0, 0, Main.myPlayer, ai0: 1, ai1: player);

				AchievementsHelper.NotifyProgressionEvent(7);
			}
			SoundEngine.PlaySound(Origins.Sounds.DefiledKill, new Vector2(i * 16, j * 16));
			destroyObject = false;
		}
	}
	#region variations
	public class Underground_Defiled_Wastelands_Biome : ModBiome {
		public override int Music => Origins.Music.UndergroundDefiled;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BackgroundPath => "Origins/UI/MapBGs/Defiled_Wastelands_Normal";
		public override string BestiaryIcon => "Origins/UI/IconStonerDefiled";
		public override string MapBackground => BackgroundPath;
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneDefiledProgress * 0.99f;
		}
		public override bool IsBiomeActive(Player player) {
			return player.ZoneRockLayerHeight && player.InModBiome<Defiled_Wastelands>();
		}
	}
	public class Defiled_Wastelands_Desert : ModBiome {
		public override int Music => Origins.Music.Defiled;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BackgroundPath => "Origins/UI/MapBGs/Defiled_Wastelands_Desert";
		public override string BestiaryIcon => "Origins/UI/IconDesertDefiled";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneDesert && player.InModBiome<Defiled_Wastelands>();
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneDefiledProgress * 0.99f;
		}
	}
	public class Defiled_Wastelands_Underground_Desert : ModBiome {
		public override int Music => Origins.Music.UndergroundDefiled;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BackgroundPath => "Origins/UI/MapBGs/Defiled_Wastelands_Desert";
		public override string BestiaryIcon => "Origins/UI/IconCatacombsDefiled";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneRockLayerHeight && player.ZoneDesert && player.InModBiome<Defiled_Wastelands>();
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneDefiledProgress;
		}
	}
	public class Defiled_Wastelands_Ice_Biome : ModBiome {
		public override int Music => Origins.Music.UndergroundDefiled;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BackgroundPath => "Origins/UI/MapBGs/Defiled_Snow";
		public override string BestiaryIcon => "Origins/UI/IconSnowDefiled";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneRockLayerHeight && player.ZoneSnow && player.InModBiome<Defiled_Wastelands>();
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneDefiledProgress;
		}
	}
	public class Defiled_Wastelands_Ocean : ModBiome {
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BackgroundPath => "Origins/UI/MapBGs/Defiled_Ocean";
		public override string BestiaryIcon => "Origins/UI/WorldGen/IconDefiledOcean";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneBeach && player.InModBiome<Defiled_Wastelands>();
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneDefiledProgress * 0.99f;
		}
	}
	#endregion variations
	public class Defiled_Wastelands_Alt_Biome : AltBiome, IItemObtainabilityProvider {
		public override string WorldIcon => "Origins/UI/WorldGen/IconDefiled";
		public override string OuterTexture => "Origins/UI/WorldGen/Outer_Defiled";
		public override string IconSmall => "Origins/UI/WorldGen/IconEvilDefiled";
		public override Color OuterColor => new(170, 170, 170);
		public override Color? BiomeSightColor => Color.White;
		public override IShoppingBiome Biome => ModContent.GetInstance<Defiled_Wastelands>();
		public override void SetStaticDefaults() {
			BiomeType = BiomeType.Evil;
			//DisplayName.SetDefault(Language.GetTextValue("{$Defiled_Wastelands}"));
			//Description.SetDefault(Language.GetTextValue("A desaturated and bleak environment that is actually a living organism growing its body."));
			//GenPassName.SetDefault(Language.GetTextValue("{$Mods.Origins.Generic.Riven_Hive}"));
			//GenPassName.SetDefault(Language.GetTextValue("{$Defiled_Wastelands}"));
			AddTileConversion(ModContent.TileType<Defiled_Grass>(), TileID.Grass);
			AddTileConversion(ModContent.TileType<Defiled_Jungle_Grass>(), TileID.JungleGrass);
			AddTileConversion(ModContent.TileType<Defiled_Stone>(), TileID.Stone);
			AddTileConversion(ModContent.TileType<Defiled_Sand>(), TileID.Sand);
			AddTileConversion(ModContent.TileType<Defiled_Sandstone>(), TileID.Sandstone);
			AddTileConversion(ModContent.TileType<Hardened_Defiled_Sand>(), TileID.HardenedSand);
			AddTileConversion(ModContent.TileType<Defiled_Ice>(), TileID.IceBlock);

			GERunnerConversion.Add(TileID.Silt, ModContent.TileType<Defiled_Sand>());

			BiomeFlesh = TileID.SilverBrick;
			BiomeFleshWall = WallID.SilverBrick;

			FleshDoorTile = TileID.ClosedDoor;
			FleshChairTile = TileID.Chairs;
			FleshTableTile = TileID.Tables;
			FleshChestTile = TileID.Containers;
			FleshDoorTileStyle = 7;
			FleshChairTileStyle = 7;
			FleshTableTileStyle = 7;
			FleshChestTileStyle = 7;

			FountainTile = TileID.WaterFountain;
			FountainTileStyle = 1;
			//FountainTile = TileID.WaterFountain;
			//Fountain = TileID.WaterFountain;

			SeedType = ModContent.ItemType<Defiled_Grass_Seeds>();
			BiomeOre = ModContent.TileType<Lost_Ore>();
			BiomeOreItem = ModContent.ItemType<Lost_Ore_Item>();
			BiomeOreBrick = ModContent.TileType<Lost_Brick>();
			AltarTile = ModContent.TileType<Defiled_Altar>();

			BiomeChestItem = ModContent.ItemType<Missing_File>();
			BiomeChestTile = ModContent.TileType<Defiled_Dungeon_Chest>();
			BiomeChestTileStyle = 1;
			BiomeKeyItem = ModContent.ItemType<Defiled_Key>();

			MimicType = ModContent.NPCType<Defiled_Mimic>();

			BloodBunny = ModContent.NPCType<Defiled_Mite>();
			BloodPenguin = ModContent.NPCType<Bile_Thrower>();
			BloodGoldfish = ModContent.NPCType<Shattered_Goldfish>();

			AddWallConversions<Defiled_Stone_Wall>(
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
			AddWallConversions<Defiled_Sandstone_Wall>(
				WallID.Sandstone,
				WallID.CorruptSandstone,
				WallID.CrimsonSandstone,
				WallID.HallowSandstone
			);
			AddWallConversions<Hardened_Defiled_Sand_Wall>(
				WallID.HardenedSand,
				WallID.CorruptHardenedSand,
				WallID.CrimsonHardenedSand,
				WallID.HallowHardenedSand
			);
			AddWallConversions(OriginsWall.GetWallID<Defiled_Grass_Wall>(WallVersion.Natural),
				WallID.GrassUnsafe,
				WallID.Grass
			);
			this.AddChambersiteConversions(ModContent.TileType<Chambersite_Ore_Defiled_Stone>(), ModContent.WallType<Chambersite_Defiled_Stone_Wall>());

			EvilBiomeGenerationPass = new Defiled_Wastelands_Generation_Pass();
		}

		public IEnumerable<int> ProvideItemObtainability() {
			yield return BiomeChestItem.Value;
		}

		public override AltMaterialContext MaterialContext {
			get {
				AltMaterialContext context = new AltMaterialContext();
				context.SetEvilHerb(ModContent.ItemType<Wilting_Rose_Item>());
				context.SetEvilBar(ModContent.ItemType<Defiled_Bar>());
				context.SetEvilOre(ModContent.ItemType<Lost_Ore_Item>());
				context.SetVileInnard(ModContent.ItemType<Strange_String>());
				context.SetVileComponent(ModContent.ItemType<Black_Bile>());
				context.SetEvilBossDrop(ModContent.ItemType<Undead_Chunk>());
				context.SetEvilSword(ModContent.ItemType<Spiker_Sword>());
				return context;
			}
		}
		public static List<int> defiledWastelandsWestEdge;
		public static List<int> defiledWastelandsEastEdge;
		public class Defiled_Wastelands_Generation_Pass : EvilBiomeGenerationPass {
			Stack<Point> defiledHearts = new();
			public override string ProgressMessage => Language.GetTextValue("Mods.Origins.AltBiomes.Defiled_Wastelands_Alt_Biome.GenPassName");
			int startY;
			public override void GetEvilSpawnLocation(int dungeonSide, int dungeonLocation, int SnowBoundMinX, int SnowBoundMaxX, int JungleBoundMinX, int JungleBoundMaxX, int currentDrunkIter, int maxDrunkBorders, out int evilBiomePosition, out int evilBiomePositionWestBound, out int evilBiomePositionEastBound) {
				base.GetEvilSpawnLocation(dungeonSide, dungeonLocation, SnowBoundMinX, SnowBoundMaxX, JungleBoundMinX, JungleBoundMaxX, currentDrunkIter, maxDrunkBorders, out evilBiomePosition, out evilBiomePositionWestBound, out evilBiomePositionEastBound);
				int startEvilBiomePosition = evilBiomePosition;
				int offset = 0;
				bool first = true;
				do {
					if (!first) {
						offset = offset > 0 ? (-offset) : ((-offset) + 2);
					} else {
						first = false;
					}
					evilBiomePosition = startEvilBiomePosition + offset;
					for (startY = (int)GenVars.worldSurfaceLow; !Framing.GetTileSafely(evilBiomePosition, startY).HasTile; startY++) ;
				} while (startY > Main.maxTilesY);
				evilBiomePositionWestBound += offset;
				evilBiomePositionEastBound += offset;
			}
			public override void GenerateEvil(int evilBiomePosition, int evilBiomePositionWestBound, int evilBiomePositionEastBound) {
				defiledWastelandsWestEdge ??= [];
				defiledWastelandsEastEdge ??= [];
				defiledWastelandsWestEdge.Add(evilBiomePositionWestBound);
				defiledWastelandsEastEdge.Add(evilBiomePositionEastBound);
				WorldBiomeGeneration.ChangeRange.ResetRange();
				Point start = new(evilBiomePosition, startY + genRand.Next(105, 150));//range of depths

				Defiled_Wastelands.Gen.StartDefiled(start.X, start.Y);
				defiledHearts.Push(start);

				int minY = WorldBiomeGeneration.ChangeRange.GetRange().Top;
				WorldBiomeGeneration.ChangeRange.AddChangeToRange(evilBiomePositionWestBound, minY);
				WorldBiomeGeneration.ChangeRange.AddChangeToRange(evilBiomePositionEastBound, minY);
				Rectangle range = WorldBiomeGeneration.ChangeRange.GetRange();
				WorldBiomeGeneration.EvilBiomeGenRanges.Add(range);
				AltBiome biome = ModContent.GetInstance<Defiled_Wastelands_Alt_Biome>();
				for (int i = range.Left; i < range.Right; i++) {
					int slopeFactor = Math.Min(Math.Min(i - range.Left, range.Right - i), 99);
					for (int j = range.Top - 10; j < range.Bottom; j++) {
						if (genRand.NextBool(5) && genRand.Next(slopeFactor, 100) < 20) continue;
						if (range.Bottom - j < 5 && genRand.NextBool(5)) break;
						Tile tile = Framing.GetTileSafely(i, j);
						if (tile.HasTile) {
							ALConvert.ConvertTile(i, j, biome);
							ALConvert.ConvertWall(i, j, biome);
						}
					}
				}

				OriginSystem.Instance.hasDefiled = true;
			}

			public override void PostGenerateEvil() {
				Point heart;
				while (defiledHearts.Count > 0) {
					heart = defiledHearts.Pop();
					Defiled_Wastelands.Gen.DefiledRibs(heart.X + genRand.NextFloat(-0.5f, 0.5f), heart.Y + genRand.NextFloat(-0.5f, 0.5f));
					for (int i = heart.X - 1; i < heart.X + 3; i++) {
						for (int j = heart.Y - 2; j < heart.Y + 2; j++) {
							Main.tile[i, j].SetActive(false);
						}
					}
					TileExtenstions.ForcePlace(heart.X, heart.Y, (ushort)ModContent.TileType<Defiled_Heart>(), 0, 1);
					ModContent.GetInstance<Defiled_Heart_TE_System>().AddTileEntity(new(heart.X, heart.Y));
				}
			}
		}
		public class Defiled_Wastelands_Fishing_Pool : FishingLootPool<Defiled_Wastelands_Alt_Biome> {
			public override void SetStaticDefaults() {
				AddCrates(ItemType<Chunky_Crate>(), ItemType<Bilious_Crate>());
				Legendary.Add(new SequentialCatches(
					FishingCatch.Item(ItemID.ScalyTruffle, (player, attempt) => Main.hardMode && player.ZoneSnow && attempt.heightLevel == 3 && !Main.rand.NextBool(3)),
					FishingCatch.Item(ItemType<Knee_Slapper>(), (player, attempt) => Main.hardMode && Main.rand.NextBool(2))
				));
				Rare.Add(FishingCatch.Item(ItemType<Manasynk>()));
				Uncommon.Add(new SequentialCatches(
					FishingCatch.QuestFish(ItemType<Prikish>()),
					FishingCatch.Item(ItemType<Bilemouth>())
				));
			}
		}
	}
}
