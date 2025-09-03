using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Speed_Booster : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"Movement",
			"RangedBoostAcc",
			"GenericBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(16, 24);
			Item.damage = 48;
			Item.knockBack = 3;
			Item.value = Item.sellPrice(gold: 14);
			Item.rare = ItemRarityID.Yellow;
			Item.accessory = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Automated_Returns_Handler>()
			.AddIngredient<Lovers_Leap>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetDamage(DamageClass.Generic) *= 1.05f;
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			player.hasMagiluminescence = true;
			if (player.accRunSpeed < 6f) player.accRunSpeed = 6f;
			if (originPlayer.shineSparkCharge > 0) {
				player.accRunSpeed += 3f;
			}
			player.rocketBoots = 2;
			player.vanityRocketBoots = 2;
			originPlayer.guardedHeart = true;
			originPlayer.loversLeap = true;
			originPlayer.loversLeapItem = Item;
			originPlayer.shineSpark = true;
			originPlayer.shineSparkItem = Item;
			originPlayer.shineSparkVisible = !hideVisual;
			originPlayer.turboReel2 = true;
			originPlayer.automatedReturnsHandler = true;

			player.blackBelt = true;
			player.dashType = 1;
			player.spikedBoots += 2;
		}
	}
}
