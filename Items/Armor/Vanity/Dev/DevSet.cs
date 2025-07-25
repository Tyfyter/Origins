using Origins.Items.Pets;
using Origins.LootConditions;
using Origins.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.Dev {
	public abstract class DevSet<TIconic> : ModItem, IItemObtainabilityProvider where TIconic : ModItem, new() {
		public override string Texture => new TIconic().Texture;
		public abstract IEnumerable<ItemTypeDropRuleWrapper> GetDrops();
		public override void SetStaticDefaults() {
			IItemDropRule rule = new DropAsSetRule(Type);
			foreach (ItemTypeDropRuleWrapper drop in GetDrops()) {
				rule.WithOnSuccess(drop.Rule);
			}
			OriginGlobalItem.OriginsDevSetRule.options = OriginGlobalItem.OriginsDevSetRule.options.Concat([
				rule
			]).ToArray();
			ItemID.Sets.OpenableBag[Type] = true;
		}
		public IEnumerable<int> ProvideItemObtainability() => GetDrops().SelectMany(w => {
			List<DropRateInfo> drops = [];
			w.Rule.ReportDroprates(drops, new DropRateInfoChainFeed(1));
			return drops.Select(d => d.itemId);
		});
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			foreach (ItemTypeDropRuleWrapper drop in GetDrops()) itemLoot.Add(drop.Rule);
		}
		public record struct ItemTypeDropRuleWrapper(IItemDropRule Rule) {
			public static implicit operator ItemTypeDropRuleWrapper(int drop) => new(ItemDropRule.Common(drop));
		}
	}
}
