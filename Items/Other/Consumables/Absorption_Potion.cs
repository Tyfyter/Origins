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
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			//Item.buffType = Absroption_Buff.ID;
		}
        public override bool CanUseItem(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage = 0;
			return true;
		}
		public override void AddRecipes() {
			Recipe recipe = Mod.CreateRecipe(Type);
			recipe.AddIngredient(ItemID.BottledWater);
			//recipe.AddIngredient(ModContent.ItemType<???>());
			//recipe.AddIngredient(ItemID.???);
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
}
