using Origins.Dev;
using Origins.Items.Armor;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Vanity.BossMasks {
	[AutoloadEquip(EquipType.Head)]
	public class Trenchmaker_Mask : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public override LocalizedText Tooltip => LocalizedText.Empty;
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Blue;
			Item.vanity = true;
		}
		public string ArmorSetName => Name;
		public int HeadItemID => Type;
		public int BodyItemID => ItemID.None;
		public int LegsItemID => ItemID.None;
		public bool HasFemaleVersion => false;
	}
}
