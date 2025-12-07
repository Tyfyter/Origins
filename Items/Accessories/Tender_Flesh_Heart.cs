using Origins.Dev;
using Origins.Layers;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
    public class Tender_Flesh_Heart : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Vitality"
        ];
        public override void SetStaticDefaults() {
            Origins.AddGlowMask(this);
		}
        public override void SetDefaults() {
            Item.DefaultToAccessory(38, 20);
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Pink;
        }
        public override void AddRecipes() {
            Recipe.Create(Type)
            .AddIngredient<Bug_Trapper>()
            .AddIngredient<Mildew_Heart>()
            .AddTile(TileID.TinkerersWorkbench)
			.Register();
        }
        public override void UpdateAccessory(Player player, bool isHidden) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.bugZapper = true;
			originPlayer.mildewHeart = true;
			originPlayer.mildewHeartRegenMult = originPlayer.tornCurrentSeverity + 0.7f;
		}
	}
}
