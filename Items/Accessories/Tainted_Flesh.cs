using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Tainted_Flesh : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.Torn
		];
		static short glowmask;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(5);
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 30);
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 4);
			Item.glowMask = glowmask;
			Item.dye = 0;
			Item.maxStack = 1;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (!originPlayer.taintedFlesh) originPlayer.tornStrengthBoost.Flat += 0.1f;
			originPlayer.taintedFlesh = true;
		}
	}
}
