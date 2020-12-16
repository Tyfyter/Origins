using Origins.Items.Weapons.Felnum.Tier2;
using Origins.Tiles;
using Origins.Tiles.Dusk;
using Origins.Tiles.Defiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria;

namespace Origins.World {
    public partial class OriginWorld : ModWorld {
		public static int voidTiles;
		public static int defiledTiles;
        public int peatSold;
        public const float biomeShaderSmoothing = 0.025f;
        public byte worldEvil = 0;
        public bool defiledResurgence = true;
        public const byte evil_corruption = 0b0001;//1
        public const byte evil_crimson = 0b0010;//2
        //difference of 4
        public const byte evil_wastelands = 0b0101;//5
        public const byte evil_riven = 0b0110;//6


        public override void Load(TagCompound tag) {
            peatSold = tag.GetAsInt("peatSold");
            worldEvil = tag.GetByte("worldEvil");
            defiledResurgenceTiles = new List<(int, int)>(){};
        }
        public override TagCompound Save() {
            return new TagCompound() { {"peatSold",  peatSold} , {"worldEvil",  worldEvil} };
        }
        public override void ResetNearbyTileEffects() {
			voidTiles = 0;
            defiledTiles = 0;
		}

		public override void TileCountsAvailable(int[] tileCounts) {
			voidTiles = tileCounts[ModContent.TileType<Dusk_Stone>()];
			defiledTiles = tileCounts[ModContent.TileType<Defiled_Stone>()]+tileCounts[ModContent.TileType<Defiled_Grass>()]+tileCounts[ModContent.TileType<Defiled_Sand>()]+tileCounts[ModContent.TileType<Defiled_Ice>()];
            Main.sandTiles+=tileCounts[ModContent.TileType<Defiled_Sand>()];
        }
        protected internal List<(int, int)> defiledResurgenceTiles;
        public override void PostUpdate() {
            if(defiledResurgence&&defiledResurgenceTiles.Count>0&&WorldGen.genRand.Next(5)==0) {
                int index = WorldGen.genRand.Next(defiledResurgenceTiles.Count);
                (int k, int l) pos = defiledResurgenceTiles[index];
				ConvertTile(ref Main.tile[pos.k, pos.l].type, evil_wastelands);
				WorldGen.SquareTileFrame(pos.k, pos.l);
				NetMessage.SendTileSquare(-1, pos.k, pos.l, 1);
                defiledResurgenceTiles.RemoveAt(index);
            }
        }
        /// <summary>
        /// the first clentaminator conversion type from Origins (from ASCII 'Org')
        /// </summary>
        public const int origin_conversion_type =  79114103;
        public static void ConvertHook(On.Terraria.WorldGen.orig_Convert orig, int i, int j, int conversionType, int size = 4) {
            Tile current;
            int tileConvertBuffer = -1;
            OriginWorld originWorld = ModContent.GetInstance<OriginWorld>();
            switch(conversionType) {
                case origin_conversion_type:
                for(int k = i - size; k <= i + size; k++) {
                    for(int l = j - size; l <= j + size; l++) {
                        if(!WorldGen.InWorld(k, l, 1) || Math.Abs(k - i) + Math.Abs(l - j) >= 6) {
                            continue;
                        }
                        current = Main.tile[k,l];
                        ConvertTile(ref current.type, evil_wastelands);
                    }
                }
                break;
                default:
                for(int k = i - size; k <= i + size; k++) {
                    for(int l = j - size; l <= j + size; l++) {
                        tileConvertBuffer = -1;
                        current = Main.tile[k,l];
                        if(originWorld.defiledResurgence&&ModContent.GetModTile(current.type) is DefiledTile) {
                            originWorld.defiledResurgenceTiles.Add((k,l));
                        }
                        if(conversionType == 0) {
                            if(!WorldGen.InWorld(k, l, 1) || Math.Abs(k - i) + Math.Abs(l - j) >= 6) {
                                continue;
                            }
                            //convert based on conversion sets
                            if(TileID.Sets.Conversion.Grass[current.type]) {
                                tileConvertBuffer = TileID.Grass;
                            } else if(TileID.Sets.Conversion.Stone[current.type]) {
                                tileConvertBuffer = TileID.Stone;
                            } else if(TileID.Sets.Conversion.Sand[current.type]) {
                                tileConvertBuffer = TileID.Sand;
                            } else if(TileID.Sets.Conversion.Sandstone[current.type]) {
                                tileConvertBuffer = TileID.Sandstone;
                            } else if(TileID.Sets.Conversion.HardenedSand[current.type]) {
                                tileConvertBuffer = TileID.HardenedSand;
                            } else if(TileID.Sets.Conversion.Ice[current.type]) {
                                tileConvertBuffer = TileID.IceBlock;
                            }
                            if(tileConvertBuffer!=-1&&tileConvertBuffer!=current.type) {
                                Main.tile[k, l].type = (ushort)tileConvertBuffer;
                                WorldGen.SquareTileFrame(k, l);
                                NetMessage.SendTileSquare(-1, k, l, 1);
                                tileConvertBuffer = -1;
                            }
                        }
                    }
                }
                orig(i, j, conversionType, size);
                break;
            }
        }
        public static bool ConvertWall(ref ushort tileType, byte evilType, bool convert = true) {
            getEvilWallConversionTypes(evilType, out ushort[] stoneTypes, out ushort[] hardenedSandTypes, out ushort[] sandstoneTypes);
            switch(tileType) {
                case WallID.Stone:
                if(convert)tileType = WorldGen.genRand.Next(stoneTypes);
                return true;
                case WallID.Sandstone:
                if(convert)tileType = WorldGen.genRand.Next(sandstoneTypes);
                return true;
                case WallID.HardenedSand:
                if(convert)tileType = WorldGen.genRand.Next(hardenedSandTypes);
                return true;
            }
            return false;
        }

