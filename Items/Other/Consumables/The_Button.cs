using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class The_Button : ModItem {
        public string[] Categories => [
            "BossSummon"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
			//ItemID.Sets.SortingPriorityBossSpawns[Type] = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WormFood);
			Item.rare = CrimsonRarity.ID;
		}
	}
}
