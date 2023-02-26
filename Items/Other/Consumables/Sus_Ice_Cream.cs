using Origins.Items.Materials;
using Origins.NPCs.Riven;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Sus_Ice_Cream : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Suspicious Looking Ice Cream");
			Tooltip.SetDefault("Summons the Primordial Amoeba\n'This ice cream will eat YOU'");

			SacrificeTotal = 3;
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WormFood);
			Item.rare = ItemRarityID.Blue;
		}
		public override bool CanUseItem(Player player) {
			return player.InModBiome<Riven_Hive>() && !NPC.AnyNPCs(ModContent.NPCType<Primordial_Amoeba>());
		}
		public override bool? UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				NPC.SpawnOnPlayer(Main.myPlayer, ModContent.NPCType<Primordial_Amoeba>());
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
