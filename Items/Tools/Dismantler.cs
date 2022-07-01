using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Tools {
	public class Dismantler : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Dismantler");
			Tooltip.SetDefault("Very pointy\nAble to mine Hellstone");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.DeathbringerPickaxe);
			Item.damage = 14;
			Item.DamageType = DamageClass.Melee;
            Item.pick = 75;
			Item.width = 34;
			Item.height = 32;
			Item.useTime = 13;
			Item.useAnimation = 22;
			Item.knockBack = 4f;
			Item.value = 3600;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 12);
			recipe.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 6);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
