using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Other {
    public class A_Perfect_World : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.FramesOnKillWall[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(120, 85, 60), Language.GetText("MapObject.Painting"));
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ModContent.ItemType<A_Perfect_World_Item>());
		}
	}
	public class A_Perfect_World_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("A Perfect World");
			Tooltip.SetDefault("'The Defiled'");
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.PaintingMartiaLisa);
			Item.value = Item.buyPrice(gold: 10);
			Item.createTile = ModContent.TileType<A_Perfect_World>();
		}
	}
}
