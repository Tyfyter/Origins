using Origins.Items.Accessories;
using Origins.Items.Armor.Laborer;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Other.Consumables.Medicine;
using Origins.Items.Tools;
using Origins.Items.Tools.Liquids;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.LootConditions;
using Origins.Tiles;
using Origins.Tiles.Ashen;
using Origins.Tiles.Other;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Core;
public abstract class LootPool : ModType {
	protected string GetNestHierarchy() {
		Type type = GetType();
		string nests = "";
		while (type.IsNested) {
			type = type.DeclaringType;
			nests = $"{type.Name}.{nests}";
		}
		return nests;
	}
	public override string Name => GetNestHierarchy() + base.Name;
	readonly List<IItemDropRule> rules = [];
	bool? sequential = null;
	public bool Sequential {
		get => sequential ?? false;
		protected set {
			if (sequential.HasValue) throw new InvalidOperationException($"{nameof(Sequential)} can only be set in {nameof(SetStaticDefaults)}");
			sequential = value;
		}
	}
	public int sequenceIndex = 0;
	public void Resolve(DropAttemptInfo info, bool selectRandomly = false) {
		if (Sequential) {
			IItemDropRule rule;
			if (selectRandomly || info.npc is not null || info.item != ItemID.None) rule = Main.rand.Next(rules);
			else rule = rules[sequenceIndex++ % rules.Count];
			OriginExtensions.ResolveRule(rule, info);
			return;
		}
		foreach (IItemDropRule item in rules) {
			OriginExtensions.ResolveRule(item, info);
		}
	}
	public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) {
		if (Sequential) ratesInfo.parentDroprateChance /= rules.Count;
		foreach (IItemDropRule item in rules) {
			item.ReportDroprates(drops, ratesInfo);
		}
	}
	protected sealed override void Register() {
		ModTypeLookup<LootPool>.Register(this);
	}
	public sealed override void SetupContent() {
		SetStaticDefaults();
		if (!sequential.HasValue) Sequential = sequential ?? false;
	}
	public abstract override void SetStaticDefaults();
	public void AddRule(IItemDropRule rule) {
		if (OriginSystem.HasSetupAllContent) throw new InvalidOperationException($"{nameof(AddRule)} can only be called during content setup");
		rules.Add(rule);
	}
}
public class Example_Chest : LootPool {
	public class Rare : LootPool {
		public override void SetStaticDefaults() {
			Sequential = true;
			AddRule(ItemDropRule.Common(ItemID.ThornWhip));
			AddRule(ItemDropRule.Common(ItemID.TikiTorch));
			AddRule(ItemDropRule.Common(ItemID.EyeOfCthulhuPetItem));
		}
	}
	public override void SetStaticDefaults() {
		AddRule(new DropLootPoolRule<Rare>());
		AddRule(ItemDropRule.Common(ItemID.Pigronata, 1, 69, 420));
		AddRule(ItemDropRule.Common(ItemID.ThrowingKnife, 2, 13, 17));
	}
}
public class Ashen_Generic : LootPool {
	public class Rarer : LootPool {
		public override void SetStaticDefaults() {
			Sequential = true;
			AddRule(ItemDropRule.Common(ModContent.ItemType<Flak_Jacket>(), 5));
			AddRule(ItemDropRule.Common(ItemID.ExtendoGrip, 5));
			AddRule(ItemDropRule.Common(ItemID.BallOfFuseWire, 5));
		}
	}
	public override void SetStaticDefaults() {
		AddRule(new DropLootPoolRule<Rarer>());
		AddRule(ItemDropRule.Common(ItemID.Compass, 40));
		AddRule(ItemDropRule.Common(ItemID.Ruler, 10));
		AddRule(ItemDropRule.Common(ItemID.Toolbelt, 10));
		AddRule(ItemDropRule.Common(ItemID.Grenade, 1, 5, 12));
		AddRule(ItemDropRule.Common(ItemID.Goggles, 2, 1, 2));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Gas_Mask>(), 4, 1, 2));
		AddRule(ItemDropRule.Common(ItemID.SilverCoin, 2, 2, 5));
		AddRule(ItemDropRule.Common(ItemID.CopperCoin, 1, 37, 99));
	}
}
public class Ashen_GenericLore : LootPool {
	public class Rarer : LootPool {
		public override void SetStaticDefaults() {
			Sequential = true;
			AddRule(ItemDropRule.Common(ModContent.ItemType<Worn_Paper_The_Packing_Slip>()));
			AddRule(ItemDropRule.Common(ModContent.ItemType<Worn_Paper_Mean_Baby>()));
		}
	}
	public override void SetStaticDefaults() {
		AddRule(new DropLootPoolRule<Rarer>());
		AddRule(ItemDropRule.Common(ModContent.ItemType<Flak_Jacket>(), 5));
		AddRule(ItemDropRule.Common(ItemID.Compass, 40));
		AddRule(ItemDropRule.Common(ItemID.Ruler, 10));
		AddRule(ItemDropRule.Common(ItemID.Toolbelt, 10));
		AddRule(ItemDropRule.Common(ItemID.Grenade, 1, 5, 12));
		AddRule(ItemDropRule.Common(ItemID.Goggles, 2, 1, 2));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Gas_Mask>(), 4, 1, 2));
		AddRule(ItemDropRule.Common(ItemID.SilverCoin, 2, 2, 5));
		AddRule(ItemDropRule.Common(ItemID.CopperCoin, 1, 37, 99));
	}
}
public class Ashen_Mineral : LootPool {
	/*public class Rarer : LootPool {
		public override void SetStaticDefaults() {
			Sequential = true;
			AddRule(ItemDropRule.Common(ItemID.ThornWhip));
			AddRule(ItemDropRule.Common(ItemID.TikiTorch));
			AddRule(ItemDropRule.Common(ItemID.EyeOfCthulhuPetItem));
		}
	}*/
	public override void SetStaticDefaults() {
		//AddRule(new DropLootPoolRule<Rarer>());
		AddRule(ItemDropRule.Common(ModContent.ItemType<Oil_Bucket>(), 2, 80, 260));
		AddRule(ItemDropRule.Common(ItemID.TinOre, 1, 21, 270));
		AddRule(ItemDropRule.Common(ItemID.LeadOre, 2, 38, 250));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Silicon_Ore_Item>(), 1, 40, 200));
		AddRule(ItemDropRule.Common(ItemID.SilverOre, 2, 45, 220));
		AddRule(ItemDropRule.Common(ItemID.TungstenOre, 2, 60, 200));
		AddRule(ItemDropRule.Common(ItemID.Amber, 4, 20, 25));
	}
}
public class Ashen_Supplies : LootPool {
	public class Rarer : LootPool {
		public override void SetStaticDefaults() {
			Sequential = true;
			AddRule(ItemDropRule.Common(ItemID.Wrench, 1, 1, 10));
			AddRule(ItemDropRule.Common(ItemID.WireCutter, 1, 1, 10));
			AddRule(ItemDropRule.Common(ItemID.GrapplingHook, 1, 1, 5));
			AddRule(ItemDropRule.Common(ModContent.ItemType<Gills>(), 2));
		}
	}
	public override void SetStaticDefaults() {
		AddRule(new DropLootPoolRule<Rarer>());
		AddRule(ItemDropRule.Common(ModContent.ItemType<C6_Jackhammer>(), 10, 1, 5));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Miter_Saw>(), 10, 1, 5));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Piledriver>(), 10, 1, 5));
		AddRule(ItemDropRule.Common(ItemID.Binoculars, 12));
		AddRule(ItemDropRule.Common(ItemID.Bomb, 1, 65, 135));
		AddRule(ItemDropRule.Common(ItemID.Dynamite, 1, 35, 80));
		AddRule(ItemDropRule.Common(ItemID.Explosives, 2, 45, 90));
		AddRule(ItemDropRule.Common(ItemID.BlueFlare, 3, 35, 78));
		AddRule(ItemDropRule.Common(ItemID.IronPickaxe, 7, 15, 35));
		AddRule(ItemDropRule.Common(ItemID.IronHammer, 7, 12, 26));
		AddRule(ItemDropRule.Common(ItemID.Minecart, 7, 10, 15));
		AddRule(ItemDropRule.Common(TileItem.Get<Modular_Light_Fixture>().Type, 7, 20, 40));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Transistor_Item>(), 7, 10, 25));
		AddRule(ItemDropRule.Common(ItemID.Wire, 7, 35, 125));
		AddRule(ItemDropRule.Common(ItemID.OrangePaint, 7, 115, 250));
		AddRule(ItemDropRule.Common(ItemID.Chain, 7, 80, 120));
		AddRule(ItemDropRule.Common(ItemID.Cog, 7, 50, 75));
		AddRule(ItemDropRule.Common(ItemID.Gel, 7, 210, 330));
	}
}
public class Ashen_Medical : LootPool {
	/*public class Rarer : LootPool {
		public override void SetStaticDefaults() {
			Sequential = true;
			AddRule(ItemDropRule.Common(ItemID.ThornWhip));
			AddRule(ItemDropRule.Common(ItemID.TikiTorch));
			AddRule(ItemDropRule.Common(ItemID.EyeOfCthulhuPetItem));
		}
	}*/
	public override void SetStaticDefaults() {
		//AddRule(new DropLootPoolRule<Rarer>());
		AddRule(ItemDropRule.Common(ModContent.ItemType<Morphine>(), 5, 8, 16));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Blood_Pack>(), 2, 3, 11));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Fire_Band>(), 1, 11, 20));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Adrenaline>(), 4, 3, 8));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Sanguis_Pack>(), 1, 1, 2));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Rasterwrap>(), 3, 1, 2));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Brightsee>(), 3, 5, 15));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Unmarked_Antidote>(), 1, 1, 4));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Medicinal_Acid>(), 2, 1, 3));
	}
}
public class Ashen_MedicalLore : LootPool {
	public override void SetStaticDefaults() {
		AddRule(ItemDropRule.Common(ModContent.ItemType<Worn_Paper_Workplace_Safety_Concern>()));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Morphine>(), 5, 8, 16));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Blood_Pack>(), 2, 3, 11));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Fire_Band>(), 1, 11, 20));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Adrenaline>(), 4, 3, 8));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Sanguis_Pack>(), 1, 1, 2));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Rasterwrap>(), 3, 1, 2));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Brightsee>(), 3, 5, 15));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Unmarked_Antidote>(), 1, 1, 4));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Medicinal_Acid>(), 2, 1, 3));
	}
}
public class Ashen_Personal : LootPool {
	public class Rarer : LootPool {
		public override void SetStaticDefaults() {
			Sequential = true;
			AddRule(ItemDropRule.Common(ModContent.ItemType<Laborer_Helmet>(), 2));
			AddRule(ItemDropRule.Common(ModContent.ItemType<Laborer_Breastplate>(), 2));
			AddRule(ItemDropRule.Common(ModContent.ItemType<Laborer_Greaves>(), 2));
		}
	}
	public override void SetStaticDefaults() {
		AddRule(new DropLootPoolRule<Rarer>());
		AddRule(ItemDropRule.Common(ModContent.ItemType<Pincushion>(), 40));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Nineball>(), 25));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Stress_Ball>(), 3));
		AddRule(ItemDropRule.Common(ItemID.FamiliarShirt, 15));
		AddRule(ItemDropRule.Common(ItemID.FamiliarPants, 15));
		AddRule(ItemDropRule.Common(ItemID.Fedora, 20));
		AddRule(ItemDropRule.Common(ItemID.BallaHat, 20));
		AddRule(ItemDropRule.Common(ItemID.EngineeringHelmet, 20));
		AddRule(ItemDropRule.Common(ItemID.SilverCoin, 1, 3, 11));
		AddRule(ItemDropRule.Common(ItemID.FlowerPacketViolet, 4, 1, 7));
		AddRule(ItemDropRule.Common(ItemID.FlowerPacketPink, 4, 1, 3));
		AddRule(ItemDropRule.Common(ItemID.FlowerPacketBlue, 4, 1, 5));
		AddRule(ItemDropRule.Common(ItemID.FlowerPacketYellow, 4, 1, 8));
		AddRule(ItemDropRule.Common(ItemID.GenderChangePotion, 14));
		AddRule(ItemDropRule.Common(ItemID.DrumSet, 10));
		AddRule(ItemDropRule.Common(ItemID.Fertilizer, 10));
		AddRule(ItemDropRule.Common(ItemID.GoldWatch, 10));
		AddRule(ItemDropRule.Common(ItemID.ShadowCandle, 10));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Bang_Snap>(), 10, 35, 70));
	}
}
public class Ashen_Rations : LootPool {
	public class Rarer : LootPool {
		public override void SetStaticDefaults() {
			Sequential = true;
			AddRule(ItemDropRule.Common(ItemID.SliceOfCake, 2));
			AddRule(ItemDropRule.Common(ModContent.ItemType<Crockin_Sprocks>(), 1, 4, 40));
		}
	}
	public override void SetStaticDefaults() {
		AddRule(new DropLootPoolRule<Rarer>());
		AddRule(ItemDropRule.Common(ItemID.Bacon, 4, 8, 29));
		AddRule(ItemDropRule.Common(ItemID.BBQRibs, 4, 5, 22));
		AddRule(ItemDropRule.Common(ItemID.Burger, 4, 4, 36));
		AddRule(ItemDropRule.Common(ItemID.Hotdog, 4, 3, 32));
		AddRule(ItemDropRule.Common(ItemID.Steak, 4, 2, 25));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Bread>(), 2, 8, 28));
		AddRule(ItemDropRule.Common(ItemID.FriedEgg, 4, 10, 22));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Irish_Cheddar>(), 8, 1, 2));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Potato>(), 1, 2, 20));
		AddRule(ItemDropRule.Common(ItemID.Apple, 4, 16, 32));
		AddRule(ItemDropRule.Common(ItemID.Banana, 4, 18, 38));
		AddRule(ItemDropRule.Common(ItemID.Lemon, 4, 12, 30));
		AddRule(ItemDropRule.Common(ItemID.Peach, 4, 14, 26));
		AddRule(ItemDropRule.Common(ItemID.Grapes, 4, 16, 28));
		AddRule(ItemDropRule.Common(ItemID.MilkCarton, 4, 10, 24));
		AddRule(ItemDropRule.Common(ItemID.CoffeeCup, 4, 20, 40));
		AddRule(ItemDropRule.Common(ItemID.FruitJuice, 4, 16, 36));
	}
}
public class Ashen_Armory : LootPool {
	public class Rarer : LootPool {
		public override void SetStaticDefaults() {
			Sequential = true;
			AddRule(ItemDropRule.Common(ModContent.ItemType<Switchblade_Broadsword>(), 2, 1, 3));
			AddRule(ItemDropRule.Common(ModContent.ItemType<Soldering_Iron>(), 2));
			AddRule(ItemDropRule.Common(ModContent.ItemType<Defective_Mortar_Shell>(), 2));
		}
	}
	public override void SetStaticDefaults() {
		AddRule(new DropLootPoolRule<Rarer>());
		AddRule(ItemDropRule.Common(ItemID.Revolver, 40));
		AddRule(ItemDropRule.Common(ItemID.Handgun, 10, 1, 3));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Flak_Jacket>(), 3));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Flashbang>(), 2, 35, 185));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Link_Grenade>(), 1, 50, 150));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Armor_Piercing_Bullet>(), 3, 55, 210));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Metal_Slug>(), 1, 15, 99));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Resizable_Mine_Iron>(), 1, 110, 240));
		AddRule(ItemDropRule.Common(ItemID.SilverBullet, 3, 100, 200));
	}
}
public class Ashen_HeavyArmory : LootPool {
	public class Rarer : LootPool {
		public override void SetStaticDefaults() {
			Sequential = true;
			AddRule(ItemDropRule.Common(ModContent.ItemType<Smog_Pod_4>()));
			AddRule(ItemDropRule.Common(ModContent.ItemType<The_Plant>()));
		}
	}
	public override void SetStaticDefaults() {
		AddRule(new DropLootPoolRule<Rarer>());
		AddRule(ItemDropRule.Common(ModContent.ItemType<Worn_Paper_Self_Preservation>()));
		AddRule(ItemDropRule.Common(ItemID.RocketIII, 1, 250, 360));
	}
}
public class Ashen_Closet : LootPool {
	public class Rarer : LootPool {
		public override void SetStaticDefaults() {
			Sequential = true;
			AddRule(ItemDropRule.Common(ModContent.ItemType<SMART_Wrench>(), 1, 1, 2));
		}
	}
	public override void SetStaticDefaults() {
		AddRule(new DropLootPoolRule<Rarer>());
		AddRule(ItemDropRule.Common(ModContent.ItemType<Worn_Paper_Loose_Wheel>()));
		AddRule(ItemDropRule.Common(ItemID.Bomb, 1, 65, 135));
		AddRule(ItemDropRule.Common(ItemID.Dynamite, 1, 35, 80));
		AddRule(ItemDropRule.Common(ItemID.TitaniumDrill));
		AddRule(ItemDropRule.Common(ItemID.SteampunkMinecart));
		AddRule(ItemDropRule.Common(ItemID.Wire, 7, 35, 125));
		AddRule(ItemDropRule.Common(ItemID.OrangePaint, 7, 115, 250));
		AddRule(ItemDropRule.Common(ItemID.Chain, 7, 80, 120));
		AddRule(ItemDropRule.Common(ItemID.Cog, 7, 50, 75));
	}
}
public class Ashen_Command : LootPool {
	/*public class Rarer : LootPool {
		public override void SetStaticDefaults() {
			Sequential = true;
			AddRule(ItemDropRule.Common(ModContent.ItemType<Flak_Jacket>(), 5));
		}
	}*/
	public override void SetStaticDefaults() {
		//AddRule(new DropLootPoolRule<Rarer>());
		AddRule(ItemDropRule.Common(ModContent.ItemType<Distress_Beacon>(), 1, 1, 2));
		AddRule(ItemDropRule.Common(ItemID.Grenade, 1, 7, 24));
		AddRule(ItemDropRule.Common(ModContent.ItemType<Gas_Mask>()));
		AddRule(ItemDropRule.Common(ItemID.GoldCoin));
	}
}