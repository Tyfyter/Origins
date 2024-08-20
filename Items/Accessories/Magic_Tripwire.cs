using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Magic_Tripwire : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"ExplosiveBoostAcc"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Trap_Charm>()] = ModContent.ItemType<Magic_Tripwire>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Magic_Tripwire>()] = ModContent.ItemType<Trap_Charm>();
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 34);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().magicTripwire = true;
		}
	}
}
