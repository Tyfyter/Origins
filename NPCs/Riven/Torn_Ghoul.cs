﻿using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Torn_Ghoul : Glowing_Mod_NPC, IRivenEnemy {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 8;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DesertGhoulCorruption);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 240;
			NPC.defense = 24;
			NPC.knockBackResist = 0.6f;
			NPC.damage = 70;
			NPC.width = 20;
			NPC.height = 44;
			NPC.value = 700;
			NPC.friendly = false;
			NPC.value = Item.buyPrice(silver: 6, copper: 50);
			Banner = NPCID.DesertGhoul;
			AnimationType = NPCID.DesertGhoulCorruption;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive_Underground_Desert>().Type
			];
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			OriginPlayer.InflictTorn(target, 6 * 60);
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!spawnInfo.DesertCave) return 0;
			if (!spawnInfo.Player.InModBiome<Riven_Hive>()) return 0;
			return Riven_Hive.SpawnRates.Ghoul;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				this.GetBestiaryFlavorText(),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.AncientCloth, 10));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Alkahest>(), 3));
			npcLoot.Add(ItemDropRule.Common(ItemID.DarkShard, 15));
		}
		public override void AI() {
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) {
				NPC.FaceTarget();
				NPC.spriteDirection = NPC.direction;
			}
		}
		public override void HitEffect(NPC.HitInfo hit) {
			//spawn gore if npc is dead after being hit
			if (NPC.life < 0) {
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(2, 4));
			}
		}
	}
}