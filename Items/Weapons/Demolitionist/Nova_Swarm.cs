using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Microsoft.Xna.Framework.MathHelper;

using Origins.Dev;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;

namespace Origins.Items.Weapons.Demolitionist {
	public class Nova_Swarm : ModItem, ICustomWikiStat {
		public const float rocket_scale = 0.85f;
        public string[] Categories => new string[] {
            "Launcher"
        };
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[Type] = AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.RocketLauncher];
			ID = Type;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ProximityMineLauncher);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 110;
			Item.noMelee = true;
			Item.width = 60;
			Item.height = 30;
			Item.useTime = 12;
			Item.useAnimation = 12;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Red;
			Item.autoReuse = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Nova_Fragment>(), 18)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-7, -4);
		}
	}
	public class Nova_Swarm_P1 : ModProjectile {

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
			float angle = Projectile.velocity.ToRotation();
			Projectile.rotation = angle + PiOver2;
			float targetOffset = 0.9f;
			float targetAngle = 1;
			NPC target;
			float dist = 641;
			for (int i = 0; i < Main.npc.Length; i++) {
				target = Main.npc[i];
				if (!target.CanBeChasedBy()) continue;
				Vector2 toHit = (Projectile.Center.Clamp(target.Hitbox.Add(target.velocity)) - Projectile.Center);
				if (!Collision.CanHitLine(Projectile.Center + Projectile.velocity, 1, 1, Projectile.Center + toHit, 1, 1)) continue;
				float tdist = toHit.Length();
				float ta = (float)Math.Abs(GeometryUtils.AngleDif(toHit.ToRotation(), angle, out _));
				if (tdist <= dist && ta <= targetOffset) {
					targetAngle = ((target.Center + target.velocity) - Projectile.Center).ToRotation();
					targetOffset = ta;
					dist = tdist;
				}
			}
			if (dist < 641) Projectile.velocity = (Projectile.velocity + new Vector2(force, 0).RotatedBy(targetAngle)).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();
			int num248 = Dust.NewDust(Projectile.Center - Projectile.velocity * 0.5f - new Vector2(0, 4), 0, 0, DustID.Torch, 0f, 0f, 100);
			Dust dust3 = Main.dust[num248];
			dust3.scale *= 1f + Main.rand.Next(10) * 0.1f;
			dust3.velocity *= 0.2f;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.RocketI;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
		}
	}
	public class Nova_Swarm_P2 : Nova_Swarm_P1 {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RocketII;
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.RocketII;
			return true;
		}
	}
	public class Nova_Swarm_P3 : Nova_Swarm_P1 {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RocketIII;
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.RocketIII;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
		}
	}
	public class Nova_Swarm_P4 : Nova_Swarm_P3 {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RocketIV;
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.RocketIV;
			return true;
		}
	}
}
