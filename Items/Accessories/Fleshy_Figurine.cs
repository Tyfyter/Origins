﻿using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Fleshy_Figurine : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat",
			"Torn",
			"TornSource"
		};
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
			if (!originPlayer.taintedFlesh2) originPlayer.tornStrengthBoost.Flat += 0.08f;
			originPlayer.taintedFlesh2 = true;
			originPlayer.symbioteSkull = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.GuideVoodooDoll);
            recipe.AddIngredient(ModContent.ItemType<Symbiote_Skull>());
            recipe.AddIngredient(ModContent.ItemType<Tainted_Flesh>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.AddCondition(new Condition(
				Language.GetOrRegister("Mods.Origins.Conditions.KillsGuide"),
				() => NPC.AnyNPCs(NPCID.Guide)
			));
			recipe.AddOnCraftCallback((r, item, _, _) => {
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
