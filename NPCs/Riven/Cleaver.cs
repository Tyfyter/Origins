using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Items.Weapons.Demolitionist;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Cleaver_Head : WormHead, IRivenEnemy, ICustomWikiStat {
		public AssimilationAmount? Assimilation => 0.04f;
		public override int BodyType => ModContent.NPCType<Cleaver_Body>();
		public override int TailType => ModContent.NPCType<Cleaver_Tail>();
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new() { // Influences how the NPC looks in the Bestiary
				CustomTexturePath = "Origins/UI/Cleaver_Preview", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
				Position = new Vector2(5, 4),
				PortraitPositionXOverride = 8,
				PortraitPositionYOverride = 10
			};
			ModContent.GetInstance<Riven_Hive.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.width = NPC.height = 12;
			NPC.lifeMax = 50;
			NPC.defense = 7;
			NPC.damage = 23;
			NPC.HitSound = SoundID.NPCHit13;
			NPC.DeathSound = SoundID.NPCDeath23;
			//NPC.scale = 0.9f;
			NPC.value = 70;
			SpawnModBiomes = [
				ModContent.GetInstance<Underground_Riven_Hive_Biome>().Type
			];
		}
		public override void AI() {
			Lighting.AddLight(NPC.Center, Riven_Hive.ColoredGlow(0.02f));
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.SpawnTileY <= Main.worldSurface || spawnInfo.PlayerSafe || spawnInfo.DesertCave) return 0;
			return Riven_Hive.SpawnRates.FlyingEnemyRate(spawnInfo) * Riven_Hive.SpawnRates.Cleaver;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ameballoon>(), 1, 3, 6));
		}
		public void ModifyWikiStats(JObject data) {
			data["SpriteWidth"] = 108;
		}

		public override void Init() {
			MinSegmentLength = MaxSegmentLength = 32;
			MoveSpeed = 5.5f;
			Acceleration = 0.045f;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			TryDeathEffect();
		}
		public void TryDeathEffect() {
			if (NPC.life > 0 || NPC.aiAction == 1) return;
			NPC.aiAction = 1;
			NPC current = NPC;
			Vector2 velocity = NPC.velocity * 1.25f;
			float speed = velocity.Length();
			HashSet<int> indecies = [];
			int tailType = TailType;
			while (current.ai[0] != 0) {
				if (!indecies.Add(current.whoAmI)) break;
				OriginExtensions.LerpEquals(
					ref Gore.NewGoreDirect(
						current.GetSource_Death(),
						current.position,
						velocity,
						Origins.instance.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4))
					).velocity,
					current.velocity,
					0.5f
				);
				if (current.type == tailType) break;
				NPC next = Main.npc[(int)current.ai[0]];
				velocity = next.DirectionTo(current.Center) * speed;
				current = next;
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			OriginPlayer.InflictTorn(target, 300, targetSeverity: 1f - 0.9f);
		}
		public override Color? GetAlpha(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
	}

	internal class Cleaver_Body : WormBody, IRivenEnemy {
		public AssimilationAmount? Assimilation => 0.04f;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.width = NPC.height = 12;
		}
		public override void AI() {
			Lighting.AddLight(NPC.Center, Riven_Hive.ColoredGlow(0.02f));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			(HeadSegment.ModNPC as Cleaver_Head)?.TryDeathEffect();
		}
		public override void Init() {
			MoveSpeed = 5.5f;
			Acceleration = 0.045f;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			OriginPlayer.InflictTorn(target, 300, targetSeverity: 1f - 0.9f);
		}
		public override Color? GetAlpha(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
	}

	internal class Cleaver_Tail : WormTail, IRivenEnemy {
		public AssimilationAmount? Assimilation => 0.04f;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.width = NPC.height = 12;
		}
		public override void AI() {
			Lighting.AddLight(NPC.Center, Riven_Hive.ColoredGlow(0.02f));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			(HeadSegment.ModNPC as Cleaver_Head)?.TryDeathEffect();
		}
		public override void Init() {
			MoveSpeed = 5.5f;
			Acceleration = 0.045f;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			OriginPlayer.InflictTorn(target, 300, targetSeverity: 1f - 0.9f);
		}
		public override Color? GetAlpha(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
	}
}