using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Fleshy_Figurine : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Fleshy Figurine");
			// Tooltip.SetDefault("Attacks tenderize targets\nIncreases the strength of afflicted Torn effects");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 30);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 5);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().taintedFlesh2 = true;
			player.GetModPlayer<OriginPlayer>().symbioteSkull = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Tainted_Flesh>());
			recipe.AddIngredient(ItemID.GuideVoodooDoll);
			recipe.AddIngredient(ModContent.ItemType<Symbiote_Skull>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.AddCondition(new Recipe.Condition(
				Terraria.Localization.NetworkText.FromLiteral("This kills the Guide"),
				(r) => NPC.AnyNPCs(NPCID.Guide)
			));
			recipe.AddOnCraftCallback((r, item, _) => {
				NPC guide = Main.npc[NPC.FindFirstNPC(NPCID.Guide)];
				guide.life = 0;
				guide.DeathSound = SoundID.Item104;
				guide.checkDead();
				for (int i = 0; i < 16; i++) {
					Dust.NewDust(guide.position, guide.width, guide.height, DustID.Torch, 0, -6);
				}
			});
			recipe.Register();
		}
	}
}
