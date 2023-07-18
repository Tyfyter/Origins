using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Enchanted_Trident : ModNPC, IDefiledEnemy {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Profaned Bident");
			Main.npcFrameCount[NPC.type] = 3;
			SpawnModBiomes = new int[] {
				ModContent.GetInstance<Defiled_Wastelands>().Type
			};
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.CursedHammer);
			NPC.aiStyle = NPCAIStyleID.Flying_Weapon;
			NPC.lifeMax = 175;
			NPC.defense = 16;
			NPC.damage = 85;
			NPC.width = 40;
			NPC.height = 40;
			NPC.knockBackResist = 0.35f;
			NPC.value = 1000;
		}
		public bool ForceSyncMana => false;
		public float Mana { get; set; }
		public override void AI() {
			if (NPC.ai[0] == 2) {
				NPC.ai[1] += 0.25f;
			}
		}
		public void SpawnWisp(NPC npc) { }
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				/*if (heartBroken <= 1) {
				new FlavorTextBestiaryInfoElement(""A weapon directly forged by the {$Defiled}. This weapon is a machination of its curiosity, and a true testament to how intelligent it is.");
				} else {
				new FlavorTextBestiaryInfoElement("A weapon directly forged by the {$Defiled}. This weapon is a machination of its curiosity.");
				}*/
				new FlavorTextBestiaryInfoElement("A weapon directly forged by the {$Defiled}. This weapon is a machination of its curiosity."),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.StatusImmunityItem(ItemID.Nazar, 100));
		}
		public override void HitEffect(NPC.HitInfo hit) {

		}
	}
}
