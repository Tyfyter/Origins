using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Celestial_Starlight : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Vitality",
			"MagicBoostAcc"
		];
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 30);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 3, silver: 30);
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.CelestialMagnet)
			.AddIngredient(ModContent.ItemType<Dim_Starlight>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.dimStarlight = true;
			player.manaMagnet = true;

			float light = 0.23f + (originPlayer.dimStarlightCooldown / 1000f);
			Lighting.AddLight(player.Center, 0.3f, 0.3f, 1f);
		}
	}
}
