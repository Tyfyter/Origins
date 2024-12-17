using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.World;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using Origins.World.BiomeData;

namespace Origins.Items.Other.Testing {
	public class Blood_Mushroom_Soup : ModItem {
		int mode;
		const int modeCount = 20;
		long packedMode => (long)mode | ((long)parameters.Count << 32);
		LinkedQueue<object> parameters = new LinkedQueue<object>();
		Vector2 basePosition;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 0;
		}
		public override void SetDefaults() {
			//item.name = "jfdjfrbh";
			Item.width = 16;
			Item.height = 26;
			Item.value = 25000;
			Item.rare = ItemRarityID.Green;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useAnimation = 10;
			Item.useTime = 10;
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool? UseItem(Player player)/* Suggestion: Return null instead of false */ {
			if (Main.myPlayer == player.whoAmI) {
				if (player.altFunctionUse == 2) {
					parameters.Clear();
					if (player.controlSmart) {
						mode = (mode + modeCount) % (modeCount + 1);
					} else {
						mode = (mode + 1) % (modeCount + 1);
					}
				} else {
					if (player.controlSmart) {
						Apply();
					} else if (player.controlDown) {
						if (parameters.Count > 0) parameters.RemoveAt(parameters.Count - 1);
					} else {
						SetParameter();
					}
				}
				return true;
			}
			return false;
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (Main.LocalPlayer.HeldItem.type == Item.type) {
				Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, GetMouseText(), Main.MouseScreen.X, Math.Max(Main.MouseScreen.Y - 24, 18), Colors.RarityNormal, Color.Black, new Vector2(0f));
				if (Main.LocalPlayer.controlLeft && Main.LocalPlayer.controlRight && Main.LocalPlayer.controlUp && Main.LocalPlayer.controlDown) {
					int O = 0;
					int OwO = 0 / O;
				}
			}

		}
		const long p0 = (0L << 32);
		const long p1 = (1L << 32);
		const long p2 = (2L << 32);
		const long p3 = (3L << 32);
		const long p4 = (4L << 32);
		const long p5 = (5L << 32);
		const long p6 = (6L << 32);
		const long p7 = (7L << 32);
		void SetParameter() {
			Point mousePos = new Point((int)(Main.MouseScreen.X / 16), (int)(Main.MouseScreen.Y / 16));
			int mousePacked = mousePos.X + (Main.screenWidth / 16) * mousePos.Y;
			double mousePackedDouble = (Main.MouseScreen.X / 16d + (Main.screenWidth / 16d) * Main.MouseScreen.Y / 16d) / 16d;
			Tile mouseTile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
			Vector2 diffFromPlayer = Main.MouseWorld - Main.LocalPlayer.MountedCenter;
			switch (packedMode) {
				case 15 | p0:
				parameters.Enqueue(Main.MouseWorld);
				break;
				case 15 | p1:
				parameters.Enqueue(Main.MouseWorld);
				Apply();
				break;
				case 14 | p0:
				parameters.Enqueue(Player.tileTargetX);
				parameters.Enqueue(Player.tileTargetY);
				Apply();
				break;
				case 13 | p0:
				parameters.Enqueue(Player.tileTargetX);
				parameters.Enqueue(Player.tileTargetY);
				Apply();
				break;
				case 12 | p0:
				parameters.Enqueue(Player.tileTargetX);
				parameters.Enqueue(Player.tileTargetY);
				basePosition = new Vector2(Player.tileTargetX, Player.tileTargetY);
				break;
				case 12 | p2:
				parameters.Enqueue((new Vector2(Player.tileTargetX, Player.tileTargetY) - basePosition).Length() / 35f);
				break;
				case 12 | p3:
				parameters.Enqueue((new Vector2(Player.tileTargetX, Player.tileTargetY) - basePosition) / 10f);
				break;
				case 11 | p0:
				parameters.Enqueue(Player.tileTargetX);
				parameters.Enqueue(Player.tileTargetY);
				Apply();
				break;
				case 10 | p0:
				parameters.Enqueue(Player.tileTargetX);
				goto case 10 | p1;
				case 10 | p1:
				parameters.Enqueue(Player.tileTargetY);
				break;
				case 10 | p2:
				parameters.Enqueue(Player.tileTargetX);
				goto case 10 | p3;
				case 10 | p3:
				parameters.Enqueue(Player.tileTargetY);
				Apply();
				break;
				case 9 | p0:
				parameters.Enqueue(new Point(Player.tileTargetX, Player.tileTargetY));
				Apply();
				break;
				case 8 | p0:
				parameters.Enqueue(new Point(Player.tileTargetX, Player.tileTargetY));
				break;
				case 8 | p1:
				parameters.Enqueue(Math.Sqrt(mousePackedDouble / 16));
				break;
				case 8 | p2:
				parameters.Enqueue(Main.MouseWorld);
				break;
				case 8 | p3:
				parameters.Enqueue(mousePackedDouble);
				break;
				case 8 | p4:
				parameters.Enqueue(Math.Sqrt(mousePackedDouble / 16));
				break;
				case 8 | p5:
				parameters.Enqueue(Main.LocalPlayer.controlUp ? 0 : diffFromPlayer.ToRotation());
				break;
				case 8 | p6:
				parameters.Enqueue(Main.MouseScreen.Y > Main.screenHeight / 2f);
				break;
				case 8 | p7:
				Apply();
				break;
				case 7 | p0:
				parameters.Enqueue(Main.MouseWorld.ToTileCoordinates());
				Apply();
				break;
				case 6 | p0:
				parameters.Enqueue(Main.MouseWorld.X / 16);
				parameters.Enqueue(Main.MouseWorld.Y / 16);
				Apply();
				break;
				case 2 | p0:
				case 3 | p0:
				case 4 | p0:
				case 5 | p0:
				parameters.Enqueue(Player.tileTargetX);
				parameters.Enqueue(Player.tileTargetY);
				Apply();
				break;
				case 1 | p0:
				case 0 | p0:
				parameters.Enqueue(Player.tileTargetX);
				parameters.Enqueue(Player.tileTargetY);
				break;
				case 1 | p1:
				case 0 | p1:
				parameters.Enqueue(Player.tileTargetY);
				break;
				case 1 | p2:
				case 0 | p2:
				parameters.Enqueue(Math.Sqrt(mousePackedDouble / 16));
				break;
				case 1 | p3:
				case 0 | p3:
				parameters.Enqueue(diffFromPlayer / 16);
				break;
				case 1 | p4:
				case 0 | p4:
				parameters.Enqueue(mousePackedDouble);
				break;
				case 1 | p5:
				case 0 | p5:
				parameters.Enqueue(Main.LocalPlayer.controlUp ? 0 : diffFromPlayer.ToRotation());
				break;
				case 1 | p6:
				case 0 | p6:
				parameters.Enqueue(Main.MouseScreen.Y > Main.screenHeight / 2f);
				break;
				case 1 | p7:
				parameters.Enqueue((byte)((mousePacked / 16) % 256));
				break;

				case -1 | p1:
				Apply();
				break;
			}
		}
		string GetMouseText() {
			Point mousePos = new Point((int)(Main.MouseScreen.X / 16), (int)(Main.MouseScreen.Y / 16));
			int mousePacked = mousePos.X + (Main.screenWidth / 16) * mousePos.Y;
			double mousePackedDouble = (Main.MouseScreen.X / 16d + (Main.screenWidth / 16d) * Main.MouseScreen.Y / 16d) / 16d;
			Vector2 diffFromPlayer = Main.MouseWorld - Main.LocalPlayer.MountedCenter;
			switch (packedMode) {
				case 15 | p0:
				return $"ravel hole start point: {Player.tileTargetX}, {Player.tileTargetY}";
				case 15 | p1:
				return $"ravel hole end point: {Player.tileTargetX}, {Player.tileTargetY}";
				case 14 | p0:
				return $"spread riven grass: {Player.tileTargetX}, {Player.tileTargetY}";
				case 13 | p0:
				return $"place brine pool start: {Player.tileTargetX}, {Player.tileTargetY}";
				case 12 | p0:
				return $"place brine cave start: {Player.tileTargetX}, {Player.tileTargetY}";
				case 12 | p2:
				return $"brine cave scale: {(new Vector2(Player.tileTargetX, Player.tileTargetY) - basePosition).Length() / 35f}";
				case 12 | p3:
				return $"brine cave stretch: {(new Vector2(Player.tileTargetX, Player.tileTargetY) - basePosition) / 10f}";
				case 11 | p0:
				return "start fiberglass undergrowth";
				case 10 | p0:
				return "place WFC test point 1";
				case 10 | p2:
				return "place WFC test point 2";
				case 9 | p0:
				return "place defiled fissure";
				case 8 | p0:
				return "defiled vein position";
				case 8 | p1:
				return "defiled vein strength: " + Math.Sqrt(mousePackedDouble / 16);
				case 8 | p2:
				return "defiled vein target";
				case 8 | p3:
				return "defiled vein length: " + mousePackedDouble;
				case 8 | p4:
				return "defiled vein wall thickness: " + Math.Sqrt(mousePackedDouble / 16);
				case 8 | p5:
				return "defiled vein twist: " + (Main.LocalPlayer.controlUp ? 0 : (double)diffFromPlayer.ToRotation());
				case 8 | p6:
				return "defiled vein twist randomization: " + (Main.MouseScreen.Y > Main.screenHeight / 2f);
				case 8 | p7:
				return "start defiled vein";
				case 7 | p0:
				return "remove tree";
				case 6 | p0:
				return "place defiled stone ring";
				case 5 | p0:
				return "place defiled start";
				case 4 | p0:
				return "place brine pool";
				case 3 | p0:
				return "place riven cave";
				case 2 | p0:
				return "place riven start";
				case 1 | p0:
				case 0 | p0:
				return $"i,j: {Player.tileTargetX}, {Player.tileTargetY}";
				case 1 | p1:
				case 0 | p1:
				return $"j: {Player.tileTargetY}";
				case 1 | p2:
				case 0 | p2:
				return $"strength: {mousePackedDouble / 16}";
				case 1 | p3:
				case 0 | p3:
				return $"speed: {diffFromPlayer / 16}";
				case 1 | p4:
				case 0 | p4:
				return $"length: {mousePackedDouble}";
				case 1 | p5:
				case 0 | p5:
				return $"twist: {(Main.LocalPlayer.controlUp ? 0 : (double)diffFromPlayer.ToRotation())}";
				case 1 | p6:
				case 0 | p6:
				return $"random twist: {Main.MouseScreen.Y > Main.screenHeight / 2f}";
				case 1 | p7:
				return $"branch count (optional): {(byte)((mousePacked / 16) % 256)}";

				case -1 | p1:
				return $"continue";
				//return $":{}";
			}
			return "";
		}
		void Apply() {
			switch (mode) {
				case -1: {
					Func<bool> function = (Func<bool>)parameters.Dequeue();
					if (function()) {
						parameters.Enqueue(function);
					} else {
						mode = 0;
					}
					break;
				}
				case 0: {
					GenRunners.VeinRunner(
						i: (int)parameters.Dequeue(),
						j: (int)parameters.Dequeue(),
						strength: (double)parameters.Dequeue(),
						speed: (Vector2)parameters.Dequeue(),
						length: (double)parameters.Dequeue(),
						twist: (float)parameters.Dequeue(),
						randomtwist: (bool)parameters.Dequeue());
					break;
				}
				case 1: {
					int i = (int)parameters.Dequeue();
					int j = (int)parameters.Dequeue();
					double strength = (double)parameters.Dequeue();
					Vector2 speed = (Vector2)parameters.Dequeue();
					Stack<((Vector2, Vector2), byte)> veins = new Stack<((Vector2, Vector2), byte)>();
					double length = (double)parameters.Dequeue();
					float twist = (float)parameters.Dequeue();
					bool twistRand = (bool)parameters.Dequeue();
					veins.Push(((new Vector2(i, j), speed), (parameters.Count > 0 ? (byte)parameters.Dequeue() : (byte)10)));
					((Vector2 p, Vector2 v) v, byte count) curr;
					(Vector2 p, Vector2 v) ret;
					byte count;
					while (veins.Count > 0) {
						curr = veins.Pop();
						count = curr.count;
						ret = GenRunners.VeinRunner(
							i: (int)curr.v.p.X,
							j: (int)curr.v.p.Y,
							strength: strength,
							speed: curr.v.v,
							length: length,
							twist: twist,
							randomtwist: twistRand);
						if (count > 0 && Main.rand.NextBool(3)) {
							veins.Push(((ret.p, ret.v.RotatedBy(Main.rand.NextBool() ? -1 : 1)), (byte)Main.rand.Next(--count)));
						}
						if (count > 0) {
							veins.Push(((ret.p, ret.v.RotatedByRandom(0.05)), --count));
						}
					}
					break;
				}
				case 2:
				World.BiomeData.Riven_Hive.Gen.StartHive((int)parameters.Dequeue(), (int)parameters.Dequeue());
				break;
				case 3:
				World.BiomeData.Riven_Hive.Gen.HiveCave_Old((int)parameters.Dequeue(), (int)parameters.Dequeue());
				break;
				case 4:
				World.BiomeData.Brine_Pool.Gen.BrineStart_Old((int)parameters.Dequeue(), (int)parameters.Dequeue());
				break;
				case 5:
				World.BiomeData.Defiled_Wastelands.Gen.StartDefiled((int)parameters.Dequeue(), (int)parameters.Dequeue());
				break;
				case 6: {
					Vector2 a = new Vector2((float)parameters.Dequeue(), (float)parameters.Dequeue());
					World.BiomeData.Defiled_Wastelands.Gen.DefiledRibs((int)a.X, (int)a.Y);
					for (int i = (int)a.X - 1; i < (int)a.X + 3; i++) {
						for (int j = (int)a.Y - 2; j < (int)a.Y + 2; j++) {
							Main.tile[i, j].SetActive(false);
						}
					}
					TileObject.CanPlace((int)a.X, (int)a.Y, (ushort)ModContent.TileType<Tiles.Defiled.Defiled_Heart>(), 0, 1, out var data);
					TileObject.Place(data);
					break;
				}
				case 7:
				Point treeLoc = (Point)parameters.Dequeue();
				//Main.NewText(treeLoc);
				OriginSystem.RemoveTree(treeLoc.X, treeLoc.Y);
				break;
				case 8: {
					Point pos = (Point)parameters.Dequeue();
					double strength = (double)parameters.Dequeue();
					Vector2 speed = (((Vector2)parameters.Dequeue()) - pos.ToVector2() * 16).SafeNormalize(Vector2.UnitY);
					double length = (double)parameters.Dequeue();
					double wallThickness = (double)parameters.Dequeue();
					float twist = (float)parameters.Dequeue();
					bool twistRand = (bool)parameters.Dequeue();
					World.BiomeData.Defiled_Wastelands.Gen.DefiledVeinRunner(
						pos.X, pos.Y,
						strength,
						speed,
						length,
						(ushort)ModContent.TileType<Tiles.Defiled.Defiled_Stone>(),
						(float)wallThickness,
						twist,
						twistRand,
						(ushort)ModContent.WallType<Walls.Defiled_Stone_Wall>()
					);
					break;
				}
				case 9: {
					ushort stoneID = (ushort)ModContent.TileType<Tiles.Defiled.Defiled_Stone>();
					ushort fissureID = (ushort)ModContent.TileType<Tiles.Defiled.Defiled_Fissure>();
					Point pos = (Point)parameters.Dequeue();
					for (int oY = 1; oY < 2; oY++) {
						for (int o = 0; o > -5; o = o > 0 ? -o : -o + 1) {
							Point p = pos;
							int loop = 0;
							for (; !Main.tile[p.X + o - 1, p.Y + oY].HasTile || !Main.tile[p.X + o, p.Y + oY].HasTile; p.Y++) {
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
							if (TileObject.CanPlace(p.X + o, p.Y, fissureID, 0, 0, out TileObject to)) {
								WorldGen.Place2x2(p.X + o, p.Y, fissureID, 0);
								break;
							}
						}
					}
					break;
				}
				case 10: {
					int x1 = (int)parameters.Dequeue();
					int y1 = (int)parameters.Dequeue();
					int x2 = (int)parameters.Dequeue();
					int y2 = (int)parameters.Dequeue();
					for (int i = x1; i < x2; i++) {
						for (int j = y1; j < y2; j++) {
							Framing.GetTileSafely(i, j).ResetToType(TileID.StoneSlab);
						}
					}
					WorldGen.RangeFrame(x1, y1, x2, y2);
					x2 = x2 - x1;
					if (x2 < 0) {
						x1 += x2;
						x2 = -x2;
					}
					x2++;
					y2 = y2 - y1;
					if (y2 < 0) {
						y1 += y2;
						y2 = -y2;
					}
					y2++;
					ushort mask_none = 0b011;
					ushort mask_full = 0b101;
					WaveFunctionCollapse.Generator<BlockType> generator = new(x2, y2, cellTypes: [
						new(new(BlockType.SlopeDownLeft,  mask_none, mask_full, mask_full, mask_none), 1),
						new(new(BlockType.SlopeDownRight, mask_none, mask_none, mask_full, mask_full), 1),
						new(new(BlockType.SlopeUpLeft,    mask_full, mask_full, mask_none, mask_none), 1),
						new(new(BlockType.SlopeUpRight,   mask_full, mask_none, mask_none, mask_full), 1)
					]);
					/*mode = -1;
                    parameters.Enqueue((Func<bool>)(() => {
                        return generator.CollapseStepWith(
							(int i, int j, BlockType type) => {
                                Main.NewText(type);
                                Framing.GetTileSafely(i + x1, j + y1).BlockType = type;
                            }
                        );
                    }));*/
					//generator.Force(0, 0, new(BlockType.SlopeDownLeft, mask_none, mask_full, mask_full, mask_none));
					retry:
					try {
						generator.Collapse();
					} catch (Exception) {
						goto retry;
					}
					for (int i = 0; i < x2; i++) {
						for (int j = 0; j < y2; j++) {
							Framing.GetTileSafely(i + x1, j + y1).BlockType = generator.GetActual(i, j);
							if (Framing.GetTileSafely(i + x1, j + y1).BlockType == (BlockType)6) {
								Framing.GetTileSafely(i + x1, j + y1).HasTile = false;
							}
						}
					}
					break;
				}
				case 11: {
					Fiberglass_Undergrowth.Gen.FiberglassStart((int)parameters.Dequeue(), (int)parameters.Dequeue());
					break;
				}
				case 12: {
					Brine_Pool.Gen.SmallCave(
						(int)parameters.Dequeue(),
						(int)parameters.Dequeue(),
						(float)parameters.Dequeue(),
						(Vector2)parameters.Dequeue()
					);
					break;
				}
				case 13: {
					Brine_Pool.Gen.BrineStart((int)parameters.Dequeue(), (int)parameters.Dequeue());
					break;
				}
				case 14: {
					Riven_Hive.Gen.SpreadRivenGrass((int)parameters.Dequeue(), (int)parameters.Dequeue());
					break;
				}
				case 15: {
					Defiled_Wastelands.Gen.RavelConnection((Vector2)parameters.Dequeue(), (Vector2)parameters.Dequeue());
					break;
				}
			}
		}
	}
}
