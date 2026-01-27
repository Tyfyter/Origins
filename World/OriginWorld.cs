using Origins.Core;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.Questing;
using Origins.Tiles.Ashen;
using Origins.Tiles.Brine;
using Origins.Tiles.Defiled;
using Origins.Tiles.Dusk;
using Origins.Tiles.Limestone;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Origins.Walls;
using Origins.World;
using Origins.World.BiomeData;
using PegasusLib;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using Tyfyter.Utils;
using static Tyfyter.Utils.ChestLootCache;
using static Tyfyter.Utils.ChestLootCache.LootQueueAction;
using static Tyfyter.Utils.ChestLootCache.LootQueueMode;

namespace Origins {
	public partial class OriginSystem : ModSystem {
		public static int voidTiles;
		public static int defiledTiles;
		public static int rivenTiles;
		public static int ashenTiles;
		public static int brineTiles;
		public static int fiberglassTiles;
		public static int limestoneTiles;
		public static int chambersiteTiles;
		public static int chambersiteWalls;
		public int peatSold;
		public const float biomeShaderSmoothing = 0.025f;
		internal bool hasDefiled = false;
		public static bool HasDefiledWastelands => Instance.hasDefiled;
		internal bool hasRiven = false;
		public static bool HasRivenHive => Instance.hasRiven;
		internal bool hasAshen = false;
		public static bool HasAshen => Instance.hasAshen;
		private static double? _worldSurfaceLow;
		public static double WorldSurfaceLow => _worldSurfaceLow ?? Main.worldSurface - 165;
		public Rectangle brinePoolRange;
		public static bool DefiledResurgenceActive => Main.hardMode && !NPC.downedPlantBoss;//true;
		public const byte evil_corruption = 0b0001;//1
		public const byte evil_crimson = 0b0010;//2
												//difference of 4 (2^2)
		public const byte evil_wastelands = 0b0101;//5
		public const byte evil_riven = 0b0110;//6
		public const byte evil_ashen = 0b0111;//7

		public static int totalDefiled;
		public static int totalDefiled2;
		public static byte tDefiled;
		public static int totalRiven;
		public static int totalRiven2;
		public static byte tRiven;
		public static int totalAshen;
		public static int totalAshen2;
		public static byte tAshen;
		bool fiberglassNeedsFraming;
		Point fiberglassMin;
		Point fiberglassMax;
		HashSet<Point> fiberglassFrameSet;
		//Task fiberglassFrameTask;
		public Point brineCenter;
		public bool forceThunderstorm = false;
		public int forceThunderstormDelay = 0;

		bool forceAF = false;
		public bool ForceAF {
			get => forceAF;
			set {
				if (value != forceAF) {
					forceAF = value;
					OriginsModIntegrations.HolidayForceChanged();
					GameCulture culture = LanguageManager.Instance.ActiveCulture;
					try {
						GameCulture french = GameCulture.FromCultureName(GameCulture.CultureName.French);
						LanguageManager.Instance.SetLanguage(culture == french ? GameCulture.FromCultureName(GameCulture.CultureName.Italian) : french);
					} finally {
						LanguageManager.Instance.SetLanguage(culture);
					}
					NetMessage.SendData(MessageID.WorldData);
				}
			}
		}

