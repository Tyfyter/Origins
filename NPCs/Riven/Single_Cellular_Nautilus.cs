using Microsoft.Xna.Framework;
using Origins.Items.Armor.Riven;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Single_Cellular_Nautilus : Glowing_Mod_NPC, IRivenEnemy {
		public AssimilationAmount? Assimilation => 0.03f;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 3;
			ModContent.GetInstance<Riven_Hive.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Tumbleweed);
			NPC.aiStyle = NPCAIStyleID.Unicorn;
			NPC.lifeMax = 110;
			NPC.defense = 31;
			NPC.damage = 28;
			NPC.width = 20;
			NPC.height = 24;
			NPC.friendly = false;
			NPC.knockBackResist = 1f;
			NPC.frame.Height = 30;
			NPC.value = 450;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type,
				ModContent.GetInstance<Underground_Riven_Hive_Biome>().Type
			];
		}
		public override void PostAI() {
			NPC.rotation += NPC.velocity.X / 24f;
			NPC.frameCounter++;
			if (NPC.frameCounter >= 24) NPC.frameCounter = 0;
			NPC.frame.Y = ((int)NPC.frameCounter / 8) * 32;
			if (NPC.velocity.X != 0) NPC.spriteDirection = Math.Sign(NPC.velocity.X);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
        public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
            return spawnInfo.SpawnTileY < Main.worldSurface ? 0 : Riven_Hive.SpawnRates.LandEnemyRate(spawnInfo) * Riven_Hive.SpawnRates.Seashell;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Mask>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Coat>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Pants>(), 525));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			//spawn gore if npc is dead after being hit
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat1");
			}
		}
	}
}
