using Origins.Dev;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace Origins.NPCs.Riven {
	public class Amoeba_Bugger : Glowing_Mod_NPC, IRivenEnemy, IWikiNPC, ICustomWikiStat {
		public override Color GetGlowColor(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
		public AssimilationAmount? Assimilation => 0.03f;
		public Rectangle DrawRect => new(0, 0, 48, 32);
		public int AnimationFrames => 1;
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
			Main.npcFrameCount[NPC.type] = 6;
			NPCID.Sets.PositiveNPCTypesExcludedFromDeathTally[Type] = true;
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Bat;
			NPC.lifeMax = 45;
			NPC.defense = 8;
			NPC.damage = 18;
			NPC.width = 28;
			NPC.height = 26;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit9;
			NPC.DeathSound = SoundID.NPCDeath16;
			NPC.noGravity = true;
			NPC.npcSlots = 0.25f;
			this.CopyBanner<Barnacle_Mound>();
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void OnSpawn(IEntitySource source) {
			NPC.velocity = new(NPC.ai[0], NPC.ai[1]);
			NPC.netUpdate = true;
		}
		public override void AI() {
			if (Main.rand.NextBool(1100)) SoundEngine.PlaySound(SoundID.Zombie77.WithVolume(0.5f), NPC.Center);
			if (NPC.velocity.HasNaNs()) NPC.velocity = default;
			if (!NPC.HasValidTarget) NPC.direction = Math.Sign(NPC.velocity.X);
			NPC.spriteDirection = NPC.direction;
			NPC.DoFrames(1);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(2, 4));
			} else {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			OriginPlayer.InflictTorn(target, 300, targetSeverity: 1f - 0.85f);
		}
	}
}
