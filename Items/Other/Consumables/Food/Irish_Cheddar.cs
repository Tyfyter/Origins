using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Irish_Cheddar : ModItem {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
			ItemID.Sets.FoodParticleColors[Type] = [
				new Color(255, 237, 168),
				new Color(218, 204, 151),
				new Color(165, 151, 99)
			];
			ItemID.Sets.IsFood[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				32, 24,
				BuffID.WellFed,
				60 * 60 * 12
			);
			Item.scale = 0.6f;
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
