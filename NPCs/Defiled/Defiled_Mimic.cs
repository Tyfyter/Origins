using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Defiled;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
    public class Defiled_Mimic : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("{$Defiled} Mimic");
            Main.npcFrameCount[NPC.type] = 14;
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.BigMimicCorruption);
        }
        public override void FindFrame(int frameHeight) {
            NPC.CloneFrame(NPCID.BigMimicCorruption, frameHeight);
        }
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.OneFromOptions(1, ModContent.ItemType<Defiled_Dart_Burst>(), 3007, 3013, 3016, 3020));
			npcLoot.Add(ItemDropRule.Common(ItemID.GreaterHealingPotion, 1, 5, 10));
			npcLoot.Add(ItemDropRule.Common(ItemID.GreaterManaPotion, 1, 5, 15));
            npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 3, 3));
        }
		public override void HitEffect(int hitDirection, double damage) {
            //spawn gore if npc is dead after being hit
            if(NPC.life<0) {
                for(int i = 0; i < 3; i++)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(Main.rand.Next(NPC.width),Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for(int i = 0; i < 6; i++)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(Main.rand.Next(NPC.width),Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            }
        }
    }
}
