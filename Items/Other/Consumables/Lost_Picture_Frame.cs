using Origins.NPCs.Brine.Boss;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Lost_Picture_Frame : ModItem {
        public string[] Categories => [
            "BossSummon"
        ];
		public List<Vector2> SpawnMap = [];
		private int rangeX = 10;
		private int rangeY = 4;
		private int spawnDst = 17;
		private readonly int spawnType = ModContent.NPCType<Lost_Diver_Spawn>();
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WormFood);
			Item.rare = ItemRarityID.Blue;
		}
		public override bool CanUseItem(Player player) {
			if (NPC.AnyNPCs(ModContent.NPCType<Lost_Diver_Spawn>()) || NPC.AnyNPCs(ModContent.NPCType<Lost_Diver>()) || NPC.AnyNPCs(ModContent.NPCType<Lost_Diver_Transformation>()) || NPC.AnyNPCs(ModContent.NPCType<Mildew_Carrion>())) return false;

			if (player.InModBiome<Brine_Pool>() && !Brine_Pool.SpawnRates.IsInBrinePool(player.Center)) return false;

			SpawnMap = [];
			for (int i = -NPC.safeRangeX + rangeX; i <= NPC.safeRangeX - rangeX; i++) {
				int x = i + (int)(player.Center.X / 16);
				for (int j = -NPC.safeRangeY + rangeY; j <= NPC.safeRangeY - rangeY; j++) {
					int y = j + (int)(player.Center.Y / 16);
					Vector2 pos = new(x, y);
					bool inRange = pos.Distance(player.Center / 16) <= spawnDst;
					bool inBP = !Brine_Pool.SpawnRates.IsInBrinePool(pos * 16);
					bool inHeight = !isHeightValidCheck(pos);
					if (!inRange || !inBP || !inHeight) {
						SpawnMap.Add(pos * 16);
					}
				}
			}

			return SpawnMap.Count != 0;
		}
		public override bool? UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				int pos = Main.rand.Next(0, SpawnMap.Count);
				SoundEngine.PlaySound(SoundID.Roar);
				NPC.NewNPCDirect(new EntitySource_BossSpawn(player), SpawnMap[pos], spawnType);
			}
			return true;
		}
		private bool isHeightValidCheck(Vector2 pos) {
			ModNPC npc = ModContent.GetModNPC(spawnType);
			Rectangle hitbox = npc.NPC.Hitbox;
			hitbox.X = (int)pos.X * 16 - npc.NPC.width / 2;
			hitbox.Y = (int)pos.Y * 16 - npc.NPC.height;
			return !hitbox.OverlapsAnyTiles();
		}
	}
}
