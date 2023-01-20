using Microsoft.Xna.Framework;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Ranged;
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
			NPC.width = 28;
			NPC.height = 44;
			NPC.aiStyle = NPCAIStyleID.Biome_Mimic;
			NPC.damage = 90;
			NPC.defense = 34;
			NPC.lifeMax = 3500;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.value = 30000f;
			NPC.knockBackResist = 0.1f;
			NPC.rarity = 5;
		}
		public override int SpawnNPC(int tileX, int tileY) {//should disable spawning
			return -1;
		}
		public override void FindFrame(int frameHeight) {
            NPC.CloneFrame(NPCID.BigMimicCorruption, frameHeight);
        }
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.OneFromOptions(5, ModContent.ItemType<Incision>(), 3007, 3013, 3016, 3020));
			npcLoot.Add(ItemDropRule.OneFromOptions(5, ModContent.ItemType<Ravel>(), 3007, 3013, 3016, 3020));
			npcLoot.Add(ItemDropRule.Common(ItemID.GreaterHealingPotion, 1, 5, 10));
			npcLoot.Add(ItemDropRule.Common(ItemID.GreaterManaPotion, 1, 5, 15));
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
