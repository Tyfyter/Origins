using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Exo_Suit : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Movement,
			WikiCategories.Combat,
			WikiCategories.ExplosiveBoostAcc
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 32);
			Item.handOnSlot = Exo_Arm.HandsOnID;
			Item.shoeSlot = Exo_Legs.ShoeID;
			Item.backSlot = Exo_Weapon_Mount.BackID;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer originPlayer = player.OriginPlayer();
			Max(ref originPlayer.exoArmMult, 1);
			ModContent.GetInstance<Exo_Legs>().UpdateAccessory(player, hideVisual);
			Exo_Weapon_Mount.ProcessEffect(player, 1);
		}
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<Exo_Arm>()
			.AddIngredient<Exo_Legs>()
			.AddIngredient<Exo_Weapon_Mount>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}
}
