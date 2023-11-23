using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[LegacyName("True_Bomb_Yeeter")]
	public class Pneumatic_Bomb_Thrower : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat",
			"Explosive"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 20);
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().bombHandlingDevice = true;
			player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed += 0.5f;
		}

        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Air_Tank>());
            recipe.AddIngredient(ModContent.ItemType<Bomb_Handling_Device>());
			recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}
