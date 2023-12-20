using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Irish_Cheddar : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MonsterLasagna);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.scale = 0.6f;
			Item.buffType = BuffID.WellFed;
			Item.buffTime = 60 * 60 * 12;
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Blue;
		}
		public override bool ConsumeItem(Player player) {
			player.AddBuff(Irish_Cheddar_Buff.ID, Item.buffTime);
			return true;
		}
	}
	public class Irish_Cheddar_Buff : ModBuff {
		public override string Texture => "Origins/Buffs/Food/Irish_Cheddar_Buff";
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetCritChance(DamageClass.Generic) += 0.08f;
		}
	}
}
