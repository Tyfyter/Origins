using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Front, EquipType.Back)]
	public class Druidic_Bee_Cloak : ModItem {
		public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
				Mod.GetLocalization($"Items.{nameof(Hive_Jacket)}.Tooltip"),
				Mod.GetLocalization($"Items.{nameof(Akaliegis)}.Tooltip"),
				Language.GetOrRegister($"ItemTooltip.PygmyNecklace")
			);
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 26);
			Item.damage = 78;
			Item.knockBack = 4f;
			Item.defense = 6;
			Item.shoot = ModContent.ProjectileType<Hive_Jacket_Explosion>();
			Item.rare = ItemRarityID.Lime;
			Item.value = Item.sellPrice(gold: 5);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Hive_Jacket>()
			.AddIngredient<Tiki_Buckler>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override bool WeaponPrefix() => true;
		public override void UpdateEquip(Player player) {
			OriginPlayer oP = player.OriginPlayer();
			player.endurance += (1 - player.endurance) * 0.05f;
			player.maxMinions += 1;
			oP.flakJacketItem = Item;
			oP.hiveJacket = true;
			oP.akaliegis = true;
			Akaliegis.DoEnduranceBuff(player);
		}
	}
}
