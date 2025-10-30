using CalamityMod.NPCs.TownNPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Core {
	internal class CustomPlaceTileItem : ILoadable {
		public void Load(Mod mod) {
			On_Player.PlaceThing_Tiles += On_Player_PlaceThing_Tiles;
		}
		public void Unload() { }
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
	}
	//TODO: use this to implement actually proper grass seeds
	public interface ICustomPlaceTileItem {
		public void PlaceTile(On_Player.orig_PlaceThing_Tiles orig, bool inRange);
	}
}
