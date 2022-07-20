using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Other {
    public class Shotty_x2 : ModItem {
        public override string Texture => "Origins/Items/Weapons/Other/2_In_1_Shotty";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("2-in-1 Shotty");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Boomstick);
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.Boomstick, 2);
            recipe.AddIngredient(ModContent.ItemType<Adhesive_Wrap>(), 6);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		    for(int i = Main.rand.Next(5,8); i-->0;) {
                Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.5f), type, damage, knockback, player.whoAmI);
            }
            return false;
        }
    }
    public class Shotty_x3 : ModItem {
        public override string Texture => "Origins/Items/Weapons/Other/3_In_1_Shotty";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("3-in-1 Shotty");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Boomstick);
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.Boomstick, 3);
            recipe.AddIngredient(ModContent.ItemType<Adhesive_Wrap>(), 9);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
            recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.Boomstick);
            recipe.AddIngredient(ModContent.ItemType<Adhesive_Wrap>(), 9);
            recipe.AddIngredient(ModContent.ItemType<Shotty_x2>());
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            for(int i = Main.rand.Next(8,12); i-->0;) {
                Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.5f), type, damage, knockback, player.whoAmI);
            }
            return false;
        }
    }
}
