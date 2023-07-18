using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Destructive_Claws : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Destructive Claws");
			// Tooltip.SetDefault("25% increased explosive throwing velocity\nIncreases attack speed of thrown explosives\nEnables autouse for all explosive weapons");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(38, 20);
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.FeralClaws);
			recipe.AddIngredient(ModContent.ItemType<Bomb_Yeeter>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			player.GetAttackSpeed(DamageClasses.Explosive) += 0.1f;
			originPlayer.destructiveClaws = true;
			originPlayer.explosiveThrowSpeed += 0.25f;
		}
	}
}
