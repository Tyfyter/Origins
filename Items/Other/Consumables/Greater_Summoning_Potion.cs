using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Accessories;
using Origins.Items.Other.Fish;
using Origins.Tiles.Brine;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Greater_Summoning_Potion : ModItem {
		public string[] Categories => [
			"Potion"
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Greater_Summoning_Buff.ID;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient(ModContent.ItemType<Toadfish>())
			.AddIngredient(ItemID.Moonglow)
			.AddTile(TileID.Bottles)
			.Register();
		}
		public static AutoLoadingAsset<Texture2D> normalTexture = typeof(Greater_Summoning_Potion).GetDefaultTMLName();
		public static AutoLoadingAsset<Texture2D> afTexture = typeof(Greater_Summoning_Potion).GetDefaultTMLName() + "_AF";
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (OriginsModIntegrations.CheckAprilFools()) {
				TextureAssets.Item[Type] = afTexture;
			} else {
				TextureAssets.Item[Type] = normalTexture;
			}
		}
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
			if (OriginsModIntegrations.CheckAprilFools()) {
				TextureAssets.Item[Type] = afTexture;
			} else {
				TextureAssets.Item[Type] = normalTexture;
			}
		}
	}
}
