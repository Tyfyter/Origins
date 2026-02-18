using Microsoft.Xna.Framework.Graphics;
using ModLiquidLib.Utils;
using Origins.Buffs;
using Origins.Items.Weapons.Demolitionist;
using Origins.Projectiles;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Brine.Boss {
	public class Lost_Diver_Torpedo_Tube : ModProjectile {
		public override string Texture => typeof(Torpedo_Tube_P).GetDefaultTMLName();
		public float Gravity => 0.1f;
		public float HomingThreshold => 5.5f;
		public (float @base, float speedFactor) HomingFactor => (0.01f, 0.002f);
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
			Origins.MagicTripwireRange[Type] = 40;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.extraUpdates = 0;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = 420;
			Projectile.penetrate = 1;
			Projectile.aiStyle = 0;
			Projectile.alpha = 255;
			Projectile.ignoreWater = true;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void AI() {
			if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height)) {
				float targetWeight = HomingThreshold;
				Vector2 targetDiff = default;
				bool foundTarget = false;
				if (Projectile.reflected) {
					foreach (NPC target in Main.ActiveNPCs) {
						if (target.GetWet(Liquids.Brine.ID) && target.CanBeChasedBy(Projectile)) {
							Vector2 currentDiff = target.Center - Projectile.Center;
							float dist = currentDiff.Length();
							currentDiff /= dist;
							float weight = Vector2.Dot(Projectile.velocity, currentDiff) * (500f / (dist + 100));
							if (weight > targetWeight && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, target.position, target.width, target.height)) {
								targetWeight = weight;
								targetDiff = currentDiff;
								foundTarget = true;
							}
						}
					}
				} else {
					foreach (Player target in Main.ActivePlayers) {
						if (!target.dead && target.GetWet(Liquids.Brine.ID)) {
							Vector2 currentDiff = target.Center - Projectile.Center;
							float dist = currentDiff.Length();
							currentDiff /= dist;
							float weight = Vector2.Dot(Projectile.velocity, currentDiff) * (500f / (dist + 100));
							if (weight > targetWeight && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, target.position, target.width, target.height)) {
								targetWeight = weight;
								targetDiff = currentDiff;
								foundTarget = true;
							}
						}
					}
				}

				if (foundTarget) {
					PolarVec2 velocity = (PolarVec2)Projectile.velocity;
					OriginExtensions.AngularSmoothing(
						ref velocity.Theta,
						targetDiff.ToRotation(),
						HomingFactor.@base + velocity.R * HomingFactor.speedFactor * Origins.HomingEffectivenessMultiplier[Projectile.type]
					);
					Projectile.velocity = (Vector2)velocity;
				}
			} else {
				Projectile.velocity.Y += Gravity;
			}
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
			if (++Projectile.frameCounter >= 4) {
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
			}
		}
		public override bool? CanHitNPC(NPC target) {
			if (Mildew_Creeper.FriendlyNPCTypes.Contains(target.type)) return false;
			return null;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.ScalingArmorPenetration += Brine_Pool_NPC.ScalingArmorPenetrationToCompensateForTSNerf;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			Projectile.penetrate--;
			if (target.GetWet(Liquids.Brine.ID)) target.AddBuff(Cavitation_Debuff.ID, 120);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.GetWet(Liquids.Brine.ID)) target.AddBuff(Cavitation_Debuff.ID, 120);
		}
		public override Color? GetAlpha(Color lightColor) {
			if (Projectile.alpha < 200) {
				return new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, (255 - Projectile.alpha) / 2);
			}
			return Color.Transparent;
		}
		public static AutoLoadingAsset<Texture2D> outerTexture = "Origins/Items/Weapons/Ammo/Canisters/Alkaline_Canister";
		public static AutoLoadingAsset<Texture2D> innerTexture = "Origins/Items/Weapons/Ammo/Canisters/Alkaline_Canister_Glow";
		public override bool PreDraw(ref Color lightColor) {
			Vector2 origin = new(50, 8);
			Vector2 canisterOrigin = new(8, 0);
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (Projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				innerTexture,
				Projectile.Center - Main.screenPosition,
				null,
				Color.White,
				Projectile.rotation + MathHelper.PiOver2,
				canisterOrigin,
				Projectile.scale,
				spriteEffects
			);
			Main.EntitySpriteDraw(
				outerTexture,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation + MathHelper.PiOver2,
				canisterOrigin,
				Projectile.scale,
				spriteEffects
			);
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				texture.Frame(verticalFrames: 3, frameY: Projectile.frame),
				new Color(255, 127, 35).MultiplyRGBA(lightColor),
				Projectile.rotation,
				origin,
				Projectile.scale,
				spriteEffects
			);
			return false;
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, false, SoundID.Item62, hostile: !Projectile.reflected, alsoFriendly: true);
		}
	}
}
