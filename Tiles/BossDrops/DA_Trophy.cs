using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.BossDrops {
	public class DA_Trophy : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.FramesOnKillWall[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(120, 85, 60), Language.GetText("MapObject.Trophy"));
			DustType = 7;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ModContent.ItemType<DA_Trophy_Item>());
		}
	}
	public class DA_Trophy_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Defiled Amalgamation Trophy");
		}

		public override void SetDefaults() {
			Item.width = 30;
			Item.height = 30;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 50000;
			Item.rare = ItemRarityID.Blue;
			Item.createTile = ModContent.TileType<DA_Trophy>();
		}
	}
}
