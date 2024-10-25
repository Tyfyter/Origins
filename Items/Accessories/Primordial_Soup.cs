using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Primordial_Soup : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Vitality"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.PanicNecklace] = ModContent.ItemType<Primordial_Soup>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Primordial_Soup>()] = ItemID.PanicNecklace;
			glowmask = Origins.AddGlowMask(this);
		}
		static short glowmask;
		public override void SetDefaults() {
			Item.DefaultToAccessory(38, 20);
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.glowMask = glowmask;
			if (OriginsModIntegrations.CheckAprilFools()) Item.DefaultToFood(Item.width, Item.height, BuffID.Suffocation, 60 * 60, true);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.PanicNecklace)
			.AddIngredient(ItemID.ThornsPotion)
			.AddTile(TileID.TinkerersWorkbench)
			.DisableDecraft()
			.Register();

			Recipe.Create(ItemID.BandofStarpower)
			.AddIngredient(ItemID.ManaCrystal)
			.AddIngredient(Type)
			.AddTile(TileID.TinkerersWorkbench)
			.DisableDecraft()
			.Register();
		}
		public override void UpdateAccessory(Player player, bool isHidden) {
			player.GetModPlayer<OriginPlayer>().primordialSoup = true;
		}
	}
}
