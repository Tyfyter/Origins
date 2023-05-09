using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Irish_Cheddar : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Irish Cheddar");
			Tooltip.SetDefault("{$CommonItemTooltip.MinorStats} and +8% critical strike chance\n'Certainly a little sharp'");
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MonsterLasagna);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.scale = 0.6f;
			Item.buffType = Irish_Cheddar_Buff.ID;
			Item.buffTime = 60 * 60 * 12;
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Blue;
		}
	}
	public class Irish_Cheddar_Buff : ModBuff {
		public override string Texture => "Origins/Buffs/Food/Irish_Cheddar_Buff";
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Irish Cheddar");
			Description.SetDefault("Very good");
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.AddBuff(BuffID.WellFed, 10);
			player.GetCritChance(DamageClass.Generic) += 0.08f;
		}
	}
}
