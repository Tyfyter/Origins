using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class Shimmer_Drone : ModNPC {
		public override string Texture => "Terraria/Images/NPC_" + NPCID.ServantofCthulhu;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 3;
			NPCID.Sets.ShimmerTransformToNPC[NPCID.ServantofCthulhu] = Type;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
			NPCID.Sets.DontDoHardmodeScaling[Type] = true;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.ServantofCthulhu);
			NPC.damage = 18;
			NPC.lifeMax = 30;
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
		}
		public override void OnSpawn(IEntitySource source) => NPC.ai[1] = -2;
		public override void AI() {
			Vector2 targetPos = default;
			if (NPC.ai[0] == 0) {
				switch (NPC.ai[1]) {
					case -1:
					NPC.TargetClosest();
					NPC.ai[0] = 1;
					break;

					case -2:
					NPC.ai[1] = NPC.FindFirstNPC(ModContent.NPCType<Shimmer_Construct>());
					if (NPC.ai[1] == -1) goto case -1;
					break;
				}
				if (NPC.ai[1] == -1) return;
				NPC.targetRect = Main.npc[(int)NPC.ai[1]].Hitbox;
				targetPos = NPC.targetRect.Center();

				Vector2 diffToOwner = targetPos - NPC.Center;
				float dist = diffToOwner.Length();
				bool passed = dist < 16 * 4 && (dist <= 0 || Vector2.Dot(diffToOwner / dist, NPC.ai[2].ToRotationVector2()) < 0);
				if (passed) {
					NPC.ai[0] = 1;
					NPC owner = Main.npc[(int)NPC.ai[1]];
					if (owner.active) {
						NPC.target = owner.target;
						NPC.targetRect = Main.player[NPC.target].Hitbox;
						targetPos = NPC.targetRect.Center();
						NPC.ai[2] = (targetPos - NPC.Center).ToRotation();
					} else {
						NPC.ai[1] = -1;
					}
				}
			} else {
				targetPos = NPC.targetRect.Center();

				if (Vector2.Dot(NPC.DirectionTo(targetPos), NPC.ai[2].ToRotationVector2()) < 0) {
					NPC.ai[0] = 0;
					if (NPC.ai[1] != -1) {
						NPC.targetRect = Main.npc[(int)NPC.ai[1]].Hitbox;
						targetPos = NPC.targetRect.Center();
					}
				}
			}
			NPC.velocity += NPC.DirectionTo(targetPos) * 0.5f;
			NPC.velocity = NPC.velocity.SafeNormalize(default) * 8;
		}
	}
}
