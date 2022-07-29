using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Origins.Items.Materials;

namespace Origins.Items.Tools {
	public class Acrid_Pickaxe : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Pickaxe");
			Tooltip.SetDefault("Non-corrosive");
			glowmask = Origins.AddGlowMask(this);
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.TitaniumPickaxe);
			Item.damage = 28;
			Item.DamageType = DamageClass.Melee;
            Item.pick = 195;
			Item.width = 34;
			Item.height = 32;
			Item.useTime = 7;
			Item.useAnimation = 22;
			Item.knockBack = 4f;
			Item.value = 3600;
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item1;
			Item.glowMask = glowmask;
		}
        public override float UseTimeMultiplier(Player player) {
            return player.wet?1.5f:1;
        }
		public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Acrid_Bar>(), 15);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
	}
}
