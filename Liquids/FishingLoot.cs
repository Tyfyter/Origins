using AltLibrary.Common.AltBiomes;
using Origins.Items.Accessories;
using Origins.Items.Other.Fish;
using Origins.Items.Tools.Liquids;
using Origins.Items.Weapons.Ranged;
using Origins.World.BiomeData;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static PegasusLib.FishingLootPool;
using static Terraria.ModLoader.ModContent;

namespace Origins.Liquids {
	#region Vanilla
	public class FishingLoot : ILoadable, IItemObtainabilityProvider {
		public IEnumerable<int> ProvideItemObtainability() => GetContent<FishingLootPool>().SelectMany(p => p.Crate.Concat(p.Legendary.Concat(p.VeryRare.Concat(p.Rare.Concat(p.Uncommon.Concat(p.Common))))).SelectMany(f => f.ReportDrops()));
		public static void AddToVanillaPools() {
			GetInstance<CrimsonAltBiome.CrimsonFishingPool>().Rare.Add(FishingCatch.Item(ItemType<Blotopus>()));
		}
		public void Load(Mod mod) { }
		public void Unload() { }
	}
	public class Jungle_Fishing_Loot : FishingLootPool {
		public override bool IsActive(Player player, FishingAttempt attempt) => player.ZoneJungle;
		public override void SetStaticDefaults() {
			Uncommon.AddRange([
				FishingCatch.Item(ItemType<Messy_Leech>()),
				new FallthroughFishingCatch(weight: 10)
			]);
		}
	}
	#endregion
	#region Liquids
	public class Amebic_Gel_Fishing_Loot : FishingLootPool {
		public override bool IsActive(Player player, FishingAttempt attempt) => attempt.BobberInLiquid<Amebic_Gel>();
		public override void SetStaticDefaults() {
			Legendary.AddRange([
				FishingCatch.Item(ItemType<Amebic_Gel_Bottomless_Bucket>()),
				FishingCatch.Item(ItemType<Amebic_Gel_Sponge>())
			]);
			Uncommon.AddRange([
				FishingCatch.Item(ItemType<Tearracuda>()),
				new FallthroughFishingCatch((player, _) => !player.InModBiome<Riven_Hive>(), 10)
			]);
		}
	}
	public class Oil_Fishing_Loot : FishingLootPool {
		public override bool IsActive(Player player, FishingAttempt attempt) => attempt.BobberInLiquid<Oil>();
		public override void SetStaticDefaults() {
			Legendary.AddRange([
				FishingCatch.Item(ItemType<Oil_Bottomless_Bucket>()),
				FishingCatch.Item(ItemType<Oil_Sponge>())
			]);
			Uncommon.AddRange([
				FishingCatch.Item(ItemType<Polyeel>()),
				new FallthroughFishingCatch((player, _) => !player.InModBiome<Ashen_Biome>(), 4)
			]);
		}
	}
	#endregion
	#region Misc
	public class Trash_Fishing_Loot : FishingLootPool {
		public override bool IsActive(Player player, FishingAttempt attempt) => attempt.rolledItemDrop >= ItemID.OldShoe && attempt.rolledItemDrop <= ItemID.TinCan;
		public override void SetStaticDefaults() {
			List<FishingCatch> trash = [
				FishingCatch.Item(ItemType<Tire>()),
				new FallthroughFishingCatch(weight: 4)
			];
			Crate.AddRange(trash);
			Legendary.AddRange(trash);
			VeryRare.AddRange(trash);
			Rare.AddRange(trash);
			Uncommon.AddRange(trash);
			Common.AddRange(trash);
		}
	}
	#endregion
}
