using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Amoeba_Bugger : Glowing_Mod_NPC {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Amoeba Bugger");
			Main.npcFrameCount[NPC.type] = 2;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Bunny);
			NPC.aiStyle = NPCAIStyleID.Bat;
			NPC.lifeMax = 45;
			NPC.defense = 8;
			NPC.damage = 18;
			NPC.width = 28;
			NPC.height = 26;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt;
			NPC.DeathSound = Origins.Sounds.DefiledKill;
			NPC.noGravity = true;
			NPC.npcSlots = 0.25f;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				new FlavorTextBestiaryInfoElement("These nasty bugs thrive in conditions like the Riven Hive feeding on flesh decayed from the barnacles. They also love it fresh whenever available."),
			});
		}
		public override void OnSpawn(IEntitySource source) {
			NPC.velocity = new(NPC.ai[0], NPC.ai[1]);
			NPC.netUpdate = true;
		}
		public override void AI() {
			if (Main.rand.NextBool(900)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(1f, 1.2f), NPC.Center);
			NPC.FaceTarget();
			if (NPC.velocity.HasNaNs()) NPC.velocity = default;
			if (!NPC.HasValidTarget) NPC.direction = Math.Sign(NPC.velocity.X);
			NPC.spriteDirection = NPC.direction;
			if (++NPC.frameCounter > 5) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 28) % 56, 32, 26);
				NPC.frameCounter = 0;
			}
		}
		public override void HitEffect(int hitDirection, double damage) {
			if (NPC.life < 0) {
				for (int i = 0; i < 3; i++) Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4)));
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/R_Effect_Meat" + Main.rand.Next(2, 4)));
			} else {
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4)));
			}
		}
		public override void OnHitPlayer(Player target, int damage, bool crit) {
			OriginPlayer.InflictTorn(target, 300, 180, 0.85f);
		}
	}
}
