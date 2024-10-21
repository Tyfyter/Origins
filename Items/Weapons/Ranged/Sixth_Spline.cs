using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Weapons.Ranged {
	public class Sixth_Spline : ModItem, ICustomWikiStat {
		static short glowmask;
		public static WeightedRandom<Sixth_Spline_Projectile> Projectiles { get; private set; }  = new();
        public string[] Categories => [
            "Gun"
        ];
        public static int ID { get; set; }
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			ID = Type;
		}
		public override void SetDefaults() {
			Item.DefaultToRangedWeapon(ProjectileID.Bullet, ModContent.ItemType<Scrap>(), 12, 6);
			Item.damage = 40;
			Item.crit = -4;
			Item.useAnimation = 12;
			Item.useTime = 12;
			Item.width = 86;
			Item.height = 22;
			Item.autoReuse = true;
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item11;
			Item.glowMask = glowmask;
		}
		public override Vector2? HoldoutOffset() => Vector2.Zero;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Sixth_Spline_Projectile proj = Projectiles.Get();
			type = proj.Type;
			damage = (int)(damage * proj.DamageMult);
			knockback *= proj.KnockbackMult;
			velocity = velocity.RotatedByRandom(0.1f);
		}
	}
	public record struct Sixth_Spline_Projectile(int Type, float DamageMult, float KnockbackMult);
	public class Sixth_Spline_Nut : ModProjectile {
		public override void SetStaticDefaults() {
			Sixth_Spline.Projectiles.Add(new(Type, 1, 1), 1.2f);
		}
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 1;
		}
		public override void AI() {
			Projectile.rotation += Projectile.direction * 0.5f;
			Projectile.velocity.Y += 0.03f;
		}
	}
	public class Sixth_Spline_Wrench : Sixth_Spline_Nut {
		public override void SetStaticDefaults() {
			Sixth_Spline.Projectiles.Add(new(Type, 0.9f, 0.9f));
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(ModContent.BuffType<Slow_Debuff>(), 60);
		}
	}
	public class Sixth_Spline_Piston : Sixth_Spline_Nut {
		public override void SetStaticDefaults() {
			Sixth_Spline.Projectiles.Add(new(Type, 0.95f, 2f));
		}
	}
	public class Sixth_Spline_Soldering_Iron : Sixth_Spline_Nut {
		public override void SetStaticDefaults() {
			Sixth_Spline.Projectiles.Add(new(Type, 0.9f, 0.9f), 0.5f);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire3, 300);
			target.AddBuff(BuffID.Bleeding, 300);
		}
	}
}
