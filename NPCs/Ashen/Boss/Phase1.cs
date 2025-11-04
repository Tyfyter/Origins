using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.NPCs.Ashen.Boss.Trenchmaker;
using static Origins.OriginExtensions;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Ashen.Boss {
	public class PhaseOneIdleState : AIState {
		#region stats
		public static float IdleTime => 60;
		#endregion stats
		public override void Load() {
			AutomaticIdleState.aiStates.Add((this, _ => 1));
		}
		public override void DoAIState(Trenchmaker boss) {
			NPC npc = boss.NPC;
			TargetSearchResults searchResults = SearchForTarget(npc, TargetSearchFlag.Players);
			if (searchResults.FoundTarget) {
				npc.target = searchResults.NearestTargetIndex;
				npc.targetRect = searchResults.NearestTargetHitbox;
				if (npc.ShouldFaceTarget(ref searchResults)) npc.FaceTarget();
			}

			//npc.velocity *= 0.97f;
			if (++npc.ai[0] > IdleTime && Main.netMode != NetmodeID.MultiplayerClient) {
				if (aiStates.Select(state => state.Index).All(boss.PreviousStates.Contains)) Array.Fill(boss.PreviousStates, Index);
				boss.SelectAIState(aiStates);
			}
			if (npc.HasValidTarget) npc.DiscourageDespawn(60 * 5);
			else npc.EncourageDespawn(60);
		}
		public override void TrackState(int[] previousStates) { }
		public static List<AIState> aiStates = [];
	}
	public class Fire_Guns_State : AIState {
		#region stats
		public static float ShotRate => 10 - DifficultyMult;
		public static int ShotDamage => (int)(18 * DifficultyMult);
		public static float ShotVelocity => 12;
		public static int Duration => 45;
		#endregion stats
		public override bool Ranged => true;
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
					ProjectileID.BulletDeadeye,
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
		public override double GetWeight(Trenchmaker boss, int[] previousStates) => boss.GunType == 0 ? base.GetWeight(boss, previousStates) : 0;
	}
	public class Fire_Cannons_State : AIState {
		#region stats
		public static float ShotRate => 20 - DifficultyMult * 2;
		public static int ShotDamage => (int)(25 * DifficultyMult);
		public static float ShotVelocity => 12;
		public static int Duration => 40;
		#endregion stats
		public override bool Ranged => true;
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
				npc.SpawnProjectile(null,
					boss.GunPos + direction * 8,
					direction * ShotVelocity,
					ProjectileID.CannonballHostile,
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
		public override double GetWeight(Trenchmaker boss, int[] previousStates) => boss.GunType == 1 ? base.GetWeight(boss, previousStates) : 0;
	}
	public class Firecracker_State : AIState {
		#region stats
		public static int ShotCount => (int)(5 + DifficultyMult);
		public static int ShotDamage => (int)(18 * DifficultyMult);
		public static float ShotVelocityMin => 8;
		public static float ShotVelocityStep => 2;
		public static int StateDuration => 90;
		/// <param name="kind">0: red, 1: tan</param>
		public static float FuseTime(int kind, float index) => 90 + index * 4;
		public static int RedExplosionDuration => 30;
		public static int TanExplosionRange => 128;
		public static float TanExplosionSpread => 0.5f;
		#endregion stats
		public override bool Ranged => true;
		public override void Load() {
			PhaseOneIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Trenchmaker boss) {
			if (++boss.NPC.ai[0] > StateDuration) boss.StartIdle();
		}
		public override void StartAIState(Trenchmaker boss) {
			NPC npc = boss.NPC;
			int length = ShotCount;
			int projType = ModContent.ProjectileType<Trenchmaker_Firecracker>();
			int mode = Main.rand.Next(0, 2);
			for (int i = 0; i < length; i++) {
				npc.SpawnProjectile(null,
					npc.Center,
					Vector2.UnitX * npc.direction * (ShotVelocityMin + ShotVelocityStep * i),
					projType,
					ShotDamage,
					1,
					mode,
					i
				);
			}
		}
		public class Trenchmaker_Firecracker : ModProjectile {
			public override void SetStaticDefaults() {
				Main.projFrames[Type] = 2;
			}
			public override void SetDefaults() {
				Projectile.width = 16;
				Projectile.height = 16;
				Projectile.aiStyle = -1;
				Projectile.hostile = true;
				Projectile.penetrate = -1;
				Projectile.timeLeft = 360;
			}
			public override bool? CanDamage() => false;
			public override void AI() {
				Projectile.frame = (int)Projectile.ai[0];
				Projectile.rotation = 0;
				Projectile.velocity.X *= 
					float.Pow(0.97f, 1f / (Projectile.ai[1] + 1))
					* float.Pow(0.999f, Projectile.ai[2] * 0.25f);
				Projectile.velocity.Y += 0.2f;
				if (++Projectile.ai[2] > FuseTime((int)Projectile.ai[0], Projectile.ai[1])) {
					Projectile.Kill();
				}
			}
			public override bool OnTileCollide(Vector2 oldVelocity) {
				Projectile.velocity *= 0.5f;
				return false;
			}
			public override void OnKill(int timeLeft) {
				Projectile.SpawnProjectile(null,
					Projectile.Center,
					Vector2.UnitY * -TanExplosionRange,
					Projectile.ai[0] == 0 ? ModContent.ProjectileType<Trenchmaker_Firecracker_Explosion_1>() : ModContent.ProjectileType<Trenchmaker_Firecracker_Explosion_2>(),
					Projectile.damage,
					Projectile.knockBack
				);
			}
		}
		public class Trenchmaker_Firecracker_Explosion_1 : ModProjectile {
			public override string Texture => typeof(Trenchmaker_Firecracker).GetDefaultTMLName();
			public override void SetDefaults() {
				Projectile.width = 0;
				Projectile.height = 0;
				Projectile.tileCollide = false;
				Projectile.hostile = true;
				Projectile.penetrate = -1;
				Projectile.timeLeft = RedExplosionDuration;
				Projectile.hide = true;
			}
			public override bool ShouldUpdatePosition() => false;
			public override bool? CanDamage() => Projectile.width != 0;
			public override void AI() {
				if (++Projectile.ai[2] >= 7) {
					Projectile.ai[2] = 0;
					ExplosiveGlobalProjectile.DoExplosion(Projectile, 64, false, SoundID.Item40.WithPitch(0.0f).WithVolume(1f), 10, 0, 0, DustID.IchorTorch, hostile: true);
					Projectile.position.X += Projectile.width / 2;
					Projectile.position.Y += Projectile.height / 2;
					Projectile.width = 0;
					Projectile.height = 0;
				}
			}
		}
		public class Trenchmaker_Firecracker_Explosion_2 : ModProjectile, IIsExplodingProjectile {
			public override string Texture => typeof(Trenchmaker_Firecracker).GetDefaultTMLName();
			bool didVisual = false;
			public override void SetDefaults() {
				Projectile.width = 0;
				Projectile.height = 0;
				Projectile.tileCollide = false;
				Projectile.hostile = true;
				Projectile.penetrate = -1;
				Projectile.timeLeft = 5;
				Projectile.hide = true;
			}
			public override bool ShouldUpdatePosition() => false;
			public override void AI() {
				if (Main.netMode == NetmodeID.Server || didVisual) return;
				didVisual = true;
				Vector2 direction = Projectile.velocity.SafeNormalize(-Vector2.UnitY);
				Vector2 spread = GetSpread(direction);
				for (int i = 0; i < 30; i++) {
					Vector2 dustVelocity = (direction * Main.rand.NextFloat(0.8f, 1) + spread * Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
					Dust.NewDustDirect(
						Projectile.position,
						Projectile.width,
						Projectile.height,
						DustID.Smoke,
						dustVelocity.X,
						dustVelocity.Y,
						100,
						default,
						1.5f
					).velocity *= 1.4f;
				}
				for (int i = 0; i < 20; i++) {
					Vector2 dustVelocity = (direction * Main.rand.NextFloat(0.8f, 1) + spread * Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
					Dust dust = Dust.NewDustDirect(
						Projectile.position,
						Projectile.width,
						Projectile.height,
						DustID.Torch,
						0,
						0,
						100,
						default,
						3.5f
					);
					dust.noGravity = true;
					dust.velocity *= 7f;
					dust.velocity += dustVelocity;
					dust = Dust.NewDustDirect(
						Projectile.position,
						Projectile.width,
						Projectile.height,
						DustID.Torch,
						0,
						0,
						100,
						default,
						1.5f
					);
					dust.velocity *= 3f;
					dust.velocity += dustVelocity;
				}
				SoundEngine.PlaySound(in SoundID.Item62, Projectile.position);
			}
			public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
				Vector2 laserStartPoint = Projectile.position;
				float factor = 1 - Math.Min(Projectile.timeLeft / 5f, 0.9f);
				Vector2 spread = GetSpread(Projectile.velocity * factor);
				return new Triangle(laserStartPoint, laserStartPoint + Projectile.velocity * factor - spread, laserStartPoint + Projectile.velocity * factor + spread).Intersects(targetHitbox);
			}
			static Vector2 GetSpread(Vector2 velocity) => velocity.RotatedBy(MathHelper.PiOver2) * TanExplosionSpread;
			public bool IsExploding() => true;
		}
	}
	public class Teabag_State : AIState {
		public override float WalkDist => 16 * 4;
		public override void Load() {
			AutomaticIdleState.aiStates.Add((this, boss => (!boss.NPC.HasValidTarget).Mul(10000)));
		}
		public override void DoAIState(Trenchmaker boss) {
			boss.NPC.FaceTarget();
		}
	}
}
