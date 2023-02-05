using Origins.Journal;
using Origins.Tiles.Other;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Stone_Mask : ModItem, IJournalEntryItem {
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Whispers";
		public string EntryName => "Origins/" + typeof(Stone_Mask_Entry).Name;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Stone Mask");
			Tooltip.SetDefault("Increases defense by 8, but your movement is hindered");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(14, 22);
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.Blue;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = Item.useAnimation = 12;
			Item.createTile = ModContent.TileType<Stone_Mask_Tile>();
			Item.consumable = true;
		}
		public override void UpdateEquip(Player player) {
			player.statDefense += 8;
			player.moveSpeed *= 0.9f;
			player.jumpSpeedBoost -= 1.8f;
		}
	}
	public class Stone_Mask_Entry : JournalEntry {
		public override string TextKey => "Stone_Mask";
		public override ArmorShaderData TextShader => null;
	}
}
