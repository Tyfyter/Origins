using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria;
using static Origins.NPCs.MiscB.Shimmer_Construct.Shimmer_Construct;
using PegasusLib;
using Terraria.ModLoader;
using Terraria.Graphics;

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class PhaseTwoIdleState : AIState {
		public static List<AIState> aiStates = [];
		public override void Load() {
			AutomaticIdleState.aiStates.Add((this, boss => (boss.NPC.life * 2 < boss.NPC.lifeMax).Mul(2)));
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
		public override double GetWeight(Shimmer_Construct boss, int[] previousStates) {
			return base.GetWeight(boss, previousStates);
		}
		public class Shimmer_Construct_Missiles : ModProjectile {
			public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MagicMissile;
			public static int ID { get; private set; }
			public override void SetStaticDefaults() {
				ProjectileID.Sets.TrailingMode[Type] = 3;
				ProjectileID.Sets.TrailCacheLength[Type] = 30;
				ID = Type;
			}
			public override void SetDefaults() {
				Projectile.friendly = false;
				Projectile.hostile = true;
				Projectile.aiStyle = 0;
				Projectile.width = 24;
				Projectile.height = 24;
				Projectile.tileCollide = false;
			}
			public override void AI() {
				if (Main.player.GetIfInRange((int)Projectile.ai[0]) is Player target) {
					if (target.active && !target.dead) {
						float difficultyMult = ContentExtensions.DifficultyDamageMultiplier;
						if (++Projectile.ai[1] > 30f - difficultyMult) {
							float acceleration = 2;
							if (Projectile.ai[1] >= 40) {
								acceleration = (0.2f + difficultyMult * 0.1f) * Math.Max(1 - (Projectile.ai[1] - 40) / 80, 0);
							}
							Projectile.velocity += Projectile.DirectionTo(target.Center) * acceleration;
						}
					} else {
						Projectile.ai[0] = -1;
					}
				}
				if (Projectile.ai[1] < 28) Projectile.velocity *= 0.97f;
				else if (Projectile.ai[1] <= 32) Projectile.velocity *= 0.9f;
				if (!Projectile.tileCollide && !Projectile.Hitbox.OverlapsAnyTiles()) {
					Projectile.tileCollide = true;
				}
			}
			public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
				if (Projectile.velocity.X != 0) modifiers.HitDirectionOverride = Math.Sign(Projectile.velocity.X);
				modifiers.Knockback.Base += 4;
			}
			public override bool PreDraw(ref Color lightColor) {
				default(MagicMissileDrawer).Draw(Projectile);
				return false;
			}
		}
	}
}
