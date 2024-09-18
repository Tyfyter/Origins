using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Walls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Defiled {
	public class Endowood : OriginTile {
        public string[] Categories => [
            "Plant"
        ];
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(40, 10, 40));
			mergeID = TileID.WoodBlock;
		}
	}
	public class Endowood_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Wood);
			Item.createTile = TileType<Endowood>();
		}
        public override void AddRecipes() {
            Recipe.Create(Type)
            .AddIngredient(ModContent.ItemType<Endowood_Wall_Item>(), 4)
            .AddTile(TileID.WorkBenches)
            .Register();

            Recipe.Create(ModContent.ItemType<Endowood_Wall_Item>(), 4)
            .AddIngredient(this)
            .AddTile(TileID.WorkBenches)
            .Register();

            Recipe.Create(ModContent.ItemType<Endowood_Bow>())
            .AddIngredient(ModContent.ItemType<Endowood_Wall_Item>(), 10)
            .AddTile(TileID.WorkBenches)
            .Register();

            Recipe.Create(ModContent.ItemType<Endowood_Sword>())
            .AddIngredient(ModContent.ItemType<Endowood_Wall_Item>(), 7)
            .AddTile(TileID.WorkBenches)
            .Register();
        }
    }
}