		public bool unlockedBrineNPC = false;
		public static int MimicSetLevel {
			get {
				float currentPercent = totalDefiled / (float)WorldGen.totalSolid;
				int currentLevel = 0;
				if (currentPercent >= 0) currentLevel++; //0% //8%
				if (currentPercent >= 0) currentLevel++; //0% //36%
				if (currentPercent >= 0) currentLevel++; //0% //81%
				return currentLevel;
			}
		}
		[Obsolete("Use TESystem.GetLocations<Defiled_Heart_TE_System>() instead")]
		public List<Point> DefiledHearts => ModContent.GetInstance<Defiled_Heart_TE_System>().tileEntityLocations.Select(Utils.ToPoint).ToList();
		internal List<Point16> LegacySave_DefiledHearts { get; set; } = [];
		private Dictionary<Point, Guid> _voidLocks;
		public Dictionary<Point, Guid> VoidLocks => _voidLocks ??= [];
		public Vector2? shimmerPosition;
		public Vector2? nearestFanSound = null;
		SlotId fanSoundInstance;
		FrameCachedValue<float> FanSoundVolume { get; } = new(() => {
			if (Instance?.nearestFanSound is not Vector2 nearestFanSound) return 0;
			return 2 / float.Max(nearestFanSound.DistanceSQ(Main.LocalPlayer.Center) / (16 * 20 * 16 * 20), 1);
		});
		public override void OnWorldUnload() {
			forceThunderstorm = false;
		}
		public override void ClearWorld() {
			peatSold = 0;
			foreach (LootPool pool in ModContent.GetContent<LootPool>()) pool.sequenceIndex = 0;
		}
		public override void LoadWorldData(TagCompound tag) {
			Mod.Logger.Info("LoadWorldData called on netmode " + Main.netMode);
			if (tag.ContainsKey("peatSold")) {
				peatSold = tag.GetAsInt("peatSold");
			}
			if (tag.ContainsKey("worldSurfaceLow")) _worldSurfaceLow = tag.GetDouble("worldSurfaceLow");
			if (tag.TryGet("brinePoolRange", out TagCompound range)) {
				if (range.TryGet("X", out int x) && range.TryGet("Y", out int y) && range.TryGet("Width", out int width) && range.TryGet("Height", out int height)) brinePoolRange = new(x, y, width, height);
			}
			if (tag.ContainsKey("defiledHearts")) {
				LegacySave_DefiledHearts = tag.Get<List<Vector2>>("defiledHearts").Select(Utils.ToPoint16).ToList();
			}
			tag.TryGet("hasDefiled", out hasDefiled);
			tag.TryGet("hasRiven", out hasRiven);
			tag.TryGet("hasAshven", out hasAshen);
			tag.TryGet("forceThunderstorm", out forceThunderstorm);
			tag.TryGet("unlockedBrineNPC", out unlockedBrineNPC);
			if (tag.TryGet("voidLocks", out List<TagCompound> voidLocks)) {
				_voidLocks = voidLocks.ToDictionary(
					t => t.Get<Vector2>("pos").ToPoint(),
					t => Guid.Parse(t.Get<string>("uuid"))
				);
			} else {
				_voidLocks = [];
			}

			defiledResurgenceTiles = [];
			defiledAltResurgenceTiles = [];
			questsTag = tag.SafeGet<TagCompound>("Quests");
			if (Main.dedServ) {
				foreach (Quest quest in Quest_Registry.Quests) {
					if (quest.SaveToWorld) {
						quest.LoadData(questsTag.SafeGet<TagCompound>(quest.FullName) ?? []);
					}
				}
			}
			hasLoggedPUP = false;
			if (tag.TryGet(nameof(shimmerPosition), out Vector2 _shimmerPosition)) shimmerPosition = _shimmerPosition;
			else {
				for (int i = 0; i < Main.maxTilesX; i++) {
					for (int j = 0; j < Main.maxTilesY; j++) {
						Tile tile = Framing.GetTileSafely(i, j);
						if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Shimmer) {
							shimmerPosition = new(i, j);
							goto foundShimmer;
						}
					}
				}
				foundShimmer:;
			}
		}
		public override void PostWorldLoad() {
			if (Main.numClouds > Main.maxClouds) Main.numClouds = Main.maxClouds;
			for (int i = 0; i < Main.maxTilesX; i++) {
				for (int j = 0; j < Main.maxTilesY; j++) {
					Tile tile = Main.tile[i, j];
					int generateType = OriginsSets.Walls.GeneratesLiquid[tile.WallType];
					if (generateType != -1) {
						tile.LiquidType = generateType;
						tile.LiquidAmount = 255;
					}
				}
			}
		}
		internal TagCompound questsTag;
		public override void SaveWorldData(TagCompound tag) {
			tag.Add("peatSold", peatSold);
			tag.Add("hasDefiled", hasDefiled);
			tag.Add("hasRiven", hasRiven);
			tag.Add("hasAshen", hasAshen);
			tag.Add("forceThunderstorm", forceThunderstorm);
			tag.Add("unlockedBrineNPC", unlockedBrineNPC);
			if (_worldSurfaceLow.HasValue) {
				tag.Add("worldSurfaceLow", _worldSurfaceLow);
			}
			tag.Add("brinePoolRange",
				new TagCompound {
					["X"] = brinePoolRange.X,
					["Y"] = brinePoolRange.Y,
					["Width"] = brinePoolRange.Width,
					["Height"] = brinePoolRange.Height
				}
			);
			tag.Add("voidLocks", VoidLocks.Select(kvp => new TagCompound() {
				["pos"] = kvp.Key.ToVector2(),
				["uuid"] = kvp.Value.ToString()
			}).ToList());
			TagCompound questsTag = [];
			foreach (Quest quest in Quest_Registry.Quests) {
				if (quest.SaveToWorld) {
					TagCompound questTag = [];
					quest.SaveData(questTag);
					if (questTag.Count > 0) {
						questsTag.Add(quest.FullName, questTag);
						if (!WorldGen.generatingWorld) Mod.Logger.Info($"Saving {quest.NameValue} to world with data: {questTag}");
					} else {
						if (!WorldGen.generatingWorld) Mod.Logger.Info($"Not saving {quest.NameValue}, no data to save");
					}
				}
			}
			if (questsTag.Count > 0) {
				tag.Add("Quests", questsTag);
			}
			if (shimmerPosition.HasValue) tag.Add(nameof(shimmerPosition), shimmerPosition.Value);
		}
		public override void NetSend(BinaryWriter writer) {
			writer.WriteFlags(forceAF, forceThunderstorm, shimmerPosition.HasValue);
			if (shimmerPosition.HasValue) writer.WriteVector2(shimmerPosition.Value);
		}
		public override void NetReceive(BinaryReader reader) {
			reader.ReadFlags(out bool forceAF, out forceThunderstorm, out bool hasShimmerPosition);
			ForceAF = forceAF;
			if (hasShimmerPosition) shimmerPosition = reader.ReadVector2();
		}
		public override void ResetNearbyTileEffects() {
			voidTiles = 0;
			defiledTiles = 0;
			rivenTiles = 0;
			ashenTiles = 0;
			brineTiles = 0;
			fiberglassTiles = 0;
			limestoneTiles = 0;
			chambersiteTiles = 0;
			chambersiteWalls = 0;
			Array.Clear(Chambersite_Stone_Wall.wallCounts);

			nearestFanSound = null;

			SC_Scene_Effect.monolithTileActive = false;
			Defiled_Wastelands.monolithActive = false;
		}

