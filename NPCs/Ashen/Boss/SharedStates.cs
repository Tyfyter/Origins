using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics.Primitives;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Projectiles;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
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
