using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using PegasusLib;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Broths {
	public class Minishark_Broth : BrothBase {
		public static float Size => 64;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				FromHexRGB(0x5e3a00),
				FromHexRGB(0x914e06),
				FromHexRGB(0xac6b1e)
			];
			ItemID.Sets.FoodParticleColors[Type] = [
				FromHexRGB(0x2e2d2b)
			];
		}
		public override int Duration => 4;
		public override void PostDrawMinion(Projectile minion, Color lightColor) {
			if (!minion.minion && !minion.sentry) return;
			Texture2D texture = TextureAssets.Item[ItemID.Minishark].Value;
			float rotation = minion.GetGlobalProjectile<MinionGlobalProjectile>().brothEffectAngle;
			Main.EntitySpriteDraw(
				texture,
				minion.Center - Main.screenPosition,
				null,
				lightColor,
				rotation,
				texture.Size() * 0.5f,
				minion.scale,
				float.Cos(rotation) < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None
			);
		}
		public override void UpdateMinion(Projectile minion, int time) {
			if (!minion.IsLocallyOwned() || (!minion.minion && !minion.sentry)) return;
			ref int timer = ref minion.GetEffectTimer<Minishark_Broth_Timer>();
			float distanceFromTarget = 2000f * 2000f;
			Vector2 targetCenter = minion.position;
			int target = -1;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (!isPriorityTarget && distanceFromTarget > 700 * 700) {
					distanceFromTarget = 700 * 700;
				}
				if (!npc.CanBeChasedBy(minion)) return;
				float between = Vector2.DistanceSquared(npc.Center, minion.Center);
				if (between < distanceFromTarget) {
					distanceFromTarget = between;
					target = npc.whoAmI;
					targetCenter = npc.Center + npc.velocity * 8;
					foundTarget = true;
				}
			}
			if (timer >= 7) {
				Player player = Main.player[minion.owner];
				Item minishark = ContentSamples.ItemsByType[ItemID.Minishark];
				if (player.OriginPlayer().GetMinionTarget(targetingAlgorithm) && player.PickAmmo(minishark, out int projToShoot, out float speed, out int damage, out float knockBack, out int usedAmmoItemId)) {
					Vector2 dir = minion.Center.DirectionTo(targetCenter);
					minion.SpawnProjectile(
						player.GetSource_ItemUse_WithPotentialAmmo(minishark, usedAmmoItemId),
						minion.Center,
						dir * speed + Main.rand.NextVector2Square(-40, 41) * 0.01f,
						projToShoot,//ModContent.ProjectileType<Minishark_Broth_Bullet>(),
						damage,
						knockBack
					);
					minion.GetGlobalProjectile<MinionGlobalProjectile>().brothEffectAngle = dir.ToRotation();
				}
				timer = 0;
			}
		}
	}
	public class Minishark_Broth_Timer : ProjectileEffectTimer {
		public override bool StartAtZero => true;
		public override bool AppliesToEntity(Projectile projectile) => projectile.minion || projectile.sentry;
	}
	public class Minishark_Broth_Bullet : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bullet;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.DamageType = DamageClasses.RangedSummon;
			AIType = ProjectileID.Bullet;
		}
	}
}
