using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Broken_Record : ModItem {
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CultistBossBag);
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = ItemRarityID.Blue;
			Item.expert = false;
		}
        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Ash_Urn>(), 30);
            recipe.AddIngredient(ModContent.ItemType<Biocomponent10>(), 15);
            recipe.AddTile(TileID.DemonAltar);
            recipe.Register();
        }
        public override void ModifyItemLoot(ItemLoot itemLoot) {
            //itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Orbccessory>(), 5));
            //itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Orbanger Weapon>(), 5));
            //itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Orblight Pet>(), 5));
            //itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Orbagic Weapon>(), 5));
            //itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Orblee Weapon>(), 5));
            //itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Orbummon Weapon>(), 5));
        }
        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.GoodieBags;
		}
		public override bool CanRightClick() => true;
	}
}
