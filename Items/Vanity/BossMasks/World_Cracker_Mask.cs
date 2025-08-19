using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Armor;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Vanity.BossMasks {
	[AutoloadEquip(EquipType.Head)]
    public class World_Cracker_Mask : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public static short GlowMask = -1;
		public static int armoredHeadID = -1;
		public override LocalizedText Tooltip => LocalizedText.Empty;
		public override void SetStaticDefaults() {
            if (Main.netMode != NetmodeID.Server) 				GlowMask = Origins.AddGlowMask(Texture + "_Head_Glow");
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults() {
            Item.rare = ItemRarityID.Blue;
            Item.vanity = true;
        }
		public override void EquipFrameEffects(Player player, EquipType type) {
			if (player.statLife > player.statLifeMax2 * 0.5f) player.head = armoredHeadID;
		}
		public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
			glowMask = GlowMask;
			glowMaskColor = new Color((color.R + 255) / 510f, (color.G + 255) / 510f, (color.B + 255) / 510f, 0.5f);
		}
		public string ArmorSetName => Name;
		public int HeadItemID => Type;
		public int BodyItemID => ItemID.None;
		public int LegsItemID => ItemID.None;
		public bool HasFemaleVersion => false;
	}
	[AutoloadEquip(EquipType.Head)]
	public class World_Cracker_Mask2 : ModItem, INoSeperateWikiPage {
		public static short GlowMask = -1;
		public override LocalizedText DisplayName => LocalizedText.Empty;
		public override LocalizedText Tooltip => LocalizedText.Empty;
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) 				GlowMask = Origins.AddGlowMask(base.Texture + "_Head_Glow");
			Item.ResearchUnlockCount = 0;
			ItemID.Sets.Deprecated[Type] = true;
			World_Cracker_Mask.armoredHeadID = Item.headSlot;
		}
		public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
			glowMask = GlowMask;
			glowMaskColor = new Color((color.R + 255) / 510f, (color.G + 255) / 510f, (color.B + 255) / 510f, 0.5f);
		}
	}
}
