using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Artifiber : ComplexFrameTile {
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(143, 114, 94));
			mergeID = TileID.WoodBlock;
			DustType = DustID.WoodFurniture;
		}
		protected override IEnumerable<TileOverlay> GetOverlays() {
			yield return new TileMergeOverlay(merge + "Murk_Overlay", TileType<Murky_Sludge>());
			yield return new TileMergeOverlay(merge + "Murk_Overlay", TileType<Ashen_Murky_Sludge_Grass>());
		}
	}
	public class Artifiber_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Wood;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Artifiber>());
		}
	}
}
