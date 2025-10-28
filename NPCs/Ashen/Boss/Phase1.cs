using Origins.NPCs.MiscB.Shimmer_Construct;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.NPCs.Ashen.Boss.Trenchmaker;
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
	public class Teabag_State : AIState {
		public override void Load() {
			AutomaticIdleState.aiStates.Add((this, boss => (!boss.NPC.HasValidTarget).Mul(10000)));
		}
		public override void DoAIState(Trenchmaker boss) {

		}
		public override void TrackState(int[] previousStates) { }
	}
}
