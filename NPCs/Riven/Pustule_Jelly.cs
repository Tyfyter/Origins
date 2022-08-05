using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    public class Pustule_Jelly : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Pustule Jelly");
            Main.npcFrameCount[NPC.type] = 4;
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.BloodJelly);
            NPC.lifeMax = 380;
            NPC.defense = 20;
            NPC.damage = 70;
            NPC.width = 32;
            NPC.height = 42;
            NPC.frame.Height = 40;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement("A Riven-infected jellyfish living in its new parasite-prevalent environment."),
            });
        }
        public override void FindFrame(int frameHeight) {
		    NPC.spriteDirection = NPC.direction;
		    NPC.frameCounter += 1.0;
		    if (NPC.frameCounter >= 24.0){
			    NPC.frameCounter = 0.0;
		    }
		    NPC.frame.Y = 42 * (int)(NPC.frameCounter / 6.0);
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bud_Barnacle>(), 1, 1, 3));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Jam_Sandwich>(), 17));
        }
    }
}
