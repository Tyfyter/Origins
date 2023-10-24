using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.Other {
	[AutoloadEquip(EquipType.Legs)]
	public class Sunflower_Sandal : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public string ArmorSetName => "Sunflower_Sandal";
		public int HeadItemID => Type;
		public int BodyItemID => ItemID.None;
		public int LegsItemID => ItemID.None;
		public bool HasFemaleVersion => false;
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Green;
		}
	}
}
