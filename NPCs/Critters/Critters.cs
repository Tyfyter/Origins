using Origins.Items.Other.Fish;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Critters {
	public class Amoeba_Buggy : ModNPC {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Amoeba Buggy");
			Main.npcCatchable[Type] = true;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.FairyCritterBlue);
			NPC.catchItem = ModContent.ItemType<Amoeba_Buggy_Item>();
		}
	}
	public class Bug : ModNPC {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Bug");
			Main.npcCatchable[Type] = true;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.GoldWaterStrider);
			NPC.catchItem = ModContent.ItemType<Bug_Item>();
		}
	}
	public class Cicada_3301 : ModNPC {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cicada 3301");
			Main.npcCatchable[Type] = true;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.FairyCritterBlue);
			NPC.catchItem = ModContent.ItemType<Cicada_3301_Item>();
		}
	}
}
