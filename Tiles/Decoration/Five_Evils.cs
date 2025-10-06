using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Decoration {
    public class Five_Evils : ModTile {
		public string[] Categories => [
			"Painting"
		];
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.FramesOnKillWall[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
			TileObjectData.newTile.CoordinateHeights = [ 16, 16, 16, 16, 16 ];
			TileObjectData.newTile.Height = 5;
			TileObjectData.newTile.Width = 8;
			TileObjectData.newTile.Origin = new Point16(3, 2);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(99, 50, 30), Language.GetText("MapObject.Painting"));
		}
	}
	public class Five_Evils_Item : ModItem {
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Five_Evils>());
			Item.value = Item.sellPrice(gold: 2);
		}
	}
}
