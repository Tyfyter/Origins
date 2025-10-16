using Origins.Dev;
using Origins.Tiles.Riven;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Marrowick {
	[AutoloadEquip(EquipType.Head)]
	public class Marrowick_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => [
            WikiCategories.ArmorSet
        ];
        public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
		}
		public override void SetDefaults() {
			Item.defense = 1;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Marrowick_Breastplate>() && legs.type == ModContent.ItemType<Marrowick_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.statDefense += 1;
			player.setBonus = Language.GetTextValue("ArmorSetBonus.Wood");
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Marrowick_Item>(), 20)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
		public string ArmorSetName => "Marrowick_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Marrowick_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Marrowick_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Marrowick_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetDefaults() {
			Item.defense = 2;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Marrowick_Item>(), 30)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Marrowick_Greaves : ModItem, INoSeperateWikiPage {
		public override void SetDefaults() {
			Item.defense = 1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Marrowick_Item>(), 25)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
