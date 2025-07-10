using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria;
using static Origins.NPCs.MiscB.Shimmer_Construct.Shimmer_Construct;
using PegasusLib;
using Terraria.ModLoader;
using Origins.Items.Weapons.Magic;
using Terraria.DataStructures;
using Terraria.Audio;
using System.IO;

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class PhaseTwoIdleState : AIState {
		#region stats
		public static float IdleTime => 60 - DifficultyMult * 10;
		#endregion stats
		public override void Load() {
			AutomaticIdleState.aiStates.Add((this, boss => boss.IsInPhase2.Mul(2)));
		}
		public override void SetStaticDefaults() {
			aiStates.Add(ModContent.GetInstance<SpawnCloudsState>());
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.TargetClosest();
			npc.velocity *= 0.97f;
			if (++npc.ai[0] > IdleTime && Main.netMode != NetmodeID.MultiplayerClient) {
				if (aiStates.Select(state => state.Index).All(boss.previousStates.Contains)) Array.Fill(boss.previousStates, Index);
				SelectAIState(boss, aiStates);
			}
		}
		public override void TrackState(int[] previousStates) { }
		public static List<AIState> aiStates = [];
	}
	public class FastDashState : DashState {
		#region stats
		public static float DashSpeedMultiplier => 1.5f;
		#endregion stats
		public override void Load() {
			PhaseTwoIdleState.aiStates.Add(this);
		}
		public override void StartAIState(Shimmer_Construct boss) {
			base.StartAIState(boss);
			NPC npc = boss.NPC;
			npc.ai[3] *= DashSpeedMultiplier;
			npc.ai[1] *= DashSpeedMultiplier;
			npc.ai[2] *= DashSpeedMultiplier;
		}
	}
	public class FastCircleState : CircleState {
		#region stats
		public static new float ShotRate => 16 - DifficultyMult * 0.75f;
		#endregion stats
		public override void Load() {
			PhaseTwoIdleState.aiStates.Add(this);
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.ai[3] = ShotRate;
		}
	}
	public class MagicMissilesState : AIState {
		#region stats
		public static int ShotDamage => (int) (25 + 8 * DifficultyMult);
		public static float ShotRate => 6 - DifficultyMult;
		public static float ShotCount => 5 + DifficultyMult;
		public static float ExtraIdleTime => DifficultyMult * 8;
		#endregion stats
		public override void Load() {
			PhaseTwoIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			if (++npc.ai[0] >= npc.ai[1]) {
				SoundEngine.PlaySound(SoundID.Item35.WithPitchRange(0.15f, 0.4f).WithVolume(0.5f), npc.Center);
				SoundEngine.PlaySound(SoundID.Item43.WithPitch(2f), npc.Center);
				npc.ai[0] -= npc.ai[1];
				npc.SpawnProjectile(null,
					npc.Center,
					Vector2.UnitY.RotatedByRandom(1.5f) * -8,
					Shimmer_Construct_Missiles.ID,
					ShotDamage,
					1,
					ai0: npc.target,
					ai1: boss.IsInPhase3.ToInt()
				);
				if (--npc.ai[2] <= 0) {
					SetAIState(boss, StateIndex<AutomaticIdleState>());
					npc.ai[0] -= ExtraIdleTime;
				}
			}
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.ai[1] = ShotRate;
			npc.ai[2] = Main.rand.RandomRound(ShotCount);
		}
		public override double GetWeight(Shimmer_Construct boss, int[] previousStates) {
			if (!boss.IsInPhase3 && !CollisionExt.CanHitRay(boss.NPC.Center, boss.NPC.targetRect.Center())) return 0;
			return base.GetWeight(boss, previousStates);
		}
		public class Shimmer_Construct_Missiles : ModProjectile {
			public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowRodBullet;
			public static int ID { get; private set; }
			public override void SetStaticDefaults() {
				ProjectileID.Sets.TrailingMode[Type] = 3;
				ProjectileID.Sets.TrailCacheLength[Type] = 30;
				ID = Type;
			}
			public override void SetDefaults() {
				Projectile.damage = 38/* + (9 * difficultyMult)*/;
				Projectile.friendly = false;
				Projectile.hostile = true;
				Projectile.aiStyle = 0;
				Projectile.width = 24;
				Projectile.height = 24;
				Projectile.tileCollide = false;
			}
			public override void OnSpawn(IEntitySource source) {
				if (Projectile.ai[1] == 1) Projectile.localAI[0] = 1;
				Projectile.ai[1] = 0;
				if (source is EntitySource_Parent parentSource && parentSource.Entity is NPC parentNPC && parentNPC.ModNPC is Shimmer_Construct construct && construct.IsInPhase3) Projectile.ai[2] = 1;
			}
			public override void AI() {
				bool inPhase3 = Projectile.localAI[0] != 0;
				if (Main.player.GetIfInRange((int)Projectile.ai[0]) is Player target) {
					if (target.active && !target.dead) {
						float difficultyMult = ContentExtensions.DifficultyDamageMultiplier;
						if (++Projectile.ai[1] > 30f - difficultyMult) {
							float acceleration = 2;
							if (Projectile.ai[1] >= 40) {
								acceleration = (0.2f + (difficultyMult - Projectile.ai[2]) * 0.1f) * Math.Max(1 - (Projectile.ai[1] - 40) / 80, 0);
							}
							if (inPhase3) acceleration *= 0.8f;
							Projectile.velocity += Projectile.DirectionTo(target.Center) * acceleration;
						}
					} else {
						Projectile.ai[0] = -1;
					}
				} else {
					Projectile.timeLeft -= 19;
					Debugging.ChatOverhead(Projectile.ai[0]);
				}
				if (Projectile.ai[1] < 28) Projectile.velocity *= 0.97f;
				else if (Projectile.ai[1] <= 32) Projectile.velocity *= 0.9f;
				if (inPhase3) {
					Projectile.tileCollide = false;
				} else if (Projectile.ai[2] == 0 && !Projectile.tileCollide && !Projectile.Hitbox.OverlapsAnyTiles()) {
					Projectile.tileCollide = true;
				}
				Projectile.rotation = Projectile.velocity.ToRotation();
			}
			public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
				if (Projectile.velocity.X != 0) modifiers.HitDirectionOverride = Math.Sign(Projectile.velocity.X);
				modifiers.Knockback.Base += 4;
			}
			public override void SendExtraAI(BinaryWriter writer) {
				writer.Write(Projectile.localAI[0] != 0);
			}
			public override void ReceiveExtraAI(BinaryReader reader) {
				Projectile.localAI[0] = reader.ReadBoolean().ToInt();
			}
			public override bool PreDraw(ref Color lightColor) {
				Shimmerstar_Staff_P.DrawShimmerstar(Projectile);
				return false;
			}
		}
	}

	public class SpawnDronesStateState : AIState {
		#region stats
		public static float SpawnCount => 2 + (2 *DifficultyMult);
		public static float SpawnRate => 15 - DifficultyMult;
		public static float ExtraIdleTime => DifficultyMult * 8;
		/// <summary>
		/// time after drones are finished spawning over which the spinning slows down
		/// measured in <see cref="SpawnRate"/>s
		/// </summary>
		public static int SpinDownAnim => 4;
		public static int CanUseThreshold => 4;
		#endregion stats
		public override void Load() {
			PhaseTwoIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;

			const float spin_spawn_ratio = 2f;
			float spinAccelFactor = npc.ai[1] * spin_spawn_ratio;
			if (npc.ai[2] > -SpinDownAnim + spin_spawn_ratio) npc.ai[3] += 1 / spinAccelFactor;
			else npc.ai[3] -= 1 / spinAccelFactor;
			npc.ai[3] = float.Clamp(npc.ai[3], 0, 1);
			npc.rotation += npc.direction * (0.5f * Math.Max(npc.ai[3] * npc.ai[3], 0));

			if (++npc.ai[0] >= npc.ai[1]) {
				npc.ai[0] -= npc.ai[1];
				if (npc.ai[2] > 0 && Main.netMode != NetmodeID.MultiplayerClient) {
					SoundEngine.PlaySound(SoundID.Item60.WithPitch(-2f), npc.Center);
					SoundEngine.PlaySound(SoundID.Item84.WithVolume(0.5f).WithPitchRange(0.85f, 1f), npc.Center);
					NPC.NewNPCDirect(
						npc.GetSource_FromAI(),
						npc.Center,
						ModContent.NPCType<Shimmer_Drone>()
					).velocity = Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2() * Main.rand.NextFloat(4, 8);
				}
				if (--npc.ai[2] <= -SpinDownAnim) {
					SetAIState(boss, StateIndex<AutomaticIdleState>());
					npc.ai[0] -= ExtraIdleTime;
				}
			}
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.ai[1] = SpawnRate;
			npc.ai[2] = Main.rand.RandomRound(SpawnCount);
			npc.ai[3] = 0;
		}
		public override double GetWeight(Shimmer_Construct boss, int[] previousStates) {
			double weight = base.GetWeight(boss, previousStates);
			if (weight <= 0) return 0;
			int droneType = ModContent.NPCType<Shimmer_Drone>();
			int drones = 0;
			int threshold = CanUseThreshold;
			foreach (NPC other in Main.ActiveNPCs) {
				if (other.type == droneType && ++drones >= threshold) return 0;
			}
			if (!CollisionExt.CanHitRay(boss.NPC.Center, boss.NPC.targetRect.Center())) return 0;
			return weight * (1 - drones / (float)threshold);
		}
	}
}
