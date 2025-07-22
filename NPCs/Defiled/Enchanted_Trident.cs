using Origins.Dev;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Enchanted_Trident : ModNPC, IDefiledEnemy, IWikiNPC, ICustomWikiStat {
		public Rectangle DrawRect => new(0, 2, 76, 76);
		public int AnimationFrames => 1;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 3;
		}
		public override void FindFrame(int frameHeight) {
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Scale = 0.85f,
				PortraitScale = 1
			};
		}
		public bool? Hardmode => true;
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
			SpawnModBiomes = [
				ModContent.GetInstance<Underground_Defiled_Wastelands_Biome>().Type
			];
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
			bestiaryEntry.AddTags(
				/*if (heartBroken <= 1) {
				this.GetBestiaryFlavorText(""A weapon directly forged by the {$Defiled}. This weapon is a machination of its curiosity, and a true testament to how intelligent it is.");
				} else {
				this.GetBestiaryFlavorText("A weapon directly forged by the {$Defiled}. This weapon is a machination of its curiosity.");
				}*/
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.MeatGrinder, 200));
			npcLoot.Add(ItemDropRule.StatusImmunityItem(ItemID.Nazar, 100));
		}
		public override void HitEffect(NPC.HitInfo hit) {

		}
	}
}
