using Origins.Dev;
using Origins.Items.Armor;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Vanity.Other.Sets.PEE {
	[AutoloadEquip(EquipType.Head)]
	public class PEE_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public override void SetDefaults() {
			Item.value = Item.sellPrice(copper: 60);
			Item.rare = ItemRarityID.Blue;
			Item.vanity = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<Rubber>(), 10)
			.AddIngredient(ModContent.ItemType<Silicon_Bar>())
			.AddTile(TileID.Loom)
			.Register();
		}
		public string ArmorSetName => "PEE_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<PEE_Breastplate>();
		public int LegsItemID => ModContent.ItemType<PEE_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class PEE_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetDefaults() {
			Item.value = Item.sellPrice(silver: 1, copper: 50);
			Item.rare = ItemRarityID.Blue;
			Item.vanity = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<Rubber>(), 25)
			.AddIngredient(ModContent.ItemType<Silicon_Bar>())
			.AddTile(TileID.Loom)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class PEE_Greaves : ModItem, INoSeperateWikiPage {
		public override void SetDefaults() {
			Item.value = Item.sellPrice(copper: 90);
			Item.rare = ItemRarityID.Blue;
			Item.vanity = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<Rubber>(), 15)
			.AddIngredient(ModContent.ItemType<Silicon_Bar>())
			.AddTile(TileID.Loom)
			.Register();
		}
	}
}