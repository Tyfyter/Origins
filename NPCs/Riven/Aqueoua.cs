using Origins.Gores.NPCs;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Aqueoua : Glowing_Mod_NPC, IRivenEnemy {
		public override Color GetGlowColor(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
		public AssimilationAmount? Assimilation => 0.07f;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 4;
			ModContent.GetInstance<Riven_Hive.SpawnRates>().AddSpawn(Type, SpawnChance);
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Position = new(5, 10),
				Rotation = 0.75f
			};
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 45;
			NPC.defense = 8;
			NPC.damage = 18;
			NPC.width = 28;
			NPC.height = 26;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit9;
			NPC.DeathSound = SoundID.NPCDeath15;
			NPC.noGravity = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type,
			];
		}
		public override bool? CanFallThroughPlatforms() => true;
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Riven_Hive.SpawnRates.FlyingEnemyRate(spawnInfo) * Riven_Hive.SpawnRates.Aqueoua * 0.5f;
		}
		public override void AI() {
			NPC.DoFlyingAI(5f, 0.025f, 0.5f);
			if (!NPC.velocity.IsWithin(default, 1)) NPC.velocity = NPC.velocity.RotatedBy(MathF.Cos(NPC.ai[0] * MathHelper.PiOver4 * 0.25f) * 0.03f);
			NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;
		}
		public override void FindFrame(int frameHeight) {
			NPC.DoFrames(5);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Dust.NewDust(
					NPC.Center + new Vector2(2 * NPC.direction, 2).RotatedBy(NPC.rotation),
					0, 0,
					ModContent.DustType<Aqueoua_Gore1>(),
					NPC.velocity.X, NPC.velocity.Y
				);
				Dust.NewDust(
					NPC.Center + new Vector2(-1 * NPC.direction, -19).RotatedBy(NPC.rotation),
					0, 0,
					ModContent.DustType<Aqueoua_Gore2>(),
					NPC.velocity.X, NPC.velocity.Y
				);
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
			} else {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
			}
		}
	}
	public class Shelly_Aqueoua : Aqueoua {
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.defense = 16;
		}
		public override void AI() {
			NPC.DoFlyingAI(4f, 0.02f, 0.4f);
			if (!NPC.velocity.IsWithin(default, 1)) NPC.velocity = NPC.velocity.RotatedBy(MathF.Cos(NPC.ai[0] * MathHelper.PiOver4 * 0.25f) * 0.03f);
			NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;
		}
	}
}
