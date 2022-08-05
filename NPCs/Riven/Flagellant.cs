using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Weapons.Riven;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    public class Flagellant : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Flagellant");
            Main.npcFrameCount[NPC.type] = 4;
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.BloodJelly);
            NPC.lifeMax = 380;
            NPC.defense = 20;
            NPC.damage = 70;
            NPC.width = 56;
            NPC.height = 60;
            NPC.frame.Height = 58;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement("A gentle swimmer in the amoeba-infested waters. It is flimsy and fragile, so travelers should be weary when approaching it."),
            });
        }
        public override void FindFrame(int frameHeight) {
		    NPC.spriteDirection = NPC.direction;
		    NPC.frameCounter += 1.0;
		    if (NPC.frameCounter >= 24.0){
			    NPC.frameCounter = 0.0;
		    }
		    NPC.frame.Y = 60 * (int)(NPC.frameCounter / 6.0);
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Amebic_Gel>(), 1, 1, 3));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Jam_Sandwich>(), 17));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Flagellash>(), 25));
        }
    }
}
