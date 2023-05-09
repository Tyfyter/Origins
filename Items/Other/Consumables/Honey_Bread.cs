using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Honey_Bread : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Honey Wheat Bread");
			Tooltip.SetDefault("{$CommonItemTooltip.MajorStats}\n'Let's get this bread!'");
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MonsterLasagna);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.buffType = Honey_Bread_Buff.ID;
			Item.buffTime = 60 * 60 * 15;
			Item.value = Item.sellPrice(silver: 10);
			Item.rare = ItemRarityID.White;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ModContent.ItemType<Honey_Bread>());
			recipe.AddIngredient(ItemID.BottledHoney);
			recipe.AddIngredient(this);
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();
		}
	}
	public class Honey_Bread_Buff : ModBuff {
		public override string Texture => "Origins/Buffs/Food/Honey_Bread_Buff";
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Honey Wheat Bread");
			Description.SetDefault("Mmmmm...");
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.AddBuff(BuffID.WellFed3, 60);
			player.AddBuff(BuffID.Honey, 60);
		}
	}
}
