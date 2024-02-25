using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Petrified_Prickly_Pear : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.BloodOrange);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.buffType = BuffID.WellFed;
			Item.buffTime = 60 * 60 * 5;
		}
        /*public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.FruitJuice));
			recipe.AddIngredient(ItemID.Bottle);
			recipe.AddIngredient(this, 2); we need to create a new recipe group including origins' fruits
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();

			recipe = Recipe.Create(ItemID.FruitSalad));
			recipe.AddIngredient(ItemID.Bowl);
			recipe.AddIngredient(this, 3); we need to create a new recipe group including origins' fruits
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();
		}*/
        public override bool ConsumeItem(Player player) {
            player.AddBuff(Petrified_Prickly_Pear_Buff.ID, Item.buffTime);
            return true;
        }
    }
    public class Petrified_Prickly_Pear_Buff : ModBuff {
        public override string Texture => "Origins/Buffs/Food/Petrified_Prickly_Pear_Buff";
        public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            ID = Type;
        }
        public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<OriginPlayer>().manaShielding += 0.25f;
        }
    }
}
