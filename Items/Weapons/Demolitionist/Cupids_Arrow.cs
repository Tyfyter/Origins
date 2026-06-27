using Microsoft.Xna.Framework.Graphics;
using Origins.Dusts;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.NPCs.Ashen;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Cupids_Arrow : ModItem, ICustomDrawItem {
		public override void SetStaticDefaults() => Origins.AddGlowMask(this);
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Cupids_Arrow_P>(50, 16, 8, 60, 24, true);
			Item.useAnimation *= 4;
			Item.useLimitPerAnimation = 3;
			Item.value = Item.sellPrice(gold: 7);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item11;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.holdStyle = ItemHoldStyleID.HoldRadio;
		}
		public override void HoldStyle(Player player, Rectangle heldItemFrame) {
			UseStyle(player, heldItemFrame);
			player.itemRotation = 0;
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) => player.itemLocation = player.MountedCenter + player.Directions(-7, -3);
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			position += velocity.SafeNormalize(default).RotatedBy(player.direction * MathHelper.PiOver2) * -6 * player.gravDir;
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			DrawData data = new(
				itemTexture,
				drawInfo.ItemLocation.Floor() - Main.screenPosition,
				null,
				lightColor,
				drawInfo.drawPlayer.itemRotation,
				drawInfo.itemEffect.ApplyToOrigin(new(8, 18), itemTexture.Bounds),
				drawInfo.drawPlayer.GetAdjustedItemScale(Item),
				drawInfo.itemEffect
			);
			drawInfo.DrawDataCache.Add(data);
			data.texture = TextureAssets.GlowMask[Item.glowMask].Value;
			data.color = Color.White;
			drawInfo.DrawDataCache.Add(data);
		}
	}
	public class Cupids_Arrow_P : ModProjectile, ICanisterProjectile {
		public override string Texture => ICanisterProjectile.base_texture_path + "Rocket_Thrust";
		public static AutoLoadingTexture thrustTexture = ICanisterProjectile.base_texture_path + "Rocket_Thrust_Tintable";
		public static AutoLoadingTexture outerTexture = ICanisterProjectile.base_texture_path + "Rocket_Outer";
		public static AutoLoadingTexture innerTexture = ICanisterProjectile.base_texture_path + "Rocket_Inner";
		public AutoLoadingTexture OuterTexture => outerTexture;
		public AutoLoadingTexture InnerTexture => innerTexture;
		public static int FreeFuel => 90;
		public float FuelMult => Utils.Remap(Projectile.ai[0], 0, FreeFuel, 1, 0);
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 40;
			ProjectileID.Sets.IsARocketThatDealsDoubleDamageToPrimaryEnemy[Type] = true;
			Defensive_Turret.TargetProjectilesLow[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RocketI);
			Projectile.extraUpdates = 1;
			Projectile.timeLeft = 420;
			Projectile.penetrate = 1;
			Projectile.aiStyle = 0;
			Projectile.alpha = 255;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void OnSpawn(IEntitySource source) {
			if (Projectile.TryGetGlobalProjectile(out ExplosiveGlobalProjectile global)) global.projectileBlastRadius *= 1.5f;
		}
		public override void AI() {
			if (++Projectile.localAI[0] < 7) return;
			Color dustColor = default;
			int dustType = DustID.Torch;
			if (Projectile.TryGetGlobalProjectile(out CanisterGlobalProjectile global)) {
				Projectile.ai[0] += global.CanisterData?.Ammo is Rocket_Dummy_Canister ? 0.25f : 0.5f;
				if (global.CanisterData?.HasSpecialEffect ?? false) {
					dustType = Tintable_Torch_Dust.ID;
					dustColor = global.CanisterData.InnerColor with { A = 100 };
				}
			}
			Vector2 dustBasePos = new(Projectile.position.X + 3f, Projectile.position.Y + 3f);
			for (int i = 0; i < 2; i++) {
				Vector2 offset = Projectile.velocity * i * 0.5f;

				if (Main.rand.NextBool(2)) {
					Dust dust = Dust.NewDustDirect(
						dustBasePos + offset - Projectile.velocity * 0.5f,
						Projectile.width - 8,
						Projectile.height - 8,
						dustType,
						0f,
						0f,
						100,
						dustColor
					);
					dust.scale *= 1.4f + Main.rand.Next(10) * 0.1f;
					dust.velocity *= 0.2f;
					dust.noGravity = true;
				}

				if (Main.rand.NextBool(2)) {
					Dust dust = Dust.NewDustDirect(
						dustBasePos + offset - Projectile.velocity * 0.5f,
						Projectile.width - 8,
						Projectile.height - 8,
						DustID.Smoke,
						0f,
						0f,
						100,
						default,
						0.5f
					);
					dust.fadeIn = 0.5f + Main.rand.Next(5) * 0.1f;
					dust.velocity *= 0.05f;
				}
			}

			float fuelMult = FuelMult;
			if (fuelMult > 0) {
				fuelMult = float.Sqrt(fuelMult);
				float targetWeight = 300;
				Vector2 targetPos = default;
				bool foundTarget = Main.player[Projectile.owner].DoHoming((target) => {
					Vector2 currentPos = target.Center;
					float dist = Math.Abs(Projectile.Center.X - currentPos.X) + Math.Abs(Projectile.Center.Y - currentPos.Y);
					if (target is Player) dist *= 2.5f;
					if (dist < targetWeight && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, target.position, target.width, target.height)) {
						targetWeight = dist;
						targetPos = currentPos;
						return true;
					}
					return false;
				});

				if (foundTarget) {
					float scaleFactor = 16f * fuelMult;
					float lerpValue = 0.083333336f * Origins.HomingEffectivenessMultiplier[Projectile.type] * fuelMult;

					Vector2 targetVelocity = (targetPos - Projectile.Center).SafeNormalize(-Vector2.UnitY) * scaleFactor;
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, lerpValue);
				}
			}
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage += FuelMult;
		}
		public void CustomDraw(Projectile projectile, CanisterData canisterData, Color lightColor) {
			Vector2 origin = OuterTexture.Value.Size() * 0.5f;
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				InnerTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.InnerColor,
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
			Main.EntitySpriteDraw(
				OuterTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.OuterColor.MultiplyRGBA(lightColor),
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
			if (Projectile.ai[0] > FreeFuel) return;
			if (Projectile.TryGetGlobalProjectile(out CanisterGlobalProjectile global) && (global.CanisterData?.HasSpecialEffect ?? false)) {
				Main.EntitySpriteDraw(
					thrustTexture,
					projectile.Center - Main.screenPosition,
					null,
					canisterData.InnerColor,
					projectile.rotation,
					origin,
					projectile.scale,
					spriteEffects
				);
			} else {
				Main.EntitySpriteDraw(
					TextureAssets.Projectile[Type].Value,
					projectile.Center - Main.screenPosition,
					null,
					Color.White,
					projectile.rotation,
					origin,
					projectile.scale,
					spriteEffects
				);
			}
		}
	}
}
