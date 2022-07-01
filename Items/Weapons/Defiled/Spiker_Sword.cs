using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Origins.Items.Materials;

namespace Origins.Items.Weapons.Defiled {
	public class Spiker_Sword : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Spiker Sword");
			Tooltip.SetDefault("Very pointy");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
			Item.damage = 30;
			Item.DamageType = DamageClass.Melee;
			Item.width = 42;
			Item.height = 50;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 7.5f;
			Item.value = 5000;
            Item.useTurn = true;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 6);
            recipe.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 3);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

			recipe = Recipe.Create(ItemID.NightsEdge);
			recipe.AddIngredient(this);
			recipe.AddIngredient(ItemID.Muramasa);
			recipe.AddIngredient(ItemID.BladeofGrass);
			recipe.AddIngredient(ItemID.FieryGreatsword);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
		}
	}
}
