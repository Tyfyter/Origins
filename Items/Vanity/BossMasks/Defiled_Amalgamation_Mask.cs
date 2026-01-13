using Origins.Dev;
using Origins.Items.Armor;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Vanity.BossMasks {
	[AutoloadEquip(EquipType.Head)]
	public class Defiled_Amalgamation_Mask : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public static short GlowMask = -1;
		public override LocalizedText Tooltip => LocalizedText.Empty;
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) GlowMask = Origins.AddGlowMask(Texture + "_Head_Glow");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Blue;
			Item.vanity = true;
			Item.value = Item.sellPrice(silver: 75);
		}
		public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
			glowMask = GlowMask;
			glowMaskColor = Color.White;
		}
		public string ArmorSetName => Name;
		public int HeadItemID => Type;
		public int BodyItemID => ItemID.None;
		public int LegsItemID => ItemID.None;
		public bool HasFemaleVersion => false;
	}
}
