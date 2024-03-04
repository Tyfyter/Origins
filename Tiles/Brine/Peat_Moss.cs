using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Brine {
    public class Peat_Moss : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileMergeDirt[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			RegisterItemDrop(ItemType<Peat_Moss_Item>());
			AddMapEntry(new Color(18, 160, 56));
			HitSound = SoundID.Dig;
		}
		public override void RandomUpdate(int i, int j) {
			if (!Framing.GetTileSafely(i, j + 1).HasTile) {
				if (TileObject.CanPlace(i, j + 1, TileType<Brineglow>(), 0, 0, out TileObject objectData, false, checkStay: true)) {
					objectData.style = 0;
					objectData.alternate = 0;
					objectData.random = 0;
					TileObject.Place(objectData);
				}
			}
		}
	}
    public class Peat_Moss_Item : MaterialItem {
		public override int ResearchUnlockCount => 100;
		public override int Value => Item.sellPrice(copper: 60);
		public override int Rare => ItemRarityID.Green;
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(ItemID.ExplosivePowder);
            recipe.AddIngredient(this, 2);
            recipe.AddTile(TileID.GlassKiln);
            recipe.DisableDecraft();
            recipe.Register();
        }
    }
}
