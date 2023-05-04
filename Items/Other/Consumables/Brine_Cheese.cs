using Origins.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Brine_Cheese : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Brine Cheese");
			Tooltip.SetDefault("{$CommonItemTooltip.MajorStats}\n'But, there's a catch...'");
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MonsterLasagna);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.scale = 0.6f;
			Item.buffType = Brine_Cheese_Buff.ID;
			Item.buffTime = 60 * 60 * 12;
			Item.value = Item.sellPrice(copper: 1);
			Item.rare = ItemRarityID.Gray;
		}
	}
	public class Brine_Cheese_Buff : ModBuff {
		public override string Texture => "Origins/Buffs/Brine_Cheese_Buff";
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Brine Cheese");
			Description.SetDefault("Quite brave");
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.AddBuff(BuffID.WellFed3, 60);
			player.AddBuff(BuffID.Rabies, 180);
			player.AddBuff(BuffID.Tipsy, 60);
		}
	}
}
