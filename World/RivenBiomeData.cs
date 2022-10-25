using Microsoft.Xna.Framework;
using Terraria.Graphics.Effects;
using Origins.Tiles.Riven;
using Origins.Walls;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Terraria.WorldGen;
using static Origins.OriginExtensions;
using Terraria.GameContent.ItemDropRules;
using Origins.Items.Weapons.Riven;
using Origins.Items.Other.Testing;
using Origins.Water;
using Origins.Backgrounds;
using Terraria.GameContent.Bestiary;
using AltLibrary.Common.AltBiomes;
using Origins.NPCs.Riven;
using AltLibrary.Core.Generation;

namespace Origins.World.BiomeData {
	public class Riven_Hive : ModBiome {
		public static IItemDropRule FirstLesionDropRule;
		public static IItemDropRule LesionDropRule;
		public override int Music => Origins.Music.Riven;
		public override ModWaterStyle WaterStyle => ModContent.GetInstance<Riven_Water_Style>();
		// turns out if a biome doesn't have a background it's treated as if its water style has no priority
		public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<Riven_Surface_Background>();
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public static ModBiomeBestiaryInfoElement BestiaryInfoElement => ModContent.GetInstance<Riven_Hive>().ModBiomeBestiaryInfoElement;
		public override bool IsBiomeActive(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.ZoneRiven = OriginSystem.rivenTiles > NeededTiles;
			originPlayer.ZoneRivenProgress = Math.Min(OriginSystem.rivenTiles - (NeededTiles - ShaderTileCount), ShaderTileCount) / ShaderTileCount;
			LinearSmoothing(ref originPlayer.ZoneRivenProgressSmoothed, originPlayer.ZoneRivenProgress, OriginSystem.biomeShaderSmoothing * 0.1f);
			
			return originPlayer.ZoneRiven;
		}
		public override void SpecialVisuals(Player player, bool isActive) {
			//OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			//Filters.Scene["Origins:ZoneRiven"].GetShader().UseProgress(originPlayer.ZoneRivenProgressSmoothed);
			//player.ManageSpecialBiomeVisuals("Origins:ZoneRiven", originPlayer.ZoneRivenProgressSmoothed > 0, player.Center);
		}
		public override void Load() {
			FirstLesionDropRule = ItemDropRule.NotScalingWithLuck(ItemID.TheUndertaker);// presumably Riven Splitter?
			FirstLesionDropRule.OnSuccess(ItemDropRule.NotScalingWithLuck(ItemID.MusketBall, 1, 100, 100));// presumably harpoons?

			LesionDropRule = new OneFromRulesRule(1,
				FirstLesionDropRule,
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<NPC_Spawner>(), 1, NPCID.Bee, NPCID.Bee),
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Riverang>()),
				ItemDropRule.NotScalingWithLuck(ItemID.ShadowOrb),
				ItemDropRule.NotScalingWithLuck(ItemID.PanicNecklace)
			);
		}
		public override void Unload() {
			FirstLesionDropRule = null;
			LesionDropRule = null;
		}
		public const int NeededTiles = 200;
		public const int ShaderTileCount = 25;
		public static class SpawnRates {
			public const float Fighter = 1;
			public const float Mummy = 1;
			public const float Moeba = 0.8f;
			public const float Tank = 0.6f;
			public const float Shark1 = 0.4f;
			public const float Worm = 0.6f;
			public const float Crawler = 0.8f;
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
											tile.HasTile = true;
											tile.TileType = fleshBlockType;
											tile.WallType = fleshWallType;
										} else if (tile.WallType != WallID.None) {
											tile.WallType = fleshWallType;
										}
									}

									if (d < baseStrength + 8 && genRand.Next(1500000 + (int)strength) < strength + 424) {
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
								if(compY > j || tile.WallType != WallID.None) tile.WallType = fleshWallType;
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
								if (genRand.Next(3000000) < strength + 924) {
									features.Add((x, y, FeatureType.CHUNK, false));
								}
							}
						}
					}
					pos += (Vector2)speed;
					if(OriginExtensions.LinearSmoothing(ref speed.Theta, targetTwist + MathHelper.PiOver2, 0.05f)) targetTwist = genRand.NextFloat(-0.5f, 0.5f);
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
							wallType:fleshWallType
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
				ushort weakFleshID = (ushort)ModContent.TileType<Weak_Riven_Flesh>();
				ushort fleshWallID = (ushort)ModContent.WallType<Riven_Flesh_Wall>();
				lesionCount = 0;
				int j2 = j;
				if (j2 > Main.worldSurface) {
					j2 = (int)Main.worldSurface;
				}
				for (; !SolidTile(i, j2); j2++) {}
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
                while (lesionPlacementSpots.Count>0) {
					Point current = lesionPlacementSpots.Dequeue();
					if (!Main.tile[current.X, current.Y].HasTile && !Main.tile[current.X, current.Y - 1].HasTile && Main.tile[current.X, current.Y + 1].HasTile) {
						if (!Main.tile[current.X - 1, current.Y].HasTile && !Main.tile[current.X - 1, current.Y - 1].HasTile && Main.tile[current.X - 1, current.Y + 1].HasTile) {
							validLesionPlacementSpots.Add(new Point(current.X - 1, current.Y));
						}
						if (!Main.tile[current.X + 1, current.Y].HasTile && !Main.tile[current.X + 1, current.Y - 1].HasTile && Main.tile[current.X + 1, current.Y + 1].HasTile) {
							validLesionPlacementSpots.Add(new Point(current.X, current.Y));
						}
					}
                }
                for (int index = 0; index < 4; index++) {
                    if (validLesionPlacementSpots.Count < 1 || lesionCount > 18) {
						break;
                    }
					Point current = genRand.Next(validLesionPlacementSpots);

					Place2x2(current.X, current.Y, lesionID, 0);

					lesionCount++;
					validLesionPlacementSpots.Remove(current.OffsetBy(-1));
					validLesionPlacementSpots.Remove(current);
					validLesionPlacementSpots.Remove(current.OffsetBy(1));
				}
				return new Point(i2, j2);
			}
		}
		public static void CheckLesion(int i, int j, int type) {
			if (destroyObject) {
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
					NPC.SpawnOnPlayer(plr, 1);
				} else {
					LocalizedText localizedText = Lang.misc[10];
					if (shadowOrbCount == 2) {
						localizedText = Lang.misc[11];
					}
					if (Main.netMode == NetmodeID.SinglePlayer) {
						Main.NewText(localizedText.ToString(), 50, byte.MaxValue, 130);
					}else if (Main.netMode == NetmodeID.Server) {
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
		public override bool IsBiomeActive(Player player) {
			return base.IsBiomeActive(player);
		}
	}
	public class Riven_Hive_Alt_Biome : AltBiome {
		public override string WorldIcon => "";//TODO: Redo tree icons for AltLib
		public override string OuterTexture => "Origins/UI/WorldGen/Outer_Riven";
		public override string IconSmall => "Origins/UI/WorldGen/IconEvilRiven";
		public override Color OuterColor => new(30, 176, 255);
		public override List<int> SpreadingTiles => new List<int> {
			ModContent.TileType<Riven_Flesh>()
		};
		public override void SetStaticDefaults() {
			BiomeType = AltLibrary.BiomeType.Evil;
			//DisplayName.SetDefault(Language.GetTextValue("Mods.Origins.Generic.Riven_Hive"));
			GenPassName.SetDefault(Language.GetTextValue("{$Mods.Origins.Generic.Riven_Hive}"));
			//BiomeGrass = ModContent.TileType<Riven_Grass>();
			BiomeStone = ModContent.TileType<Riven_Flesh>();
			//BiomeSand = ModContent.TileType<Defiled_Sand>();
			//BiomeSandstone = ModContent.TileType<Defiled_Sandstone>();
			//BiomeHardenedSand = ModContent.TileType<Hardened_Defiled_Sand>();
			//BiomeIce = ModContent.TileType<Riven_Ice>();
			BiomeOre = ModContent.TileType<Infested_Ore>();
			AltarTile = ModContent.TileType<Riven_Altar>();
			BiomeChestTile = ModContent.TileType<Riven_Dungeon_Chest>();
			MimicType = ModContent.NPCType<Riven_Mimic>();
		}
		public override EvilBiomeGenerationPass GetEvilBiomeGenerationPass() {
			return new Riven_Hive_Generation_Pass();
		}
		public class Riven_Hive_Generation_Pass : EvilBiomeGenerationPass {
			public override void GenerateEvil(int evilBiomePosition, int evilBiomePositionWestBound, int evilBiomePositionEastBound) {
				int y = (int)worldSurface - 100;
				for (; !Main.tile[evilBiomePosition, y].HasTile; y++);
				Riven_Hive.Gen.StartHive(evilBiomePosition, y);
			}

			public override void PostGenerateEvil() { }
		}
	}
}
