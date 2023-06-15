using Microsoft.Xna.Framework;
using Origins.Items.Accessories;
using Origins.Items.Weapons;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.Questing;
using Origins.Tiles;
using Origins.Tiles.Brine;
using Origins.Tiles.Defiled;
using Origins.Tiles.Dusk;
using Origins.Tiles.Riven;
using Origins.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Tyfyter.Utils;
using static Tyfyter.Utils.ChestLootCache;
using static Tyfyter.Utils.ChestLootCache.LootQueueAction;
using static Tyfyter.Utils.ChestLootCache.LootQueueMode;

namespace Origins {
	public partial class OriginSystem : ModSystem {
		public static int voidTiles;
		public static int defiledTiles;
		public static int rivenTiles;
		public static int brineTiles;
		public static int fiberglassTiles;
		public int peatSold;
		public const float biomeShaderSmoothing = 0.025f;
		public byte worldEvil {
			get {
				switch (AltLibrary.Common.Systems.WorldBiomeManager.WorldEvil) {
					case "Origins/Defiled_Wastelands_Alt_Biome":
					hasDefiled = true;
					return evil_wastelands;

					case "Origins/Riven_Hive_Alt_Biome":
					hasRiven = true;
					return evil_riven;

					default:
					return WorldGen.crimson ? evil_crimson : evil_corruption;
				}
			}
		}
		internal bool hasDefiled = false;
		public bool HasDefiledWastelands => Instance.hasDefiled;
		internal bool hasRiven = false;
		public bool HasRivenHive => Instance.hasRiven;
		private static double? _worldSurfaceLow;
		public static double worldSurfaceLow => _worldSurfaceLow ?? Main.worldSurface - 165;
		public static byte WorldEvil => Instance.worldEvil;
		public static bool DefiledResurgenceActive => Main.hardMode && !NPC.downedPlantBoss;//true;
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
		bool fiberglassNeedsFraming;
		Point fiberglassMin;
		Point fiberglassMax;
		HashSet<Point> fiberglassFrameSet;
		Task fiberglassFrameTask;
		Point brineCenter;
		public bool forceThunderstorm = false;
		public int forceThunderstormDelay = 0;

		public bool taxCollectorWearsPartyhat;
		public static int MimicSetLevel {
			get {
				float currentPercent = totalDefiled / (float)WorldGen.totalSolid;
				int currentLevel = 0;
				if (currentPercent >= 0) currentLevel++; //8%
				if (currentPercent >= 0) currentLevel++; //36%
				if (currentPercent >= 0) currentLevel++; //81%
				return currentLevel;
			}
		}
		public List<Point> Defiled_Hearts { get; set; } = new List<Point>();
		private List<Point> _abandonedBombs;
		public List<Point> AbandonedBombs => _abandonedBombs ??= new List<Point>();
		public override void OnWorldUnload() {
			forceThunderstorm = false;
		}
		public override void LoadWorldData(TagCompound tag) {
			Mod.Logger.Info("LoadWorldData called on netmode " + Main.netMode);
			if (tag.ContainsKey("peatSold")) {
				peatSold = tag.GetAsInt("peatSold");
			}
			if (tag.ContainsKey("worldSurfaceLow")) _worldSurfaceLow = tag.GetDouble("worldSurfaceLow");
			if (tag.ContainsKey("defiledHearts")) Defiled_Hearts = tag.Get<List<Vector2>>("defiledHearts").Select(Utils.ToPoint).ToList();
			tag.TryGet("hasDefiled", out hasDefiled);
			tag.TryGet("hasRiven", out hasRiven);
			tag.TryGet("forceThunderstorm", out forceThunderstorm);

			defiledResurgenceTiles = new List<(int, int)>() { };
			defiledAltResurgenceTiles = new List<(int, int, ushort)>() { };
			questsTag = tag.SafeGet<TagCompound>("Quests");
		}
		internal TagCompound questsTag;
		public override void SaveWorldData(TagCompound tag) {
			tag.Add("peatSold", peatSold);
			tag.Add("worldEvil", worldEvil);
			tag.Add("hasDefiled", hasDefiled);
			tag.Add("hasRiven", hasRiven);
			tag.Add("forceThunderstorm", forceThunderstorm);
			tag.Add("defiledHearts", Defiled_Hearts.Select(Utils.ToVector2).ToList());
			if (_worldSurfaceLow.HasValue) {
				tag.Add("worldSurfaceLow", _worldSurfaceLow);
			}
			TagCompound questsTag = new TagCompound();
			foreach (var quest in Quest_Registry.Quests) {
				if (quest.SaveToWorld) {
					TagCompound questTag = new TagCompound();
					quest.SaveData(questTag);
					if (questTag.Count > 0) questsTag.Add(quest.FullName, questTag);
				}
			}
			if (questsTag.Count > 0) {
				tag.Add("Quests", questsTag);
			}
		}
		public override void ResetNearbyTileEffects() {
			voidTiles = 0;
			defiledTiles = 0;
			rivenTiles = 0;
			brineTiles = 0;
			fiberglassTiles = 0;
		}

