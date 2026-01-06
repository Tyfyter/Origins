using Origins.Dev;
using Origins.Tiles.Ashen;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Artifiber {
	[AutoloadEquip(EquipType.Head)]
	public class Artifiber_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 1;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Artifiber_Breastplate>() && legs.type == ModContent.ItemType<Artifiber_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.statDefense += 1;
			player.setBonus = Language.GetTextValue("ArmorSetBonus.Wood");
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Artifiber_Item>(), 20)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
		public string ArmorSetName => "Artifiber_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Artifiber_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Artifiber_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Artifiber_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 2;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Artifiber_Item>(), 30)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Artifiber_Greaves : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Artifiber_Item>(), 25)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
