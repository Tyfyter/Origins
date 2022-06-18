﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;

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
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.StatusImmunityItem(ItemID.Nazar, 100));
        }
		public override void HitEffect(int hitDirection, double damage) {

        }
    }
}