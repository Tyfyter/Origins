using Origins.Dev;
using Origins.Items.Armor;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Vanity.Other {
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
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateEquip(Player player) {
			player.OriginPlayer().felnumEnemiesFriendly = true;
		}
	}
}
