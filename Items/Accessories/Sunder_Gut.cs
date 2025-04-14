using Origins.Buffs;
using Origins.Dev;
using Origins.Journal;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Sunder_Gut : ModItem, ICustomWikiStat, IJournalEntrySource<Sunder_Gut_Entry> {
		public string[] Categories => [
			"Combat"
		];
		public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Mildew_Heart)}.Tooltip"),
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Lousy_Liver)}.Tooltip")
			);
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 22);
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Mildew_Heart>()
			.AddIngredient<Lousy_Liver>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.mildewHeart = true;
			if (player.statLife > 0) {
				originPlayer.lousyLiverCount = 4;
				originPlayer.lousyLiverDebuffs.Add((Lousy_Liver_Debuff.ID, 10));
			} else {
				originPlayer.lousyLiverCount = 8;
				originPlayer.lousyLiverDebuffs.Add((Rasterized_Debuff.ID, 20));
				originPlayer.lousyLiverRange = 512;
			}
		}
	}
	public class Sunder_Gut_Entry : JournalEntry {
		public override JournalSortIndex SortIndex => new("Lost_Crone", 2);
	}
}
