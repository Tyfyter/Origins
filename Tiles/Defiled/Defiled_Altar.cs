using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Defiled {
	public class Defiled_Altar : ModTile, IComplexMineDamageTile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileHammer[Type] = true;
			Main.tileLighted[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.CoordinateHeights = new[] { 18, 18 };
			TileObjectData.addTile(Type);
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("{$Defiled} Altar");
			AddMapEntry(new Color(200, 200, 200), name);
			//disableSmartCursor = true;
			RegisterItemDrop(-1);
			AdjTiles = new int[] { TileID.DemonAltar };
			ID = Type;
		}

		public void MinePower(int i, int j, int minePower, ref int damage) {
			Player player = Main.LocalPlayer;
			if (Main.hardMode && player.HeldItem.hammer >= 80) {
				damage = (int)(1.2f * minePower);
			} else {
				player.Hurt(PlayerDeathReason.ByOther(4), player.statLife / 2, -player.direction);
				damage = 0;
			}
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			WorldGen.SmashAltar(i, j);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = g = b = 0.5f;
		}
		public override bool CanExplode(int i, int j) => false;
	}
	public class Defiled_Altar_Item : ModItem {
		public override string Texture => "Origins/Tiles/Defiled/Defiled_Altar";
		public override void SetStaticDefaults() {
			ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
		}

		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 500;
			Item.createTile = ModContent.TileType<Defiled_Altar>();
		}
	}
}
