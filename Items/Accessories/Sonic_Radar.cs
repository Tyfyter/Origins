using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Sonic_Radar : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Info"
		];
        public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
        }
        static short glowmask;
        public override void SetDefaults() {
			Item.DefaultToAccessory(30, 26);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
            Item.glowMask = glowmask;
        }
		public override void UpdateEquip(Player player) {
			player.dangerSense = true;
			player.findTreasure = true;
			player.detectCreature = true;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.TrapsightPotion, 5);
            recipe.AddIngredient(ItemID.MetalDetector);
			recipe.AddIngredient(ItemID.Radar);
			recipe.AddIngredient(ItemID.SpelunkerPotion, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
