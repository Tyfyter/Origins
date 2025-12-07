using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Decoration {
    public class A_Perfect_World : ModTile {
		public string[] Categories => [
			"Painting"
		];
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.FramesOnKillWall[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
			TileObjectData.newTile.Origin = new Point16(1, 0);
			TileObjectData.newTile.Height = 2;
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(120, 85, 60), Language.GetText("MapObject.Painting"));
		}
	}
	public class A_Perfect_World_Item : ModItem {
		public override void SetStaticDefaults() {
			OriginsSets.Items.PaintingsNotFromVendor[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<A_Perfect_World>());
			Item.value = Item.sellPrice(gold: 2);
		}
	}
}
