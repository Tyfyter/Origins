using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Rebreather_Gear : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Misc"
		];
		public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Rebreather)}.Tooltip"),
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Air_Tank)}.Tooltip")
			);
		public override void SetDefaults() {
			Item.DefaultToAccessory(24, 22);
			Item.rare = ItemRarityID.LightRed;
			Item.faceSlot = Rebreather.FaceSlot;
			Item.backSlot = Air_Tank.BackSlot;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			player.OriginPlayer().rebreather = true;
			player.buffImmune[BuffID.Suffocation] = true;
			player.AddMaxBreath(257);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Rebreather>()
			.AddIngredient<Air_Tank>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
