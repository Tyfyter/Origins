using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Air_Tank : ModItem {
		public static int BackSlot { get; private set; }
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Air Tank");
			// Tooltip.SetDefault("Extends underwater breathing\nImmunity to ‘Suffocation’");
			BackSlot = Item.backSlot;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			player.buffImmune[BuffID.Suffocation] = true;
			player.breathMax += 257;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.AdamantiteBar, 20);
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 12);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();

			recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.TitaniumBar, 20);
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 12);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
}
