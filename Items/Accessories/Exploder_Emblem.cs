using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Exploder_Emblem : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"ExplosiveBoostAcc"
		];
		public override void SetStaticDefaults() {
			OriginExtensions.InsertIntoShimmerCycle(Type, ItemID.SummonerEmblem);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WarriorEmblem);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClasses.Explosive) += 0.15f;
			player.GetArmorPenetration(DamageClasses.Explosive) -= 3;
		}
		public override void AddRecipes() {
			Recipe.Create(ItemID.AvengerEmblem)
            .AddIngredient(ItemID.SoulofFright, 5)
            .AddIngredient(ItemID.SoulofMight, 5)
			.AddIngredient(ItemID.SoulofSight, 5)
            .AddIngredient(Type)
            .AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
