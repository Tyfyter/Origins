using Origins.NPCs.Brine.Boss;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Lost_Picture_Frame : ModItem {
        public string[] Categories => [
            "BossSummon"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WormFood);
			Item.rare = ItemRarityID.Blue;
		}
		public override bool CanUseItem(Player player) {
			if (NPC.AnyNPCs(ModContent.NPCType<Lost_Diver_Spawn>()) || NPC.AnyNPCs(ModContent.NPCType<Lost_Diver>()) || NPC.AnyNPCs(ModContent.NPCType<Lost_Diver_Transformation>()) || NPC.AnyNPCs(ModContent.NPCType<Mildew_Carrion>()) || Lost_Diver_Spawn.spawnLD) return false;
			return player.InModBiome<Brine_Pool>();
		}
		public override bool? UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				SoundEngine.PlaySound(SoundID.Roar);
				SpawnDiver(player);
			}
			return true;
		}

		public void SpawnDiver(Player player) {
			Lost_Diver_Spawn.spawnLD = true;
		}
	}
}
