using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Rocodile : ModItem {

		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Rocodile");
			// Tooltip.SetDefault("Uses rockets as ammo\n'Older cousin of the minishark'");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ProximityMineLauncher);
			Item.damage = 100;
			Item.useTime = 50;
			Item.useAnimation = 50;
			Item.shoot = ModContent.ProjectileType<Rocodile_P1>();
			Item.value = Item.sellPrice(gold: 7);
			Item.rare = ItemRarityID.Lime;
			Item.autoReuse = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			type = Item.shoot + (type - Item.shoot) / 3;
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 8);
			return false;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, 0);
		}
	}
	public class Rocodile_P1 : ModProjectile {

		const float force = 1;

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RocketI;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RocketI);
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.width -= 4;
			Projectile.height -= 4;
			Projectile.scale = 0.75f;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			float dist = 641;
			if (dist < 641) Projectile.velocity = (Projectile.velocity + new Vector2(force, 0).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length());
			int num248 = Dust.NewDust(Projectile.Center - Projectile.velocity * 0.5f - new Vector2(0, 4), 0, 0, DustID.Torch, 0f, 0f, 100);
			Dust dust3 = Main.dust[num248];
			dust3.scale *= 1f + Main.rand.Next(10) * 0.1f;
			dust3.velocity *= 0.2f;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Wet, 600);
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.RocketI;
			return true;
		}
		public override void Kill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
		}
	}
	public class Rocodile_P2 : Rocodile_P1 {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RocketII;
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.RocketII;
			return true;
		}
	}
	public class Rocodile_P3 : Rocodile_P2 {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RocketIII;
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.RocketIII;
			return true;
		}
		public override void Kill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
		}
	}
	public class Rocodile_P4 : Rocodile_P3 {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RocketIV;
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.RocketIV;
			return true;
		}
	}
}
