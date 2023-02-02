using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Neck)]
	public class Volatile_Charm : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Volatile Charm");
			Tooltip.SetDefault("Increases explosive blast radius by 40% and reduces explosive self-damage by 20%");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaultsKeepSlots(ItemID.YoYoGlove);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.explosiveSelfDamage -= 0.2f;
			originPlayer.explosiveBlastRadius *= 1.4f;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<Bomb_Charm>());
			recipe.AddIngredient(ModContent.ItemType<Nitro_Crate>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
