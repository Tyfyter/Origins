using AltLibrary.Common.AltBiomes;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.Reflection;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using static PegasusLib.FishingLootPool;
using static Terraria.ModLoader.ModContent;

namespace Origins.Items.Other.Fish {
	public class FishingLoot : ILoadable, IItemObtainabilityProvider {
		static FishingLootInfo _pool;
		public static FishingLootInfo lastProcessedLoot;
		public static FishingLootInfo Pool => _pool ??= new OrderedFishingLoot(
		#region brine
				new LeadingConditionFishLoot(new OrderedFishingLoot(new LeadingConditionFishLoot(
						new OrderedFishingLoot(
							new ItemFishingLoot(ItemType<Basic_Crate>(), (_, attempt) => attempt.crate && Main.hardMode),
								new ItemFishingLoot(ItemType<Residual_Crate>(), (_, attempt) => attempt.crate),
								new ItemFishingLoot(ItemType<Huff_Puffer_Bait>(), (_, _) => true)
					), (_, attempt) => attempt.rare && !(attempt.veryrare || attempt.legendary)),
					new LeadingConditionFishLoot(
						new OrderedFishingLoot(
						new ItemFishingLoot(ItemType<Bobbit_Worm>(), (_, attempt) => attempt.questFish == ItemType<Bobbit_Worm>()),
						new ItemFishingLoot(ItemType<Mithrafin>(), (_, _) => Main.rand.NextBool(10)),
						new ItemFishingLoot(ItemType<Toadfish>(), (_, _) => true)
					), (_, attempt) => attempt.uncommon)
				), (player, _) => player.InModBiome<Brine_Pool>()),
		#endregion brine

		#region fiberglass undergrowth
			new LeadingConditionFishLoot(
				new ItemFishingLoot(ItemType<Fiberbass>(), (_, attempt) => attempt.uncommon && attempt.questFish == ItemType<Fiberbass>()),
			(player, _) => player.InModBiome<Fiberglass_Undergrowth>()),
		#endregion fiberglass undergrowth

			new LeadingConditionFishLoot(new ComboFishingLoot(
				((_, _) => 1, new ItemFishingLoot(ItemType<Tire>(), (_, _) => Main.rand.NextBool(4)))
			), (_, attempt) => attempt.rolledItemDrop >= ItemID.OldShoe && attempt.rolledItemDrop <= ItemID.TinCan),

		#region jungle
			new LeadingConditionFishLoot(
				new ItemFishingLoot(ItemType<Messy_Leech>(), (player, attempt) => (attempt.uncommon && !(attempt.rare || attempt.veryrare || attempt.legendary)) && Main.rand.NextBool(10)),
			(player, _) => player.ZoneJungle)
		#endregion jungle
		);
		public IEnumerable<int> ProvideItemObtainability() => Pool.ReportDrops()
			.Concat(GetContent<FishingLootPool>().SelectMany(p => p.Crate.Concat(p.Legendary.Concat(p.VeryRare.Concat(p.Rare.Concat(p.Uncommon.Concat(p.Common))))).SelectMany(f => f.ReportDrops())));
		public static void AddToVanillaPools() {
			GetInstance<CrimsonAltBiome.CrimsonFishingPool>().Rare.Add(FishingCatch.Item(ItemType<Blotopus>()));
		}
		public void Load(Mod mod) { }
		public void Unload() {
			_pool = null;
		}
	}
	public abstract class FishingLootInfo {
		public abstract IEnumerable<int> ReportDrops();
		public abstract bool CatchFish(Player player, FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition);
	}
	public class ComboFishingLoot(IEnumerable<(Func<Player, FishingAttempt, double>, FishingLootInfo)> loots) : FishingLootInfo {
		readonly List<(Func<Player, FishingAttempt, double> weight, FishingLootInfo loot)> loots = loots as List<(Func<Player, FishingAttempt, double>, FishingLootInfo)> ?? loots.ToList();
		public ComboFishingLoot(params (Func<Player, FishingAttempt, double>, FishingLootInfo)[] loots) : this(loots.AsEnumerable()) { }

		public override IEnumerable<int> ReportDrops() {
			return loots.SelectMany(l => l.loot.ReportDrops());
		}
		public override bool CatchFish(Player player, FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
			FishingLoot.lastProcessedLoot = this;
			WeightedRandom<FishingLootInfo> pool = new(loots.Select(l => new Tuple<FishingLootInfo, double>(l.loot, l.weight(player, attempt))).ToArray());
			do {
				if (pool.Pop()?.CatchFish(player, attempt, ref itemDrop, ref npcSpawn, ref sonar, ref sonarPosition) ?? false) {
					return true;
				}
			} while (pool.elements.Count > 0);
			return false;
		}
	}
	public class OrderedFishingLoot(IEnumerable<FishingLootInfo> loots) : FishingLootInfo {
		readonly List<FishingLootInfo> loots = loots as List<FishingLootInfo> ?? loots.ToList();
		public OrderedFishingLoot(params FishingLootInfo[] loots) : this(loots.AsEnumerable()) { }

		public override IEnumerable<int> ReportDrops() {
			return loots.SelectMany(l => l.ReportDrops());
		}
		public override bool CatchFish(Player player, FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
			FishingLoot.lastProcessedLoot = this;
			for (int i = 0; i < loots.Count; i++) {
				FishingLootInfo loot = loots[i];
				if (loot.CatchFish(player, attempt, ref itemDrop, ref npcSpawn, ref sonar, ref sonarPosition)) {
					return true;
				}
			}
			return false;
		}
	}
	public class LeadingConditionFishLoot(FishingLootInfo loot, Func<Player, FishingAttempt, bool> condition, bool alwaysTrue = false) : FishingLootInfo {
		public override IEnumerable<int> ReportDrops() {
			return loot.ReportDrops();
		}
		public override bool CatchFish(Player player, FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
			FishingLoot.lastProcessedLoot = this;
			if (condition(player, attempt)) {
				loot.CatchFish(player, attempt, ref itemDrop, ref npcSpawn, ref sonar, ref sonarPosition);
				return true;
			}
			return alwaysTrue;
		}
	}
	public class ItemFishingLoot(int itemId, Func<Player, FishingAttempt, bool> condition) : FishingLootInfo {
		public Func<Player, FishingAttempt, bool> condition = condition;
		public int itemId = itemId;

		public override IEnumerable<int> ReportDrops() {
			yield return itemId;
		}
		public override bool CatchFish(Player player, FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
			FishingLoot.lastProcessedLoot = this;
			if (condition(player, attempt)) {
				itemDrop = itemId;
				return true;
			}
			return false;
		}
	}
}
