using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Weapons.Riven;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    public class Measly_Moeba : Glowing_Mod_NPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Measly Moeba");
            Main.npcFrameCount[NPC.type] = 4;
            SpawnModBiomes = new int[] {
                ModContent.GetInstance<Riven_Hive>().Type
            };
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.BloodJelly);
            NPC.lifeMax = 34;
            NPC.defense = 4;
            NPC.damage = 25;
            NPC.width = 20;
            NPC.height = 20;
            NPC.frame.Height = 22;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement(""),
            });
        }
        public override void FindFrame(int frameHeight) {
		    NPC.spriteDirection = NPC.direction;
		    NPC.frameCounter += 1.0;
		    if (NPC.frameCounter >= 24.0){
			    NPC.frameCounter = 0.0;
		    }
		    NPC.frame.Y = 24 * (int)(NPC.frameCounter / 6.0);
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {

        }
    }
}
