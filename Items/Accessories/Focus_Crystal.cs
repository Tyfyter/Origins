using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.NPCs.Brine.Boss;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Balloon)]
	public class Focus_Crystal : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 30);
			Item.rare = ItemRarityID.Yellow;
			Item.expert = true;
			Item.value = Item.sellPrice(gold: 7);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
            .AddIngredient(ItemID.ShinyStone)
            .AddIngredient(ModContent.ItemType<Ruby_Reticle>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.rubyReticle = true;
			originPlayer.focusCrystal = true;
		}
		public static AutoLoadingAsset<Texture2D> normalTexture = typeof(Focus_Crystal).GetDefaultTMLName();
		public static AutoLoadingAsset<Texture2D> afTexture = typeof(Focus_Crystal).GetDefaultTMLName() + "_AF";
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
