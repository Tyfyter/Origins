using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Helium_Tank : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Misc"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			player.buffImmune[BuffID.Suffocation] = true;
			player.breathMax += 257;
			player.GetModPlayer<OriginPlayer>().heliumTank = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.WhoopieCushion);
			recipe.AddIngredient(ModContent.ItemType<Air_Tank>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
