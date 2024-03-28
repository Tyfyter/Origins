using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Balloon)]
	public class Plasma_Phial : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Vitality"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(24, 24);
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.lifeRegen += 1;
			player.GetModPlayer<OriginPlayer>().plasmaPhial = true;
		}
	}
}
