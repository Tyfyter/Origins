﻿using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
    public class Buckethead_Zombie : ModNPC {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Buckethead Zombie");
			Main.npcFrameCount[NPC.type] = 3;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 45;
			NPC.defense = 28;
			NPC.damage = 14;
			NPC.width = 38;
			NPC.height = 46;
			NPC.friendly = false;
		}
		public override void FindFrame(int frameHeight) {
			NPC.CloneFrame(NPCID.Zombie , frameHeight);
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.Player.ZoneGraveyard || !Main.dayTime) {
				return 0.085f;
			}
			return 0;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				new FlavorTextBestiaryInfoElement("Buckethead zombie always wore a bucket. Part of it was to assert his uniqueness in an uncaring world. Mostly he just forgot it was there in the first place."),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.EmptyBucket, 3));
			npcLoot.Add(ItemDropRule.Common(ItemID.Diamond, 20));
		}
	}
}