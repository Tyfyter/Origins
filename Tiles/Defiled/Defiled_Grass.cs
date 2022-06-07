using Microsoft.Xna.Framework;
using Origins.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Tiles.Defiled {
    public class Defiled_Grass : DefiledTile {
		public override void SetStaticDefaults() {
            TileID.Sets.Grass[Type] = true;
            TileID.Sets.NeedsGrassFraming[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.Conversion.Grass[Type] = true;
            TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
            Main.tileMerge[Type] = Main.tileMerge[TileID.Grass];
            Main.tileMerge[Type][TileID.Dirt] = true;
            Main.tileMerge[TileID.Dirt][Type] = true;
            Main.tileMerge[Type][TileID.Mud] = true;
            Main.tileMerge[TileID.Mud][Type] = true;
            for(int i = 0; i < TileLoader.TileCount; i++) {
                if(TileID.Sets.Grass[i]||TileID.Sets.GrassSpecial[i]) {
                    Main.tileMerge[Type][i] = true;
                    Main.tileMerge[i][Type] = true;
                }
            }
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(200, 200, 200));
			//SetModTree(Defiled_Tree.Instance);
            ItemDrop = ItemID.DirtBlock;
		}
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
            fail = true;
            noItem = true;
            OriginSystem originWorld = ModContent.GetInstance<OriginSystem>();
            if(!(originWorld is null)) {
                if(originWorld.defiledAltResurgenceTiles is null)originWorld.defiledAltResurgenceTiles = new List<(int, int, ushort)> { };
                originWorld.defiledAltResurgenceTiles.Add((i, j, Type));
            }
            bool half = Main.tile[i, j].IsHalfBlock;
            SlopeType slope = Main.tile[i, j].Slope;
            Main.tile[i, j].ResetToType(TileID.Dirt);
            WorldGen.SquareTileFrame(i, j);
            Main.tile[i, j].SetHalfBlock(half);
            Main.tile[i, j].SetSlope(slope);
            NetMessage.SendTileSquare(-1, i, j, 1);

        }
        public override void RandomUpdate(int i, int j) {
            int retryCount = 0;
            retry:
            if(retryCount++>100)return;
            switch(WorldGen.genRand.Next(Main.hardMode?3:2)) {
                case 0:
                WeightedRandom<(int, int)> rand = new WeightedRandom<(int, int)>();
                Tile current;
                for(int y = -1; y < 2&&(j+y)<Main.worldSurface; y++) {
                    for(int x = -1; x < 2; x++) {
                        current = Main.tile[i+x, j+y];
                        if(current.TileType==TileID.Grass) {
                            if(Main.tile[i+x, j+y-1].TileType!=TileID.Sunflower)rand.Add((i+x,j+y));
                        }else if(current.TileType==TileID.Dirt) {
                            if(!(Main.tile[i+x-1, j+y].HasTile&&Main.tile[i+x+1, j+y].HasTile&&Main.tile[i+x, j+y-1].HasTile&&Main.tile[i+x, j+y+1].HasTile)) {
                                rand.Add((i+x,j+y));
                            }
                        }
                    }
                }
                if(rand.elements.Count>0) {
                    (int x, int y) pos = rand.Get();
                    OriginSystem.ConvertTileWeak(ref Main.tile[pos.x, pos.y].TileType, OriginSystem.evil_wastelands);
                    OriginSystem.ConvertWall(ref Main.tile[pos.x, pos.y].WallType, OriginSystem.evil_wastelands);
				    WorldGen.SquareTileFrame(pos.x, pos.y);
				    NetMessage.SendTileSquare(-1, pos.x, pos.y, 1);
                } else {
                    goto retry;
                }
                break;
                case 1:
                if(!Main.tile[i, j-1].HasTile&&!(Main.tile[i, j].Slope!=0||Main.tile[i, j].IsHalfBlock)) {
                    Main.tile[i, j-1].ResetToType((ushort)ModContent.TileType<Defiled_Foliage>());
                } else {
                    goto retry;
                }
                break;
                case 2:
                base.RandomUpdate(i,j);
                break;
            }
        }
		/*public override int SaplingGrowthType(ref int style) {
			style = 0;
			return ModContent.TileType<Defiled_Tree_Sapling>();
		}
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
            Main.tile[i, i].type = TileID.CorruptGrass;
            WorldGen.TileFrame(i,j,true,noBreak);
            Main.tile[i, i].type = Type;
            return false;
        }*/
    }
    public class Defiled_Grass_Seeds : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Seeds");
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.GrassSeeds);
            Item.placeStyle = ModContent.TileType<Defiled_Grass>();
		}
        public override bool ConsumeItem(Player player) {
            Main.tile[Player.tileTargetX, Player.tileTargetY].TileType = (ushort)Item.placeStyle;
            return true;
        }
    }
}
