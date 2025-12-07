using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Cirrhosis_Abhorrence : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.RasterSource
		];
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(26, 22);
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 2);
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Lousy_Liver>())
			.AddIngredient(ModContent.ItemType<Messy_Magma_Leech>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.magmaLeech = true;
			originPlayer.lousyLiverCount = 4;
			originPlayer.lousyLiverDebuffs.Add((Lousy_Liver_Debuff.ID, 9));
			originPlayer.lousyLiverDebuffs.Add((BuffID.OnFire, 15));
			originPlayer.lousyLiverDebuffs.Add((BuffID.Bleeding, 15));
		}
	}
}
