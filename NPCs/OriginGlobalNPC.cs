using Origins.Items.Materials;
using Origins.Items.Weapons.Explosives;
using Origins.Items.Weapons.Felnum.Tier2;
using Origins.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs {
    public class OriginGlobalNPC : GlobalNPC{
        public override void SetupShop(int type, Chest shop, ref int nextSlot) {
            if(type==NPCID.Demolitionist && ModContent.GetInstance<OriginWorld>().peatSold>=20) {
				shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Grenade>());
				shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Bomb>());
				shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Dynamite>());
            }
        }
        public override bool PreAI(NPC npc) {
            if(npc.HasBuff(ModContent.BuffType<ImpaledBuff>())) {
                npc.position-=npc.velocity;
                return false;
            }
            return base.PreAI(npc);
        }
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) {
            if(npc.HasBuff(ModContent.BuffType<ImpaledBuff>()))return false;
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }
    }
}
