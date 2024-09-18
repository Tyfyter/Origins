using Origins.Tiles.Cubekon;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Quantum_Injector : ModItem {
		public const int mana_per_use = 40;
		public const int max_uses = 5;
        public string[] Categories => [
            "PermaBoost"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 30;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ManaCrystal);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ButterscotchRarity.ID;
			Item.UseSound = SoundID.Item90;
		}
		public override bool? UseItem(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (originPlayer.quantumInjectors < max_uses) {
				originPlayer.quantumInjectors++;
				return true;
			}
			return false;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.ManaCrystal)
			.AddIngredient(ModContent.ItemType<Qube_Item>(), 50)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
