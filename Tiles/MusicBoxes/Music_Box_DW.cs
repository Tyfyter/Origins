using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.MusicBoxes {
	internal class Music_Box_DW : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.addTile(Type);
			TileID.Sets.DisableSmartCursor[Type] = true;
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Music Box");
			AddMapEntry(new Color(255, 255, 255), name);
		}

		/*public override void KillMultiTile(int i, int j, int frameX, int frameY) {
		}*/

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
		}
	}

	public class Music_Box_DW_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Music Box (Defiled Wastelands)");
	    }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.MusicBoxCorruption);
            Item.createTile = ModContent.TileType<Music_Box_DW>();
		}
	}
}
