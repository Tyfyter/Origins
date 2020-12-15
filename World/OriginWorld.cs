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
        public const byte evil_corruption = 0b0001;//1
        public const byte evil_crimson = 0b0010;//2
        //difference of 4
        public const byte evil_wastelands = 0b0101;//5
        public const byte evil_riven = 0b0110;//6


        public override void Load(TagCompound tag) {
            peatSold = tag.GetAsInt("peatSold");
            worldEvil = tag.GetByte("worldEvil");
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

        public static bool ConvertTile(ref ushort tileType, byte evilType, bool aggressive = false) {
            getEvilTileConversionTypes(evilType, out ushort stoneType, out ushort stoneWallType, out ushort grassType, out ushort plantType, out ushort sandType, out ushort sandstoneType, out ushort hardenedSandType, out ushort iceType);
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
