using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Balloon)]
	public class Blood_Donor_Bag : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Misc
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(24, 24);
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 35);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer oP = player.OriginPlayer();
			player.lifeRegen += 1;
			player.pStone = true;
			oP.medicalMask = true;
			oP.plasmaPhial = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.PhilosophersStone)
			.AddIngredient<Infusion_Bag>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
