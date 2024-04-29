using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Priority_Mail : ModItem {
        public string[] Categories => new string[] {
            "Combat",
			"SummonBoostAcc"
        };
        public override void SetDefaults() {
			Item.DefaultToAccessory(30, 28);
			Item.maxStack = 2;
			Item.accessory = true;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.priorityMail = true;
			originPlayer.asylumWhistle = true;
			player.GetDamage(DamageClass.Summon) += 0.1f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 2);
			recipe.AddIngredient(ItemID.FallenStar);
			recipe.AddIngredient(ItemID.PaperAirplaneA);
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
