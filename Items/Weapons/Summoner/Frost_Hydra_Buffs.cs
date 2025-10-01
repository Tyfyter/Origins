using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Projectiles;
using PegasusLib;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Frost_Hydra_Buffs_Stats : GlobalItem {
		public override bool IsLoadingEnabled(Mod mod) => OriginConfig.Instance.FrostHydra;
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.StaffoftheFrostHydra;
		public override void SetDefaults(Item entity) {
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[entity.type] = [Slow_Debuff.ID];
			entity.damage = 75;
		}
	}
	public class Frost_Hydra_AI_Buffs : GlobalProjectile {
		public override bool IsLoadingEnabled(Mod mod) => OriginConfig.Instance.FrostHydra;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.type == ProjectileID.FrostHydra;
		public override bool PreAI(Projectile projectile) {
			if (projectile.localAI[0] == 0f) {
				projectile.localAI[0] = 1f;
				projectile.ai[0] = 40f;
				SoundEngine.PlaySound(SoundID.Item46, projectile.position);
				for (int i = 0; i < 80; i++) {
					Dust dust = Dust.NewDustDirect(projectile.position + Vector2.UnitY * 16, projectile.width, projectile.height - 16, DustID.FrostHydra);
					dust.velocity *= 2f;
					dust.noGravity = true;
					dust.scale *= 1.15f;
				}
			}

			projectile.velocity.X = 0f;
			projectile.velocity.Y += 0.2f;
			if (projectile.velocity.Y > 16f)
				projectile.velocity.Y = 16f;

			// Starting search distance
			float distanceFromTarget = 1000f;
			Vector2 targetCenter = default;
			int target = -1;
			bool hasPriorityTarget = false;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				bool isCurrentTarget = npc.whoAmI == projectile.ai[0];
				if ((isCurrentTarget || isPriorityTarget || !hasPriorityTarget) && npc.CanBeChasedBy()) {
					Vector2 pos = projectile.Center;
					int dir = Math.Sign(npc.Center.X - pos.X);
					float between = Vector2.Distance(npc.Center, projectile.Center);
					bool closer = distanceFromTarget > between;
					if (!closer) return;
					bool lineOfSight = CollisionExt.CanHitRay(pos, npc.Center);
					for (int j = 0; j < 5 && !lineOfSight; j++) {
						lineOfSight = CollisionExt.CanHitRay(pos, Main.rand.NextVector2FromRectangle(npc.Hitbox));
					}
					if (lineOfSight) {
						distanceFromTarget = between;
						targetCenter = npc.Center;
						target = npc.whoAmI;
						foundTarget = true;
						hasPriorityTarget = isPriorityTarget;
					}
				}
			}

			if (Main.player[projectile.owner].GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm)) {
				int frame = 0;
				if (projectile.frameCounter > 0)
					projectile.frameCounter--;

				if (projectile.frameCounter <= 0) {
					int oldDirection = projectile.spriteDirection;

					float xDiff = targetCenter.X - projectile.Center.X;
					float yDiff = Math.Abs(targetCenter.Y - projectile.Center.Y);

					if (xDiff < 0f)
						projectile.spriteDirection = -1;
					else
						projectile.spriteDirection = 1;
					xDiff = Math.Abs(xDiff);

					if (targetCenter.Y < projectile.Center.Y) {
						frame = (xDiff / yDiff) switch {
							< 1f / 3f => 4,
							< 1f / 2f => 3,
							< 2f => 2,
							< 3 => 1,
							_ => 0
						};
					} else {
						frame = 0;
					}
					int oldFrame = projectile.frame;
					projectile.frame = frame * 2;

					if (projectile.ai[0] > 40f)
						projectile.frame++;

					if (oldFrame != projectile.frame || oldDirection != projectile.spriteDirection) {
						projectile.frameCounter = 8;
						if (projectile.ai[0] <= 0f)
							projectile.frameCounter = 4;
					}
				}

				if (projectile.ai[0] <= 0f) {
					projectile.ai[0] = 60f;
					projectile.netUpdate = true;
					if (projectile.IsLocallyOwned()) {
						Vector2 projectileOrigin = CalculateProjectileOrigin(projectile, frame);

						if (distanceFromTarget < 16 * 15) {
							projectile.ai[2] = 3;
						}

						Vector2 shotDirection = projectileOrigin.DirectionTo(targetCenter);
						(projectile.localAI[1], projectile.localAI[2]) = shotDirection;
						Shoot(projectile, projectileOrigin, shotDirection);
					}
				} else if (projectile.ai[2] > 0 && ((int)projectile.ai[0] % 5) == 0) {
					Shoot(projectile, CalculateProjectileOrigin(projectile, frame), shotDirection: new(projectile.localAI[1], projectile.localAI[2]));
					projectile.ai[2]--;
				}
			} else {
				if (projectile.ai[0] <= 60f && (projectile.frame & 1) != 0)
					projectile.frame--;
			}

			if (projectile.ai[0] > 0f)
				projectile.ai[0] -= 1f;
			return false;

			static Vector2 CalculateProjectileOrigin(Projectile projectile, int frame) {
				Vector2 projectileOrigin = projectile.Center;
				switch (frame) {
					case 0:
					projectileOrigin.Y += 12f;
					projectileOrigin.X += 24 * projectile.spriteDirection;
					break;
					case 1:
					projectileOrigin.Y += 0f;
					projectileOrigin.X += 24 * projectile.spriteDirection;
					break;
					case 2:
					projectileOrigin.Y -= 2f;
					projectileOrigin.X += 24 * projectile.spriteDirection;
					break;
					case 3:
					projectileOrigin.Y -= 6f;
					projectileOrigin.X += 14 * projectile.spriteDirection;
					break;
					case 4:
					projectileOrigin.Y -= 14f;
					projectileOrigin.X += 2 * projectile.spriteDirection;
					break;
				}

				if (projectile.spriteDirection < 0)
					projectileOrigin.X += 10f;
				return projectileOrigin;
			}
			static void Shoot(Projectile projectile, Vector2 projectileOrigin, Vector2 shotDirection) {
				int damage = projectile.damage;
				float knockback = projectile.knockBack;
				int type = ModContent.ProjectileType<Frost_Hydra_Sustained_Blast>();
				if (projectile.ai[2] == 0) {
					type = ProjectileID.FrostBlastFriendly;
					shotDirection *= 9;
					damage *= 2;
				} else {
					shotDirection *= 8;
					knockback *= 0.25f;
				}
				projectile.SpawnProjectile(null,
					projectileOrigin,
					shotDirection,
					type,
					damage,
					knockback
				);
			}
		}
	}
	public class Frost_Hydra_Blast_Explosion : GlobalProjectile {
		public override bool IsLoadingEnabled(Mod mod) => OriginConfig.Instance.FrostHydra;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.type == ProjectileID.FrostBlastFriendly;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.SummonTagDamageMultiplier[ProjectileID.FrostBlastFriendly] = 4;
		}
		public override void SetDefaults(Projectile projectile) {
			projectile.penetrate = 1;
			projectile.extraUpdates += 1;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = -1;
			projectile.appliesImmunityTimeOnSingleHits = true;
		}
		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Slow_Debuff.ID, 60 * 3);
			projectile.localNPCImmunity[target.whoAmI] = -1;
		}
		public override void OnKill(Projectile projectile, int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(projectile, 96, false, sound: SoundID.Shatter, fireDustAmount: 0, smokeDustAmount: 10, smokeGoreAmount: 0);
			Rectangle hitbox = projectile.Hitbox;
			float baseWidth = hitbox.Width;
			ProjectileLoader.ModifyDamageHitbox(projectile, ref hitbox);
			float dustMult = hitbox.Width / baseWidth;
			int fireDustAmount = (int)(20 * dustMult);
			Vector2 vel = projectile.velocity * 0.3f;
			for (int i = 0; i < fireDustAmount; i++) {
				Dust dust = Dust.NewDustDirect(
					hitbox.TopLeft(),
					hitbox.Width,
					hitbox.Height,
					DustID.IceTorch,
					0,
					0,
					100,
					default,
					3.5f
				);
				dust.noGravity = true;
				dust.velocity *= 7f;
				dust.velocity += vel;
				dust = Dust.NewDustDirect(
					hitbox.TopLeft(),
					hitbox.Width,
					hitbox.Height,
					DustID.IceTorch,
					0,
					0,
					100,
					default,
					1.5f
				);
				dust.velocity *= 3f;
				dust.velocity += vel * 0.7f;
			}
		}
	}
	public class Frost_Hydra_Sustained_Blast : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_85";
		public static float Lifetime => 72f;
		public static float MinSize => 16f;
		public static float MaxSize => 66f;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 7;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 21;
		}
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 6;
			Projectile.penetrate = 4;
			Projectile.friendly = true;
			Projectile.alpha = 255;
			Projectile.extraUpdates = 1;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void AI() {
			if (Projectile.lavaWet) {
				Projectile.localAI[2] += 1.9f;
				Projectile.localAI[0] -= 0.9f;
			}
			Projectile.localAI[0] += 1f;
			if (Projectile.ai[0] == 0) {
				float progress = (Projectile.localAI[0] + Projectile.localAI[2]) / Lifetime;
				if (Main.rand.NextFloat(1) < progress * 0.5f) {
					Projectile.ai[0] = Main.rand.NextFloat(0.02f, 0.05f) * Main.rand.NextBool().ToDirectionInt();
					Projectile.ai[1] = Main.rand.NextFloat(0.5f, 1f);
					Projectile.ai[2] = Main.rand.NextFloat(0f, 0.1f);
					Projectile.netUpdate = true;
				}
			} else {
				Projectile.localAI[1] += Projectile.ai[1] + Projectile.ai[2] * Projectile.localAI[1];
				Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[0] * (Projectile.localAI[1] - 10) * 0.3f);
				float abs = Math.Abs(Projectile.ai[0]) * 6;
				Projectile.localAI[2] += abs * 2;
				Projectile.localAI[0] -= abs;
			}
			Rectangle hitbox = Projectile.Hitbox;
			ModifyDamageHitbox(ref hitbox);
			Dust.NewDust(hitbox.Location.ToVector2(), hitbox.Width, hitbox.Height, DustID.IceTorch);
			Projectile.scale = Utils.Remap(Projectile.localAI[0], 0f, Lifetime, MinSize / 96f, MaxSize / 96f);
			Projectile.alpha = (int)(200 * (1 - (Projectile.localAI[0] / Lifetime)));
			Projectile.rotation += 0.3f * Projectile.direction;
			if (Projectile.localAI[0] + Projectile.localAI[2] > Lifetime) {
				Projectile.Kill();
			}
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			int scale = (int)Utils.Remap(Projectile.localAI[0], 0f, Lifetime, MinSize - 6, MaxSize - 6);
			hitbox.Inflate(scale, scale);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Frostburn2, 300);
			Projectile.damage = (int)(Projectile.damage * 0.9f);
		}
		public override bool PreDraw(ref Color lightColor) {
			Color color1 = new(96, 200, 255, 200);
			Color color2 = new(96, 200, 255, 200);
			Color color3 = new(0, 100, 255, 93);
			Color color4 = new(30, 30, 30, 100);
			const float num = 60f;
			const float num2 = 12f;
			const float fromMax = num + num2;
			Texture2D value = TextureAssets.Projectile[Projectile.type].Value;
			float num3 = 0.35f;
			float num4 = 0.7f;
			float num5 = 0.85f;
			float num6 = ((Projectile.localAI[0] > num - 10f) ? 0.175f : 0.2f);
			int verticalFrames = 7;
			float progress = Utils.Remap(Projectile.localAI[0], 0f, fromMax, 0f, 1f);
			float fade = Utils.Remap(Projectile.localAI[0] + Projectile.localAI[2], num, fromMax, 1f, 0f);
			float num10 = Math.Min(Projectile.localAI[0] + Projectile.localAI[2], 20f);
			float num11 = Utils.Remap(Projectile.localAI[0] + Projectile.localAI[2], 0f, fromMax, 0f, 1f);
			float scale = Utils.Remap(progress, 0.2f, 0.5f, 0.25f, 1f);
			Rectangle rectangle = value.Frame(1, verticalFrames, 0, (int)Utils.Remap(num11, 0.5f, 1f, 3f, 5f));
			if (num11 >= 1f) return false;
			for (int i = 0; i < 2; i++) {
				for (float num13 = 1f; num13 >= 0f; num13 -= num6) {
					Color obj = ((num11 < 0.1f) ? Color.Lerp(Color.Transparent, color1, Utils.GetLerpValue(0f, 0.1f, num11, clamped: true)) : ((num11 < 0.2f) ? Color.Lerp(color1, color2, Utils.GetLerpValue(0.1f, 0.2f, num11, clamped: true)) : ((num11 < num3) ? color2 : ((num11 < num4) ? Color.Lerp(color2, color3, Utils.GetLerpValue(num3, num4, num11, clamped: true)) : ((num11 < num5) ? Color.Lerp(color3, color4, Utils.GetLerpValue(num4, num5, num11, clamped: true)) : ((!(num11 < 1f)) ? Color.Transparent : Color.Lerp(color4, Color.Transparent, Utils.GetLerpValue(num5, 1f, num11, clamped: true))))))));
					float num14 = (1f - num13) * Utils.Remap(num11, 0f, 0.2f, 0f, 1f);
					Vector2 vector = Projectile.oldPos[(int)(num10 * num13)] - Main.screenPosition;
					Color color5 = obj * num14;
					float num15 = 1f / num6 * (num13 + 1f);
					float num16 = Projectile.rotation + num13 * MathHelper.PiOver2 + Main.GlobalTimeWrappedHourly * num15 * 2f;
					float num17 = Projectile.rotation - num13 * MathHelper.PiOver2 - Main.GlobalTimeWrappedHourly * num15 * 2f;
					switch (i) {
						case 0:
						Main.EntitySpriteDraw(value, vector + Projectile.velocity * (0f - num10) * num6 * 0.5f, rectangle, color5 * fade * 0.25f, num16 + (float)Math.PI / 4f, rectangle.Size() / 2f, scale, SpriteEffects.None);
						Main.EntitySpriteDraw(value, vector, rectangle, color5 * fade, num17, rectangle.Size() / 2f, scale, SpriteEffects.None);
						break;
						case 1:
						Main.EntitySpriteDraw(value, vector + Projectile.velocity * (0f - num10) * num6 * 0.2f, rectangle, color5 * fade * 0.25f, num16 + (float)Math.PI / 2f, rectangle.Size() / 2f, scale * 0.75f, SpriteEffects.None);
						Main.EntitySpriteDraw(value, vector, rectangle, color5 * fade, num17 + (float)Math.PI / 2f, rectangle.Size() / 2f, scale * 0.75f, SpriteEffects.None);
						break;
					}
				}
			}
			return false;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity = Vector2.Zero;
			return false;
		}
	}
}
