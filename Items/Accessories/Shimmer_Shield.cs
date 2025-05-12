using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	//[AutoloadEquip(EquipType.Shield)]
	public class Shimmer_Shield : ModItem, ICustomWikiStat {
		public override string Texture => "Terraria/Images/Item_" + ItemID.WineGlass;
		public string[] Categories => [
			"Movement"
		];
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WarriorEmblem);
			Item.shieldSlot = ArmorIDs.Shield.ShieldofCthulhu;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.expert = true;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.endurance += (1 - player.endurance) * 0.09f;
			player.OriginPlayer().shimmerShield = true;
		}

	}
}
