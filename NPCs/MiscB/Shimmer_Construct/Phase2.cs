using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria;
using static Origins.NPCs.MiscB.Shimmer_Construct.Shimmer_Construct;
using PegasusLib;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class PhaseTwoIdleState : AIState {
		public static List<AIState> aiStates = [];
		public override void SetStaticDefaults() {
			aiStates.Add(ModContent.GetInstance<SpawnCloudsState>());
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.ai[0] = 0;
			npc.ai[1] = 0;
			npc.ai[2] = 0;
			npc.ai[3] = 0;
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
			npc.ai[3] = 11;
		}
	}
	public class MagicMissilesState : AIState {
		public override void Load() {
			PhaseTwoIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.SpawnProjectile(null,
				npc.Center,
				Vector2.UnitY.RotatedByRandom(1) * -7,
				ProjectileID.FairyQueenSunDance,
				1,
				1
			);
			if (--npc.ai[0] <= 0) SetAIState(boss, StateIndex<PhaseOneIdleState>());
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.ai[0] = 5 + Main.rand.RandomRound(ContentExtensions.DifficultyDamageMultiplier);
		}
	}
}
