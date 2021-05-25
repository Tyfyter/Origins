using Origins.Items.Materials;
using Origins.Items.Weapons.Other;
using Origins.Tiles;
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
        bool downedSkeletron = false;
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
                case NPCID.SkeletronHead:
                if(!downedSkeletron)GenFelnumOre();
                break;
                case NPCID.ArmoredSkeleton:
                case NPCID.SkeletonArcher:
                if(Main.rand.Next(50)==0)Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ModContent.ItemType<Tiny_Sniper>());
                break;
                default:
                break;
            }
            Player closestPlayer = Main.player[Player.FindClosest(npc.position, npc.width, npc.height)];
            OriginPlayer originPlayer = closestPlayer.GetModPlayer<OriginPlayer>();
		    if (Main.rand.Next(2500) == 0 && originPlayer.ZoneDefiled) {
			    Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ModContent.ItemType<Defiled_Key>());
		    }
        }
        void GenFelnumOre() {
            if(!Main.gameMenu && Main.netMode != NetmodeID.MultiplayerClient) {
                int x = 0, y = 0;
                int felnumOre = ModContent.TileType<Felnum_Ore>();
                int type = TileID.BlueDungeonBrick;
                Tile tile = new Tile();
                int fails = 0;
                int success = 0;
                for (int k = 0; k < (int)((Main.maxTilesX * Main.maxTilesY) * (Main.expertMode?6E-06:4E-06)); k++) {
                    int tries = 0;
                    type = TileID.BlueDungeonBrick;
                    while(type!=TileID.Cloud&&type!=TileID.Dirt&&type!=TileID.Grass&&type!=TileID.Stone&&type!=TileID.RainCloud) {
				        x = WorldGen.genRand.Next(0, Main.maxTilesX);
						y = WorldGen.genRand.Next(90, (int)OriginWorld.worldSurfaceLow - 5);
                        tile = Framing.GetTileSafely(x, y);
                        type = tile.active()?tile.type:TileID.BlueDungeonBrick;
                        if(++tries >= 150) {
                            if(++fails%2==0)k--;
                            success--;
                            type = TileID.Dirt;
                        }
                    }
                    success++;
				    GenRunners.FelnumRunner(x, y, WorldGen.genRand.Next(2, 6), WorldGen.genRand.Next(2, 6), felnumOre);
			    }
                //Main.NewText($"generation complete, ran {runCount} times with {fails} fails");
            }
        }
    }
}
