using Origins.Items.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs {
    public partial class OriginGlobalNPC : GlobalNPC{
        public override void NPCLoot(NPC npc) {
            switch(npc.type) {
                case NPCID.CaveBat:
                case NPCID.GiantBat:
                case NPCID.IceBat:
                case NPCID.IlluminantBat:
                case NPCID.JungleBat:
                case NPCID.VampireBat:
                Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ModContent.ItemType<Bat_Hide>(), 1+Main.rand.Next(2));
                break;
                default:
                break;
            }
        }
    }
}
