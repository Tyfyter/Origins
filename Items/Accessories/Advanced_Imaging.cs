using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Advanced_Imaging : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat"
		};
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Yellow;
			Item.glowMask = glowmask;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().advancedImaging = true;
			player.buffImmune[BuffID.Confused] = true;
		}
	}
}
