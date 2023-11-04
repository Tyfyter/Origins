using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Shoes)]
	public class Lovers_Leap : ModItem {
		
		public override void SetDefaults() {
			Item.DefaultToAccessory(16, 24);
			Item.damage = 30;
			Item.knockBack = 2;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
			Item.accessory = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.SpectreBoots);
			recipe.AddIngredient(ModContent.ItemType<Locket_Necklace>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			player.accRunSpeed = 6f;
			player.rocketBoots = player.vanityRocketBoots = 2;
			originPlayer.guardedHeart = true;
			originPlayer.loversLeap = true;
			originPlayer.loversLeapItem = Item;
		}
	}
}
