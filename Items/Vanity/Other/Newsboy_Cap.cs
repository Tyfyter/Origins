using Origins.Dev;
using Origins.Items.Armor;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Vanity.Other {
	[AutoloadEquip(EquipType.Head)]
	public class Newsboy_Cap : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public string ArmorSetName => Name;
		public int HeadItemID => Type;
		public int BodyItemID => ItemID.None;
		public int LegsItemID => ItemID.None;
		public bool HasFemaleVersion => false;
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Green;
			Item.vanity = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.Silk, 20)
			.AddTile(TileID.Loom)
			.Register();
		}
	}
}
