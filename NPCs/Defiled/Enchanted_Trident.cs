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

namespace Origins.NPCs.Defiled {
    public class Enchanted_Trident : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Profaned Bident");
            Main.npcFrameCount[npc.type] = 3;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.CursedHammer);
            npc.aiStyle = NPCAIStyleID.Flying_Weapon;
            npc.lifeMax = 175;
            npc.defense = 16;
            npc.damage = 85;
            npc.width = 40;
            npc.height = 40;
            npc.knockBackResist = 0.35f;
        }
		public override void AI() {
			if (npc.ai[0] == 2) {
                npc.ai[1] += 0.25f;
			}
		}
		public override void NPCLoot() {
            if (Main.rand.NextBool(100)|| (Main.expertMode && Main.rand.NextBool(100))) {
                Item.NewItem(npc.Hitbox, ItemID.Nazar, prefixGiven:-1);
            }
        }
		public override void HitEffect(int hitDirection, double damage) {

        }
    }
}
