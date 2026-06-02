using Origins.Projectiles;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.NPCs.Ashen.Boss.Trenchmaker;

namespace Origins.NPCs.Ashen.Boss {
	public class Fire_Guns_State : AIState {
		#region stats
		public static float ShotRate => 10 - DifficultyMult;
		public static int ShotDamage => (int)(18 * DifficultyMult);
		public static float ShotVelocity => 12;
		public static int Duration => 45;
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
				//SoundEngine.PlaySound(SoundID.Item12.WithVolume(0.5f).WithPitchRange(0.25f, 0.4f), npc.Center);
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
	}
}
