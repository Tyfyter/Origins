using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Ranged;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Ashen.Boss {
	public class Trenchmaker_Turret : ModNPC {
		static AutoLoadingTexture[] gunTextures;
		static AutoLoadingTexture[] gunGlowTextures;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 2;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			// That comic by market pliers' brother?
			TurretKind[] turretKinds = Enum.GetValues<TurretKind>();
			gunTextures = new AutoLoadingTexture[turretKinds.Length];
			gunGlowTextures = new AutoLoadingTexture[turretKinds.Length];
			for (int i = 0; i < turretKinds.Length; i++) {
				gunTextures[(int)turretKinds[i]] = $"{Texture}_{turretKinds[i]}";
				gunGlowTextures[(int)turretKinds[i]] = $"{Texture}_{turretKinds[i]}_Glow";
			}
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Slow] = true;
		}
		public TurretKind GunType { get; private set; }
		public Vector2 GunPos => NPC.position + new Vector2(19, 11) * NPC.scale;
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.width = 38;
			NPC.height = 30;
			NPC.lifeMax = 600;
			NPC.damage = 27;
			NPC.defense = 6;
			NPC.npcSlots = 0;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-2f);
			NPC.knockBackResist = 0;
			GunType = Main.rand.Next(Enum.GetValues<TurretKind>());
		}
		public override void OnSpawn(IEntitySource source) {
			base.OnSpawn(source);
		}
		public override void AI() {
			NPC.TargetClosest();
			if (NPC.HasValidTarget) NPC.targetRect = NPC.GetTargetData().Hitbox;
			Vector2 diff = NPC.targetRect.Center() - GunPos;
			switch (GunType) {
				case TurretKind.Cannon: {
					NPC.ai[1].Cooldown();
					if (TargetAngle(diff.ToRotation()) && NPC.ai[1] == 0) {
						NPC.SpawnProjectile(null,
							GunPos,
							NPC.rotation.ToRotationVector2() * 12,
							ModContent.ProjectileType<TM_Turret_Cannon_P>(),
							(int)(18 * ContentExtensions.DifficultyDamageMultiplier),
							1
						);
						NPC.ai[1] = 6;
						if (NPC.ai[2].CycleUp(3)) NPC.ai[1] = 80;
					}
					break;
				}
				case TurretKind.Launcher: {
					NPC.ai[1].Cooldown();
					if (TargetAngle(diff.ToRotation()) && NPC.ai[1] == 0) {
						NPC.SpawnProjectile(null,
							GunPos,
							NPC.rotation.ToRotationVector2() * 12,
							ModContent.ProjectileType<TM_Turret_Launcher_P>(),
							(int)(25 * ContentExtensions.DifficultyDamageMultiplier),
							1
						);
						NPC.ai[1] = 120;
					}
					break;
				}
				case TurretKind.Flamer: {
					NPC.ai[1].Cooldown();
					switch (NPC.ai[0]) {
						case 0:
						TargetAngle(diff.ToRotation());
						if (NPC.ai[1] == 0) {
							NPC.ai[0] = 1;
							NPC.ai[1] = 10;
							for (int i = 0; i < 2; i++) {
								Vector2 dir = NPC.rotation.ToRotationVector2();
								Dust.NewDustPerfect(GunPos + dir * 16 + 8 * Main.rand.NextFloatDirection() * dir.Perpendicular(), DustID.Torch).velocity += dir * 2;
							}
							SoundEngine.PlaySound(SoundID.Camera.WithPitchOffset(1.5f), GunPos);
						}
						break;
						case 1: {
							if (Main.rand.NextBool(3)) {
								Vector2 dir = NPC.rotation.ToRotationVector2();
								Dust.NewDustPerfect(GunPos + dir * 16 + 8 * Main.rand.NextFloatDirection() * dir.Perpendicular(), DustID.Torch).velocity += dir * 2;
							}
							if (NPC.ai[1] == 0) {
								NPC.ai[0] = 2;
								NPC.ai[1] = 60;
							}
							break;
						}
						case 2:
						GeometryUtils.AngularSmoothing(ref NPC.rotation, diff.ToRotation(), 0.03f);
						if (NPC.ai[1] % 5 == 0) {
							SoundEngine.PlaySound(SoundID.Item34, GunPos);
							Vector2 dir = NPC.rotation.ToRotationVector2();
							NPC.SpawnProjectile(null,
								GunPos + dir * 16,
								dir * 8,
								ModContent.ProjectileType<TM_Turret_Flamer_P>(),
								(int)(25 * ContentExtensions.DifficultyDamageMultiplier),
								1
							);
						}
						if (NPC.ai[1] == 0) {
							NPC.ai[0] = 0;
							NPC.ai[1] = 120;
						}
						break;
					}
					break;
				}
				case TurretKind.Laser: {
					NPC.ai[1].Cooldown();
					switch (NPC.ai[0]) {
						case 0: {
							if (TargetAngle(diff.ToRotation()) && NPC.ai[1] == 0) {
								Vector2 dir = NPC.rotation.ToRotationVector2();
								NPC.SpawnProjectile(null,
									GunPos + dir * 16,
									dir,
									ModContent.ProjectileType<TM_Turret_Laser_P>(),
									(int)(15 * ContentExtensions.DifficultyDamageMultiplier),
									1
								);
								NPC.ai[0] = 1;
								NPC.ai[1] = TM_Turret_Laser_P.ChargeTime;
							}
							break;
						}
						case 1: {
							GeometryUtils.AngularSmoothing(ref NPC.rotation, diff.ToRotation(), 0.05f * NPC.ai[1] / TM_Turret_Laser_P.ChargeTime);
							if (NPC.ai[1] == 0) {
								NPC.ai[0] = 2;
								NPC.ai[1] = TM_Turret_Laser_P.ActiveTime;
							}
							break;
						}
						case 2: {
							if (NPC.ai[1] == 0) {
								NPC.ai[0] = 0;
								NPC.ai[1] = 120; //time between attacks
							}
							break;
						}
					}
					break;
				}
			}
			bool TargetAngle(float direction) => GeometryUtils.AngularSmoothing(ref NPC.rotation, direction, 0.05f);
		}
		public override void FindFrame(int frameHeight) {
			NPC.frame.Y = NPC.direction == 1 ? 0 : frameHeight;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				NPC.position - screenPos,
				NPC.frame,
				drawColor
			);
			spriteBatch.Draw(
				gunTextures[(int)GunType],
				GunPos - screenPos,
				null,
				drawColor,
				NPC.rotation + MathHelper.Pi,
				new Vector2(35, 11),
				NPC.scale,
				SpriteEffects.None,
			0);
			if (gunGlowTextures[(int)GunType].Exists) {
				switch (GunType) {
					case TurretKind.Flamer: {
						float glowFactor = 0.75f;
						if (NPC.ai[0] == 1) glowFactor += Utils.PingPongFrom01To010(NPC.ai[1] / 10) * 0.25f;
						spriteBatch.Draw(
							gunGlowTextures[(int)GunType],
							GunPos - screenPos,
							null,
							Color.White * glowFactor,
							NPC.rotation + MathHelper.Pi,
							new Vector2(35, 11),
							NPC.scale,
							SpriteEffects.None,
						0);
						break;
					}
					default: {
						spriteBatch.Draw(
							gunGlowTextures[(int)GunType],
							GunPos - screenPos,
							null,
							Color.White,
							NPC.rotation + MathHelper.Pi,
							new Vector2(35, 11),
							NPC.scale,
							SpriteEffects.None,
						0);
						break;
					}
				}
			}
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)GunType);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			GunType = (TurretKind)reader.ReadByte();
		}
		public enum TurretKind {
			Cannon,
			Launcher,
			Flamer,
			Laser,
		}
		public class TM_Turret_Cannon_P : ModProjectile {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.BulletDeadeye}";
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.BulletDeadeye);
				AIType = ProjectileID.BulletDeadeye;
			}
		}
		public class TM_Turret_Launcher_P : Fire_Cannons_State.Trenchmaker_Cannon_P { }
		public class TM_Turret_Flamer_P : ModProjectile {
			public override string Texture =>typeof(Welding_Torch_P).GetDefaultTMLName();
			public static float Lifetime => 60f;
			public static float MinSize => 24f;
			public static float MaxSize => 48f;
			private readonly float[] sizes = new float[21];
			public override void SetStaticDefaults() {
				Main.projFrames[Type] = 7;
				ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
				ProjectileID.Sets.TrailCacheLength[Projectile.type] = 21;
				OriginsSets.Projectiles.FireProjectiles[Type] = true;
			}
			public override void SetDefaults() {
				Projectile.DamageType = DamageClass.Ranged;
				Projectile.width = Projectile.height = 0;
				Projectile.penetrate = 4;
				Projectile.hostile = true;
				Projectile.alpha = 255;
				Projectile.extraUpdates = 3;
				Projectile.usesLocalNPCImmunity = true;
				Projectile.localNPCHitCooldown = -1;
				for (int i = 0; i < Projectile.oldPos.Length; i++)
					Projectile.oldRot[i] = Main.rand.NextFloatDirection();
			}
			float Size => Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize, MaxSize);
			public override void AI() {
				Lighting.AddLight(Projectile.Center, 0.85f, 0.4f, 0f);
				Projectile.ai[0]++;
				if (Projectile.velocity == default) Projectile.ai[0]++;
				for (int i = sizes.Length - 1; i > 0; i--) {
					sizes[i] = sizes[i - 1];
				}
				sizes[0] = Size;
				Projectile.scale = Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize / 96f, MaxSize / 96f);
				Projectile.alpha = (int)(200 * (1 - (Projectile.ai[0] / Lifetime)));
				Projectile.rotation += 0.3f * Projectile.direction;
				Projectile.velocity *= 0.97f;
				if (Projectile.ai[0] > Lifetime) Projectile.Kill();
			}
			public override void ModifyDamageHitbox(ref Rectangle hitbox) {
				int scale = (int)((Size - hitbox.Width) / 2);
				hitbox.Inflate(scale, scale);
			}
			public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
				target.AddBuff(BuffID.OnFire3, hit.Crit ? 360 : 180);
			}
			public override bool PreDraw(ref Color lightColor) {
				float progress = Projectile.ai[0] / Lifetime;
				Flamethrower_Drawer.Draw(
					Projectile,
					float.Pow(1 - progress, 0.5f),
					TextureAssets.Projectile[Type].Value,
					Color.Black,
					sizes,
					//brightnessColorExponent: 1.75f,
					//smokeAmount: 0,
					sizeProgressOverride: _ => 0
				);
				return false;
			}
			public override bool OnTileCollide(Vector2 oldVelocity) {
				Projectile.velocity = Vector2.Zero;
				return false;
			}
		}
		public class TM_Turret_Laser_P : ModProjectile {
			public static int ChargeTime => 30;
			public static int ActiveTime => 15;
			public override string Texture => typeof(Laser_Target_Locator).GetDefaultTMLName();
			public override void SetStaticDefaults() {
				ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600 + 64;
			}
			public override void SetDefaults() {
				Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
				Projectile.width = 0;
				Projectile.height = 0;
				Projectile.hostile = true;
				Projectile.tileCollide = false;
				Projectile.usesLocalNPCImmunity = true;
				Projectile.localNPCHitCooldown = 5;
			}
			public override bool ShouldUpdatePosition() => false;
			public Vector2 TargetPos {
				get => new(Projectile.ai[0], Projectile.ai[1]);
				set => (Projectile.ai[0], Projectile.ai[1]) = value;
			}
			bool IsActive {
				get => Projectile.localAI[0] != 0;
				set => Projectile.localAI[0] = value.ToInt();
			}
			public override void OnSpawn(IEntitySource source) {
				Projectile.ai[2] = -1;
				if (source is EntitySource_Parent { Entity: NPC owner }) Projectile.ai[2] = owner.whoAmI;
			}
			public override void AI() {
				if (Main.npc.GetIfInRange((int)Projectile.ai[2]) is not NPC { active: true } owner || owner.ai[0] == 0 || owner.ModNPC is not Trenchmaker_Turret turret) {
					Projectile.Kill();
					return;
				}
				IsActive = owner.ai[0] == 2;
				Vector2 gunPos = turret.GunPos;
				float pitchFactor = IsActive ? 1 : (1 - owner.ai[1] / ChargeTime);
				SoundEngine.SoundPlayer.Play(SoundID.Item158.WithPitch(pitchFactor).WithVolume(0.8f), gunPos);
				Projectile.velocity = owner.rotation.ToRotationVector2();
				Projectile.position = gunPos + Projectile.velocity * 24;
				Vector2 targetPos = Projectile.position + Projectile.velocity * Raymarch(Projectile.position, Projectile.velocity, ProjectileID.Sets.DrawScreenCheckFluff[Type] - 64);
				if (IsActive) {
					Dust.NewDust(targetPos - Vector2.One * 2, 4, 4, DustID.AmberBolt);
				}
				TargetPos = targetPos;
			}
			public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
				if (!IsActive) return false;
				return targetHitbox.Contains(targetHitbox.Center().SnapToLine(Projectile.position, TargetPos, radius: 4));
			}
			public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
				if (!target.immune) modifiers = modifiers with { CooldownCounter = -2 };
				modifiers.Knockback *= 0.85f;
			}
			public override void OnHitPlayer(Player target, Player.HurtInfo info) {
				if (info.CooldownCounter == -2) {
					target.immune = true;
					target.immuneTime = target.longInvince ? 15 : 7;
				}
			}
			public override bool PreDraw(ref Color lightColor) {
				if (!Collision.CheckAABBvLineCollision(Main.screenPosition, Main.ScreenSize.ToVector2(), Projectile.position, TargetPos)) return false;
				if (Main.npc.GetIfInRange((int)Projectile.ai[2]) is not NPC { active: true } owner || owner.ModNPC is not Trenchmaker_Turret turret) {
					Projectile.Kill();
					return false;
				}
				Vector2 diff = TargetPos - Projectile.position;
				Vector2 position = Projectile.position;
				position -= Main.screenPosition;
				float rotation = diff.ToRotation();
				float dist = diff.Length();
				const float scale = 1f / 256f;
				DrawData data = new(
					TextureAssets.Extra[ExtrasID.RainbowRodTrailShape].Value,//TextureAssets.MagicPixel.Value,
					position,
					null,
					new Color(255, IsActive ? 40 : 100, 0, 0),
					rotation,
					Vector2.UnitY * 128,
					new Vector2(dist * scale, 8 * scale),
					0
				);
				data.Draw(Main.spriteBatch);
				float progress = 1 - owner.ai[1] / ChargeTime;
				progress *= progress;
				if (IsActive) progress = 1;
				Min(ref progress, 1);
				data.color *= progress;
				Vector2 offset = (rotation + MathHelper.PiOver2).ToRotationVector2() * (1 - progress) * 8;
				data.position = position + offset;
				data.scale.X = Raymarch(data.position + Main.screenPosition, Projectile.velocity, dist * 1.15f + 16) * scale;
				data.Draw(Main.spriteBatch);
				data.position = position - offset;
				data.scale.X = Raymarch(data.position + Main.screenPosition, Projectile.velocity, dist * 1.15f + 16) * scale;
				data.Draw(Main.spriteBatch);
				return false;
			}
			public static float Raymarch(Vector2 position, Vector2 direction, float maxLength = float.PositiveInfinity) {
				float dist = CollisionExt.Raymarch(position, direction, maxLength);
				foreach (NPC npc in Main.ActiveNPCs) {
					if (dist < 16) return dist;
					if (!npc.friendly) continue;
					if (position.Clamp(npc.Hitbox).DistanceSQ(position) >= dist * dist) continue;
					float collisionPoint = 1;
					if (Collision.CheckAABBvLineCollision(npc.position, npc.Size, position, position + direction * dist, 1, ref collisionPoint)) {
						Min(ref dist, collisionPoint);
					}
				}
				foreach (Player player in Main.ActivePlayers) {
					if (dist < 16) return dist;
					if (position.Clamp(player.Hitbox).DistanceSQ(position) >= dist * dist) continue;
					float collisionPoint = 1;
					if (Collision.CheckAABBvLineCollision(player.position, player.Size, position, position + direction * dist, 1, ref collisionPoint)) {
						Min(ref dist, collisionPoint);
					}
				}
				return dist;
			}
		}
	}
}
