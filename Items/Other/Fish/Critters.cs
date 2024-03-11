using Origins.NPCs.Critters;
using Terraria;
using Terraria.ModLoader;

//what is this? bees according to California?
namespace Origins.Items.Other.Fish {
	public class Amoeba_Buggy_Item : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Amoeba Buggy");
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.DefaultToCapturedCritter(ModContent.NPCType<Amoeba_Buggy>());
		}
		// TODO: Needs critter behavior
	}
	public class Bug_Item : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Bug");
			// Tooltip.SetDefault("'Don't try to smash it, it'll make more problems'");
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.DefaultToCapturedCritter(ModContent.NPCType<Bug>());
		}
		// TODO: Needs critter behavior
	}
	public class Cicada_3301_Item : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Cicada 3301");
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.DefaultToCapturedCritter(ModContent.NPCType<Cicada_3301>());
		}
		// TODO: Needs critter behavior
	}
}
