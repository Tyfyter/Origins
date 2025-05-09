using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria;
using static Origins.NPCs.MiscB.Shimmer_Construct.Shimmer_Construct;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class PhaseThreeIdleState : AIState {
		public static List<AIState> aiStates = [];
		public override void Load() {
			AutomaticIdleState.aiStates.Add((this, boss => boss.IsInPhase3.Mul(3)));
		}
		public override void SetStaticDefaults() {

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
	public class Weak_Shimmer_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Shimmer;
		public static int ID { get; private set; }
		public override void Load() {
			On_Player.ShimmerCollision += (On_Player.orig_ShimmerCollision orig, Player self, bool fallThrough, bool ignorePlats, bool noCollision) => {
				if (self.OriginPlayer().weakShimmer) {
					self.position += self.velocity * new Vector2(0.75f, 0.5f);
				} else {
					orig(self, fallThrough, ignorePlats, noCollision);
				}
			};
			On_Player.TryLandingOnDetonator += (orig, self) => {
				orig(self);
				if (self.OriginPlayer().weakShimmer) {
					Collision.up = false;
					Collision.down = false;
				}
			};
		}
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.shimmering = true;
			player.timeShimmering = 0;
			player.OriginPlayer().weakShimmer = true;
		}
	}
}
