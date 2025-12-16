using Origins.Buffs;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Projectiles.Weapons {
	public class Brine_Droplet : ModProjectile, IElementalProjectile {
		public ushort Element => Elements.Acid;
		public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.DamageType = DamageClass.Default;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
			Projectile.width = Projectile.height = 10;
			Projectile.light = 0;
			Projectile.timeLeft = 180;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 5;
			Projectile.ArmorPenetration += 26;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				Projectile.DamageType = itemUse.Item.DamageType;
			} else if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile projParent) {
				Projectile.DamageType = projParent.DamageType;
			}
		}
		public override void AI() {
			if (Projectile.ai[1] <= 0/*projectile.timeLeft<168*/) {
				Lighting.AddLight(Projectile.Center, 0, 0.75f * Projectile.scale, 0.3f * Projectile.scale);
				if (Projectile.timeLeft % 3 == 0) {
					Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Projectile.velocity * -0.25f, 100, new Color(0, 255, 0), Projectile.scale);
					dust.shader = GameShaders.Armor.GetShaderFromItemId(ItemID.AcidDye);
					dust.noGravity = false;
					dust.noLight = true;
				}
			} else {
				Projectile.Center = Main.player[Projectile.owner].itemLocation + Projectile.velocity;
				Projectile.ai[1]--;
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.ai[1] > 0) {
				Projectile.velocity -= oldVelocity - Projectile.velocity;
				return false;
			}
			return true;
		}
		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 7; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, 10, 10, DustID.Electric, 0, 0, 100, new Color(0, 255, 0), 1.25f * Projectile.scale);
				dust.shader = GameShaders.Armor.GetShaderFromItemId(ItemID.AcidDye);
				dust.noGravity = true;
				dust.noLight = true;
			}
			Projectile.friendly = true;
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = (int)(96 * Projectile.scale);
			Projectile.height = (int)(96 * Projectile.scale);
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.damage = (int)(Projectile.damage * 0.75f);
			Projectile.Damage();
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.ai[1] <= 0) {
				Projectile.penetrate = 0;
			}
			target.AddBuff(Toxic_Shock_Debuff.ID, 180);
			Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Electric, 0, 0, 100, new Color(0, 255, 0), 1.25f * Projectile.scale);
			dust.shader = GameShaders.Armor.GetShaderFromItemId(ItemID.AcidDye);
			dust.noGravity = false;
			dust.noLight = true;
		}
		public override bool PreDraw(ref Color lightColor) {
			return false;
		}
	}
}