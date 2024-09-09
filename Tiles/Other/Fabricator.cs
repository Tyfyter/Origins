using Microsoft.Xna.Framework;
using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Other {
	public class Fabricator : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileLighted[Type] = true;
			//TileID.Sets.HasOutlines[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
			TileObjectData.newTile.Width = 4;
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(6, 157, 44), CreateMapEntryName());
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 4) {
				frameCounter = 0;
				frame = (frame + 1) % 6;
			}
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			frameYOffset = Main.tileFrame[type] * 54;
		}
	}
	public class Fabricator_Item : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Furniture",
			"CraftingStation"
		];
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Fabricator>());
			Item.value = Item.buyPrice(platinum: 1);
			Item.rare = ItemRarityID.LightPurple;
		}
	}
}