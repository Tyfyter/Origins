using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Exploder_Emblem : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Exploder Emblem");
			Tooltip.SetDefault("+5% explosive damage");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WarriorEmblem);
			Item.accessory = true;
			Item.width = 28;
			Item.height = 28;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClasses.Explosive) += 0.05f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.AvengerEmblem);
			recipe.AddIngredient(Type);
			recipe.AddIngredient(ItemID.SoulofMight, 5);
			recipe.AddIngredient(ItemID.SoulofSight, 5);
			recipe.AddIngredient(ItemID.SoulofFright, 5);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
