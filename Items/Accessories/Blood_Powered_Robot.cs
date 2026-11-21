using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Blood_Powered_Robot : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
			Mod.GetLocalization($"Items.{nameof(Haggard_Artery)}.Tooltip"),
			base.Tooltip);
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(58, 44);
			Item.damage = 42;
			Item.DamageType = DamageClasses.Explosive;
			Item.knockBack = 4;
			Item.useTime = 6;
			Item.useAnimation = 1;//used as the numerator for the chance
			Item.reuseDelay = 30;//used as the denominator for the chance
			Item.shoot = ModContent.ProjectileType<Explosive_Artery_P>();
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(gold: 3);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer oP = player.GetModPlayer<OriginPlayer>();
			oP.scrapCompactor = true;
			oP.explosiveArtery = true;
			oP.explosiveArteryItem = Item;
			oP.messyLeech = true;
			oP.bloodRobot = true;
		}
	}
}
