using System;
using System.Collections;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Utilities;
using static Tyfyter.Utils.ChestLootCache.LootQueueAction;
using static Tyfyter.Utils.ChestLootCache.LootQueueMode;

namespace Tyfyter.Utils {
	public class ChestLootCache : IEnumerable<KeyValuePair<int, List<int>>> {
		public static int[] vanillaGenChests => [0, 2, 4, 11, 12, 13, 15, 16, 17, 50, 51];
		public Dictionary<int, List<int>> ChestLoots = new();
		public ChestLootCache() {
			ChestLoots = new Dictionary<int, List<int>>();
		}
		public ChestLootCache(IEnumerable<KeyValuePair<int, List<int>>> enumerable) {
			ChestLoots = new Dictionary<int, List<int>>(enumerable);
		}
		public List<int> this[int lootType] {
			get {
				if (ChestLoots.ContainsKey(lootType)) {
					return ChestLoots[lootType];
				} else {
					return null;
				}
			}
		}
		public void AddLoot(int lootType, int chestIndex) {
			if (ChestLoots.ContainsKey(lootType)) {
				ChestLoots[lootType].Add(chestIndex);
			} else {
				ChestLoots.Add(lootType, new List<int> { chestIndex });
			}
		}
		public int CountLoot(int lootType) {
			if (ChestLoots.ContainsKey(lootType)) {
				return ChestLoots[lootType].Count;
			} else {
				return 0;
			}
		}
		public WeightedRandom<int> GetWeightedRandom(bool cullUnique = true, UnifiedRandom random = null) {
			bool cull = false;
			WeightedRandom<int> rand = new(random ?? WorldGen.genRand);
			foreach (KeyValuePair<int, List<int>> kvp in ChestLoots) {
				if (kvp.Value.Count > 1) {
					cull = cullUnique;
				}
				rand.Add(kvp.Key, kvp.Value.Count);
			}
			if (cull) rand.elements.RemoveAll((e) => e.Item2 <= 1);
			return rand;
		}
		public static ChestLootCache[] BuildCaches(int[] chestTypes = null) {
			if (chestTypes is null) {
				chestTypes = vanillaGenChests;
			}
			ChestLootCache[] chestLoots = Origins.OriginExtensions.BuildArray<ChestLootCache>(56, chestTypes);
			Chest chest;
			int lootType;
			ChestLootCache cache;
			for (int i = 0; i < Main.chest.Length; i++) {
				chest = Main.chest[i];
				if (chest != null && Main.tile[chest.x, chest.y].TileType == TileID.Containers) {
					cache = chestLoots[Main.tile[chest.x, chest.y].TileFrameX / 36];
					if (cache is null) continue;
					lootType = chest.item[0].type;
					cache.AddLoot(lootType, i);
				}
			}
			return chestLoots;
		}

		public IEnumerator<KeyValuePair<int, List<int>>> GetEnumerator() {
			return ChestLoots.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return ChestLoots.GetEnumerator();
		}
		public enum LootQueueAction {
			ENQUEUE,
			CHANGE_QUEUE,
			SWITCH_MODE,
			SET_COUNT_RANGE
		}
		public static class LootQueueMode {
			public const int MODE_REPLACE = 0;
			public const int MODE_ADD = 1;
		}
		[Obsolete]
		public static void ApplyLootQueue(ChestLootCache[] lootCaches, params (LootQueueAction action, int param)[] actions) {
			int lootType;
			ChestLootCache cache = null;
			Chest chest;
			int chestIndex = -1;
			Queue<int> items = new();
			WeightedRandom<int> random;
			int newLootType;
			int queueMode = MODE_REPLACE;
			switch (actions[0].action) {
				case CHANGE_QUEUE:
				cache = lootCaches[actions[0].param];
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
				cache = lootCaches[actions[actionIndex].param];
				actionIndex++;
				goto cont;
			}
		}
	}
}