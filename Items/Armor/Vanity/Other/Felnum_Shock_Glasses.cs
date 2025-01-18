using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.Other {
	[AutoloadEquip(EquipType.Head)]
	public class Felnum_Shock_Glasses : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public string ArmorSetName => Name;
		public int HeadItemID => Type;
		public int BodyItemID => ItemID.None;
		public int LegsItemID => ItemID.None;
		public bool HasFemaleVersion => false;
		public override void SetStaticDefaults() {
			Origins.AddHelmetGlowmask(this, "");
			ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Orange;
			Item.vanity = true;
		}
	}
}
