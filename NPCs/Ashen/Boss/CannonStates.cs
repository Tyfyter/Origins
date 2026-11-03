using Microsoft.Xna.Framework.Graphics;
using Origins.Dusts;
using Origins.Items.Mounts.Star_Soldier;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Items.Mounts.Star_Soldier.Star_Soldier_Pod;
using static Origins.NPCs.Ashen.Boss.Trenchmaker;

namespace Origins.NPCs.Ashen.Boss; 
public class Fire_Guns_State : AIState {
	#region stats
	public static float ShotRate => 7 - DifficultyMult;
	public static int ShotDamage => (int)(18 * DifficultyMult);
	public static float ShotVelocity => 12;
	public static int Duration => 60;
	#endregion stats
	public override bool Ranged => true;
	public override GunKind? ForGunType => GunKind.Cannon;
	public override void Load() {
		PhaseOneIdleState.aiStates.Add(this);
	}
	public override void DoAIState(Trenchmaker boss) {
		NPC npc = boss.NPC;
		Vector2 direction = npc.rotation.ToRotationVector2();
		int shotsToHaveFired = (int)((++npc.ai[0]) / npc.ai[3]);
		if (shotsToHaveFired > npc.ai[1]) {
			SoundEngine.PlaySound(Origins.Sounds.HeavyCannon.WithPitchRange(1.2f, 1.3f).WithVolume(0.75f), npc.Center);
			SoundEngine.PlaySound(SoundID.Item89.WithPitchRange(2f, 2.2f).WithVolume(0.75f), npc.Center);
			npc.ai[1]++;
			Vector2 perp = direction.RotatedBy(MathHelper.PiOver2);
			npc.SpawnProjectile(null,
				boss.GunPos + direction * 8 + perp * (6 * ((npc.ai[1] % 2) == 0).ToDirectionInt() + 2),
				direction * ShotVelocity,
				ModContent.ProjectileType<Trenchmaker_Bullet_P>(),
				ShotDamage,
				1
			);
		}
		if (npc.ai[0] > Duration) boss.StartIdle();
	}
	public override void StartAIState(Trenchmaker boss) {
		NPC npc = boss.NPC;
		npc.ai[3] = ShotRate;
	}
	public class Trenchmaker_Bullet_P : ModProjectile {
		public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.BulletDeadeye}";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.BulletDeadeye);
			AIType = ProjectileID.BulletDeadeye;
		}
	}
}
public class Carpet_Bomb_State : AIState {
	public static int MaxCount => (int)(12 + DifficultyMult);
	public static int DropRate => (int)(25 - DifficultyMult);
	public static int ShotDamage => (int)(18 * DifficultyMult);
	public static float ChargeSoundVolume => 1;
	public static float ChargeSoundFadeTime => 30;
	public override bool CanHaveThrustersActive => true;
	public override GunKind? ForGunType => GunKind.Cannon;
	public override void Load() {
		PhaseOneIdleState.aiStates.Add(this);
		iconTexture = typeof(Trenchmaker_Carpet_Bomb).GetDefaultTMLName();
	}
	public override void DoAIState(Trenchmaker boss) {
		NPC npc = boss.NPC;
		if (boss.legs[0].CurrentAnimation is Jump_Preparation_Animation or Jump_Squat_Animation) {
			if (npc.soundDelay == 0) {
				SoundEngine.PlaySound(Origins.Sounds.ThrusterChargeUp.WithVolume(ChargeSoundVolume).WithPitchVarience(1.07f), npc.Center, sound => {
					if (npc.ai[0] > 0) {
						sound.Volume -= ChargeSoundVolume / ChargeSoundFadeTime;
					}
					return sound.Volume > 0;
				});
			}
			npc.soundDelay = 30;
			return;
		}
		if (boss.legs[0].CurrentAnimation is Jump_Extend_Animation) {
			npc.ai[1] = 1;
			return;
		}
		if (boss.legs[0].CurrentAnimation is not Jump_Air_Animation) {
			if (npc.ai[1] > 0) {
				npc.frame.Y = 0;
				npc.frameCounter = 0;
				boss.StartIdle();
			}
			return;
		}
		npc.velocity.Y -= 0.33f;
		npc.velocity.X += npc.direction * 0.01f;
		npc.DoFrames(4, 1..7);
		if (++npc.ai[0] > (npc.ai[1] * DropRate) && npc.ai[1] < MaxCount + 1) {
			npc.ai[1]++;
			npc.SpawnProjectile(null,
				npc.Center,
				Vector2.Zero,
				ModContent.ProjectileType<Trenchmaker_Carpet_Bomb>(),
				ShotDamage,
				1,
				Main.rand.Next(0, 2)
			);
		}
		if (npc.ai[1] >= MaxCount + 1) {
			//npc.velocity.Y = -0.1f;
			//Min(ref npc.velocity.Y, 1);
		}
	}
	public override LegAnimation ForceAnimation(Trenchmaker npc, Leg leg, Leg otherLeg) => npc.NPC.ai[1] == 0 ? ModContent.GetInstance<Jump_Preparation_Animation>() : null;
	public class Trenchmaker_Carpet_Bomb : ModProjectile {
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.width = 34;
			Projectile.height = 34;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 360;
			Projectile.hide = true;
		}
		public override void AI() {
			Projectile.rotation += 0.1f;
			Projectile.velocity.Y += 0.2f;
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 128, sound: SoundID.Item14, hostile: true);
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCs.Add(index);
		}
	}
	public override double GetWeight(Trenchmaker boss, int[] previousStates) {
		double baseWeight = base.GetWeight(boss, previousStates);
		if (baseWeight == 0) return 0;
		if (false) return 0; //todo: check ceiling height
		return baseWeight * baseWeight;
	}
}
public class Tracker_Missiles_State : AIState {
	public static int MissileCount => (int)(2.5f + DifficultyMult);
	public static int ShotRate => (int)(13 - DifficultyMult);
	public static float ShotVelocity => 11 + DifficultyMult;
	public static float ShotSpread => 4;
	public static int ShotDamage => (int)(8 + 8 * DifficultyMult);
	public static int HomingTime => (int)(36 + 2 * DifficultyMult);
	public static int Duration => 120;
	public override bool Ranged => true;
	public override GunKind? ForGunType => GunKind.Cannon;
	public override void Load() {
		PhaseOneIdleState.aiStates.Add(this);
		iconTexture = typeof(Star_Soldier_Pod_Missile).GetDefaultTMLName();
		iconFrame = new(0, 66, 42, 22);
	}
	public override void DoAIState(Trenchmaker boss) {
		NPC npc = boss.NPC;
		int shotsToHaveFired = (int)((++npc.ai[0]) / npc.ai[3]);
		Min(ref shotsToHaveFired, MissileCount);
		if (shotsToHaveFired > npc.ai[1]) {
			SoundEngine.PlaySound(SoundID.Item108.WithPitch(-1f), npc.Center);
			SoundEngine.PlaySound(SoundID.Item113.WithPitch(1.2f), npc.Center);
			npc.ai[1]++;
			npc.SpawnProjectile(null,
				npc.Center,
				Main.rand.NextVector2Circular(ShotSpread, ShotSpread) - new Vector2(0, 8),
				ModContent.ProjectileType<Trenchmaker_Seeker_Missile>(),
				ShotDamage,
				1
			);
		}
		if (npc.ai[0] > Duration) boss.StartIdle();
	}
	public override void StartAIState(Trenchmaker boss) {
		NPC npc = boss.NPC;
		npc.ai[3] = ShotRate;
	}
	public class Trenchmaker_Seeker_Missile : ModProjectile {
		public override string Texture => typeof(Star_Soldier_Pod_Missile).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
			Main.projFrames[Type] = 4;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.hostile = true;
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
			if (Projectile.velocity != default) Projectile.ai[1] = MathF.Round(60 / Projectile.velocity.Length());
			Projectile.ai[2] = -1;
			if (source is EntitySource_Parent { Entity: NPC npc }) Projectile.ai[2] = npc.whoAmI;
		}
		public override void AI() {
			if (Projectile.ai[1] > 0) {
				SoundEngine.PlaySound(Origins.Sounds.StarDash.WithPitch(0.5f), Projectile.Center);
				SoundEngine.PlaySound(Origins.Sounds.DrillLeaveGround.WithPitch(2f), Projectile.Center);
				Projectile.ai[1]--;
				Projectile.numUpdates++;
				return;
			}
			if (++Projectile.localAI[0] < 30) {
				Projectile.velocity *= 0.97f;
				if (Projectile.velocity != default) Projectile.rotation = Projectile.velocity.ToRotation();
				Projectile.frame = (int)((Projectile.localAI[0] / 30) * (Main.projFrames[Type] - 1));
				return;
			}
			Projectile.frame = Main.projFrames[Type] - 1;
			Color dustColor = default;
			int dustType = DustID.Torch;
			if (Projectile.TryGetGlobalProjectile(out CanisterGlobalProjectile global)) {
				if (global.CanisterData?.HasSpecialEffect ?? false) {
					dustType = Tintable_Torch_Dust.ID;
					dustColor = global.CanisterData.InnerColor with { A = 100 };
				}
			}
			Vector2 dustBasePos = Projectile.Center - Projectile.rotation.ToRotationVector2() * 35 - new Vector2(4);
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
			if (Projectile.velocity != default) Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
			if (Projectile.ai[2] != -1 && Projectile.localAI[0] > 30 + HomingTime) Projectile.ai[2] = -1;
			Vector2 targetVelocity;
			if (Main.npc.GetIfInRange((int)Projectile.ai[2]) is NPC owner) {
				NPCAimedTarget target = owner.GetTargetData();
				if (Projectile.Hitbox.Intersects(target.Hitbox)) Projectile.ai[2] = -1;
				targetVelocity = (target.Center - Projectile.Center).Normalized(out _);
			} else {
				targetVelocity = Projectile.rotation.ToRotationVector2();
			}
			float lerpValue = 0.07f;
			targetVelocity *= ShotVelocity;
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, lerpValue);
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) => Projectile.penetrate = 0;
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, sound: SoundID.Item14, hostile: true);
		}
		public override bool PreDraw(ref Color lightColor) {
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (Projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;
			Rectangle frame = TextureAssets.Projectile[Type].Value.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
			Vector2 origin = spriteEffects.ApplyToOrigin(new(35, 10), frame);
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				frame,
				lightColor,
				Projectile.rotation,
				origin,
				Projectile.scale,
				spriteEffects
			);
			return false;
		}
	}
}