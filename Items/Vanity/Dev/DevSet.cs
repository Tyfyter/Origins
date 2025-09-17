using Origins.Items.Pets;
using Origins.LootConditions;
using Origins.NPCs;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Vanity.Dev {
	public abstract class DevSet<TIconic> : ModItem, IItemObtainabilityProvider where TIconic : ModItem, new() {
		public override string Texture => new TIconic().Texture;
		public abstract IEnumerable<ItemTypeDropRuleWrapper> GetDrops();
		public override void SetStaticDefaults() {
			IItemDropRule rule = new DropAsSetRule(Type);
			foreach (ItemTypeDropRuleWrapper drop in GetDrops()) 				rule.WithOnSuccess(drop.Rule);
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
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			List<DropRateInfo> drops = [];
			foreach (ItemTypeDropRuleWrapper rule in GetDrops()) {
				DropRateInfoChainFeed ratesInfo = new(1f);
				rule.Rule.ReportDroprates(drops, ratesInfo);
			}
			for (int i = 0; i < drops.Count; i++) {
				StringBuilder text = new($"[i:{drops[i].itemId}] {Lang.GetItemNameValue(drops[i].itemId)}");
				if ((drops[i].conditions?.Count ?? 0) > 0) text.Append($" ({TextUtils.Format("Mods.PegasusLib.ListAll", drops[i].conditions.Select(c => c.GetConditionDescription()).ToArray())})");
				tooltips.Add(new(Mod, "Drop" + i, text.ToString()) {
					OverrideColor = PegasusLib.PegasusLib.GetRarityColor(ContentSamples.ItemsByType[drops[i].itemId].rare)
				});
			}
		}
	}
}
