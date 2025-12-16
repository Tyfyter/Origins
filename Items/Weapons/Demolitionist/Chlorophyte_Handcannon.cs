using Origins.Items.Weapons.Ammo;
using PegasusLib;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Chlorophyte_Handcannon : ModItem {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			ID = Type;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Musket);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 72;
			Item.width = 56;
			Item.height = 26;
			Item.useTime = 57;
			Item.useAnimation = 57;
			Item.shoot = ModContent.ProjectileType<Chlorophyte_Slug_P>();
			Item.useAmmo = ModContent.ItemType<Metal_Slug>();
			Item.knockBack = 8f;
			Item.shootSpeed = 9f;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.Lime;
			Item.UseSound = Origins.Sounds.Krunch.WithPitch(-0.25f);
			Item.autoReuse = true;
            Item.ArmorPenetration += 6;
        }
		public override Vector2? HoldoutOffset() => Vector2.Zero;
		public override void AddRecipes() => Recipe.Create(Type)
			.AddIngredient(ItemID.ChlorophyteBar, 18)
			.AddIngredient(ItemID.IllegalGunParts, 2)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == Metal_Slug_P.ID) type = Item.shoot;
            Vector2 offset = (velocity.RotatedBy(MathHelper.PiOver2 * -player.direction) * 5) / velocity.Length();
            position += offset;
        }
	}
	public class Chlorophyte_Slug_P : Metal_Slug_P {
		public override string Texture => "Origins/Projectiles/Ammo/Acrid_Slug_P";
		public override void PostAI() {
			if (Projectile.ai[0].CycleUp(10) && Projectile.IsLocallyOwned()) {
				int type = ModContent.ProjectileType<Chlorophyte_Handcannon_Bubble>();
				Vector2 perp = new Vector2(Projectile.velocity.Y, -Projectile.velocity.X) * 0.1f;
				Projectile.SpawnProjectile(Projectile.GetSource_FromAI(),
					Projectile.Center,
					perp,
					type,
					Projectile.damage / 2,
					Projectile.knockBack
				);
				Projectile.SpawnProjectile(Projectile.GetSource_FromAI(),
					Projectile.Center,
					-perp,
					type,
					Projectile.damage / 2,
					Projectile.knockBack
				);
			}
		}
	}
	public class Chlorophyte_Handcannon_Bubble : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.aiStyle = 0;
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.friendly = true;
			Projectile.penetrate = 7;
			Projectile.timeLeft = 90;
			Projectile.alpha = 0;
			Projectile.hide = false;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
			Projectile.extraUpdates = 0;
		}
		public override void AI() {
			Projectile.velocity *= 0.97f;
			if (Projectile.frameCounter.CycleUp(5)) Projectile.frame.CycleUp(Main.projFrames[Type]);
			Projectile.Opacity = Projectile.timeLeft / 30f;
		}
		public override Color? GetAlpha(Color lightColor) => new Color(255, 255, 255, 180) * Projectile.Opacity;
	}
}
