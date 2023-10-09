using Origins.Items.Materials;
using Origins.NPCs.Riven;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Sus_Ice_Cream : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 3;
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WormFood);
			Item.rare = ItemRarityID.Blue;
			Item.glowMask = glowmask;
		}
		public override bool CanUseItem(Player player) {
			return player.InModBiome<Riven_Hive>() && !NPC.AnyNPCs(ModContent.NPCType<Riven_Fighter>());
		}
		public override bool? UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				NPC.SpawnOnPlayer(Main.myPlayer, ModContent.NPCType<Riven_Fighter>());
			}
			return true;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<Sentient_Powder>(30);
			recipe.AddIngredient<Bud_Barnacle>(15);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
		}
	}
}
