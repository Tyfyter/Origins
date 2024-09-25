using Origins.Dev;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Laser_Tag_Vest : ModItem, ICustomWikiStat {
		public string[] Categories => [
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(26, 26);
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightPurple;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.laserTagVest = true;
			if (Laser_Tag_Console.LaserTagGameActive) {
				player.noBuilding = !Laser_Tag_Console.LaserTagRules.Building;
			}
		}
	}
}
