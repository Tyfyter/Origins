using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Quantum_Cell : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Quantum Cell");
			Tooltip.SetDefault("Used at the Cubekon Altar");

			SacrificeTotal = 3;
			//ItemID.Sets.SortingPriorityBossSpawns[Type] = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LihzahrdPowerCell);
		}
	}
}