		public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) {
			voidTiles = tileCounts[ModContent.TileType<Dusk_Stone>()];

			defiledTiles = tileCounts[ModContent.TileType<Defiled_Stone>()]
				+ tileCounts[ModContent.TileType<Defiled_Grass>()]
				+ tileCounts[ModContent.TileType<Defiled_Jungle_Grass>()]
				+ tileCounts[ModContent.TileType<Defiled_Sand>()]
				+ tileCounts[ModContent.TileType<Defiled_Sandstone>()]
				+ tileCounts[ModContent.TileType<Hardened_Defiled_Sand>()]
				+ tileCounts[ModContent.TileType<Defiled_Ice>()];

			rivenTiles = tileCounts[ModContent.TileType<Spug_Flesh>()]
				+ tileCounts[ModContent.TileType<Riven_Grass>()]
				+ tileCounts[ModContent.TileType<Riven_Jungle_Grass>()]
				+ tileCounts[ModContent.TileType<Silica>()]
				+ tileCounts[ModContent.TileType<Brittle_Quartz>()]
				+ tileCounts[ModContent.TileType<Quartz>()]
				+ tileCounts[ModContent.TileType<Primordial_Permafrost>()];

			ashenTiles = tileCounts[ModContent.TileType<Tainted_Stone>()]
				+ tileCounts[ModContent.TileType<Ashen_Grass>()]
				+ tileCounts[ModContent.TileType<Ashen_Jungle_Grass>()]
				+ tileCounts[ModContent.TileType<Ashen_Murky_Sludge_Grass>()]
				+ tileCounts[ModContent.TileType<Sootsand>()]
				+ tileCounts[ModContent.TileType<Soot_Sandstone>()]
				+ tileCounts[ModContent.TileType<Hardened_Sootsand>()]
				+ tileCounts[ModContent.TileType<Brown_Ice>()];

			brineTiles = tileCounts[ModContent.TileType<Baryte>()];

			fiberglassTiles = tileCounts[ModContent.TileType<Fiberglass_Tile>()];

			limestoneTiles = tileCounts[ModContent.TileType<Limestone>()]
				+ tileCounts[ModContent.TileType<Limestone_Stalactite>()]
				+ tileCounts[ModContent.TileType<Limestone_Stalagmite>()]
				+ tileCounts[ModContent.TileType<Limestone_Pile_Medium>()];

			chambersiteTiles = tileCounts[ModContent.TileType<Chambersite>()];
			for (int i = 0; i < Chambersite_Ore.chambersiteTiles.Count; i++) {
				chambersiteTiles += tileCounts[Chambersite_Ore.chambersiteTiles[i].Type];
			}
			chambersiteWalls = 0;
			for (int i = 0; i < Chambersite_Stone_Wall.chambersiteWalls.Count; i++) {
				chambersiteWalls += Chambersite_Stone_Wall.wallCounts[Chambersite_Stone_Wall.chambersiteWalls[i]];
			}

			if (!Main.SceneMetrics.HasSunflower && Main.dayTime) {
				int team = Main.LocalPlayer.team;
				foreach (Projectile projectile in Main.ActiveProjectiles) {
					if (projectile.type == Sunny_Sunflower.ID && projectile.ai[2] != 1 && team == Main.player[projectile.owner].team && projectile.Center.Clamp(Main.LocalPlayer.Hitbox).WithinRange(projectile.Center, 16 * 15)) {
						Main.SceneMetrics.HasSunflower = true;
						break;
					}
				}
			}

			if (!Main.dedServ && nearestFanSound.HasValue && (!fanSoundInstance.IsValid || !SoundEngine.TryGetActiveSound(fanSoundInstance, out _))) {
				fanSoundInstance = SoundEngine.PlaySound(Origins.Sounds.ThrusterLoop.WithPitch(-0.5f).WithVolume(0.1f), nearestFanSound.Value, sound => {
					sound.Position = nearestFanSound;
					sound.Volume = FanSoundVolume.Value;
					return nearestFanSound.HasValue && sound.Volume > 0.001f;
				});
			}
		}
		public bool TryAddVoidLock(Point position, Guid owner, bool fromNet = false, int netOwner = -1) {
			if (VoidLocks.TryAdd(position, owner)) {
				switch (Main.netMode) {
					case NetmodeID.MultiplayerClient:
					if (!fromNet) {
						ModPacket packet = Origins.instance.GetPacket();
						packet.Write(Origins.NetMessageType.add_void_lock);
						packet.Write(position.X);
						packet.Write(position.Y);
						packet.Write(owner.ToByteArray());
						packet.Send();
					}
					break;
					case NetmodeID.Server: {
						ModPacket packet = Origins.instance.GetPacket();
						packet.Write(Origins.NetMessageType.add_void_lock);
						packet.Write(position.X);
						packet.Write(position.Y);
						packet.Write(owner.ToByteArray());
						packet.Send(ignoreClient: netOwner);
						break;
					}
				}
				return true;
			}
			return false;
		}
		public void RemoveVoidLock(Point position, bool fromNet = false, int netOwner = -1) {
			if (!VoidLocks.Remove(position)) return;
			switch (Main.netMode) {
				case NetmodeID.MultiplayerClient:
				if (!fromNet) {
					ModPacket packet = Origins.instance.GetPacket();
					packet.Write(Origins.NetMessageType.remove_void_lock);
					packet.Write(position.X);
					packet.Write(position.Y);
					packet.Send();
				}
				break;
				case NetmodeID.Server: {
					ModPacket packet = Origins.instance.GetPacket();
					packet.Write(Origins.NetMessageType.remove_void_lock);
					packet.Write(position.X);
					packet.Write(position.Y);
					packet.Send(ignoreClient: netOwner);
					break;
				}
			}
		}
		protected internal List<(int x, int y)> defiledResurgenceTiles;
		protected internal List<(int x, int y, ushort oldType, ushort hurtType)> defiledAltResurgenceTiles;
		protected internal Queue<(int x, int y)> queuedKillTiles;
		protected internal HashSet<Point> anoxicAirTiles;
		public override void PostUpdateWorld() {
			if (DefiledResurgenceActive) {
				if (defiledResurgenceTiles.Count > 0 && WorldGen.genRand.NextBool(5)) {
					int index = WorldGen.genRand.Next(defiledResurgenceTiles.Count);
					(int k, int l) = defiledResurgenceTiles[index];
					ConvertTile(ref Main.tile[k, l].TileType, evil_wastelands);
					WorldGen.SquareTileFrame(k, l);
					NetMessage.SendTileSquare(-1, k, l, 1);
					defiledResurgenceTiles.RemoveAt(index);
				}
			}
			if (defiledAltResurgenceTiles.Count > 0 && WorldGen.genRand.NextBool(30)) {
				int index = WorldGen.genRand.Next(defiledAltResurgenceTiles.Count);
				(int k, int l, ushort oldType, ushort hurtType) = defiledAltResurgenceTiles[index];
				if (Main.tile[k, l].TileIsType(hurtType)) {
					Main.tile[k, l].TileType = oldType;
					WorldGen.SquareTileFrame(k, l);
					NetMessage.SendTileSquare(-1, k, l, 1);
				}
				defiledAltResurgenceTiles.RemoveAt(index);
			}
			int q = 100;
			while (!WorldGen.gen && (queuedKillTiles?.Count ?? 0) > 0) {
				(int i, int j) = queuedKillTiles.Dequeue();
				WorldGen.KillTile(i, j, true);
				if (q-- < 0) {
					break;
				}
			}
		}
		public void QueueKillTile(int i, int j) {
			queuedKillTiles ??= new Queue<(int, int)>();
			queuedKillTiles.Enqueue((i, j));
		}
		public override void PostWorldGen() {
			shimmerPosition = new((float)GenVars.shimmerPosition.X, (float)GenVars.shimmerPosition.Y);
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
				ChestID.Sandstone,
				ChestID.LockedHallow
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
					} else if (tileType == TileID.Containers2) {
						cache = chestLoots[Main.tile[chest.x, chest.y].TileFrameX / 36 + 56];
						if (cache is null) continue;
						lootType = chest.item[0].type;
						cache.AddLoot(lootType, i);
						noLoot = false;
					}
				}
			}
			if (noLoot) return;
			ApplyWeightedLootQueue(chestLoots, ChestLoot.Actions);
			_worldSurfaceLow = GenVars.worldSurfaceLow;
		}
		public static void ApplyWeightedLootQueue(ChestLootCache[] lootCaches, params (LootQueueAction action, int param, float weight)[] actions) {
			int lootType;
			ChestLootCache cache = null;
			Chest chest;
			int chestIndex = -1;
			Queue<(int, float, int)> items = new();
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
				if (cache is null) {
					Origins.instance.Logger.Info($"failed to use chest cache ");
					return;
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
						Origins.LogError($"broke on {Lang.GetItemNameValue(newLootType.param)} with {i} remaining, no further valid chests");
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
			if (actionIndex < actions.Length && actions[actionIndex].action is CHANGE_QUEUE) {
				cache = lootCaches[actions[actionIndex].param];
				filterCache();
				actionIndex++;
				goto cont;
			}
			if (actionIndex < actions.Length && actions[actionIndex].action is SET_COUNT_RANGE) {
				countRange = (actions[actionIndex].param, (int)actions[actionIndex].weight);
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
		internal static void UpdateTotalEvilTiles() {
			totalDefiled = totalDefiled2;
			tDefiled = (byte)Math.Round((totalDefiled / (float)WorldGen.totalSolid2) * 100f);
			totalRiven = totalRiven2;
			tRiven = (byte)Math.Round((totalRiven / (float)WorldGen.totalSolid2) * 100f);
			totalAshen = totalAshen2;
			tAshen = (byte)Math.Round((totalAshen / (float)WorldGen.totalSolid2) * 100f);
			if (tDefiled == 0 && totalDefiled > 0) {
				tDefiled = 1;
			}
			if (tRiven == 0 && totalRiven > 0) {
				tRiven = 1;
			}
			if (tAshen == 0 && totalAshen > 0) {
				tAshen = 1;
			}
			if (Main.netMode == NetmodeID.Server) {
				ModPacket packet = Origins.instance.GetPacket(3);
				packet.Write(Origins.NetMessageType.tile_counts);
				packet.Write(tDefiled);
				packet.Write(tRiven);
				packet.Write(tAshen);
			}
			totalDefiled2 = 0;
		}
		public static bool ConvertTile(ref ushort tileType, byte evilType, bool aggressive = false) {
			getEvilTileConversionTypes(evilType, out ushort stoneType, out ushort grassType, out _, out ushort sandType, out ushort sandstoneType, out ushort hardenedSandType, out ushort iceType);
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
			List<Vector2> dirs = new(8);
			for (int k = -1; k <= 1; k++) {
				for (int l = -1; l <= 1; l++) {
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
			if (fiberglassFrameSet is null) fiberglassFrameSet = [];
			fiberglassFrameSet.Add(new(i, j));
			fiberglassNeedsFraming = true;
		}
	}
}