		public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) {
			voidTiles = tileCounts[ModContent.TileType<Dusk_Stone>()];

			defiledTiles = tileCounts[ModContent.TileType<Defiled_Stone>()]
				+ tileCounts[ModContent.TileType<Defiled_Grass>()]
				+ tileCounts[ModContent.TileType<Defiled_Sand>()]
				+ tileCounts[ModContent.TileType<Defiled_Ice>()];

			rivenTiles = tileCounts[ModContent.TileType<Riven_Flesh>()];//+tileCounts[ModContent.TileType<Riven_Grass>()]+tileCounts[ModContent.TileType<Riven_Sand>()]+tileCounts[ModContent.TileType<Riven_Ice>()];

			brineTiles = tileCounts[ModContent.TileType<Sulphur_Stone>()];

			fiberglassTiles = tileCounts[ModContent.TileType<Tiles.Other.Fiberglass_Tile>()];
		}
		protected internal List<(int x, int y)> defiledResurgenceTiles;
		protected internal List<(int x, int y, ushort)> defiledAltResurgenceTiles;
		protected internal Queue<(int x, int y)> queuedKillTiles;
		protected internal HashSet<Point> anoxicAirTiles;
		public override void PostUpdateWorld() {
			if (DefiledResurgenceActive) {
				if (defiledResurgenceTiles.Count > 0 && WorldGen.genRand.NextBool(5)) {
					int index = WorldGen.genRand.Next(defiledResurgenceTiles.Count);
					(int k, int l) pos = defiledResurgenceTiles[index];
					ConvertTile(ref Main.tile[pos.k, pos.l].TileType, evil_wastelands);
					WorldGen.SquareTileFrame(pos.k, pos.l);
					NetMessage.SendTileSquare(-1, pos.k, pos.l, 1);
					defiledResurgenceTiles.RemoveAt(index);
				} else if (defiledAltResurgenceTiles.Count > 0 && WorldGen.genRand.NextBool(30)) {
					int index = WorldGen.genRand.Next(defiledAltResurgenceTiles.Count);
					(int k, int l, ushort type) tile = defiledAltResurgenceTiles[index];
					if (Main.tile[tile.k, tile.l].HasTile) {
						Main.tile[tile.k, tile.l].TileType = tile.type;
						WorldGen.SquareTileFrame(tile.k, tile.l);
						NetMessage.SendTileSquare(-1, tile.k, tile.l, 1);
					}
					defiledAltResurgenceTiles.RemoveAt(index);
				}
			}
			int q = 100;
			while (!WorldGen.gen && (queuedKillTiles?.Count ?? 0) > 0) {
				var (i, j) = queuedKillTiles.Dequeue();
				WorldGen.KillTile(i, j, true);
				if (q-- < 0) {
					break;
				}
			}
			if (fiberglassNeedsFraming) {
				if (fiberglassFrameTask?.IsCompleted ?? true)
					fiberglassFrameTask = Task.Run(FrameFiberglass);
			}
		}
		public void FrameFiberglass() {
			return;
			fiberglassNeedsFraming = false;
			Point fiberglassMin = this.fiberglassMin;
			Point fiberglassMax = this.fiberglassMax;
			HashSet<Point> fiberglassFrameSet;
			lock (this.fiberglassFrameSet) {
				fiberglassFrameSet = this.fiberglassFrameSet.ToHashSet();
			}
			const ushort none = 0x8001;//0b1000000000000001
			var cellTypes = new Tuple<WaveFunctionCollapse.Generator<(short X, short Y)>.Cell, double>[] {
#region first 3 columns
                    new(new(new(0, 0), none, 1 << 4, none, 3 << 4), 1),
					new(new(new(1, 0), none, 3 << 4, none, 1 << 5), 1),
					new(new(new(2, 0), none, 1 << 4, none, 1 << 5), 1),

					new(new(new(0, 1), none, 1 << 4, 1 << 5, none), 1),
					new(new(new(1, 1), none, 3 << 4, 1 << 4, none), 1),
					new(new(new(2, 1), none, 1 << 4, 1 << 5, none), 1),

					new(new(new(0, 2), 1 << 5, 1 << 5, none, 1 << 5), 1),
					new(new(new(1, 2), 1 << 5, 1 << 5, none, 1 << 5), 1),
					new(new(new(2, 2), 1 << 5, 1 << 4, none, 1 << 5), 1),

					new(new(new(0, 3), 1 << 4, none, none, none), 0.5f),
					new(new(new(1, 3), 1 << 5, none, none, none), 0.5f),
					new(new(new(2, 3), 1 << 5, none, none, none), 0.5f),

					new(new(new(0, 4), 3 << 4, 1 << 5 , 1 << 4, 1 << 5), 1),
					new(new(new(1, 4), 1 << 4, 1 << 4, 1 << 4, 1 << 5), 1),
					new(new(new(2, 4), 1 << 5, 1 << 4, 1 << 4, none), 1),
#endregion first 3 columns
#region second 3 columns
                    new(new(new(4, 0), 1 << 4, none, 1 << 4, none), 1),
					new(new(new(5, 0), 1 << 5, none, 1 << 5, none), 1),
					new(new(new(6, 0), 1 << 4, none, 1 << 4, none), 1),

					new(new(new(4, 1), 1 << 3, none, none, 1 << 5), 1),
					new(new(new(5, 1), 1 << 4, none, none, 1 << 4), 1),
					new(new(new(6, 1), 1 << 4, none, none, 1 << 4), 1),

					new(new(new(4, 2), none, 1 << 5, 1 << 5, 1 << 5), 1),
					new(new(new(5, 2), none, 1 << 4, 1 << 5, 1 << 4), 1),
					new(new(new(6, 2), none, 1 << 5, 1 << 5, 1 << 4), 1),

					new(new(new(4, 3), none, none, none, 1 << 5), 0.5f),
					new(new(new(5, 3), none, none, none, 1 << 4), 0.5f),
					new(new(new(6, 3), none, none, none, 1 << 5), 0.5f),

					new(new(new(4, 4), none, none, none, none), 0.15f),
					new(new(new(5, 4), none, none, none, none), 0.15f),
					new(new(new(6, 4), none, none, none, none), 0.15f),
#endregion second 3 columns
#region last 3 columns
                    new(new(new(8, 0),  none, none, 1 << 3, 1 << 4), 1),
					new(new(new(9, 0),  none, none, 1 << 4, 1 << 5), 1),
					new(new(new(10, 0), none, none, 1 << 4, 1 << 5), 1),

					new(new(new(8, 1),  3 << 4, 1 << 5, none, none), 1),
					new(new(new(9, 1),  none, 1 << 5, none, none), 1),
					new(new(new(10, 1), 1 << 4, 1 << 5, none, none), 1),

					new(new(new(8, 2),  none, none, 1 << 4, none), 0.5f),
					new(new(new(9, 2),  none, none, 1 << 5, none), 0.5f),
					new(new(new(10, 2), none, none, 1 << 4, none), 0.5f),

					new(new(new(8, 3),  none, 1 << 4, none, none), 0.5f),
					new(new(new(9, 3),  none, 1 << 5, none, none), 0.5f),
					new(new(new(10, 3), none, 1 << 4, none, none), 0.5f),
#endregion last 3 columns
                    new(new(new(3, 4), none, none, none, none), 0.15f)// empty spot
                };
			fiberglassMin -= new Point(1, 1);
			fiberglassMax += new Point(1, 1);
			var actuals = new WaveFunctionCollapse.Generator<(short X, short Y)>.Cell?[fiberglassMax.X - fiberglassMin.X + 1, fiberglassMax.Y - fiberglassMin.Y + 1];
			int fiberglassType = ModContent.TileType<Tiles.Other.Fiberglass_Tile>();
			for (int i = fiberglassMin.X; i <= fiberglassMax.X; i++) {
				for (int j = fiberglassMin.Y; j <= fiberglassMax.Y; j++) {
					if (!fiberglassFrameSet.Contains(new(i, j)) || i == fiberglassMin.X || i == fiberglassMax.X || j == fiberglassMin.Y || j == fiberglassMax.Y) {
						bool notFiberglass = !Framing.GetTileSafely(i, j).TileIsType(fiberglassType);
						var cell = notFiberglass ? new(new(0, 0), none, none, none, none) : cellTypes.Where(v => {
							var curr = Framing.GetTileSafely(i, j).Get<TileExtraVisualData>();
							return v.Item1.value.X == curr.TileFrameX && v.Item1.value.Y == curr.TileFrameY;
						}).First().Item1;
						actuals[i - fiberglassMin.X, j - fiberglassMin.Y] = cell;
					}
				}
			}
			fiberglassFrameSet.Clear();
			var generator = new WaveFunctionCollapse.Generator<(short X, short Y)>(actuals, matchAll: false, cellTypes: cellTypes);
			//fiberglassMin += new Point(1, 1);
			//fiberglassMax -= new Point(1, 1);
			while (!generator.GetCollapsed()) {
				//generator.Collapse();
				generator.CollapseWith((x, y, val) => {
					Framing.GetTileSafely(x + fiberglassMin.X, y + fiberglassMin.Y).Get<TileExtraVisualData>().TileFrameX = val.X;
					Framing.GetTileSafely(x + fiberglassMin.X, y + fiberglassMin.Y).Get<TileExtraVisualData>().TileFrameY = val.Y;
				});
			}
			/*for (int i = fiberglassMin.X; i <= fiberglassMax.X; i++) {
                for (int j = fiberglassMin.Y; j <= fiberglassMax.Y; j++) {
                    //if (Framing.GetTileSafely(i, j).TileIsType(fiberglassType)) {
                    (short X, short Y) = generator.GetActual(i - fiberglassMin.X, j - fiberglassMin.Y);
                    Framing.GetTileSafely(i, j).Get<TileExtraVisualData>().TileFrameX = X;
                    Framing.GetTileSafely(i, j).Get<TileExtraVisualData>().TileFrameY = Y;
                    //}
                }
            }*/
		}
		public void QueueKillTile(int i, int j) {
			if (queuedKillTiles is null) {
				queuedKillTiles = new Queue<(int, int)>();
			}
			queuedKillTiles.Enqueue((i, j));
		}
		public override void PostWorldGen() {
			ChestLootCache[] chestLoots = OriginExtensions.BuildArray<ChestLootCache>(56 + 17,
				ChestID.Normal,
				ChestID.Gold,
				ChestID.LockedGold,
				ChestID.LockedShadow,
				ChestID.Ice,
				ChestID.LivingWood,
				ChestID.Skyware,
				ChestID.Web,
				ChestID.Lihzahrd,
				ChestID.Water,
				ChestID.Granite,
				ChestID.Marble,
				ChestID.DeadMan,
				ChestID.Sandstone
			);
			Chest chest;
			int lootType;
			ChestLootCache cache;
			bool noLoot = true;
			for (int i = 0; i < Main.chest.Length; i++) {
				chest = Main.chest[i];
				if (chest != null) {
					int tileType = Main.tile[chest.x, chest.y].TileType;
					if (tileType == TileID.Containers) {
						cache = chestLoots[Main.tile[chest.x, chest.y].TileFrameX / 36];
						if (cache is null) continue;
						lootType = chest.item[0].type;
						cache.AddLoot(lootType, i);
						noLoot = false;
					}else if (tileType == TileID.Containers2) {
						cache = chestLoots[Main.tile[chest.x, chest.y].TileFrameX / 36 + 56];
						if (cache is null) continue;
						lootType = chest.item[0].type;
						cache.AddLoot(lootType, i);
						noLoot = false;
					}
				}
			}
			if (noLoot) return;
			ApplyWeightedLootQueue(chestLoots,
				/*example:
				(SWITCH_MODE, MODE_ADD, 1f),
				(ENQUEUE, ItemID.Sashimi, 0.5f),
				(SWITCH_MODE, MODE_REPLACE, 1f),*/
				(CHANGE_QUEUE, ChestID.Normal, 0b0000),
				(ENQUEUE, ModContent.ItemType<Cyah_Nara>(), 1f),
				(SET_COUNT_RANGE, 50, 186),
				(ENQUEUE, ModContent.ItemType<Bang_Snap>(), 1f),
				(SET_COUNT_RANGE, 1, 1),
				(CHANGE_QUEUE, ChestID.LivingWood, 0b0000),
				(ENQUEUE, ModContent.ItemType<Woodsprite_Staff>(), 1f),

				(CHANGE_QUEUE, ChestID.LockedShadow, 0b0000),
				(ENQUEUE, ModContent.ItemType<Boiler>(), 0.5f),
				(ENQUEUE, ModContent.ItemType<Firespit>(), 0.5f),
				(ENQUEUE, ModContent.ItemType<Dragons_Breath>(), 0.5f),
				(ENQUEUE, ModContent.ItemType<Hand_Grenade_Launcher>(), 0.5f),

				(CHANGE_QUEUE, ChestID.Ice, 0b0000),
				(ENQUEUE, ModContent.ItemType<Cryostrike>(), 1f),

				(CHANGE_QUEUE, ChestID.Gold, 0b0101),
				(ENQUEUE, ModContent.ItemType<Bomb_Charm>(), 1f),
				(ENQUEUE, ModContent.ItemType<Beginners_Tome>(), 1f),
				(ENQUEUE, ModContent.ItemType<Rope_Of_Sharing>(), 1f),

				(CHANGE_QUEUE, ChestID.Gold, 0b1101),
				(ENQUEUE, ModContent.ItemType<Nitro_Crate>(), 1f),
				(ENQUEUE, ModContent.ItemType<Bomb_Charm>(), 1f),
				(ENQUEUE, ModContent.ItemType<Beginners_Tome>(), 1f),
				(ENQUEUE, ModContent.ItemType<Rope_Of_Sharing>(), 1f),

				(CHANGE_QUEUE, ChestID.DeadMan, 0b0000),
				(ENQUEUE, ModContent.ItemType<Magic_Tripwire>(), 1f),
				(ENQUEUE, ModContent.ItemType<Trap_Charm>(), 1f),

				(CHANGE_QUEUE, ChestID.LockedGold, 0b0000),
				(ENQUEUE, ModContent.ItemType<Tones_Of_Agony>(), 1f),
				(ENQUEUE, ModContent.ItemType<Asylum_Whistle>(), 1f),
				(ENQUEUE, ModContent.ItemType<Bomb_Launcher>(), 1f),
				(ENQUEUE, ModContent.ItemType<Bomb_Yeeter>(), 1f));
			_worldSurfaceLow = WorldGen.worldSurfaceLow;
		}
		[Obsolete]
		public static void ApplyLootQueue(ChestLootCache[] lootCache, params (LootQueueAction action, int param)[] actions) {
			int lootType;
			ChestLootCache cache = null;
			Chest chest;
			int chestIndex = -1;
			Queue<int> items = new Queue<int>();
			WeightedRandom<int> random;
			int newLootType;
			int queueMode = MODE_REPLACE;
			switch (actions[0].action) {
				case CHANGE_QUEUE:
				cache = lootCache[actions[0].param];
				break;
				case SWITCH_MODE:
				queueMode = actions[0].param;
				break;
				case ENQUEUE:
				throw new ArgumentException("the first action in ApplyLootQueue must not be ENQUEUE", "actions");
			}
			int actionIndex = 1;
			cont:
			if (actionIndex < actions.Length && actions[actionIndex].action == ENQUEUE) {
				items.Enqueue(actions[actionIndex].param);
				Origins.instance.Logger.Info("adding item " + actions[actionIndex].param + " to world");
				actionIndex++;
				goto cont;
			}
			int i = actions.Length;
			if (cache is null) {
				return;
			}
			while (items.Count > 0) {
				random = cache.GetWeightedRandom();
				lootType = random.Get();
				chestIndex = WorldGen.genRand.Next(cache[lootType]);
				chest = Main.chest[chestIndex];
				newLootType = items.Dequeue();
				int targetIndex = 0;
				switch (queueMode) {
					case MODE_ADD:
					for (targetIndex = 0; targetIndex < Chest.maxItems; targetIndex++) if (chest.item[targetIndex].IsAir) break;
					break;
				}
				if (targetIndex >= Chest.maxItems) {
					if (--i > 0) items.Enqueue(newLootType);
				}
				chest.item[targetIndex].SetDefaults(newLootType);
				chest.item[targetIndex].Prefix(-2);
				cache[lootType].Remove(chestIndex);
			}
			if (actionIndex < actions.Length && actions[actionIndex].action == CHANGE_QUEUE) {
				cache = lootCache[actions[actionIndex].param];
				actionIndex++;
				goto cont;
			}
		}
		public static void ApplyWeightedLootQueue(ChestLootCache[] lootCaches, params (LootQueueAction action, int param, float weight)[] actions) {
			int lootType;
			ChestLootCache cache = null;
			Chest chest;
			int chestIndex = -1;
			Queue<(int, float, int)> items = new Queue<(int, float, int)>();
			WeightedRandom<int> random;
			(int param, float weight, int mode) newLootType;
			int queueMode = MODE_REPLACE;
			(int min, int max) countRange = (1, 1);

			int actionIndex = 0;
			void filterCache() {
				switch (actions[actionIndex].param) {
					case ChestID.Gold: {
						int typeVar = (int)actions[actionIndex].weight;
						//lower 2 bits:
						//00, 10: no exclusion for pyramid chests
						//01: exclude pyramid chests
						//11: exclude non-pyramid chests
						if ((typeVar & 0b0001) != 0) {
							if ((typeVar & 0b0010) == 0) {
								cache = new ChestLootCache(cache.Where((c) => {
									Chest selChest = Main.chest[c.Value[0]];
									return Main.tile[selChest.x, selChest.y + 2].TileType != TileID.SandstoneBrick;
								}));
							} else {
								cache = new ChestLootCache(cache.Where((c) => {
									Chest selChest = Main.chest[c.Value[0]];
									return Main.tile[selChest.x, selChest.y + 2].TileType == TileID.SandstoneBrick;
								}));
							}
						}
						//second 2 bits:
						//00, 10: no exclusion for height
						//01: only above caverns
						//11: only in caverns
						if ((typeVar & 4) != 0) {
							if ((typeVar & 8) == 0) {
								cache = new ChestLootCache(
									cache.Select(
										(c) => new KeyValuePair<int, List<int>>(
											c.Key,
											c.Value.Where(
												(c2) => {
													return Main.chest[c2].y < Main.rockLayer;
												}
											).ToList()
										)
									)
								);
							} else {
								cache = new ChestLootCache(
									cache.Select(
										(c) => new KeyValuePair<int, List<int>>(
											c.Key,
											c.Value.Where(
												(c2) => {
													return Main.chest[c2].y >= Main.rockLayer;
												}
											).ToList()
										)
									)
								);
							}
						}
					}
					break;
				}
				cache = new ChestLootCache(cache.Where((c) => c.Value.Count > 0));
				Origins.instance.Logger.Info($"using chest cache #{actions[actionIndex].param} {ChestID.Search.GetName(actions[actionIndex].param)} with mode {(int)actions[actionIndex].weight}: {string.Join(", ", cache.Select(v => $"{v.Value.Count} {new Item(v.Key).Name} (s)"))}");
			}
			switch (actions[actionIndex].action) {
				case CHANGE_QUEUE:
				cache = lootCaches[actions[actionIndex].param];
				filterCache();
				break;
				case SWITCH_MODE:
				queueMode = actions[actionIndex].param;
				break;
				case SET_COUNT_RANGE:
				countRange = (actions[actionIndex].param, (int)actions[actionIndex].weight);
				break;
				case ENQUEUE:
				throw new ArgumentException("the first action in ApplyLootQueue must not be ENQUEUE", nameof(actions));
			}
			actionIndex = 1;
			cont:
			if (actionIndex < actions.Length && actions[actionIndex].action == ENQUEUE) {
				items.Enqueue((actions[actionIndex].param, actions[actionIndex].weight, queueMode));
				actionIndex++;
				goto cont;
			}
			if (actionIndex < actions.Length && actions[actionIndex].action == SWITCH_MODE) {
				queueMode = actions[actionIndex].param;
				actionIndex++;
				goto cont;
			}
			int actionCount = actions.Length;
			int neutralWeight = cache.ChestLoots
				.GroupBy((n) => n.Value.Count)
				.OrderByDescending((g) => g.Count())
				.Select((g) => g.Key).FirstOrDefault();
			while (items.Count > 0) {
				random = cache.GetWeightedRandom();
				newLootType = items.Dequeue();
				int i = WorldGen.genRand.RandomRound(neutralWeight * newLootType.weight);
				switch (newLootType.mode) {
					case MODE_ADD:
					i = WorldGen.genRand.RandomRound(cache.ChestLoots.Count * newLootType.weight);
					break;
				}
				Origins.instance.Logger.Info($"adding {i} {Lang.GetItemNameValue(newLootType.param)}(s) to chests (weight {newLootType.weight}, mode {newLootType.mode})");
				for (; i-- > 0;) {
					lootType = random.Get();
					if (cache[lootType].Count == 0) {
						Origins.instance.Logger.Error($"broke on {Lang.GetItemNameValue(newLootType.param)} with {i} remaining, no further valid chests");
						break;
					}
					chestIndex = WorldGen.genRand.Next(cache[lootType]);
					chest = Main.chest[chestIndex];
					int targetIndex = 0;
					switch (newLootType.mode) {
						case MODE_ADD:
						for (targetIndex = 0; targetIndex < Chest.maxItems; targetIndex++) if (chest.item[targetIndex].IsAir) break;
						break;
					}
					if (targetIndex >= Chest.maxItems) {
						if (--actionCount > 0) items.Enqueue(newLootType);
					}
					chest.item[targetIndex].SetDefaults(newLootType.param);
					chest.item[targetIndex].stack = WorldGen.genRand.Next(countRange.min, countRange.max + 1);
					chest.item[targetIndex].Prefix(-1);
					cache[lootType].Remove(chestIndex);
					random = cache.GetWeightedRandom();
				}
			}
			if (actionIndex < actions.Length && actions[actionIndex].action == CHANGE_QUEUE) {
				cache = lootCaches[actions[actionIndex].param];
				filterCache();
				actionIndex++;
				goto cont;
			}
		}
		public static byte GetAdjTileCount(int i, int j) {
			byte count = 0;
			if (Main.tile[i - 1, j - 1].HasTile) count++;
			if (Main.tile[i, j - 1].HasTile) count++;
			if (Main.tile[i + 1, j - 1].HasTile) count++;
			if (Main.tile[i - 1, j].HasTile) count++;
			if (Main.tile[i + 1, j].HasTile) count++;
			if (Main.tile[i - 1, j + 1].HasTile) count++;
			if (Main.tile[i, j + 1].HasTile) count++;
			if (Main.tile[i + 1, j + 1].HasTile) count++;
			return count;
		}
		/*
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
                case origin_conversion_type + 1:
                for (int k = i - size; k <= i + size; k++) {
                    for (int l = j - size; l <= j + size; l++) {
                        if (!WorldGen.InWorld(k, l, 1) || Math.Abs(k - i) + Math.Abs(l - j) >= 6) {
                            continue;
                        }
                        current = Main.tile[k, l];
                        ConvertTile(ref current.TileType, evil_riven);
                        ConvertWall(ref current.WallType, evil_riven);
                    }
                }
                break;
                default:
                for(int k = i - size; k <= i + size; k++) {
                    for(int l = j - size; l <= j + size; l++) {
                        tileConvertBuffer = -1;
                        current = Main.tile[k,l];
                        if(DefiledResurgenceActive && ModContent.GetModTile(current.TileType) is DefiledTile) {
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
        //*/
		internal static void UpdateTotalEvilTiles() {
			totalDefiled = totalDefiled2;
			tDefiled = (byte)Math.Round((totalDefiled / (float)WorldGen.totalSolid2) * 100f);
			totalRiven = totalRiven2;
			tRiven = (byte)Math.Round((totalRiven / (float)WorldGen.totalSolid2) * 100f);
			if (tDefiled == 0 && totalDefiled > 0) {
				tDefiled = 1;
			}
			if (tRiven == 0 && totalRiven > 0) {
				tRiven = 1;
			}
			if (Main.netMode == NetmodeID.Server) {
				ModPacket packet = Origins.instance.GetPacket(2);
				packet.Write(MessageID.TileCounts);
				packet.Write(tDefiled);
				packet.Write(tRiven);
			}
			totalDefiled2 = 0;
		}
		public static bool ConvertWall(ref ushort tileType, byte evilType, bool convert = true) {
			getEvilWallConversionTypes(evilType, out ushort[] stoneTypes, out ushort[] hardenedSandTypes, out ushort[] sandstoneTypes);
			switch (tileType) {
				case WallID.Stone:
				if (convert) tileType = WorldGen.genRand.Next(stoneTypes);
				return true;
				case WallID.Sandstone:
				if (convert) tileType = WorldGen.genRand.Next(sandstoneTypes);
				return true;
				case WallID.HardenedSand:
				if (convert) tileType = WorldGen.genRand.Next(hardenedSandTypes);
				return true;
			}
			return false;
		}

