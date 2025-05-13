using Origins.Items.Other;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Other {
	public abstract class ModGemLock : ModTile {
		public abstract int GemType { get; }
		public override void Load() {
			Mod.AddContent(new TileItem(this).WithExtraDefaults(item => {
				item.width = 20;
				item.height = 20;
				item.value = Item.sellPrice(0, 0, 1);
			}));
		}
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.FramesOnKillWall[Type] = true;
			TileID.Sets.AvoidedByNPCs[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			AddMapEntry(new(55, 204, 212), CreateMapEntryName());

			// Placement
			// In addition to copying from the TileObjectData.Something templates, modders can copy from specific tile types. CopyFrom won't copy subtile data, so style specific properties won't be copied, such as how Obsidian doors are immune to lava.
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
			TileObjectData.addTile(Type);
		}
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			int self = TileLoader.GetItemDropFromTypeAndStyle(Type, 0);
			yield return new Item(self);
			if (Framing.GetTileSafely(i, j).TileFrameY >= 54) yield return new Item(GemType);
		}
		public override void MouseOver(int i, int j) {
			int type = GemType;
			Player player = Main.LocalPlayer;
			if (type != -1 && ((Main.tile[i, j].TileFrameY / 54) == 1 || player.HasItem(type))) {
				player.noThrow = 2;
				player.cursorItemIconEnabled = true;
				player.cursorItemIconID = type;
			}
		}
		public override bool RightClick(int i, int j) {
			Player player = Main.LocalPlayer;
			int frameY = Main.tile[i, j].TileFrameY / 54;
			int type = GemType;

			if (type != -1) {
				if (frameY == 0 && player.HasItem(type) && player.selectedItem != 58) {
					player.GamepadEnableGrappleCooldown();
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						player.ConsumeItem(type);
						ToggleGemLock(i, j, on: true);
					} else {
						player.ConsumeItem(type);
						ModPacket packet = Origins.instance.GetPacket();
						packet.Write(Origins.NetMessageType.set_gem_lock);
						packet.Write((short)i);
						packet.Write((short)j);
						packet.Write(true);
						packet.Send();
					}
				} else if (frameY == 1) {
					player.GamepadEnableGrappleCooldown();
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						ToggleGemLock(i, j, on: false);
					} else {
						ModPacket packet = Origins.instance.GetPacket();
						packet.Write(Origins.NetMessageType.set_gem_lock);
						packet.Write((short)i);
						packet.Write((short)j);
						packet.Write(false);
						packet.Send();
					}
				}
			}
			return true;
		}
		public void ToggleGemLock(int i, int j, bool on) {

			bool alreadyOn = false;
			int type = GemType;
			Tile tile = Framing.GetTileSafely(i, j);
			if (!tile.HasTile || (tile.TileFrameY < 54 && !on)) return;
			if (tile.TileFrameY >= 54) alreadyOn = true;

			int xOffset = tile.TileFrameX % 54 / 18;
			int yOffset = tile.TileFrameY % 54 / 18;

			for (int k = i - xOffset; k < i - xOffset + 3; k++) {
				for (int l = j - yOffset; l < j - yOffset + 3; l++) {
					Main.tile[k, l].TileFrameY = (short)((on ? 54 : 0) + (l - j + yOffset) * 18);
				}
			}

			if (type != -1 && alreadyOn)
				Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 32, 32, type);

			WorldGen.SquareTileFrame(i, j);
			NetMessage.SendTileSquare(-1, i - xOffset, j - yOffset, 3, 3);
			SoundEngine.PlaySound(SoundID.Mech, new Vector2(i + 1 - xOffset, j + 1 - yOffset) * 16);
			Wiring.TripWire(i - xOffset, j - yOffset, 3, 3);
			NetMessage.SendData(MessageID.HitSwitch, -1, -1, null, i - xOffset, j - yOffset);
		}
	}
	public class Chambersite_Gem_Lock : ModGemLock {
		public override int GemType => ModContent.ItemType<Large_Chambersite>();
	}
}