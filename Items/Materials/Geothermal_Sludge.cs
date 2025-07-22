using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Materials {
	public class Geothermal_Sludge : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ExtractinatorMode[Type] = Type;
			Item.ResearchUnlockCount = 20;
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.maxStack = Item.CommonMaxStack;
			Item.rare = ItemRarityID.Gray;
			Item.autoReuse = true;
			Item.consumable = true;
		}
		public override void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack) {
			resultStack = 1;
			if (Main.rand.NextBool(10)) {
				resultType = ModContent.ItemType<Chambersite_Item>();
				if (Main.rand.NextBool(10)) resultStack += Main.rand.Next(0, 2);
				if (Main.rand.NextBool(15)) resultStack += Main.rand.Next(0, 3);
				if (Main.rand.NextBool(20)) resultStack += Main.rand.Next(0, 4);
				if (Main.rand.NextBool(25)) resultStack += Main.rand.Next(0, 5);
				if (Main.rand.NextBool(30)) resultStack += Main.rand.Next(0, 6);
			} else if (Main.rand.NextBool(3)) {
				if (Main.rand.NextBool(40)) {
					resultType = ItemID.GoldCoin;
					for (int i = 0; i < 4; i++) {
						if (Main.rand.NextBool(5)) resultStack += 1;
					}
					if (Main.rand.NextBool(5)) resultStack += Main.rand.Next(5, 25);
				} else {
					resultType = ItemID.SilverCoin;
					for (int i = 0; i < 4; i++) {
						if (Main.rand.NextBool(3)) resultStack += Main.rand.Next(1, 26);
					}
					if (Main.rand.NextBool(3)) resultStack += Main.rand.Next(1, 25);
				}
			} else {
				resultType = Main.rand.NextFromList(
					ItemID.CobaltOre,
					ItemID.PalladiumOre,
					ItemID.MythrilOre,
					ItemID.OrichalcumOre,
					ItemID.AdamantiteOre,
					ItemID.TitaniumOre
				); // TODO: Alt Ore support
				if (Main.rand.NextBool(20)) resultStack += Main.rand.Next(0, 2);
				if (Main.rand.NextBool(30)) resultStack += Main.rand.Next(0, 3);
				if (Main.rand.NextBool(40)) resultStack += Main.rand.Next(0, 4);
				if (Main.rand.NextBool(50)) resultStack += Main.rand.Next(0, 5);
				if (Main.rand.NextBool(60)) resultStack += Main.rand.Next(0, 6);
			}
		}
	}
}
