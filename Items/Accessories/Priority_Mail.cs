using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Priority_Mail : ModItem {
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 28);
			Item.accessory = true;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().priorityMail = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);// non-stackable items can't have their stacks split to make a stack of 2+ different from a stack of 1, and iirc accessories being stackable causes issues
			recipe.AddIngredient(ItemID.Book);
			recipe.AddIngredient(ItemID.FallenStar, 2);
			recipe.AddIngredient(ItemID.PaperAirplaneA, 4);
			recipe.AddIngredient(ModContent.ItemType<Asylum_Whistle>());
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
	public class Priority_Mail_Buff : ModBuff {
		public override string Texture => "Origins/Items/Accessories/Priority_Mail";
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
		}
	}
}
