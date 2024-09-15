using Origins.NPCs.Critters;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

//what is this? bees according to California?
namespace Origins.Items.Other.Fish {
	public class Amoeba_Buggy_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.DefaultToCapturedCritter(ModContent.NPCType<Amoeba_Buggy>());
		}
	}
	public class Bug_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.DefaultToCapturedCritter(ModContent.NPCType<Bug>());
			Item.rare = ItemRarityID.Lime;
		}
	}
	public class Cicada_3301_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.DefaultToCapturedCritter(ModContent.NPCType<Cicada_3301>());
		}
	}
}
