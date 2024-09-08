using Origins.Dev;
using Origins.Water;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.Graphics.Capture.IL_CaptureBiome.Sets;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;

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
			//"WaterFountain"
		];
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Fabricator>());
			Item.value = Item.buyPrice(gold: 4);
		}
	}
}