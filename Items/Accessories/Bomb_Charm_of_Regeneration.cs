using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Bomb_Charm_of_Regeneration : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Vitality",
			"Explosive"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 26);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.15f;
			player.lifeRegen += 1;
            //player.GetModPlayer<OriginPlayer>().bombCharminIt = true;
        }
	}
}
