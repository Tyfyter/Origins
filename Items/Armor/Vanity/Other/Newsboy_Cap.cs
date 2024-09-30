using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.Other {
	[AutoloadEquip(EquipType.Head)]
	public class Newsboy_Cap : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public string ArmorSetName => Name;
		public int HeadItemID => Type;
		public int BodyItemID => ItemID.None;
		public int LegsItemID => ItemID.None;
		public bool HasFemaleVersion => false;
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.Silk, 20)
			.AddTile(TileID.Loom)
			.Register();
		}
	}
}
