using Origins.NPCs.Fiberglass;
using Origins.Tiles.Other;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Shaped_Glass : ModItem {
        public string[] Categories => new string[] {
            "BossSummon"
        };
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SuspiciousLookingEye);
		}
		public override bool? UseItem(Player player) {
			if (player.InModBiome<Fiberglass_Undergrowth>() && !NPC.AnyNPCs(ModContent.NPCType<Fiberglass_Weaver>())) {
                SoundEngine.PlaySound(SoundID.Roar);
				player.SpawnBossOn(ModContent.NPCType<Fiberglass_Weaver>());
				return true;
			}
			return false;
		}
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<Fiberglass_Item>(12);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
}
