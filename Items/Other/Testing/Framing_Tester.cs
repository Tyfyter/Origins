using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.Items.Other.Testing {
	public class Framing_Tester : TestingItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.ItemFrame;
		public static int mode;
		public static int ModeMax => missingConfigurations.Count + 2;
		public static int type;
		public static List<(int up, int down, int left, int right)> missingConfigurations = [];
		public static List<(int up, int down, int left, int right, List<(Vector2 offset, int type)> extras)> allConfigurations = [];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 0;
		}
		public override void SetDefaults() {
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
					if (player.controlSmart) {
						mode = (mode + (ModeMax - 1)) % ModeMax;
					} else {
						mode = (mode + 1) % ModeMax;
					}
				} else if (mode >= 0 && mode < ModeMax) {
					if (mode >= missingConfigurations.Count) {
						switch (mode - missingConfigurations.Count) {
							case 0:
							foreach ((Vector2 pos, int index) in IterateAllFrameTestPositions()) {
								void Set(int type, Vector2 offset) {
									offset += pos;
									Tile tile = Framing.GetTileSafely((int)offset.X, (int)offset.Y);
									if (type == -1) {
										tile.HasTile = false;
									} else {
										tile.ResetToType((ushort)type);
									}
								}
								Set(type, Vector2.Zero);
								Framing.GetTileSafely((int)pos.X, (int)pos.Y).TileColor = PaintID.DeepRedPaint;
								Set(missingConfigurations[index].up, -Vector2.UnitY);
								Set(missingConfigurations[index].down, Vector2.UnitY);
								Set(missingConfigurations[index].left, -Vector2.UnitX);
								Set(missingConfigurations[index].right, Vector2.UnitX);
								WorldGen.SquareTileFrame((int)pos.X, (int)pos.Y);
							}
							break;
							case 1:
							foreach ((Vector2 pos, int index) in IterateAllFrameTestPositions()) {
								void Set(int type, Vector2 offset) {
									offset += pos;
									Tile tile = Framing.GetTileSafely((int)offset.X, (int)offset.Y);
									if (type == -1) {
										tile.HasTile = false;
									} else {
										tile.ResetToType((ushort)type);
									}
								}
								Set(type, Vector2.Zero);
								if (index >= allConfigurations.Count - missingConfigurations.Count) Framing.GetTileSafely((int)pos.X, (int)pos.Y).TileColor = PaintID.DeepRedPaint;
								Set(allConfigurations[index].up, -Vector2.UnitY);
								Set(allConfigurations[index].down, Vector2.UnitY);
								Set(allConfigurations[index].left, -Vector2.UnitX);
								Set(allConfigurations[index].right, Vector2.UnitX);
								foreach ((Vector2 offset, int type) in allConfigurations[index].extras) {
									Set(type, offset);
								}
								WorldGen.SquareTileFrame((int)pos.X, (int)pos.Y);
							}
							break;
						}
					} else {
						static void Set(int x, int y, int type) {
							Tile tile = Framing.GetTileSafely(x, y);
							if (type == -1) {
								tile.HasTile = false;
							} else {
								tile.ResetToType((ushort)type);
							}
						}
						Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY).ResetToType((ushort)type);
						Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY).TileColor = PaintID.DeepRedPaint;
						Set(Player.tileTargetX, Player.tileTargetY - 1, missingConfigurations[mode].up);
						Set(Player.tileTargetX, Player.tileTargetY + 1, missingConfigurations[mode].down);
						Set(Player.tileTargetX - 1, Player.tileTargetY, missingConfigurations[mode].left);
						Set(Player.tileTargetX + 1, Player.tileTargetY, missingConfigurations[mode].right);
						WorldGen.SquareTileFrame(Player.tileTargetX, Player.tileTargetY);
					}
				}
				return true;
			}
			return false;
		}
		public static IEnumerable<(Vector2 pos, int index)> IterateAllFrameTestPositions() {
			Vector2 start = new(Player.tileTargetX, Player.tileTargetY);
			Vector2 pos;
			int count = missingConfigurations.Count;
			if (mode == missingConfigurations.Count + 1) count = allConfigurations.Count;
			int len = (int)Math.Ceiling(Math.Sqrt(count));
			int index = 0;
			for (int j = 0; j < len && index < count; j++) {
				pos = start;
				pos.Y += 4 * j;
				for (int i = 0; i < len && index < count; i++) {
					yield return (pos, index);
					pos.X += 4;
					index++;
				}
			}
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (Main.LocalPlayer.HeldItem.type == Item.type) {
				string display = mode.ToString();
				switch (mode - missingConfigurations.Count) {
					case 0:
					display = "missing";
					break;
					case 1:
					display = "all";
					break;
				}
				Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, display, Main.MouseScreen.X, Math.Max(Main.MouseScreen.Y - 24, 18), Colors.RarityNormal, Color.Black, new Vector2(0f));
				if (Main.LocalPlayer.controlLeft && Main.LocalPlayer.controlRight && Main.LocalPlayer.controlUp && Main.LocalPlayer.controlDown) {
					int O = 0;
					int OwO = 0 / O;
				}
			}
		}
	}
	public class Framing_Tester_UI : ModSystem {
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryIndex != -1 && Main.LocalPlayer.HeldItem.type == ModContent.ItemType<Framing_Tester>()) {
				layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
					"Origins: Framing Tester",
					delegate {
						Rectangle frame = new(162, 54, 16, 16);
						int totalIndex = 0;
						switch (Framing_Tester.mode - Framing_Tester.missingConfigurations.Count) {
							case 0:
							foreach ((Vector2 pos, int index) in Framing_Tester.IterateAllFrameTestPositions()) {
								totalIndex = index;
								void Draw(int type, Vector2 offset) {
									if (type == -1) return;
									Main.instance.LoadTiles(type);
									Main.spriteBatch.Draw(TextureAssets.Tile[type].Value, (pos + offset) * 16 - Main.screenPosition, frame, Color.White);
								}
								Draw(Framing_Tester.type, Vector2.Zero);
								Draw(Framing_Tester.missingConfigurations[index].up, -Vector2.UnitY);
								Draw(Framing_Tester.missingConfigurations[index].down, Vector2.UnitY);
								Draw(Framing_Tester.missingConfigurations[index].left, -Vector2.UnitX);
								Draw(Framing_Tester.missingConfigurations[index].right, Vector2.UnitX);
							}
							break;
							case 1:
							foreach ((Vector2 pos, int index) in Framing_Tester.IterateAllFrameTestPositions()) {
								totalIndex = index;
								void Draw(int type, Vector2 offset) {
									if (type == -1) return;
									Main.instance.LoadTiles(type);
									Main.spriteBatch.Draw(TextureAssets.Tile[type].Value, (pos + offset) * 16 - Main.screenPosition, frame, Color.White);
								}
								Draw(Framing_Tester.type, Vector2.Zero);
								Draw(Framing_Tester.allConfigurations[index].up, -Vector2.UnitY);
								Draw(Framing_Tester.allConfigurations[index].down, Vector2.UnitY);
								Draw(Framing_Tester.allConfigurations[index].left, -Vector2.UnitX);
								Draw(Framing_Tester.allConfigurations[index].right, Vector2.UnitX);
								foreach ((Vector2 offset, int type) in Framing_Tester.allConfigurations[index].extras) {
									Draw(type, offset);
								}
							}
							break;
						}
						return true;
					},
					InterfaceScaleType.Game)
				);
			}
		}
	}
}
