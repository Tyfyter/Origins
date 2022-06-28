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
using Microsoft.Xna.Framework;
using Terraria.Utilities;
using Origins.Items.Weapons.Other;
using Origins.Walls;
using static Origins.OriginSystem.LootQueueAction;
using Origins.Tiles.Riven;
using Origins.Tiles.Brine;
using Origins.World;

namespace Origins {
    public partial class OriginSystem : ModSystem {
		public static int voidTiles;
		public static int defiledTiles;
        public static int rivenTiles;
        public static int brineTiles;
        public int peatSold;
        public const float biomeShaderSmoothing = 0.025f;
        public byte worldEvil = 0;
        private static double? _worldSurfaceLow;
        public static double worldSurfaceLow => _worldSurfaceLow??Main.worldSurface-165;
        public static double WorldEvil => instance.worldEvil;
        public bool defiledResurgence => Main.hardMode;//true;
        public const byte evil_corruption = 0b0001;//1
        public const byte evil_crimson = 0b0010;//2
        //difference of 4 (2^2)
        public const byte evil_wastelands = 0b0101;//5
        public const byte evil_riven = 0b0110;//6

		public static int totalDefiled;
		public static int totalDefiled2;
        public static byte tDefiled;
        public static int totalRiven;
        public static int totalRiven2;
        public static byte tRiven;
        public List<Point> Defiled_Hearts { get; set; } = new List<Point>();

        public override void LoadWorldData(TagCompound tag) {
            if (tag.ContainsKey("peatSold")) {
                peatSold = tag.GetAsInt("peatSold");
            }
            if (tag.ContainsKey("worldEvil")) {
                worldEvil = tag.GetByte("worldEvil");
            }
			if (worldEvil == 0) {
                worldEvil = WorldGen.crimson ? evil_crimson : evil_corruption;
            }
            if(tag.ContainsKey("worldSurfaceLow"))_worldSurfaceLow = tag.GetDouble("worldSurfaceLow");
            if(tag.ContainsKey("defiledHearts"))Defiled_Hearts = tag.Get<List<Vector2>>("defiledHearts").Select(Utils.ToPoint).ToList();

            defiledResurgenceTiles = new List<(int, int)>(){};
            defiledAltResurgenceTiles = new List<(int, int, ushort)>(){};
        }
        public override void SaveWorldData(TagCompound tag)/* Edit tag parameter rather than returning new TagCompound */ {
            tag.Add("peatSold", peatSold);
            tag.Add("worldEvil", worldEvil);
            tag.Add( "defiledHearts", Defiled_Hearts.Select(Utils.ToVector2).ToList());
            if(_worldSurfaceLow.HasValue) {
                tag.Add("worldSurfaceLow", _worldSurfaceLow);
            }
        }
        public override void ResetNearbyTileEffects() {
			voidTiles = 0;
            defiledTiles = 0;
            rivenTiles = 0;
            brineTiles = 0;
        }

