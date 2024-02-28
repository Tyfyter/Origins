using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Ace_Shrapnel : ModItem {
        public string[] Categories => new string[] {
            "Launcher"
        };
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Flamethrower);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 18;
			Item.noMelee = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 1;
			Item.useAnimation = 20;
			Item.shoot = ModContent.ProjectileType<Impeding_Shrapnel_Shard>();
			Item.shootSpeed = 2;
			Item.useAmmo = ModContent.ItemType<Scrap>();
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.consumeAmmoOnFirstShotOnly = true;
		}
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<NE8>(), 10);
            recipe.AddIngredient(ModContent.ItemType<Scrap>(), 15);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            velocity = velocity.RotatedByRandom(0.27f);
			type = Item.shoot;
		}
    }
}
