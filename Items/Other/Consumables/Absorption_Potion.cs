using Origins.Buffs;
using Origins.Items.Other.Fish;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Absorption_Potion : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Absorption Potion");
			Tooltip.SetDefault("Fully protected from explosive self-damage");
			SacrificeTotal = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			//Item.buffType = Absorption_Buff.ID;
		}
		public override bool CanUseItem(Player player) {
			//↓this will only have an effect for a single tick↓, so it needs to be done in a buff's Update hook or an accessory's UpdateEquip hook
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage = 0;
			return true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.BottledWater);
			//recipe.AddIngredient(ModContent.ItemType<???>());
			//recipe.AddIngredient(ItemID.???);
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
}
