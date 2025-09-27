using Origins.Dev;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Testing {
	public abstract class TestingItem : ModItem, ICustomWikiStat, IItemObtainabilityProvider {
		public IEnumerable<int> ProvideItemObtainability() => [Type];
		public bool ShouldHavePage => false;
/*#if !DEBUG
		public override bool IsLoadingEnabled(Mod mod) => false;
#endif*/
		public override void SetStaticDefaults() {
			ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
			Item.ResearchUnlockCount = 0;
		}
	}
}
