using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Brick_Breaker : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Tool"
		];
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
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 10)
			.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
