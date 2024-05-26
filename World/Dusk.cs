using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using static Origins.OriginExtensions;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Origins.World.BiomeData {
	public class Dusk : ModBiome {
		public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
		public override int Music => Origins.Music.Dusk;
		public override bool IsBiomeActive(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.ZoneVoid = OriginSystem.voidTiles > 300;
			originPlayer.ZoneVoidProgress = Math.Clamp(OriginSystem.voidTiles - 200, 0, 200) / 300f;
			LinearSmoothing(ref originPlayer.ZoneVoidProgressSmoothed, originPlayer.ZoneVoidProgress, OriginSystem.biomeShaderSmoothing);
			return originPlayer.ZoneVoid;
		}
		public override void SpecialVisuals(Player player, bool isActive) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			Filters.Scene["Origins:ZoneDusk"].GetShader().UseProgress(originPlayer.ZoneVoidProgressSmoothed);
			player.ManageSpecialBiomeVisuals("Origins:ZoneDusk", originPlayer.ZoneVoidProgressSmoothed > 0, player.Center);
		}
		public class Gen {
			internal static int duskLeft;
			internal static int duskRight;
			internal static int duskTop;
			internal static int duskBottom;
			public static void HellRunner(int i, int j, double strength, int steps, int type, bool addTile = false, float speedX = 0f, float speedY = 0f, bool noYChange = false, bool overRide = true) {
				double num = strength;
				float num2 = steps;
				Vector2 vector = default;
				vector.X = i;
				vector.Y = j;
				Vector2 vector2 = default;
				vector2.X = (float)WorldGen.genRand.Next(-10, 11) * 0.1f;
				vector2.Y = (float)WorldGen.genRand.Next(-10, 11) * 0.1f;
				if (speedX != 0f || speedY != 0f) {
					vector2.X = speedX;
					vector2.Y = speedY;
				}
				while (num > 0.0 && num2 > 0f) {
					if (vector.Y < 0f && num2 > 0f && type == 59) {
						num2 = 0f;
					}
					num = strength * (double)(num2 / (float)steps);
					num2 -= 1f;
					duskLeft = (int)((double)vector.X - num * 0.5);
					duskRight = (int)((double)vector.X + num * 0.5);
					duskTop = (int)((double)vector.Y - num * 0.5);
					duskBottom = (int)((double)vector.Y + num * 0.5);
					if (duskLeft < 1) {
						duskLeft = 1;
					}
					if (duskRight > Main.maxTilesX - 1) {
						duskRight = Main.maxTilesX - 1;
					}
					if (duskTop < Main.maxTilesY - 200) {
						duskTop = Main.maxTilesY - 200;
					}
					if (duskBottom > Main.maxTilesY - 1) {
						duskBottom = Main.maxTilesY - 1;
					}
					int tilesSinceSpike = 0;
					Point? spike = null;
					for (int k = duskLeft; k < duskRight; k++) {
						spike = null;
						for (int l = duskBottom; l > duskTop; l--) {
							if (!((Math.Abs(k - vector.X) + Math.Abs(l - vector.Y)) < strength * 0.5)) {
								continue;
							}
							if (Main.tile[k, l].TileType == TileID.Pots) {
								Main.tile[k, l].TileType = 0;
								Main.tile[k, l].SetActive(false);
								continue;
							}
							if (Main.tile[k, l].TileType == type) {
								continue;
							}
							if (Main.tile[k, l].LiquidAmount != 0) {
								Tile tile = Main.tile[k, l];
								tile.TileType = (ushort)type;
								tile.HasTile = true;
								tile.LiquidAmount = 0;
								tile.LiquidType = 1;
								//WorldGen.paintTile(k, l, 29);
								Main.tile[k + 1, l].SetSlope(SlopeType.Solid);
								Main.tile[k - 1, l].SetSlope(SlopeType.Solid);
								if (l < Main.maxTilesY) Main.tile[k, l + 1].SetSlope(SlopeType.Solid);
								Main.tile[k, l - 1].SetSlope(SlopeType.Solid);
								if (Main.tile[k, l - 1].LiquidAmount != 0) continue;
								spike = new Point(k, l);
								continue;
							}
							if ((Main.tile[k, l].HasTile) || Main.tile[k, l].WallType == WallID.ObsidianBrickUnsafe || Main.tile[k, l].WallType == WallID.HellstoneBrickUnsafe) {
								Tile tile = Main.tile[k, l];
								if (TileID.Sets.CanBeClearedDuringGeneration[tile.TileType]) {
									if (tile.TileType == TileID.ObsidianBrick || tile.TileType == TileID.HellstoneBrick || tile.WallType != 0 || (!Main.tileSolid[tile.TileType] && tile.TileType != TileID.Containers && tile.TileType != TileID.Containers2)) {
										tile.WallType = 0;
										bool tileleft = Main.tile[k - 1, l].TileType == type && Main.tile[k - 1, l].HasTile;
										if (!tileleft && Main.tile[k, l - 1].TileType != TileID.Containers && Main.tile[k, l - 1].TileType != TileID.Containers2) {
											tile.TileType = 0;
											tile.HasTile = false;
											continue;
										}
									}
									tile.TileType = (ushort)type;
									tile.HasTile = true;
								}
							}
						}
						if (spike.HasValue) {
							int l = spike.Value.Y;
							if (WorldGen.genRand.Next(0, 10 + OriginSystem.HellSpikes.Count) <= tilesSinceSpike / 5) {
								Origins.instance.Logger.Info("Adding spike @ " + k + ", " + l);
								OriginSystem.HellSpikes.Add((new Point(k, l), WorldGen.genRand.Next(5, 10) + tilesSinceSpike / 5));
								//WorldGen.paintTile(k, l, 11);
								tilesSinceSpike = -7;
							} else {
								tilesSinceSpike++;
							}
						}
					}
					vector += vector2;
					if (num > 50.0) {
						vector += vector2;
						num2 -= 1f;
						vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
						vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
						if (num > 100.0) {
							vector += vector2;
							num2 -= 1f;
							vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
							vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
							if (num > 150.0) {
								vector += vector2;
								num2 -= 1f;
								vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
								vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
								if (num > 200.0) {
									vector += vector2;
									num2 -= 1f;
									vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
									vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
									if (num > 250.0) {
										vector += vector2;
										num2 -= 1f;
										vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
										vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
										if (num > 300.0) {
											vector += vector2;
											num2 -= 1f;
											vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
											vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
											if (num > 400.0) {
												vector += vector2;
												num2 -= 1f;
												vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
												vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
												if (num > 500.0) {
													vector += vector2;
													num2 -= 1f;
													vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
													vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
													if (num > 600.0) {
														vector += vector2;
														num2 -= 1f;
														vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
														vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
														if (num > 700.0) {
															vector += vector2;
															num2 -= 1f;
															vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
															vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
															if (num > 800.0) {
																vector += vector2;
																num2 -= 1f;
																vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
																vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
																if (num > 900.0) {
																	vector += vector2;
																	num2 -= 1f;
																	vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
																	vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
					vector2.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
					if (vector2.X > 1f) {
						vector2.X = 1f;
					}
					if (vector2.X < -1f) {
						vector2.X = -1f;
					}
					if (!noYChange) {
						vector2.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
						if (vector2.Y > 1f) {
							vector2.Y = 1f;
						}
						if (vector2.Y < -1f) {
							vector2.Y = -1f;
						}
					} else if (type != 59 && num < 3.0) {
						if (vector2.Y > 1f) {
							vector2.Y = 1f;
						}
						if (vector2.Y < -1f) {
							vector2.Y = -1f;
						}
					}
					if (type == 59 && !noYChange) {
						if ((double)vector2.Y > 0.5) {
							vector2.Y = 0.5f;
						}
						if ((double)vector2.Y < -0.5) {
							vector2.Y = -0.5f;
						}
						if ((double)vector.Y < Main.rockLayer + 100.0) {
							vector2.Y = 1f;
						}
						if (vector.Y > (float)(Main.maxTilesY - 300)) {
							vector2.Y = -1f;
						}
					}
				}
			}
		}
	}
}