		public static bool ConvertTileWeak(ref ushort tileType, byte evilType, bool convert = true) {
			getEvilTileConversionTypes(evilType, out ushort stoneType, out ushort grassType, out ushort plantType, out ushort sandType, out ushort sandstoneType, out ushort hardenedSandType, out ushort iceType);
			switch (tileType) {
				case TileID.Grass:
				if (convert) tileType = grassType;
				return true;
				case TileID.Stone:
				if (convert) tileType = stoneType;
				return true;
				case TileID.Sand:
				if (convert) tileType = sandType;
				return true;
				case TileID.Sandstone:
				if (convert) tileType = sandstoneType;
				return true;
				case TileID.HardenedSand:
				if (convert) tileType = hardenedSandType;
				return true;
				case TileID.IceBlock:
				if (convert) tileType = iceType;
				return true;
			}
			if (Main.tileMoss[tileType]) {
				if (convert) tileType = stoneType;
				return true;
			}
			return false;
		}
		public static bool ConvertTile(ref ushort tileType, byte evilType, bool aggressive = false) {
			getEvilTileConversionTypes(evilType, out ushort stoneType, out ushort grassType, out ushort plantType, out ushort sandType, out ushort sandstoneType, out ushort hardenedSandType, out ushort iceType);
			if (TileID.Sets.Conversion.Grass[tileType]) {
				tileType = grassType;
				return true;
			} else if (TileID.Sets.Conversion.Stone[tileType]) {
				tileType = stoneType;
				return true;
			} else if (TileID.Sets.Conversion.Sandstone[tileType]) {
				tileType = sandstoneType;
				return true;
			} else if (TileID.Sets.Conversion.HardenedSand[tileType]) {
				tileType = hardenedSandType;
				return true;
			} else if (TileID.Sets.Conversion.Ice[tileType]) {
				tileType = iceType;
				return true;
			} else if (TileID.Sets.Conversion.Sand[tileType] || (aggressive && TileID.Sets.Falling[tileType])) {
				tileType = sandType;
				return true;
			}
			return false;
		}
		public static bool HallowConvertTile(ref ushort tileType, bool aggressive = false) {
			if (TileID.Sets.Conversion.Grass[tileType]) {
				tileType = TileID.HallowedGrass;
				return true;
			} else if (TileID.Sets.Conversion.Stone[tileType]) {
				tileType = TileID.Pearlstone;
				return true;
			} else if (TileID.Sets.Conversion.Sandstone[tileType]) {
				tileType = TileID.HallowSandstone;
				return true;
			} else if (TileID.Sets.Conversion.HardenedSand[tileType]) {
				tileType = TileID.HallowHardenedSand;
				return true;
			} else if (TileID.Sets.Conversion.Ice[tileType]) {
				tileType = TileID.HallowedIce;
				return true;
			} else if (TileID.Sets.Conversion.Sand[tileType] || (aggressive && TileID.Sets.Falling[tileType])) {
				tileType = TileID.Pearlsand;
				return true;
			}
			return false;
		}
		public static byte GetTileAdj(int i, int j) {
			byte adj = 0;
			if (Main.tile[i - 1, j - 1].HasTile) adj |= AdjID.tl;
			if (Main.tile[i, j - 1].HasTile) adj |= AdjID.t;
			if (Main.tile[i + 1, j - 1].HasTile) adj |= AdjID.tr;
			if (Main.tile[i - 1, j].HasTile) adj |= AdjID.l;
			if (Main.tile[i + 1, j].HasTile) adj |= AdjID.r;
			if (Main.tile[i - 1, j + 1].HasTile) adj |= AdjID.bl;
			if (Main.tile[i, j + 1].HasTile) adj |= AdjID.b;
			if (Main.tile[i + 1, j + 1].HasTile) adj |= AdjID.br;
			return adj;
		}
		public static List<Vector2> GetTileDirs(int i, int j) {
			List<Vector2> dirs = new List<Vector2>(8);
			for (int k = 1; k <= 1; k++) {
				for (int l = 1; l <= 1; l++) {
					if (!(k == 0 && l == 0) && Main.tile[i + k, j + l].HasTile) {
						dirs.Add(new Vector2(k, l));
					}
				}
			}
			return dirs;
		}

		internal void AddFiberglassFrameTile(int i, int j) {
			if (i < fiberglassMin.X || !fiberglassNeedsFraming) fiberglassMin.X = i;
			if (j < fiberglassMin.Y || !fiberglassNeedsFraming) fiberglassMin.Y = j;
			if (i > fiberglassMax.X || !fiberglassNeedsFraming) fiberglassMax.X = i;
			if (j > fiberglassMax.Y || !fiberglassNeedsFraming) fiberglassMax.Y = j;
			if (fiberglassFrameSet is null) fiberglassFrameSet = new();
			fiberglassFrameSet.Add(new(i, j));
			fiberglassNeedsFraming = true;
		}
	}
}
