using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	//[AutoloadEquip(EquipType.Neck)] doesn't have neck spritesheet
	public class Hazard_Charm : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Misc",
			"Explosive"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 26);
			Item.value = Item.sellPrice(gold: 9, silver:50);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LavaCharm);
            recipe.AddIngredient(ModContent.ItemType<Bomb_Charm>());
			recipe.AddIngredient(ModContent.ItemType<Trap_Charm>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.explosiveSelfDamage -= 0.2f;
			originPlayer.trapCharm = true;
			player.lavaMax += 7 * 60;
		}
	}
}
