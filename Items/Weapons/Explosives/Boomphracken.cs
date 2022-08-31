using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Other;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Explosives {
    public class Boomphracken : ModItem {
        static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Boomphracken");
			Tooltip.SetDefault("Chance to throw an explosive when used\n'He works his work, I work mine.'");
            glowmask = Origins.AddGlowMask(this);
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Musket);
            Item.damage = 48;
            Item.width = 56;
            Item.height = 28;
			Item.value = 200000;
			Item.useTime = 57;
			Item.useAnimation = 57;
            Item.shoot = ModContent.ProjectileType<Boomphracken_P>();
            Item.useAmmo = ModContent.ItemType<Giant_Metal_Slug>();
            Item.knockBack = 10f;
            Item.shootSpeed = 24f;
			Item.rare = ItemRarityID.Pink;
            Item.UseSound = Origins.Sounds.Krunch.WithPitch(-0.25f);
            Item.autoReuse = true;

        }
		public override Vector2? HoldoutOffset() {
			return Vector2.Zero;
		}
		public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.IllegalGunParts, 8);//is this intentional? each of these costs 20 gold
            recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Shard>(), 60);
            recipe.AddIngredient(ModContent.ItemType<Cleaver_Rifle>());
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            if (type == Giant_Metal_Slug_P.ID) type = Item.shoot;
		}
	}
    public class Boomphracken_P  : ModProjectile {
        public override string Texture => "Origins/Projectiles/Ammo/Boomphracken_P";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Obsidian Slug");
		}
		public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.ExplosiveBullet);
            Projectile.width = 10;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 900;
            Projectile.alpha = 0;
            //projectile.aiStyle = 14;
            //projectile.usesLocalNPCImmunity = true;
            //projectile.localNPCHitCooldown = 7;
		}
		public override void AI() {
			
		}
	}
}
