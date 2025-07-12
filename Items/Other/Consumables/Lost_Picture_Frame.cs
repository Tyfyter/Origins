using Origins.Journal;
using Origins.NPCs.Brine.Boss;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Lost_Picture_Frame : ModItem, IJournalEntrySource<Lost_Picture_Frame_Entry> {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WormFood);
			Item.rare = ItemRarityID.Blue;
		}
		public override bool? UseItem(Player player) {
			const int range_reduction_x = 10;
			const int range_reduction_y = 4;
			const int min_spawn_dist = 17;
			int spawn = ModContent.NPCType<Lost_Diver_Spawn>();
			if (NPC.AnyNPCs(spawn) || NPC.AnyNPCs(ModContent.NPCType<Lost_Diver>()) || NPC.AnyNPCs(ModContent.NPCType<Lost_Diver_Transformation>()) || NPC.AnyNPCs(ModContent.NPCType<Mildew_Carrion>())) goto fail;

			if (player.InModBiome<Brine_Pool>() && !Brine_Pool.SpawnRates.IsInBrinePool(player.Center)) goto fail;

			List<Vector2> SpawnMap = [];
			ModNPC npc = ModContent.GetModNPC(spawn);
			Rectangle hitbox = npc.NPC.Hitbox;
			bool IsPositionValidCheck(Vector2 pos) {
				hitbox.X = (int)pos.X * 16 - npc.NPC.width / 2;
				hitbox.Y = (int)pos.Y * 16 - npc.NPC.height;
				return !hitbox.OverlapsAnyTiles();
			}
			for (int i = -NPC.safeRangeX + range_reduction_x; i <= NPC.safeRangeX - range_reduction_x; i++) {
				int x = i + (int)(player.Center.X / 16);
				for (int j = -NPC.safeRangeY + range_reduction_y; j <= NPC.safeRangeY - range_reduction_y; j++) {
					int y = j + (int)(player.Center.Y / 16);
					Vector2 pos = new(x, y);
					if (!pos.WithinRange(player.Center / 16, min_spawn_dist) && Brine_Pool.SpawnRates.IsInBrinePool(pos * 16) && IsPositionValidCheck(pos)) {
						SpawnMap.Add(pos * 16);
					}
				}
			}
			if (SpawnMap.Count <= 0) goto fail;
			SoundEngine.PlaySound(SoundID.Roar, player.Center);
			if (Main.netMode != NetmodeID.MultiplayerClient) NPC.NewNPCDirect(new EntitySource_BossSpawn(player), Main.rand.Next(SpawnMap), spawn);
			return true;
			fail:
			player.itemAnimation = 0;
			return false;
		}
	}
	public class Lost_Picture_Frame_Entry : JournalEntry {
		public override JournalSortIndex SortIndex => new("Brine_Pool_And_Lost_Diver", 6);
	}
}
