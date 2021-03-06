﻿using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Weapons.Acid;
using Origins.Items.Weapons.Explosives;
using Origins.Items.Weapons.Felnum.Tier2;
using Origins.NPCs.Defiled;
using Origins.Tiles.Defiled;
using Origins.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.World.BiomeData;
using Origins.Buffs;

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
            if(npc.HasBuff(Impaled_Debuff.ID)) {
                //npc.position = npc.oldPosition;//-=npc.velocity;
                npc.velocity = Vector2.Zero;
                return false;
            }
            return true;
        }
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) {
            if(npc.HasBuff(Impaled_Debuff.ID)||npc.HasBuff(Stunned_Debuff.ID))return false;
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }
        public override bool StrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit) {
            if(npc.HasBuff(SolventDebuff.ID)&&crit) {
                damage*=1.3;
            }
            return true;
        }
        public override void OnHitNPC(NPC npc, NPC target, int damage, float knockback, bool crit) {
            knockback*=MeleeCollisionNPCData.knockbackMult;
            MeleeCollisionNPCData.knockbackMult = 1f;
        }
        public override bool PreNPCLoot(NPC npc) {
            byte worldEvil = ModContent.GetInstance<OriginWorld>().worldEvil;
            if((worldEvil&4)!=0) {
                switch(npc.type) {
                    case NPCID.EaterofWorldsHead:
                    case NPCID.EaterofWorldsBody:
                    case NPCID.EaterofWorldsTail:
                    case NPCID.BrainofCthulhu:
                    break;
                    default:
                    NPCLoader.blockLoot.Add(ItemID.CorruptSeeds);
                    NPCLoader.blockLoot.Add(ItemID.DemoniteOre);
                    break;
                }
            }
            switch(npc.type) {
                case NPCID.SkeletronHead:
                downedSkeletron = NPC.downedBoss3;
                break;
            }
            return true;
        }
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
            Player player = spawnInfo.player;
            if(player.GetModPlayer<OriginPlayer>().ZoneDefiled) {
                pool[0] = 0;

                pool.Add(ModContent.NPCType<Defiled_Cyclops>(), DefiledWastelands.SpawnRates.Cyclops);

                pool.Add(ModContent.NPCType<Defiled_Brute>(), DefiledWastelands.SpawnRates.Brute);

                if(spawnInfo.playerFloorY <= Main.worldSurface+50&&spawnInfo.spawnTileY < Main.worldSurface-50)pool.Add(ModContent.NPCType<Defiled_Flyer>(), DefiledWastelands.SpawnRates.Flyer*(player.ZoneSkyHeight?2:1));
                if(Main.hardMode) {
                    pool.Add(ModContent.NPCType<Defiled_Hunter_Head>(), DefiledWastelands.SpawnRates.Hunter);
                }

                if(spawnInfo.spawnTileY>Main.worldSurface) {
                    pool.Add(ModContent.NPCType<Defiled_Digger_Head>(), DefiledWastelands.SpawnRates.Worm);
                    int yPos = spawnInfo.spawnTileY;
                    Tile tile;
                    for(int i = 0; i < Defiled_Mite.spawnCheckDistance; i++) {
                        tile = Main.tile[spawnInfo.spawnTileX, ++yPos];
                        if(tile.active()) {
                            yPos--;
                            break;
                        }
                    }
                    bool? halfSlab = null;
                    for(int i = spawnInfo.spawnTileX-1; i<spawnInfo.spawnTileX+2; i++) {
                        tile = Main.tile[i, yPos+1];
                        if(!tile.active()||!Main.tileSolid[tile.type]||tile.slope()!=SlopeID.None||(halfSlab.HasValue&&halfSlab.Value!=tile.halfBrick())) {
                            tile = null;
                            goto SkipMiteSpawn;
                        }
                        halfSlab = tile.halfBrick();
                    }
                    tile = null;
                    pool.Add(ModContent.NPCType<Defiled_Mite>(), DefiledWastelands.SpawnRates.Mite);
                    SkipMiteSpawn:;
                }
            }
        }
    }
}
