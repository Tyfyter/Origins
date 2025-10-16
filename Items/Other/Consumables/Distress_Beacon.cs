using Origins.Dev;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	[LegacyName("Broken_Record")]
	public class Distress_Beacon : ModItem {
		public string[] Categories => [
			WikiCategories.BossSummon
		];
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CultistBossBag);
			Item.maxStack = Item.CommonMaxStack;
			Item.rare = ItemRarityID.Blue;
			Item.expert = false;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Biocomponent10>(), 15)
			.AddIngredient(ModContent.ItemType<Ash_Urn>(), 30)
			.AddTile(TileID.DemonAltar)
			.Register();
		}
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			itemLoot.Add(Ashen_Biome.OrbDropRule);
		}
		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.GoodieBags;
		}
		public override bool CanRightClick() => true;
	}
}
