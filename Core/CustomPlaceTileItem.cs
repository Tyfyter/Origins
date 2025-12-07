using CalamityMod.Items.Potions.Alcohol;
using CalamityMod.NPCs.TownNPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Core {
	public class CustomPlaceTileItem : ILoadable {
		void ILoadable.Load(Mod mod) {
			On_Player.PlaceThing_Tiles += On_Player_PlaceThing_Tiles;
		}
		void ILoadable.Unload() { }
		static void On_Player_PlaceThing_Tiles(On_Player.orig_PlaceThing_Tiles orig, Player self) {
			Item item = self.HeldItem;
			if (item?.ModItem is ICustomPlaceTileItem customPlaceItem) {
				if (!self.ItemTimeIsZero || self.itemAnimation <= 0 || !self.controlUseItem) return;
				bool inRange = 
					(self.position.X / 16f - Player.tileRangeX - item.tileBoost - self.blockRange <= Player.tileTargetX) &&
					((self.position.X + self.width) / 16f + Player.tileRangeX + item.tileBoost - 1f + self.blockRange >= Player.tileTargetX) &&
					(self.position.Y / 16f - Player.tileRangeY - item.tileBoost - self.blockRange <= Player.tileTargetY) &&
					((self.position.Y + self.height) / 16f + Player.tileRangeY + item.tileBoost - 2f + self.blockRange >= Player.tileTargetY);
				customPlaceItem.PlaceTile(orig, inRange);
				return;
			}
			orig(self);
		}
		public static bool PlantSeeds(int i, int j, int[] set) {
			Tile tile = Main.tile[i, j];
			if (!tile.HasTile || set[tile.TileType] == -1) return false;
			SoundEngine.PlaySound(SoundID.Dig, new(i * 16 + 8, j * 16 + 8));
			tile.TileType = (ushort)set[tile.TileType];
			WorldGen.SquareTileFrame(i, j);
			return true;
		}
		public static void PlantSeedsAtCursor(int[] set) {
			if (PlantSeeds(Player.tileTargetX, Player.tileTargetY, set)) {
				Main.LocalPlayer.ApplyItemTime(Main.LocalPlayer.HeldItem, Main.LocalPlayer.tileSpeed);
				NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY, TileChangeType.None);
				if (PlayerInput.UsingGamepad && ItemID.Sets.SingleUseInGamepad[Main.LocalPlayer.HeldItem.type] && !Main.SmartCursorIsUsed) {
					Main.blockMouse = true;
				}
				TileLoader.PlaceInWorld(Player.tileTargetX, Player.tileTargetY, Main.LocalPlayer.HeldItem);
			}
		}
	}
	//TODO: use this to implement actually proper grass seeds
	public interface ICustomPlaceTileItem {
		public void PlaceTile(On_Player.orig_PlaceThing_Tiles orig, bool inRange);
	}
}
