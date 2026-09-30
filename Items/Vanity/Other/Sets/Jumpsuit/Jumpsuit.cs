using Origins.Dev;
using Origins.Items.Armor;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Vanity.Other.Sets.Jumpsuit {
	[AutoloadEquip(EquipType.Body, EquipType.Legs)]
	public class Jumpsuit : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Blue;
			Item.vanity = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.Silk, 30)
			.AddIngredient(ModContent.ItemType<Rubber>(), 15)
			.AddTile(TileID.Loom)
			.Register();
		}
		public string ArmorSetName => "Jumpsuit_Armor";
		public int HeadItemID => ItemID.None;
		public int BodyItemID => Type;
		public int LegsItemID => Type;
	}
}