		public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) {
			voidTiles = tileCounts[ModContent.TileType<Dusk_Stone>()];

			defiledTiles = tileCounts[ModContent.TileType<Defiled_Stone>()]+tileCounts[ModContent.TileType<Defiled_Grass>()]+tileCounts[ModContent.TileType<Defiled_Sand>()]+tileCounts[ModContent.TileType<Defiled_Ice>()];

            rivenTiles = tileCounts[ModContent.TileType<Riven_Flesh>()];//+tileCounts[ModContent.TileType<Riven_Grass>()]+tileCounts[ModContent.TileType<Riven_Sand>()]+tileCounts[ModContent.TileType<Riven_Ice>()];

            brineTiles = tileCounts[ModContent.TileType<Sulphur_Stone>()];
        }
        protected internal List<(int x, int y)> defiledResurgenceTiles;
        protected internal List<(int x, int y, ushort)> defiledAltResurgenceTiles;
        protected internal Queue<(int x, int y)> queuedKillTiles;
        protected internal HashSet<Point> anoxicAirTiles;
		public override void PostUpdateWorld() {
            if(defiledResurgence) {
                if(defiledResurgenceTiles.Count>0&&WorldGen.genRand.NextBool(5)) {
                    int index = WorldGen.genRand.Next(defiledResurgenceTiles.Count);
                    (int k, int l) pos = defiledResurgenceTiles[index];
                    ConvertTile(ref Main.tile[pos.k, pos.l].TileType, evil_wastelands);
                    WorldGen.SquareTileFrame(pos.k, pos.l);
                    NetMessage.SendTileSquare(-1, pos.k, pos.l, 1);
                    defiledResurgenceTiles.RemoveAt(index);
                }else if(defiledAltResurgenceTiles.Count>0&&WorldGen.genRand.NextBool(30)) {
                    int index = WorldGen.genRand.Next(defiledAltResurgenceTiles.Count);
                    (int k, int l, ushort type) tile = defiledAltResurgenceTiles[index];
                    if(Main.tile[tile.k, tile.l].HasTile) {
                        Main.tile[tile.k, tile.l].TileType = tile.type;
                        WorldGen.SquareTileFrame(tile.k, tile.l);
                        NetMessage.SendTileSquare(-1, tile.k, tile.l, 1);
                    }
                    defiledAltResurgenceTiles.RemoveAt(index);
                }
            }
            int q = 100;
            while (!WorldGen.gen && (queuedKillTiles?.Count??0) > 0) {
                var (i, j) = queuedKillTiles.Dequeue();
                WorldGen.KillTile(i, j, true);
                if (q--<0) {
                    break;
                }
            }
        }
        public void QueueKillTile(int i, int j) {
            if (queuedKillTiles is null) {
                queuedKillTiles = new Queue<(int, int)>();
            }
            queuedKillTiles.Enqueue((i, j));
        }
        public override void PostWorldGen() {
            ChestLootCache[] chestLoots = OriginExtensions.BuildArray<ChestLootCache>(56,0,2,4,11,12,13,15,16,17,50,51);
            Chest chest;
            int lootType;
            ChestLootCache cache;
            bool noLoot = true;
            for(int i = 0; i < Main.chest.Length; i++) {
                chest = Main.chest[i];
				if (chest != null && Main.tile[chest.x, chest.y].TileType == TileID.Containers){
                    cache = chestLoots[Main.tile[chest.x, chest.y].TileFrameX/36];
                    if(cache is null)continue;
                    lootType = chest.item[0].type;
                    cache.AddLoot(lootType, i);
                    noLoot = false;
				}
            }
            if(noLoot)return;
            ApplyLootQueue(chestLoots,
                (CHANGE_QUEUE, ChestID.LockedShadow),
                (ENQUEUE, ModContent.ItemType<Boiler_Pistol>()),
                (ENQUEUE, ModContent.ItemType<Firespit>()),
                (CHANGE_QUEUE, ChestID.Ice),
                (ENQUEUE, ModContent.ItemType<Cryostrike>())
            );
            _worldSurfaceLow = WorldGen.worldSurfaceLow;
        }
        public static void ApplyLootQueue(ChestLootCache[] lootCache, params (LootQueueAction action, int param)[] actions) {
            int lootType;
            ChestLootCache cache = null;
            Chest chest;
            int chestIndex = -1;
            Queue<int> items = new Queue<int>();
            WeightedRandom<int> random;
            int newLootType;
            if(actions[0].action==CHANGE_QUEUE) {
                cache = lootCache[actions[0].param];
            } else {
                throw new ArgumentException("the first action in ApplyLootQueue must be CHANGE_QUEUE", nameof(actions));
            }
            int actionIndex = 1;
            cont:
            if(actionIndex<actions.Length&&actions[actionIndex].action==ENQUEUE) {
                items.Enqueue(actions[actionIndex].param);
                Origins.instance.Logger.Info("adding item "+actions[actionIndex].param+" to world");
                actionIndex++;
                goto cont;
            }
            while(items.Count>0) {
                random = cache.GetWeightedRandom();
                lootType = random.Get();
                chestIndex = WorldGen.genRand.Next(cache[lootType]);
                chest = Main.chest[chestIndex];
                newLootType = items.Dequeue();
                chest.item[0].SetDefaults(newLootType);
                chest.item[0].Prefix(-2);
                cache[lootType].Remove(chestIndex);
            }
            if(actionIndex<actions.Length&&actions[actionIndex].action==CHANGE_QUEUE) {
                cache = lootCache[actions[actionIndex].param];
                actionIndex++;
                goto cont;
            }
        }
        public enum LootQueueAction {
            ENQUEUE,
            CHANGE_QUEUE
        }
        public class ChestLootCache {
            Dictionary<int, List<int>> ChestLoots = new Dictionary<int, List<int>>();
            public List<int> this[int lootType] {
                get {
                    if(ChestLoots.ContainsKey(lootType)) {
                        return ChestLoots[lootType];
                    } else {
                        return null;
                    }
                }
            }
            public void AddLoot(int lootType, int chestIndex) {
                if(ChestLoots.ContainsKey(lootType)) {
                    ChestLoots[lootType].Add(chestIndex);
                } else {
                    ChestLoots.Add(lootType, new List<int>{chestIndex});
                }
            }
            public int CountLoot(int lootType) {
                if(ChestLoots.ContainsKey(lootType)) {
                    return ChestLoots[lootType].Count;
                } else {
                    return 0;
                }
            }
            public WeightedRandom<int> GetWeightedRandom(bool cullUnique = true, UnifiedRandom random = null) {
                bool cull = false;
                WeightedRandom<int> rand = new WeightedRandom<int>(random??WorldGen.genRand);
                foreach(KeyValuePair<int,List<int>> kvp in ChestLoots) {
                    if(kvp.Value.Count>1) {
                        cull = cullUnique;
                    }
                    rand.Add(kvp.Key, kvp.Value.Count);
                }
                if(cull)rand.elements.RemoveAll((e)=>e.Item2<=1);
                return rand;
            }
        }
        public static byte GetAdjTileCount(int i, int j) {
            byte count = 0;
            if(Main.tile[i-1,j-1].HasTile) count++;
            if(Main.tile[i,j-1].HasTile)   count++;
            if(Main.tile[i+1,j-1].HasTile) count++;
            if(Main.tile[i-1,j].HasTile)   count++;
            if(Main.tile[i+1,j].HasTile)   count++;
            if(Main.tile[i-1,j+1].HasTile) count++;
            if(Main.tile[i,j+1].HasTile)   count++;
            if(Main.tile[i+1,j+1].HasTile) count++;
            return count;
        }
        /// <summary>
        /// the first clentaminator conversion type from Origins (from ASCII 'Org')
        /// </summary>
        public const int origin_conversion_type =  79114103;
        public static void ConvertHook(On.Terraria.WorldGen.orig_Convert orig, int i, int j, int conversionType, int size = 4) {
            Tile current;
            int tileConvertBuffer = -1;
            OriginSystem originWorld = ModContent.GetInstance<OriginSystem>();
            switch(conversionType) {
                case origin_conversion_type:
                for(int k = i - size; k <= i + size; k++) {
                    for(int l = j - size; l <= j + size; l++) {
                        if(!WorldGen.InWorld(k, l, 1) || Math.Abs(k - i) + Math.Abs(l - j) >= 6) {
                            continue;
                        }
                        current = Main.tile[k,l];
                        ConvertTile(ref current.TileType, evil_wastelands);
                        ConvertWall(ref current.WallType, evil_wastelands);
                    }
                }
                break;
                default:
                for(int k = i - size; k <= i + size; k++) {
                    for(int l = j - size; l <= j + size; l++) {
                        tileConvertBuffer = -1;
                        current = Main.tile[k,l];
                        if(originWorld.defiledResurgence&&ModContent.GetModTile(current.TileType) is DefiledTile) {
                            originWorld.defiledResurgenceTiles.Add((k,l));
                        }
                        if(conversionType == 0) {
                            if(!WorldGen.InWorld(k, l, 1) || Math.Abs(k - i) + Math.Abs(l - j) >= 6) {
                                continue;
                            }
                            //convert based on conversion sets
                            if(TileID.Sets.Conversion.Grass[current.TileType]) {
                                tileConvertBuffer = TileID.Grass;
                            } else if(TileID.Sets.Conversion.Stone[current.TileType]) {
                                tileConvertBuffer = TileID.Stone;
                            } else if(TileID.Sets.Conversion.Sand[current.TileType]) {
                                tileConvertBuffer = TileID.Sand;
                            } else if(TileID.Sets.Conversion.Sandstone[current.TileType]) {
                                tileConvertBuffer = TileID.Sandstone;
                            } else if(TileID.Sets.Conversion.HardenedSand[current.TileType]) {
                                tileConvertBuffer = TileID.HardenedSand;
                            } else if(TileID.Sets.Conversion.Ice[current.TileType]) {
                                tileConvertBuffer = TileID.IceBlock;
                            }
                            if(tileConvertBuffer!=-1&&tileConvertBuffer!=current.TileType) {
                                if(WallID.Sets.Conversion.Stone[Main.tile[k, l].WallType]) {
                                    Main.tile[k, l].WallType = WallID.Stone;
                                }else if(WallID.Sets.Conversion.Sandstone[Main.tile[k, l].WallType]) {
                                    Main.tile[k, l].WallType = WallID.Sandstone;
                                }else if(WallID.Sets.Conversion.HardenedSand[Main.tile[k, l].WallType]) {
                                    Main.tile[k, l].WallType = WallID.HardenedSand;
                                }
                                Main.tile[k, l].TileType = (ushort)tileConvertBuffer;
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
        internal static void UpdateTotalEvilTiles() {
	        totalDefiled = totalDefiled2;
	        tDefiled = (byte)Math.Round((totalDefiled / (float)WorldGen.totalSolid2) * 100f);
            totalRiven = totalRiven2;
            tRiven = (byte)Math.Round((totalRiven / (float)WorldGen.totalSolid2) * 100f);
            if (tDefiled == 0 && totalDefiled > 0){
		        tDefiled = 1;
            }
            if (tRiven == 0 && totalRiven > 0) {
                tRiven = 1;
            }
            if (Main.netMode == NetmodeID.Server){
		        ModPacket packet = Origins.instance.GetPacket(2);
                packet.Write(MessageID.TileCounts);
                packet.Write(tDefiled);
                packet.Write(tRiven);
            }
	        totalDefiled2 = 0;
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
        public static byte GetTileAdj(int i, int j) {
            byte adj = 0;
            if(Main.tile[i-1, j-1].HasTile)adj|=AdjID.tl;
            if(Main.tile[i, j-1].HasTile)  adj|=AdjID.t;
            if(Main.tile[i+1, j-1].HasTile)adj|=AdjID.tr;
            if(Main.tile[i-1, j].HasTile)  adj|=AdjID.l;
            if(Main.tile[i+1, j].HasTile)  adj|=AdjID.r;
            if(Main.tile[i-1, j+1].HasTile)adj|=AdjID.bl;
            if(Main.tile[i, j+1].HasTile)  adj|=AdjID.b;
            if(Main.tile[i+1, j+1].HasTile)adj|=AdjID.br;
            return adj;
        }
        public static List<Vector2> GetTileDirs(int i, int j) {
            List<Vector2> dirs = new List<Vector2>(8);
            for(int k = 1; k <= 1; k++) {
                for(int l = 1; l <= 1; l++) {
                    if(!(k==0&&l==0)&&Main.tile[i+k,j+l].HasTile) {
                        dirs.Add(new Vector2(k,l));
                    }
                }
            }
            return dirs;
        }
    }
}
