using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Brine.Boss {
	public class Rock_Bottom : Brine_Pool_NPC, IInteractableNPC {
		public bool NeedsSync => true;
		public override bool CanChat() {
			if (Main.mouseRight && Main.npcChatRelease) {
				Interact();
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					ModPacket packet = Mod.GetPacket();
					packet.Write(Origins.NetMessageType.entity_interaction);
					packet.Write((byte)NPC.whoAmI);
					packet.Send(-1, Main.myPlayer);
				}
			}
			return false;
		}
		public void Interact() {
			NPC.ai[1] = 1;
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 15;
			NPCID.Sets.CantTakeLunchMoney[Type] = false;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			Mildew_Creeper.FriendlyNPCTypes.Add(Type);
			ModContent.GetInstance<Brine_Pool.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 6000;
			NPC.defense = 24;
			NPC.noGravity = true;
			NPC.width = 76;
			NPC.height = 58;
			NPC.knockBackResist = 0;
			NPC.SuperArmor = true;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) => NPC.AnyNPCs(Type) ? 0 : Brine_Pool.SpawnRates.Dead_Guy;
		public override bool PreAI() {
			NPC.velocity.X = 0f;
			NPC.velocity.Y = 0f;
			if (NPC.justHit) NPC.ai[1] = 1;
			if (NPC.ai[1] == 0) return false;
			NPC.dontTakeDamage = true;
			switch ((int)NPC.ai[0]) {
				default:
				NPC.ai[0] += 1f / 15;
				break;
			}
			NPC.frame.Y = NPC.frame.Height * (int)NPC.ai[0];
			if (NPC.ai[0] >= Main.npcFrameCount[Type]) {
				NPC.Transform(ModContent.NPCType<Lost_Diver>());
			}
			return false;
		}
	}
}
