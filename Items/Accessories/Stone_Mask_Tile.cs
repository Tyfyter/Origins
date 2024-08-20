using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Journal;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
    public class Stone_Mask : ModItem, IJournalEntryItem, ICustomWikiStat {
        public string[] Categories => [
            "Vitality",
			"LoreItem"
        ];
        public string IndicatorKey => "Mods.Origins.Journal.Indicator.Whispers";
        public string EntryName => "Origins/" + typeof(Stone_Mask_Entry).Name;

        public override void SetDefaults() {
            Item.DefaultToAccessory(14, 22);
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Blue;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = Item.useAnimation = 12;
            //Item.createTile = ModContent.TileType<Stone_Mask>();
            Item.consumable = true;
        }
        public override void UpdateEquip(Player player) {
            player.statDefense += 8;
            player.moveSpeed *= 0.9f;
            player.jumpSpeedBoost -= 1.8f;
        }
    }
	public class Stone_Mask_Tile : ModTile {
		public const int NextStyleHeight = 40; // Calculated by adding all CoordinateHeights + CoordinatePaddingFix.Y applied to all of them + 2

		public override void SetStaticDefaults() {
			// Properties
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;
			DustType = DustID.Stone;

			// Names
			AddMapEntry(new Color(100, 100, 100), CreateMapEntryName());

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
			TileObjectData.newTile.CoordinateHeights = [16, 16];
			TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.addTile(Type);
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 32, ModContent.ItemType<Stone_Mask>());
		}

		public override bool RightClick(int i, int j) {
			WorldGen.KillTile(i, j);
			return true;
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;

			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<Stone_Mask>();

			if (Main.tile[i, j].TileFrameX / 18 < 1) {
				player.cursorItemIconReversed = true;
			}
		}
	}
	public class Stone_Mask_Entry : JournalEntry {
		public override string TextKey => "Stone_Mask";
		public override ArmorShaderData TextShader => null;
	}
}
