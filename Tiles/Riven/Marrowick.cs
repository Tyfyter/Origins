using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Walls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
    public class Marrowick : OriginTile {
        public string[] Categories => [
            "Plant"
        ];
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            TileID.Sets.DrawsWalls[Type] = true;
            AddMapEntry(new Color(165, 175, 100));
			mergeID = TileID.WoodBlock;
            HitSound = SoundID.NPCHit2;
            DustType = DustID.Sand;
        }
	}
	public class Marrowick_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Marrowick>());
		}
        public override void AddRecipes() {
            Recipe.Create(Type)
            .AddIngredient(ModContent.ItemType<Marrowick_Wall_Item>(), 4)
            .AddTile(TileID.WorkBenches)
            .Register();

            Recipe.Create(ModContent.ItemType<Marrowick_Wall_Item>(), 4)
            .AddIngredient(this)
            .AddTile(TileID.WorkBenches)
            .Register();

            Recipe.Create(ModContent.ItemType<Marrowick_Bow>())
            .AddIngredient(ModContent.ItemType<Endowood_Wall_Item>(), 10)
            .AddTile(TileID.WorkBenches)
            .Register();

            Recipe.Create(ModContent.ItemType<Marrowick_Sword>())
            .AddIngredient(ModContent.ItemType<Endowood_Wall_Item>(), 7)
            .AddTile(TileID.WorkBenches)
            .Register();
        }
    }
}
