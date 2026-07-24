using Microsoft.Xna.Framework.Graphics;
using Origins.Liquids;
using Origins.Tiles.Ashen;
using Terraria;
using Terraria.ID;

namespace Origins.Items.Tools.Liquids {
	public class Oil_Bucket : BucketBase<Oil> {
		public override int GetLiquid(int x, int y) {
			Tile tile = Main.tile[x, y];
			if (tile.LiquidType == Burning_Oil.ID || y > Main.UnderworldLayer) return Burning_Oil.ID;
			return LiquidType;
		}
		public override void AddRecipes() {
			if (GetType() != typeof(Oil_Bucket)) return;
			CreateRecipe()
			.AddIngredient(ItemID.EmptyBucket)
			.AddTile<Oil_Derrick>()
			.Register();
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (Main.mouseItem == Item && Main.HoverItem?.ModItem is IOilableItem) Item_Lubrication.DrawOilOverlay(Item, spriteBatch, position);
		}
	}
	public class Oil_Bottomless_Bucket : Oil_Bucket {
		public override bool Endless => true;
	}
}
