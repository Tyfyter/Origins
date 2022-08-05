using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Tools {
	public class Rivenator : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("{$Riven}ator");
			Tooltip.SetDefault("Able to mine Hellstone");
			glowmask = Origins.AddGlowMask(this);
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.NightmarePickaxe);
			Item.damage = 16;
			Item.DamageType = DamageClass.Melee;
            Item.pick = 80;
			Item.width = 34;
			Item.height = 32;
			Item.useTime = 13;
			Item.useAnimation = 24;
			Item.knockBack = 4f;
			Item.value = 3600;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Infested_Bar>(), 12);
			recipe.AddIngredient(ModContent.ItemType<Riven_Sample>(), 6);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
