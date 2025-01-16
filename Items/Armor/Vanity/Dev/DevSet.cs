using Origins.Items.Pets;
using Origins.LootConditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.Dev {
	public abstract class DevSet<TIconic> : ModItem, IItemObtainabilityProvider where TIconic : ModItem, new() {
		public override string Texture => new TIconic().Texture;
		public abstract IEnumerable<int> GetDrops();
		public override void SetStaticDefaults() {
			IItemDropRule rule = new DropAsSetRule(Type);
			foreach (int drop in GetDrops()) {
				rule.WithOnSuccess(ItemDropRule.Common(drop));
			}
			OriginGlobalItem.OriginsDevSetRule.options = OriginGlobalItem.OriginsDevSetRule.options.Concat([
				rule
			]).ToArray();
		}
		public IEnumerable<int> ProvideItemObtainability() => GetDrops();
	}
}
