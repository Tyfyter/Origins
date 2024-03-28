using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Stack_of_Shraps : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat",
			"ExplosiveBoostAcc"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 26);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Expert;
			Item.expert = true;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.madHand = true;
			originPlayer.explosiveBlastRadius += 0.15f;
			originPlayer.explosiveThrowSpeed += 0.2f;
		}
	}
}
