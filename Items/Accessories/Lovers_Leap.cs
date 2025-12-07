using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Shoes)]
	public class Lovers_Leap : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.GenericBoostAcc
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(16, 24);
			Item.damage = 50;
			Item.knockBack = 6;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
			Item.accessory = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SpectreBoots)
			.AddIngredient(ModContent.ItemType<Locket_Necklace>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) *= 1.05f;
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			player.accRunSpeed = 6f;
			player.rocketBoots = player.vanityRocketBoots = 2;
			originPlayer.guardedHeart = true;
			originPlayer.loversLeap = true;
			originPlayer.loversLeapItem = Item;
		}
	}
}
