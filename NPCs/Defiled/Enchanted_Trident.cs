using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
    public class Enchanted_Trident : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Profaned Bident");
            Main.npcFrameCount[NPC.type] = 3;
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
        }
		public override void AI() {
			if (NPC.ai[0] == 2) {
                NPC.ai[1] += 0.25f;
			}
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement("Forged by the {$Defiled}. This weapon is a machination of its curiosity."),
            });
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.StatusImmunityItem(ItemID.Nazar, 100));
            npcLoot.Add(ItemDropRule.Common(ItemID.SilverCoin, 1, 10, 10));
        }
		public override void HitEffect(int hitDirection, double damage) {

        }
    }
}
