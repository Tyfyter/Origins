using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
    public class Acrid_Hamaxe : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Hamaxe");
			Tooltip.SetDefault("Non-corrosive");
			glowmask = Origins.AddGlowMask(this);
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.TitaniumWaraxe);
			Item.damage = 31;
			Item.DamageType = DamageClass.Melee;
            Item.pick = 0;
            Item.hammer = 65;
            Item.axe = 22;
			Item.width = 34;
			Item.height = 34;
			Item.useTime = 7;
			Item.useAnimation = 22;
			Item.knockBack = 4f;
			Item.value = Item.buyPrice(gold: 20);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item1;
		}
        public override float UseTimeMultiplier(Player player) {
            return player.wet?1.5f:1;
        }
		public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Acrid_Bar>(), 20);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
	}
}
