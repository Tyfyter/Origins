using Origins.NPCs.MiscB.Shimmer_Construct;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.NPCs.StateBossMethods<Origins.NPCs.Ashen.Boss.Trenchmaker>;

namespace Origins.NPCs.Ashen.Boss {
	[AutoloadBossHead]
	public class Trenchmaker : ModNPC, IStateBoss<Trenchmaker> {
		public static AIList<Trenchmaker> AIStates { get; } = [];
		public int[] PreviousStates { get; } = new int[6];
		public override void Load() {
			this.AddBossControllerItem();
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 8;
			this.SetupStates();
		}
		public override void SetDefaults() {
			NPC.width = 120;
			NPC.height = 98;
			NPC.lifeMax = 6600;
			NPC.damage = 27;
			NPC.defense = 6;
			NPC.boss = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.npcSlots = 200;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-2f);
			NPC.knockBackResist = 0;
			Array.Fill(PreviousStates, NPC.aiAction);
		}
		public override void AI() {
			this.GetState().DoAIState(this);
		}
		public class AutomaticIdleState : AutomaticIdleState<Trenchmaker> { }
		public abstract class AIState : AIState<Trenchmaker> { }
	}
}
