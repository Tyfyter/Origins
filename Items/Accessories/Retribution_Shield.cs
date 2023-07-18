using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Shield)]
	public class Retribution_Shield : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Shield of Retribution");
			// Tooltip.SetDefault("Grants immunity to knockback\nGrants immunity to fire blocks\n67% of damage recieved is redirected to 3 enemies\nEnemies are less likely to target you");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(36, 38);
			Item.defense = 3;
			Item.shoot = ProjectileID.BulletHighVelocity;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.aggro -= 125;
			player.noKnockback = true;
			player.fireWalk = true;
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.razorwire = true;
			originPlayer.razorwireItem = Item;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.ObsidianShield);
			recipe.AddIngredient(ModContent.ItemType<Razorwire>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
