using Origins.Items.Materials;
using Origins.Items.Weapons;
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
    }
}
