using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Tools {
	public class Brick_Breaker : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Brick Breaker");
			Tooltip.SetDefault("Very pointy");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.DeathbringerPickaxe);
			Item.damage = 14;
			Item.DamageType = DamageClass.Melee;
            Item.pick = 0;
            Item.hammer = 60;
			Item.width = 34;
			Item.height = 34;
			Item.useTime = 17;
			Item.useAnimation = 27;
			Item.knockBack = 4.3f;
			Item.value = 3600;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 10);
			recipe.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
