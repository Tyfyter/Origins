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
			Item.rare = ItemRarityID.Orange;
            Item.glowMask = glowmask;
        }
		public override void UpdateEquip(Player player) {
			player.dangerSense = true;
			player.findTreasure = true;
			player.detectCreature = true;
			Lighting.AddLight(player.MountedCenter, new Vector3(0.4f));
		}
		public override void AddRecipes() {
			CreateRecipe()
            .AddIngredient(ItemID.TrapsightPotion, 5)
            .AddIngredient(ItemID.MetalDetector)
			.AddIngredient(ItemID.Radar)
			.AddIngredient(ItemID.SpelunkerPotion, 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
