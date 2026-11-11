using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Beard)]
	public class Medical_Mask : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Misc
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(36, 26);
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 15);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.OriginPlayer().medicalMask = true;
		}
	}
}
