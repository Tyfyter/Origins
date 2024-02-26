using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
    public class Eternabrite : ModItem {
        public string[] Categories => new string[] {
            "UsesBookcase",
            "SpellBook"
        };
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Flamethrower);
			Item.damage = 28;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 14;
			Item.useAmmo = AmmoID.None;
			Item.noUseGraphic = false;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.knockBack = 8;
			Item.shootSpeed = 14f;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item82;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Book);
			recipe.AddIngredient(ItemID.HellstoneBar, 12);
			recipe.AddIngredient(ItemID.WandofSparking);
			recipe.AddTile(TileID.Bookcases);
			recipe.Register();
		}
	}
}
