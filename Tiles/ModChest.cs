using Origins.Core;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Origins.Core.SpecialChest;
using static Origins.Tiles.Ashen.Medium_Storage_Container;

namespace Origins.Tiles {
	public abstract class ModChest : ModTile {
		public int keyItem { get; protected set; } = -1;
		public override void SetStaticDefaults() {
			Main.tileSpelunker[Type] = true;
			Main.tileContainer[Type] = true;
			TileID.Sets.IsAContainer[Type] = true;
			TileID.Sets.BasicChest[Type] = true;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 1200;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileOreFinderPriority[Type] = 500;
			TileID.Sets.HasOutlines[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.CoordinateHeights = [16, 18];
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(new Func<int, int, int, int, int, int, int>(Chest.FindEmptyChest), -1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(new Func<int, int, int, int, int, int, int>(Chest.AfterPlacement_Hook), -1, 0, false);
			TileObjectData.newTile.AnchorInvalidTiles = [127];
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			ModifyTileData();
			TileObjectData.addTile(Type);
			DustType = Defiled_Wastelands.DefaultTileDust;
			_ = DefaultContainerName(0, 0);
		}
		public virtual void ModifyTileData() { }
		public override ushort GetMapOption(int i, int j) => (ushort)(Main.tile[i, j].TileFrameX < 36 ? 0 : 1);

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

		public override bool IsLockedChest(int i, int j) => Main.tile[i, j].TileFrameX >= 36;

		public virtual bool CanUnlockChest(int i, int j) => true;

		public override bool UnlockChest(int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual) {
			dustType = this.DustType;
			return true;
		}

		public string MapChestName(string name, int i, int j) {
			GetTopLeft(i, j, Main.tile[i, j], out int left, out int top);
			int chest = Chest.FindChest(left, top);
			if (chest < 0) {
				return Language.GetTextValue("LegacyChestType.0");
			} else if (Main.chest[chest].name == "") {
				return name;
			} else {
				return name + ": " + Main.chest[chest].name;
			}
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 3 : 10;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Chest.DestroyChest(i, j);
		}
		public override bool RightClick(int i, int j) {
			Player player = Main.LocalPlayer;
			Main.mouseRightRelease = false;
			GetTopLeft(i, j, Main.tile[i, j], out int left, out int top);
			if (player.sign >= 0) {
				SoundEngine.PlaySound(SoundID.MenuClose);
				player.sign = -1;
				Main.editSign = false;
				Main.npcChatText = "";
			}
			if (Main.editChest) {
				SoundEngine.PlaySound(SoundID.MenuTick);
				Main.editChest = false;
				Main.npcChatText = "";
			}
			if (player.editedChestName) {
				NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f, 0f, 0f, 0, 0, 0);
				player.editedChestName = false;
			}
			bool isLocked = IsLockedChest(left, top);
			if (Main.netMode == NetmodeID.MultiplayerClient && !isLocked) {
				if (left == player.chestX && top == player.chestY && player.chest >= 0) {
					player.chest = -1;
					Recipe.FindRecipes();
					SoundEngine.PlaySound(SoundID.MenuClose);
				} else {
					NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, (float)top, 0f, 0f, 0, 0, 0);
					Main.stackSplit = 600;
				}
			} else {
				if (isLocked) {
					if (CanUnlockChest(left, top) && player.ConsumeItem(keyItem) && Chest.Unlock(left, top)) {
						if (Main.netMode == NetmodeID.MultiplayerClient) {
							NetMessage.SendData(MessageID.LockAndUnlock, -1, -1, null, player.whoAmI, 1f, (float)left, (float)top);
						}
					}
				} else {
					int chest = Chest.FindChest(left, top);
					if (chest >= 0) {
						if (chest == player.chest) {
							player.chest = -1;
							SoundEngine.PlaySound(SoundID.MenuClose);
						} else {
							player.OpenChest(i, j, chest);
							SoundEngine.PlaySound(SoundID.MenuOpen);
						}
						Recipe.FindRecipes();
					}
				}
			}
			return true;
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			GetTopLeft(i, j, Main.tile[i, j], out int left, out int top);
			int chestIndex = Chest.FindChest(left, top);
			player.cursorItemIconID = -1;
			if (chestIndex < 0) {
				player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
			} else {
				if ((Main.chest[chestIndex].name?.Length ?? 0) <= 0) {
					if (IsLockedChest(left, top)) player.cursorItemIconID = keyItem;
					else player.cursorItemIconID = TileLoader.GetItemDropFromTypeAndStyle(Type);
					player.cursorItemIconText = "";
				} else {
					player.cursorItemIconText = Main.chest[chestIndex].name;
				}
			}
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
		}

		public override void MouseOverFar(int i, int j) {
			MouseOver(i, j);
			Player player = Main.LocalPlayer;
			if (player.cursorItemIconText == "") {
				player.cursorItemIconEnabled = false;
				player.cursorItemIconID = ItemID.None;
			}
		}
		public virtual void GetTopLeft(int i, int j, in Tile tile, out int left, out int top) {
			left = i;
			top = j;
			if (tile.TileFrameX % 36 != 0) left--;
			if (tile.TileFrameY != 0) top--;
		}
	}
	public abstract class ModSpecialChest : ModTile {
		public abstract ChestData CreateChestData();
		public sealed override void SetStaticDefaults() {
			Main.tileSpelunker[Type] = true;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 12000;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileOreFinderPriority[Type] = 500;
			TileID.Sets.HasOutlines[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(1, 3);
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.SetHeight(4);
			TileObjectData.newTile.AnchorInvalidTiles = [127];
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			ModifyTileData();
			TileObjectData.addTile(Type);
			_ = DefaultContainerName(0, 0);
			AdjTiles = [TileID.Containers];
		}
		public virtual void ModifyTileData() { }
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
		public override void PlaceInWorld(int i, int j, Item item) {
			TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(Main.tile[i, j]), out i, out j);
			new Set_Special_Chest_Action(new(i, j), CreateChestData()).Perform();
		}
		public override bool RightClick(int i, int j) {
			SpecialChest.OpenChest(i, j);
			return true;
		}
	}
}
