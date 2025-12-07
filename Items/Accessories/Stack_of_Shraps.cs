using Origins.Dev;
using Origins.Journal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using AltLibrary.Common.AltBiomes;
using AltLibrary.Common.Systems;
using Terraria.Localization;
using Origins.World.BiomeData;

namespace Origins.Items.Accessories {
	public class Stack_of_Shraps : ModItem, ICustomWikiStat, IJournalEntrySource {
		public string[] Categories => [
			"Combat",
			"ExplosiveBoostAcc"
		];
		public string EntryName => "Origins/" + typeof(Stack_of_Shraps_Entry).Name;
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 26);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Blue;
			Item.master = true;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.madHand = true;
		}
		public override void AddRecipes() {
			AddShimmerRecipe<CorruptionAltBiome, Forbidden_Voice>();
			AddShimmerRecipe<CrimsonAltBiome, Weakpoint_Analyzer>();
			AddShimmerRecipe<Defiled_Wastelands_Alt_Biome, Mysterious_Spray>();
			AddShimmerRecipe<Riven_Hive_Alt_Biome, Amebic_Vial>();
		}
		public void AddShimmerRecipe<TBiome, TItem>() where TItem : ModItem where TBiome : AltBiome {
			AltBiome biome = ModContent.GetInstance<TBiome>();
			CreateRecipe()
			.AddIngredient<TItem>()
			.AddCondition(RecipeConditions.ShimmerTransmutation)
			.AddDecraftCondition(new Condition(LocalizedText.Empty, () => WorldBiomeManager.GetWorldEvil(true, true) == biome))
			.Register();

			Recipe.Create(ModContent.ItemType<TItem>())
			.AddIngredient(Type)
			.AddCondition(RecipeConditions.ShimmerTransmutation)
			.AddCondition(new Condition(Language.GetOrRegister("Mods.AltLibrary.Condition.Base").WithFormatArgs(biome.DisplayName), () => false))
			.Register();
		}
		
	}
	public class Stack_of_Shraps_Entry : JournalEntry {
		public override string TextKey => nameof(Stack_of_Shraps);
		public override JournalSortIndex SortIndex => new("The_Ashen", 1);
	}
}
