using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Neck)]
	public class Last_Descendant : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"GenericBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(26, 26);
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightPurple;
			Item.accessory = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.StarVeil)
			.AddIngredient(ModContent.ItemType<Locket_Necklace>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) *= 1.05f;
			player.GetModPlayer<OriginPlayer>().guardedHeart = true;
            player.longInvince = true;
			player.starCloakItem = Item;
			player.starCloakItem_starVeilOverrideItem = Item;
		}
	}
}
