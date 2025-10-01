using Origins.Items.Other.Testing;
using Origins.Tiles.Defiled;
using Origins.World.BiomeData;
using PegasusLib;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Ashen_Altar : ModTile, IComplexMineDamageTile {
		public override string Texture => typeof(Defiled_Altar).GetDefaultTMLName();
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileHammer[Type] = true;
			Main.tileLighted[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.CoordinateHeights = [16, 18];
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(255, 200, 200), CreateMapEntryName());
			RegisterItemDrop(-1);
			AdjTiles = [TileID.DemonAltar];
			ID = Type;
			DustType = Ashen_Biome.DefaultTileDust;
		}

		public void MinePower(int i, int j, int minePower, ref int damage) {
			Player player = Main.LocalPlayer;
			if (!Main.hardMode || player.HeldItem.hammer < 80) {
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
	public class Ashen_Altar_Item : TestingItem {
		public override string Texture => "Origins/Tiles/Defiled/Defiled_Altar";
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
			Item.createTile = ModContent.TileType<Ashen_Altar>();
		}
	}
}
