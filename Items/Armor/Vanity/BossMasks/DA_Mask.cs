using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.BossMasks {
	[AutoloadEquip(EquipType.Head)]
	public class DA_Mask : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public string ArmorSetName => Name;
		public int HeadItemID => Type;
		public int BodyItemID => ItemID.None;
		public int LegsItemID => ItemID.None;
		public bool HasFemaleVersion => false;
		public override LocalizedText Tooltip => LocalizedText.Empty;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("{$Defiled} Amalgamation Mask");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Vanity/BossMasks/DA_Mask_Head_Glow");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Blue;
			Item.vanity = true;
		}
	}
}
