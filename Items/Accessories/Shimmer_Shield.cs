using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Shield)]
	public class Shimmer_Shield : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Movement
		];
		public static float ReflectionDamageMultiplier => 2;
		public static float ReflectionHostileDamageMultiplier => 2;
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 32);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Blue;
			Item.expert = true;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.endurance += (1 - player.endurance) * 0.09f;
			player.OriginPlayer().shimmerShield = true;
		}

	}
}
