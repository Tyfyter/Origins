using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
    public class Crimbrain : ModNPC {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crimbrain");
			Main.npcFrameCount[NPC.type] = 4;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DemonEye);
			NPC.aiStyle = 86;
			NPC.lifeMax = 56;
			NPC.defense = 10;
			NPC.damage = 25;
			NPC.width = 36;
			NPC.height = 34;
			NPC.friendly = false;
			NPC.knockBackResist = 0.85f;
		}
		public override void OnHitPlayer(Player target, int damage, bool crit) {
			target.AddBuff(BuffID.Confused, 50);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				new FlavorTextBestiaryInfoElement("These entities possess a higher standing within the Crimson heirarchy as they bear the knowledge of its secrets."),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			
		}
		public override void AI() {
			if (Main.rand.NextBool(900)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(1f, 1.1f), NPC.Center);
			NPC.FaceTarget();
			if (!NPC.HasValidTarget) NPC.direction = Math.Sign(NPC.velocity.X);
			NPC.spriteDirection = NPC.direction;
			if (++NPC.frameCounter > 5) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 38) % 152, 104, 36);
				NPC.frameCounter = 0;
			}
		}
	}
}
