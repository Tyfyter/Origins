using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Void_Lock : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.ExpendableTool
		];
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(0);
			Item.createTile = -1;
			Item.rare = ItemRarityID.Orange;
			Item.consumable = true;
			ContentSamples.CreativeHelper.GetItemGroup(Item, out _);
		}
		public override bool? UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
				if (!Main.tileContainer[tile.TileType]) return false;
				int left = Player.tileTargetX;
				int top = Player.tileTargetY;
				if (tile.TileFrameX % 36 != 0) {
					left--;
				}
				if (tile.TileFrameY != 0) {
					top--;
				}
				return ModContent.GetInstance<OriginSystem>().TryAddVoidLock(new(left, top), player.GetModPlayer<OriginPlayer>().guid);
			}
			return false;
		}
		public override bool CanUseItem(Player player) {
			return base.CanUseItem(player);
		}
		public override void AddRecipes() {
			CreateRecipe(3)
			.AddIngredient(ItemID.Bone, 7)
			.AddIngredient(ItemID.ChestLock, 3)
			.AddIngredient(ItemID.JungleSpores, 5)
			.AddRecipeGroupWithItem(OriginSystem.ShadowScaleRecipeGroupID, showItem: ItemID.ShadowScale, 10)
			.AddTile(TileID.DemonAltar)
			.Register();
		}
	}
}