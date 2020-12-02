using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Weapons.Acid;
using Origins.Items.Weapons.Explosives;
using Origins.Items.Weapons.Felnum.Tier2;
using Origins.NPCs.Defiled;
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
    public partial class OriginGlobalNPC : GlobalNPC {
        public override void SetupShop(int type, Chest shop, ref int nextSlot) {
            if(type==NPCID.Demolitionist && ModContent.GetInstance<OriginWorld>().peatSold>=20) {
				shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Grenade>());
				shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Bomb>());
				shop.item[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Dynamite>());
            }
        }
        public override bool PreAI(NPC npc) {
            if(npc.HasBuff(ModContent.BuffType<ImpaledBuff>())) {
                //npc.position = npc.oldPosition;//-=npc.velocity;
                npc.velocity = Vector2.Zero;
                return false;
            }
            return true;
        }
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) {
            if(npc.HasBuff(ModContent.BuffType<ImpaledBuff>()))return false;
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit) {
            if(npc.HasBuff(ModContent.BuffType<SolventBuff>())) {
                damage+=Math.Max(npc.defense/2, 20);
            }
        }
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(npc.HasBuff(ModContent.BuffType<SolventBuff>())) {
                damage+=Math.Max(npc.defense/2, 20);
            }
        }
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
            if(Main.LocalPlayer.GetModPlayer<OriginPlayer>().ZoneDefiled) {
                pool[0] = 0;
                byte flatTiles = 0;
                Tile tile;
                for(int i = spawnInfo.spawnTileX-1; i<spawnInfo.spawnTileX+2; i++) {
                    tile = Main.tile[i, spawnInfo.spawnTileY+1];
                    if(!tile.active()||!Main.tileSolid[tile.type]||tile.slope()!=SlopeID.None)flatTiles++;//break;
                    //flatTiles++;
                }
                tile = null;
                if(flatTiles>2) {//check if on flat ground
                    pool.Add(ModContent.NPCType<Defiled_Mite>(), 1);
                }
            }
        }
    }
}
