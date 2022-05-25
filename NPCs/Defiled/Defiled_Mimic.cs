using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Origins.Items.Weapons.Defiled;

namespace Origins.NPCs.Defiled {
    public class Defiled_Mimic : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Mimic");
            Main.npcFrameCount[npc.type] = 14;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.BigMimicCorruption);
        }
        public override void FindFrame(int frameHeight) {
            npc.CloneFrame(NPCID.BigMimicCorruption, frameHeight);
        }
		public override void NPCLoot() {
			switch (Main.rand.Next(5)) {
				case 0:
				Item.NewItem(npc.position, npc.Size, ModContent.ItemType<Defiled_Dart_Burst>(), 1, prefixGiven: -1);
				break;
				case 1:
				Item.NewItem(npc.position, npc.Size, ItemID.ClingerStaff, 1, prefixGiven: -1);
				break;
				case 2:
				Item.NewItem(npc.position, npc.Size, ItemID.ChainGuillotines, 1, prefixGiven: -1);
				break;
				case 3:
				Item.NewItem(npc.position, npc.Size, ItemID.PutridScent, 1, prefixGiven: -1);
				break;
				case 4:
				Item.NewItem(npc.position, npc.Size, ItemID.WormHook, 1, prefixGiven:-1);
				break;
			}
			Item.NewItem(npc.position, npc.Size, ItemID.GreaterHealingPotion, Main.rand.Next(5, 11));
			Item.NewItem(npc.position, npc.Size, ItemID.GreaterManaPotion, Main.rand.Next(5, 16));
		}
		public override void HitEffect(int hitDirection, double damage) {
            //spawn gore if npc is dead after being hit
            if(npc.life<0) {
                for(int i = 0; i < 3; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for(int i = 0; i < 6; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            }
        }
    }
}
