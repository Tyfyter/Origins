using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Charm_Of_Naivety : ModItem, ICustomWikiStat {
		public bool? Hardmode => true;
		public string[] Categories => [
			WikiCategories.Vitality,
			WikiCategories.ExplosiveBoostAcc,
			WikiCategories.SelfDamageProtek
		];
		public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Mildew_Heart)}.Tooltip"),
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Bomb_Charm_of_Regeneration)}.Tooltip")
			);
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 22);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Green;
		}
        public override void AddRecipes() {
            CreateRecipe()
            .AddIngredient<Mildew_Heart>()
            .AddIngredient<Bomb_Charm_of_Regeneration>()
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
        }
        public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.explosiveSelfDamage -= 0.2f;
			player.lifeRegen += 1;
            originPlayer.bombCharminIt = true;
            originPlayer.bombCharminItStrength = originPlayer.mildewHealth > player.statLife ? 100 : 60;
            originPlayer.mildewHeart = true;
        }
	}
}
