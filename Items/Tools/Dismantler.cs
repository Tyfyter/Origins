using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Dismantler : ModItem {
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		static short glowmask;
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
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 12)
			.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 6)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
