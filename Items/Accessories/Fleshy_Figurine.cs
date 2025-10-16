using Origins.Buffs;
using Origins.Dev;
using Origins.Journal;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Fleshy_Figurine : ModItem, ICustomWikiStat, IJournalEntrySource<Fleshy_Figurine_Entry>, ITornSource {
		public static float TornSeverity => 0.1f;
		float ITornSource.Severity => TornSeverity;
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.Torn,
			WikiCategories.TornSource
		];
		static short glowmask;
        public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
        }
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(8);
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 30);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 5);
            Item.glowMask = glowmask;
        }
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (!originPlayer.taintedFlesh2) originPlayer.tornStrengthBoost.Flat += 0.1f;
			originPlayer.taintedFlesh2 = true;
			originPlayer.symbioteSkull = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
            .AddIngredient(ItemID.GuideVoodooDoll)
            .AddIngredient(ModContent.ItemType<Symbiote_Skull>())
            .AddIngredient(ModContent.ItemType<Tainted_Flesh>())
			.AddTile(TileID.TinkerersWorkbench)
			.AddCondition(new Condition(
				Language.GetOrRegister("Mods.Origins.Conditions.KillsGuide"),
				() => NPC.AnyNPCs(NPCID.Guide)
			))
			.AddOnCraftCallback((r, item, _, _) => {
				NPC guide = Main.npc[NPC.FindFirstNPC(NPCID.Guide)];
				guide.life = 0;
				guide.DeathSound = SoundID.Item104;
				guide.checkDead();
				for (int i = 0; i < 16; i++) {
					Dust.NewDust(guide.position, guide.width, guide.height, DustID.Torch, 0, -6);
				}
			})
			.Register();
		}
	}
	public class Fleshy_Figurine_Entry : JournalEntry {
		public override JournalSortIndex SortIndex => new("Lost_Crone", 4);
	}
}