        public static bool ConvertTileWeak(ref ushort tileType, byte evilType, bool convert = true) {
            getEvilTileConversionTypes(evilType, out ushort stoneType, out ushort grassType, out ushort plantType, out ushort sandType, out ushort sandstoneType, out ushort hardenedSandType, out ushort iceType);
            switch(tileType) {
                case TileID.Grass:
                if(convert)tileType = grassType;
                return true;
                case TileID.Stone:
                if(convert)tileType = stoneType;
                return true;
                case TileID.Sand:
                if(convert)tileType = sandType;
                return true;
                case TileID.Sandstone:
                if(convert)tileType = sandstoneType;
                return true;
                case TileID.HardenedSand:
                if(convert)tileType = hardenedSandType;
                return true;
                case TileID.IceBlock:
                if(convert)tileType = iceType;
                return true;
            }
            if(Main.tileMoss[tileType]) {
                if(convert)tileType = stoneType;
                return true;
            }
            return false;
        }
        public static bool ConvertTile(ref ushort tileType, byte evilType, bool aggressive = false) {
            getEvilTileConversionTypes(evilType, out ushort stoneType, out ushort grassType, out ushort plantType, out ushort sandType, out ushort sandstoneType, out ushort hardenedSandType, out ushort iceType);
            if(TileID.Sets.Conversion.Grass[tileType]) {
                tileType = grassType;
                return true;
            }else if(TileID.Sets.Conversion.Stone[tileType]) {
                tileType = stoneType;
                return true;
            }else if(TileID.Sets.Conversion.Sandstone[tileType]) {
                tileType = sandstoneType;
                return true;
            }else if(TileID.Sets.Conversion.HardenedSand[tileType]) {
                tileType = hardenedSandType;
                return true;
            }else if(TileID.Sets.Conversion.Ice[tileType]) {
                tileType = iceType;
                return true;
            }else if(TileID.Sets.Conversion.Sand[tileType]||(aggressive&&TileID.Sets.Falling[tileType])) {
                tileType = sandType;
                return true;
            }
            return false;
        }
    }
}
