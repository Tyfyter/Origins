using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using static Origins.OriginExtensions;
using Microsoft.Xna.Framework;
using Terraria.ID;
using AltLibrary.Common.Systems;
using Terraria.WorldBuilding;
using Terraria.IO;
using Origins.Tiles.Dusk;
using System.Collections.Generic;
using Tyfyter.Utils;
using PegasusLib;
using Origins.Buffs;

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
			float progress = player.OriginPlayer().ZoneVoidProgressSmoothed - player.HasBuff<Voidsight_Buff>().Mul(0.2f);
			Filters.Scene["Origins:ZoneDusk"].GetShader().UseProgress(progress);
			player.ManageSpecialBiomeVisuals("Origins:ZoneDusk", progress != 0, player.Center);
		}
		public class Gen {
			internal static int duskLeft;
			internal static int duskRight;
			internal static int duskTop;
			internal static int duskBottom;
			internal static Rectangle duskRect;
			static Stack<(Point position, int size)> hellSpikes;
			public static void GenerateDusk() {
				hellSpikes = [];
				ushort duskStoneID = (ushort)ModContent.TileType<Dusk_Stone>();
				int X = (int)(Main.maxTilesX * (WorldGen.genRand.NextBool() ? 0.7 : 0.3));
				HellRunner(X, Main.maxTilesY - 25, 650, WorldGen.genRand.Next(100, 200), duskStoneID, 0f, 0f);
				//Mod.Logger.Info(HellSpikes.Count + " Void Spikes: " + string.Join(", ", HellSpikes));
				duskRect = WorldBiomeGeneration.ChangeRange.GetRange();
			}
			public static void FinishDusk(GenerationProgress progress, GameConfiguration _) {
				progress.Message = "Finishing Dusk";
				ushort duskStoneID = (ushort)ModContent.TileType<Dusk_Stone>();
				ushort duskStoneLiquidID = (ushort)ModContent.TileType<Dusk_Stone_Liquid>();
				ushort oreID = (ushort)ModContent.TileType<Bleeding_Obsidian>();
				for (int i = duskRect.Left; i < duskRect.Right; i++) {
					for (int j = duskRect.Bottom; j > duskRect.Top; j--) {
						Tile tile = Main.tile[i, j];
						if (tile.LiquidAmount != 0) {
							tile.TileType = duskStoneLiquidID;
							tile.HasTile = true;
							tile.LiquidAmount = 0;
							tile.LiquidType = 1;
							WorldGen.paintTile(i, j, PaintID.ShadowPaint);
							FillLavaGapsAndSlope(i + 1, j, duskStoneLiquidID);
							FillLavaGapsAndSlope(i - 1, j, duskStoneLiquidID);
							if (j < Main.maxTilesY) FillLavaGapsAndSlope(i, j + 1, duskStoneLiquidID);
							FillLavaGapsAndSlope(i, j - 1, duskStoneLiquidID);
						}
					}
				}
				int oreLeft = 1500;
				int oreTriesLeft = 5000;
				HashSet<ushort> replacables = [duskStoneID, duskStoneLiquidID];
				while (oreLeft > 0 && --oreTriesLeft >= 0) {
					Vector2 dir = WorldGen.genRand.NextVector2CircularEdge(1, 1) * WorldGen.genRand.NextFloat(0.75f, 1.5f);
					bool twist = WorldGen.genRand.NextBool();
					int x = WorldGen.genRand.Next(duskRect.Left, duskRect.Right);
					int y = WorldGen.genRand.Next(duskRect.Top, duskRect.Bottom);
					float strength = WorldGen.genRand.NextFloat(2f, 4f);
					float decay = WorldGen.genRand.NextFloat(0.5f, 0.75f);
					oreLeft -= GenRunners.SpikeVeinRunner(
						x,
						y,
						strength,
						replacables,
						oreID,
						dir,
						decay: decay,
						twist: twist ? 0.2f : 0,
						randomTwist: twist
					);
					oreLeft -= GenRunners.SpikeVeinRunner(
						x,
						y,
						strength,
						replacables,
						oreID,
						-dir,
						decay: decay,
						twist: twist ? 0.2f : 0,
						randomTwist: twist
					);
				}
				bool canBeCleared = TileID.Sets.CanBeClearedDuringGeneration[oreID];
				TileID.Sets.CanBeClearedDuringGeneration[oreID] = false;
				while ((hellSpikes?.Count ?? 0) > 0) {
					(Point pos, int size) = hellSpikes.Pop();
					Vector2 vel = new Vector2(0, (pos.Y < Main.maxTilesY - 150) ? 2.75f : -2.75f).RotatedByRandom(1.25f, WorldGen.genRand);
					bool twist = WorldGen.genRand.NextBool();
					HellSpikeRunner(pos.X, pos.Y, size * 0.75, duskStoneID, vel, decay: WorldGen.genRand.NextFloat(0.75f, 1f), twist: twist ? 0.3f : 0, randomTwist: twist, cutoffStrength: 1);
				}
				TileID.Sets.CanBeClearedDuringGeneration[oreID] = canBeCleared;
				for (int i = duskRect.Left; i < duskRect.Right; i++) {
					for (int j = duskRect.Bottom; j > duskRect.Top; j--) {
						Tile tile = Main.tile[i, j];
						if (tile.TileType == duskStoneID) {
							GenRunners.AutoSlope(i, j, true);
						} else if (tile.TileType == TileID.Containers || tile.TileType == TileID.SmallPiles) {
							WorldGen.paintTile(i, j, PaintID.ShadowPaint);
						}
					}
				}
				hellSpikes = null;
			}
			static void FillLavaGapsAndSlope(int i, int j, ushort fillWith) {
				static bool HasTileOrLava(Tile tile) => tile.HasTile || tile.LiquidAmount > 0;
				Tile tile = Main.tile[i, j];
				if (tile.HasTile) {
					tile.SetSlope(SlopeType.Solid);
				} else {
					if (HasTileOrLava(Main.tile[i + 1, j]) && HasTileOrLava(Main.tile[i - 1, j]) && HasTileOrLava(Main.tile[i, j - 1])) {
						tile.ResetToType(fillWith);
						WorldGen.paintTile(i, j, PaintID.ShadowPaint);
					}
				}
			}
			public static void HellRunner(int x, int y, double strength, int steps, ushort type, float speedX = 0f, float speedY = 0f) {
				WorldBiomeGeneration.ChangeRange.ResetRange();
				double num = strength;
				float num2 = steps;
				Vector2 vector = default;
				vector.X = x;
				vector.Y = y;
				Vector2 vector2 = default;
				vector2.X = WorldGen.genRand.Next(-10, 11) * 0.1f;
				vector2.Y = WorldGen.genRand.Next(-10, 11) * 0.1f;
				if (speedX != 0f || speedY != 0f) {
					vector2.X = speedX;
					vector2.Y = speedY;
				}
				while (num > 0.0 && num2 > 0f) {
					if (vector.Y < 0f && num2 > 0f && type == 59) {
						num2 = 0f;
					}
					num = strength * (num2 / steps);
					num2 -= 1f;
					duskLeft = (int)(vector.X - num * 0.5);
					duskRight = (int)(vector.X + num * 0.5);
					duskTop = (int)(vector.Y - num * 0.5);
					duskBottom = (int)(vector.Y + num * 0.5);
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
					Point? spike;
					for (int i = duskLeft; i < duskRight; i++) {
						spike = null;
						for (int j = duskBottom; j > duskTop; j--) {
							if ((Math.Abs(i - vector.X) + Math.Abs(j - vector.Y)) >= strength * 0.5) {
								continue;
							}
							Tile tile = Main.tile[i, j];
							if (tile.TileType == type) continue;
							if (tile.TileType == TileID.Pots) {
								tile.TileType = 0;
								tile.SetActive(false);
								continue;
							}
							if (Main.tileFrameImportant[tile.TileType]) continue;
							if (TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Main.tile[i, j - 1].TileType]) {
								tile.TileType = type;
								continue;
							}
							if (tile.HasTile && TileID.Sets.CanBeClearedDuringGeneration[tile.TileType]) {
								tile.TileType = type;
								tile.HasTile = true;
								WorldBiomeGeneration.ChangeRange.AddChangeToRange(i, j);
							}
						}
						if (spike.HasValue) {
							int l = spike.Value.Y;
							if (WorldGen.genRand.Next(0, 10 + hellSpikes.Count) <= tilesSinceSpike / 5) {
								Origins.instance.Logger.Info("Adding spike @ " + i + ", " + l);
								hellSpikes.Push((new Point(i, l), WorldGen.genRand.Next(5, 10) + tilesSinceSpike / 5));
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
						vector2.Y += WorldGen.genRand.Next(-10, 11) * 0.05f;
						vector2.X += WorldGen.genRand.Next(-10, 11) * 0.05f;
						if (num > 100.0) {
							vector += vector2;
							num2 -= 1f;
							vector2.Y += WorldGen.genRand.Next(-10, 11) * 0.05f;
							vector2.X += WorldGen.genRand.Next(-10, 11) * 0.05f;
							if (num > 150.0) {
								vector += vector2;
								num2 -= 1f;
								vector2.Y += WorldGen.genRand.Next(-10, 11) * 0.05f;
								vector2.X += WorldGen.genRand.Next(-10, 11) * 0.05f;
								if (num > 200.0) {
									vector += vector2;
									num2 -= 1f;
									vector2.Y += WorldGen.genRand.Next(-10, 11) * 0.05f;
									vector2.X += WorldGen.genRand.Next(-10, 11) * 0.05f;
									if (num > 250.0) {
										vector += vector2;
										num2 -= 1f;
										vector2.Y += WorldGen.genRand.Next(-10, 11) * 0.05f;
										vector2.X += WorldGen.genRand.Next(-10, 11) * 0.05f;
										if (num > 300.0) {
											vector += vector2;
											num2 -= 1f;
											vector2.Y += WorldGen.genRand.Next(-10, 11) * 0.05f;
											vector2.X += WorldGen.genRand.Next(-10, 11) * 0.05f;
											if (num > 400.0) {
												vector += vector2;
												num2 -= 1f;
												vector2.Y += WorldGen.genRand.Next(-10, 11) * 0.05f;
												vector2.X += WorldGen.genRand.Next(-10, 11) * 0.05f;
												if (num > 500.0) {
													vector += vector2;
													num2 -= 1f;
													vector2.Y += WorldGen.genRand.Next(-10, 11) * 0.05f;
													vector2.X += WorldGen.genRand.Next(-10, 11) * 0.05f;
													if (num > 600.0) {
														vector += vector2;
														num2 -= 1f;
														vector2.Y += WorldGen.genRand.Next(-10, 11) * 0.05f;
														vector2.X += WorldGen.genRand.Next(-10, 11) * 0.05f;
														if (num > 700.0) {
															vector += vector2;
															num2 -= 1f;
															vector2.Y += WorldGen.genRand.Next(-10, 11) * 0.05f;
															vector2.X += WorldGen.genRand.Next(-10, 11) * 0.05f;
															if (num > 800.0) {
																vector += vector2;
																num2 -= 1f;
																vector2.Y += WorldGen.genRand.Next(-10, 11) * 0.05f;
																vector2.X += WorldGen.genRand.Next(-10, 11) * 0.05f;
																if (num > 900.0) {
																	vector += vector2;
																	num2 -= 1f;
																	vector2.Y += WorldGen.genRand.Next(-10, 11) * 0.05f;
																	vector2.X += WorldGen.genRand.Next(-10, 11) * 0.05f;
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
					vector2.X += WorldGen.genRand.Next(-10, 11) * 0.05f;
					if (vector2.X > 1f) {
						vector2.X = 1f;
					}
					if (vector2.X < -1f) {
						vector2.X = -1f;
					}
				}
			}
			public static void HellSpikeRunner(int i, int j, double strength, ushort type, Vector2 speed, double decay = 0, float twist = 0, bool randomTwist = false, bool forceSmooth = true, double cutoffStrength = 0.0) {
				double strengthLeft = strength;
				Vector2 pos = new Vector2(i, j);
				if (randomTwist) twist = Math.Abs(twist);
				int X0 = int.MaxValue;
				int X1 = 0;
				int Y0 = int.MaxValue;
				int Y1 = 0;

				int forceTwistThreshold;
				int forceTwistDirection;
				if (j < Main.maxTilesY - 150) {
					forceTwistThreshold = 160;
					forceTwistDirection = -1;
				} else {
					forceTwistThreshold = 140;
					forceTwistDirection = 1;
				}
				while (strengthLeft > cutoffStrength) {
					strengthLeft -= decay;
					int minX = (int)(pos.X - strengthLeft * 0.5);
					int maxX = (int)(pos.X + strengthLeft * 0.5);
					int minY = (int)(pos.Y - strengthLeft * 0.5);
					int maxY = (int)(pos.Y + strengthLeft * 0.5);
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
							if ((Math.Pow(Math.Abs(l - pos.X), 2) + Math.Pow(Math.Abs(k - pos.Y), 2)) > Math.Pow(strength, 2)) {//if (!((Math.Abs(l - pos.X) + Math.Abs(k - pos.Y)) < strength)) {
								continue;
							}
							Tile tile = Main.tile[l, k];
							if (TileID.Sets.CanBeClearedDuringGeneration[tile.TileType]) {
								tile.TileType = type;
								Main.tile[l, k].SetActive(true);
								Main.tile[l, k].LiquidAmount = 0;
								Main.tile[l, k].SetLiquidType(1);
								WorldGen.SquareTileFrame(l, k);
								if (l > X1) {
									X1 = l;
								} else if (l < X0) {
									X0 = l;
								}
								if (k > Y1) {
									Y1 = k;
								} else if (k < Y0) {
									Y0 = k;
								}
							}
						}
					}
					if (forceSmooth && speed.Length() > strengthLeft * 0.75) {
						speed.Normalize();
						speed *= (float)strengthLeft;
					}
					pos += speed;
					if (pos.Y - forceTwistThreshold * forceTwistDirection < 0) {
						GeometryUtils.AngleDif(speed.ToRotation(), MathHelper.PiOver2 * forceTwistDirection, out int dir);
						speed = speed.RotatedBy(dir * -0.1f);
					} else if (twist != 0.0) {
						speed = speed.RotatedBy(twist);
					} else if (randomTwist) {
						speed = speed.RotatedBy(WorldGen.genRand.NextFloat(-twist, twist));
					}
				}
				for (int l = X0; l < X1; l++) {
					for (int k = Y0; k < Y1; k++) {
						GenRunners.AutoSlopeForSpike(l, k);
					}
				}
				NetMessage.SendTileSquare(Main.myPlayer, X0, Y0, X1 - X0, Y1 - Y1);
			}
		}
	}
}
