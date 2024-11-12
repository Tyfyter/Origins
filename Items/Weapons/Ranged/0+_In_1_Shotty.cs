using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Ranged {
	public class Shotty_x2 : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Gun"
        ];
        public override string Texture => "Origins/Items/Weapons/Ranged/2_In_1_Shotty";
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Boomstick);
			Item.value = Item.sellPrice(gold: 4);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Boomstick, 2)
			.AddIngredient(ModContent.ItemType<Adhesive_Wrap>(), 6)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			for (int i = Main.rand.Next(5, 8); i-- > 0;) {
				Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.5f), type, damage, knockback, player.whoAmI);
			}
			return false;
		}
	}
	public class Shotty_x3 : ModItem {
		public override string Texture => "Origins/Items/Weapons/Ranged/3_In_1_Shotty";
		
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Boomstick);
			Item.value = Item.sellPrice(gold: 6);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Boomstick, 3)
			.AddIngredient(ModContent.ItemType<Adhesive_Wrap>(), 9)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
			Recipe.Create(Type)
			.AddIngredient(ItemID.Boomstick)
			.AddIngredient(ModContent.ItemType<Shotty_x2>())
			.AddIngredient(ModContent.ItemType<Adhesive_Wrap>(), 3)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			for (int i = Main.rand.Next(8, 12); i-- > 0;) {
				Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.5f), type, damage, knockback, player.whoAmI);
			}
			return false;
		}
	}
}
