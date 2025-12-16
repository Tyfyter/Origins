using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Microsoft.Xna.Framework.MathHelper;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Demolitionist {
	public class Ace_Shrapnel_Old : ModItem {
		public override void SetDefaults() {
			Item.DefaultToLauncher(85, 28, 72, 30);
			Item.shootSpeed = 6;
			Item.shoot = ModContent.ProjectileType<Ace_Shrapnel_Old_P>();
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.Lime;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			type -= ModContent.ProjectileType<Ace_Shrapnel_Old_P>();
			type /= 3;
			Projectile.NewProjectile(source, position, velocity, Item.shoot, damage, knockback, player.whoAmI, 6 + type, 0 - type);
			return false;
		}
	}
	public class Ace_Shrapnel_Old_P : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		internal sbyte[] npcCDs = null;

		public static int maxHits = 3;
		public static int hitCD = 5;

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 0;
			Projectile.width = Projectile.height = 10;
			Projectile.light = 0;
			Projectile.timeLeft = 168;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			Projectile.ignoreWater = true;
			if (npcCDs is null) npcCDs = new sbyte[Main.maxNPCs];
		}
		public override void AI() {
			Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, Scale: 0.4f).noGravity = true;
			if (Projectile.ai[0] > 0 && Projectile.timeLeft % 6 == 0) {
				Projectile.ai[0]--;
				if (Projectile.velocity.Length() < 1) {
					Vector2 v = Main.rand.NextVector2Unit() * 6;
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + v * 8, v.RotatedBy(PiOver2), ModContent.ProjectileType<Ace_Shrapnel_Old_Shard>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI, Projectile.ai[1] + 1);
					return;
				}
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(1) * 1.1f, ModContent.ProjectileType<Ace_Shrapnel_Old_Shard>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI, Projectile.ai[1] + 1);
			}
		}
		public override bool? CanHitNPC(NPC target) {
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			return false;
		}
		internal bool AddHit(int i) {
			if (Projectile.localNPCImmunity[i] <= 0) {
				npcCDs[i] = 0;
			}
			Projectile.localNPCImmunity[i] = hitCD;
			if (npcCDs[i]++ == 0) {
				return false;
			}
			return true;
		}
	}
	public class Ace_Shrapnel_Old_Shard : ModProjectile {

		const float cohesion = 0.5f;

		const double chaos = 0.1f;

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BoneGloveProj;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.aiStyle = 0;
			Projectile.penetrate = 3;
			Projectile.extraUpdates = 0;
			Projectile.width = Projectile.height = 10;
			Projectile.timeLeft = 120;
			Projectile.ignoreWater = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void AI() {
			Dust.NewDustPerfect(Projectile.Center, DustID.Stone, Vector2.Zero).noGravity = true;
			if (Projectile.ai[0] >= 0) {
				Projectile center = Main.projectile[(int)Projectile.ai[0]];
				if (!center.active) {
					Projectile.ai[0] = -1;
					return;
				}
				Projectile.velocity = Projectile.velocity.RotatedByRandom(chaos);
				//float angle = projectile.velocity.ToRotation();
				float targetAngle = (center.Center - Projectile.Center).ToRotation();
				Projectile.velocity = (Projectile.velocity + new Vector2(cohesion * (Projectile.ai[1] > 1 ? 2 : 1), 0).RotatedBy(targetAngle)).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();
				//projectile.velocity = projectile.velocity.RotatedBy(Clamp((float)AngleDif(targetAngle,angle), -0.05f, 0.05f));
				//Dust.NewDustDirect(projectile.Center+new Vector2(16,0).RotatedBy(targetAngle), 0, 0, 6, Scale:2).noGravity = true;
			}
		}
		public override bool? CanHitNPC(NPC target) {
			if (Projectile.ai[0] == -1) return false;
			Projectile parent = Main.projectile[(int)Projectile.ai[0]];
			if (parent.ModProjectile is Ace_Shrapnel_Old_P center) {
				return (parent.localNPCImmunity[target.whoAmI] > 0 && center.npcCDs[target.whoAmI] > 0) ? (bool?)false : null;
			}
			return null;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.immune[Projectile.owner] /= 2;
			if (target.life <= 0 && Projectile.ai[1] < 5) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Ace_Shrapnel_Old_P>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 8 - Projectile.ai[1], Projectile.ai[1]);
			} else {
				if (Main.projectile[(int)Projectile.ai[0]]?.ModProjectile is Ace_Shrapnel_Old_P center && center.AddHit(target.whoAmI)) {
					Projectile.penetrate++;
				}
			}
		}
	}
}
