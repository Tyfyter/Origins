﻿using Origins.Dev;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Chunky_Slime : ModNPC, IDefiledEnemy, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 32, 26);
		public int AnimationFrames => 2;
		public int FrameDuration => 8;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public AssimilationAmount? Assimilation => 0.01f;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 2;
			DefiledGlobalNPC.NPCTransformations.Add(NPCID.BlueSlime, Type);
			ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Slime;
			NPC.lifeMax = 60;
			NPC.defense = 5;
			NPC.damage = 30;
			NPC.width = 32;
			NPC.height = 24;
			NPC.friendly = false;
			NPC.alpha = 55;
			NPC.HitSound = Origins.Sounds.DefiledHurt;
			NPC.DeathSound = Origins.Sounds.DefiledKill;
			NPC.value = 40;
			AIType = NPCID.Crimslime;
			AnimationType = NPCID.Crimslime;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type,
				ModContent.GetInstance<Underground_Defiled_Wastelands_Biome>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public int MaxMana => 50;
		public int MaxManaDrain => 10;
		public float Mana { get; set; }
		public void Regenerate(ref int lifeRegen) {
			int factor = 48 / ((NPC.life / 10) + 1);
			lifeRegen = factor;
			Mana -= factor / 120f;// 1 mana for every 1 health regenerated
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.DesertCave || spawnInfo.SpawnTileY > Main.worldSurface) return 0;
			return Defiled_Wastelands.SpawnRates.LandEnemyRate(spawnInfo, false) * Defiled_Wastelands.SpawnRates.ChunkSlime;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.Gel, 1, 2, 4));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
			}
		}
	}
}
