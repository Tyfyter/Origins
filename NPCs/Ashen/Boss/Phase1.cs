using Origins.NPCs.MiscB.Shimmer_Construct;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
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
	public class Fire_Guns_State : AIState {
		#region stats
		public static float ShotRate => 10 - DifficultyMult;
		public static int ShotDamage => (int)(18 * DifficultyMult);
		public static float ShotVelocity => 6;
		public static float MoveSpeed => 6.5f + ContentExtensions.DifficultyDamageMultiplier * 0.5f;
		public static int Duration => 45;
		#endregion stats
		public override bool Ranged => true;
		public override void Load() {
			PhaseOneIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Trenchmaker boss) {
			NPC npc = boss.NPC;
			Vector2 diff = npc.GetTargetData().Center - npc.Center;
			Vector2 direction = diff.SafeNormalize(Vector2.UnitY);
			npc.rotation = direction.ToRotation() - MathHelper.PiOver2;
			int shotsToHaveFired = (int)((++npc.ai[0]) / npc.ai[3]);
			if (shotsToHaveFired > npc.ai[1]) {
				SoundEngine.PlaySound(SoundID.Item12.WithVolume(0.5f).WithPitchRange(0.25f, 0.4f), npc.Center);
				npc.ai[1]++;
				npc.SpawnProjectile(null,
					npc.Center,
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
