using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Neck)]
	public class Dryads_Inheritance : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Dryad's Inheritance");
			Tooltip.SetDefault("Increases damage, movement speed, and length of invincibility after taking damage\nEmit a damaging aura that inflicts poison");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(26, 26);
			Item.neckSlot = ArmorIDs.Neck.StarVeil;
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Lime;
			Item.accessory = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.SporeSac);
			recipe.AddIngredient(ModContent.ItemType<Last_Descendent>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
		public override void UpdateEquip(Player player) {
			player.longInvince = true;
			player.GetModPlayer<OriginPlayer>().guardedHeart = true;
			//player.GetModPlayer<OriginPlayer>().dryadNecklace = true;
		}
	}
}
