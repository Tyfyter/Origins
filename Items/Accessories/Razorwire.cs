using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Razorwire : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 28);
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 1);
			Item.shoot = ProjectileID.BulletHighVelocity;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.razorwire = true;
			originPlayer.razorwireItem = Item;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.Shackle)
			.AddIngredient(ModContent.ItemType<Return_To_Sender>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
