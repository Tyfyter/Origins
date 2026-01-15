using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Items.Armor.Laborer {
	[AutoloadEquip(EquipType.Head)]
	public class Laborer_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public string[] SetCategories => [
			WikiCategories.ArmorSet,
			WikiCategories.ExplosiveBoostGear
		];
		public override void SetDefaults() {
			Item.defense = 2;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 45);
		}
		public override void UpdateEquip(Player player) {
			Vector3 light = new(1.4f, 0.7f, 0.15f);
			Lighting.AddLight(player.Center, light * 0.9f);// adjust brightness here
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			if (body.type != ItemID.MiningShirt && body.type != ItemType<Laborer_Breastplate>()) return false;
			return legs.type == ItemID.MiningPants || legs.type == ItemType<Laborer_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("ArmorSetBonus.Mining");
			player.pickSpeed -= 0.1f;
		}
		public string ArmorSetName => "Laborer_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ItemType<Laborer_Breastplate>();
		public int LegsItemID => ItemType<Laborer_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Laborer_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetDefaults() {
			Item.defense = 2;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 75);
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed<Explosive>() += 0.08f;
			player.pickSpeed -= 0.1f;
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Laborer_Greaves : ModItem, INoSeperateWikiPage {
		public override void SetDefaults() {
			Item.defense = 2;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 60);
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed<Explosive>() += 0.08f;
			player.pickSpeed -= 0.1f;
		}
	}
}
