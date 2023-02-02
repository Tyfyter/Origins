using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class No_U_Card : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("No U Card");
			Tooltip.SetDefault("Attackers recieve 50% of incoming damage and debuffs regardless of immunity\nImmune to most debuffs");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaultsKeepSlots(ItemID.YoYoGlove);
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightPurple;
		}
		public override void UpdateEquip(Player player) {
			player.thorns += 0.5f;
			player.GetModPlayer<OriginPlayer>().noU = true;
			for (int i = 0; i < Player.MaxBuffs; i++) {
				switch (player.buffType[i]) {
					case BuffID.Weak:
					case BuffID.BrokenArmor:
					case BuffID.Bleeding:
					case BuffID.Poisoned:
					case BuffID.Slow:
					case BuffID.Confused:
					case BuffID.Silenced:
					case BuffID.Cursed:
					case BuffID.Darkness:
					player.DelBuff(i--);
					break;
				}
			}
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.AnkhCharm);
			recipe.AddIngredient(ModContent.ItemType<Return_To_Sender>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
