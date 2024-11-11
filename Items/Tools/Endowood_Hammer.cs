using Origins.Dev;
using Origins.Tiles.Defiled;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Endowood_Hammer : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Tool"
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShadewoodHammer);
			Item.damage = 8;
			Item.DamageType = DamageClass.Melee;
			Item.hammer = 40;
			Item.value = Item.sellPrice(copper: 10);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Endowood_Item>(), 8)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
