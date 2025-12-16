using FullSerializer.Internal;
using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
	public class Living_Bile : OriginTile, ICustomCanPlaceTile {

		public override void SetStaticDefaults() {
			Main.tileLighted[Type] = true;
			AddMapEntry(new(45, 40, 50));
			DustType = DustID.Wraith;
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			offsetY = 2;
			tileFrameY = (short)(tileFrameY + Main.tileFrame[Type] * 90);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.1f;
			g = 0.05f;
			b = 0.15f;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			frame = Main.tileFrame[TileID.LivingCursedFire];
		}
		public void CanPlace(Player self, Tile targetTile, Item sItem, ref int tileToCreate, ref int previewPlaceStyle, ref bool? overrideCanPlace, ref int? forcedRandom) {
			static bool IsValidDirection(int x, int y) => Main.tile[Player.tileTargetX + x, Player.tileTargetY + y].HasTile || Main.tile[Player.tileTargetX + x, Player.tileTargetY + y].WallType > WallID.None;
			if (IsValidDirection(1, 0) || IsValidDirection(-1, 0) || IsValidDirection(0, 1) || IsValidDirection(0, -1)) overrideCanPlace = true;
		}
	}
	public class Living_Bile_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Living_Bile>());
		}
		public override void AddRecipes() {
			CreateRecipe(20)
			.AddIngredient(ItemID.LivingFireBlock, 20)
			.AddIngredient<Black_Bile>()
			.AddTile(TileID.CrystalBall)
			.Register();
		}
	}
}
