using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Bomb_Charm_of_Regeneration : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Vitality",
			"ExplosiveBoostAcc",
			"SelfDamageProtek"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 22);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Green;
		}
        public override void AddRecipes() {
            CreateRecipe()
            .AddIngredient(ItemID.BandofRegeneration)
            .AddIngredient(ModContent.ItemType<Bomb_Charm>())
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
        }
        public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.2f;
			player.lifeRegen += 1;
            player.GetModPlayer<OriginPlayer>().bombCharminIt = true;
        }
	}
}
