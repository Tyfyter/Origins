using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Nova_Cascade : ModItem {
		static short glowmask;
		public static int ID { get; private set; }

		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			ID = Type;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Musket);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.noMelee = true;
			Item.damage = 180;
			Item.width = 56;
			Item.height = 26;
			Item.useTime = 57;
			Item.useAnimation = 57;
			Item.shoot = ModContent.ProjectileType<Metal_Slug_P>();
			Item.useAmmo = ModContent.ItemType<Metal_Slug>();
			Item.knockBack = 8f;
			Item.shootSpeed = 12f;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.Red;
			Item.UseSound = Origins.Sounds.HeavyCannon.WithPitchRange(0.9f, 1f);
			Item.autoReuse = true;
			Item.glowMask = glowmask;
		}
		public override Vector2? HoldoutOffset() {
			return Vector2.Zero;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.IllegalGunParts, 2)
			.AddIngredient(ModContent.ItemType<Nova_Fragment>(), 18)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Vector2 offset = (velocity.RotatedBy(MathHelper.PiOver2 * -player.direction) * 6) / velocity.Length();
			position += offset;
		}
	}
	public class Nova_Cascade_Explosion : ModProjectile, IIsExplodingProjectile {
		public override string Texture => "Origins/CrossMod/Thorium/Items/Weapons/Bard/Sonorous_Shredder_P";
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item62);
				Projectile.ai[0] = 1;
			}
			ExplosiveGlobalProjectile.DealSelfDamage(Projectile);
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding => true;
	}
}
