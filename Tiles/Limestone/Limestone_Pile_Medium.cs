using Origins.Items.Other.Testing;
using Origins.Tiles.Brine;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Limestone {
	public class Limestone_Pile_Medium : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = false;
			Main.tileNoFail[Type] = true;
			AddMapEntry(new(203, 194, 149));

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Origin = new Point16(0, 2);
			if (!Name.EndsWith("_Fake")) TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.addTile(Type);

			DustType = DustID.Sand;
		}
	}
	public class Limestone_Pile_Medium_Fake : Limestone_Pile_Medium {
		public override string Texture => base.Texture[..^"_Fake".Length];
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<Limestone_Item>(), Type, 0, 1, 2);
			RegisterItemDrop(ModContent.ItemType<Limestone_Item>());
		}
	}
}
