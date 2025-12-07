using Origins.LootConditions;
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
