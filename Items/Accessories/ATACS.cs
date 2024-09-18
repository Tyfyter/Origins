using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class ATACS : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"GenericBoostAcc"
		];
        static short glowmask;
        public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
        }
        public override void SetDefaults() {
			Item.DefaultToAccessory(28, 20);
			Item.value = Item.sellPrice(gold: 7);
			Item.rare = ItemRarityID.Yellow;
            Item.glowMask = glowmask;
        }
		public override void UpdateEquip(Player player) {
			player.GetCritChance(DamageClass.Generic) += 10f;
			//player.GetModPlayer<OriginPlayer>().strangeComputer = true; red laser
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.EyeoftheGolem)
			.AddIngredient(ModContent.ItemType<Strange_Computer>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
