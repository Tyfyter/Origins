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

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class PhaseTwoIdleState : AIState {
		public static List<AIState> aiStates = [];
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
			if (++npc.ai[0] > (60 - ContentExtensions.DifficultyDamageMultiplier * 10) && Main.netMode != NetmodeID.MultiplayerClient) {
				if (aiStates.Select(state => state.Index).All(boss.previousStates.Contains)) Array.Fill(boss.previousStates, Index);
				SelectAIState(boss, aiStates);
			}
		}
		public override void TrackState(int[] previousStates) { }
	}
	public class FastDashState : DashState {
		public override void Load() {
			PhaseTwoIdleState.aiStates.Add(this);
		}
		public override void StartAIState(Shimmer_Construct boss) {
			base.StartAIState(boss);
			NPC npc = boss.NPC;
			npc.ai[3] *= 1.5f;
			npc.ai[1] *= 1.5f;
			npc.ai[2] *= 1.5f;
		}
	}
	public class FastCircleState : CircleState {
		public override void Load() {
			PhaseTwoIdleState.aiStates.Add(this);
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.ai[3] = 14 - ContentExtensions.DifficultyDamageMultiplier * 1.25f;
		}
	}
	public class MagicMissilesState : AIState {
		public override void Load() {
			PhaseTwoIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			if (++npc.ai[0] >= npc.ai[1]) {
				npc.ai[0] -= npc.ai[1];
				npc.SpawnProjectile(null,
					npc.Center,
					Vector2.UnitY.RotatedByRandom(1.5f) * -8,
					Shimmer_Construct_Missiles.ID,
					1,
					1
				);
				if (--npc.ai[2] <= 0) {
					SetAIState(boss, StateIndex<AutomaticIdleState>());
					npc.ai[0] -= ContentExtensions.DifficultyDamageMultiplier * 8;
				}
			}
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.ai[1] = 6 - ContentExtensions.DifficultyDamageMultiplier;
			npc.ai[2] = 5 + Main.rand.RandomRound(ContentExtensions.DifficultyDamageMultiplier);
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
				if (source is EntitySource_Parent parentSource && parentSource.Entity is NPC parentNPC && parentNPC.ModNPC is Shimmer_Construct construct && construct.IsInPhase3) Projectile.ai[2] = 1;
			}
			public override void AI() {
				if (Main.player.GetIfInRange((int)Projectile.ai[0]) is Player target) {
					if (target.active && !target.dead) {
						float difficultyMult = ContentExtensions.DifficultyDamageMultiplier;
						if (++Projectile.ai[1] > 30f - difficultyMult) {
							float acceleration = 2;
							if (Projectile.ai[1] >= 40) {
								acceleration = (0.2f + (difficultyMult - Projectile.ai[2]) * 0.1f) * Math.Max(1 - (Projectile.ai[1] - 40) / 80, 0);
							}
							Projectile.velocity += Projectile.DirectionTo(target.Center) * acceleration;
						}
					} else {
						Projectile.ai[0] = -1;
					}
				}
				if (Projectile.ai[1] < 28) Projectile.velocity *= 0.97f;
				else if (Projectile.ai[1] <= 32) Projectile.velocity *= 0.9f;
				if (Projectile.ai[2] == 0 && !Projectile.tileCollide && !Projectile.Hitbox.OverlapsAnyTiles()) {
					Projectile.tileCollide = true;
				}
				Projectile.rotation = Projectile.velocity.ToRotation();
			}
			public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
				if (Projectile.velocity.X != 0) modifiers.HitDirectionOverride = Math.Sign(Projectile.velocity.X);
				modifiers.Knockback.Base += 4;
			}
			public override bool PreDraw(ref Color lightColor) {
				Shimmerstar_Staff_P.DrawShimmerstar(Projectile);
				return false;
			}
		}
	}

	public class SpawnDronesStateState : AIState {
		public override void Load() {
			PhaseTwoIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;

			const float spin_spawn_ratio = 2f;
			float spinAccelFactor = npc.ai[1] * spin_spawn_ratio;
			if (npc.ai[2] > -4 + spin_spawn_ratio) npc.ai[3] += 1 / spinAccelFactor;
			else npc.ai[3] -= 1 / spinAccelFactor;
			npc.ai[3] = float.Clamp(npc.ai[3], 0, 1);
			npc.rotation += npc.direction * (0.5f * Math.Max(npc.ai[3] * npc.ai[3], 0));

			if (++npc.ai[0] >= npc.ai[1]) {
				npc.ai[0] -= npc.ai[1];
				if (npc.ai[2] > 0 && Main.netMode != NetmodeID.MultiplayerClient) {
					NPC.NewNPCDirect(
						npc.GetSource_FromAI(),
						npc.Center,
						ModContent.NPCType<Shimmer_Drone>()
					).velocity = Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2() * Main.rand.NextFloat(4, 8);
				}
				if (--npc.ai[2] <= -4) {
					SetAIState(boss, StateIndex<AutomaticIdleState>());
					npc.ai[0] -= ContentExtensions.DifficultyDamageMultiplier * 8;
				}
			}
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.ai[1] = 15 - ContentExtensions.DifficultyDamageMultiplier;
			npc.ai[2] = 5 + Main.rand.RandomRound(ContentExtensions.DifficultyDamageMultiplier);
			npc.ai[3] = 0;
		}
		public override double GetWeight(Shimmer_Construct boss, int[] previousStates) {
			double weight = base.GetWeight(boss, previousStates);
			if (weight <= 0) return 0;
			int droneType = ModContent.NPCType<Shimmer_Drone>();
			int drones = 0;
			const int threshold = 4;
			foreach (NPC other in Main.ActiveNPCs) {
				if (other.type == droneType && ++drones >= threshold) return 0;
			}
			return weight * (1 - drones / (float)threshold);
		}
	}
}
