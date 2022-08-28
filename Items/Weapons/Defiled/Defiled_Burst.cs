using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Defiled {
    public class Defiled_Burst : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("The Kruncher");
            Tooltip.SetDefault("Very pointy");
            glowmask = Origins.AddGlowMask(this);
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.damage = 6;
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;
            Item.crit = -1;
            Item.width = 56;
            Item.height = 18;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5;
            Item.shootSpeed = 15f;
            Item.shoot = ProjectileID.Bullet;
            Item.useAmmo = AmmoID.Bullet;
            Item.value = 15000;
            Item.useTurn = false;
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = Origins.Sounds.Krunch.WithPitch(-0.05f);
            Item.glowMask = glowmask;
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		    for(int i = 0; i<5; i++)Projectile.NewProjectile(source, position, velocity.RotatedByRandom(i/10f), type, damage, knockback, player.whoAmI);
            return false;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.IllegalGunParts, 3);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 6);
            recipe.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 3);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
	}
}
