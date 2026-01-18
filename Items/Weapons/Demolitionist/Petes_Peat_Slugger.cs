using Origins.Dev;
using Origins.Items.Weapons.Ammo;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Empowerments;

namespace Origins.Items.Weapons.Demolitionist {
	public class Petes_Peat_Slugger : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.ToxicSource
		];
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			ID = Type;
		}
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 62;
			Item.crit = 7;
			Item.width = 56;
			Item.height = 26;
			Item.useTime = 50;
			Item.useAnimation = 50;
			Item.shoot = ModContent.ProjectileType<Petes_Peat_Slug>();
			Item.useAmmo = ModContent.ItemType<Metal_Slug>();
			Item.knockBack = 8f;
			Item.shootSpeed = 12f;
			Item.value = Item.buyPrice(gold: 30);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = Origins.Sounds.Krunch.WithPitch(-0.25f);
			Item.autoReuse = true;
			Item.ArmorPenetration += 6;
		}
		public override Vector2? HoldoutOffset() {
			return Vector2.Zero;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == Metal_Slug_P.ID) type = Item.shoot;
			Vector2 offset = (velocity.RotatedBy(MathHelper.PiOver2 * -player.direction) * 5) / velocity.Length();
			position += offset;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			type = ModContent.ProjectileType<Petes_Peat>();
			const float spread = 0.25f;
			for (int i = -2; i <= 2; i++) {
				if (i == 0) continue;
				Projectile.NewProjectile(
					source,
					position,
					velocity.RotatedBy((i - Main.rand.NextFloat(Math.Sign(i) * 0.9f)) * spread),
					type,
					damage,
					knockback
				);
			}
			return true;
		}
	}
	public class Petes_Peat_Slug : Metal_Slug_P {
		public override string Texture => typeof(Metal_Slug_P).GetDefaultTMLName();
		public override void AI() {
			base.AI();
			if (Projectile.ai[0].CycleUp(10)) {
				Projectile.SpawnProjectile(null,
					Projectile.Center,
					Projectile.velocity * 0.5f,
					ModContent.ProjectileType<Petes_Peat>(),
					Projectile.damage,
					Projectile.knockBack
				);
			}
		}
		public override void OnKill(int timeLeft) {
			int type = ModContent.ProjectileType<Petes_Peat>();
			for (int i = 0; i < 5; i++) {
				Projectile.NewProjectile(
					Projectile.GetSource_Death(),
					Projectile.Center,
					Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(6, 8),
					type,
					(int)(Projectile.damage*0.6f),
					Projectile.knockBack
				);
			}
			base.OnKill(timeLeft);
		}
	}
	public class Petes_Peat : Peatball_P {
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.timeLeft = 90;
		}
	}
	/*public override void OnKill(int timeLeft) {
		int t = ModContent.ProjectileType<Acid_Shot>();
		for (int i = Main.rand.Next(3); i < 6; i++) Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Main.rand.NextVector2Unit() * 4) + (Projectile.velocity / 8), t, Projectile.damage / 8, 6, Projectile.owner, ai1: -0.5f).scale = 0.85f;
	} */
}
