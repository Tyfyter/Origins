using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
    public class Crimbrain : ModNPC {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Crimbrain");
			Main.npcFrameCount[NPC.type] = 4;
			CrimsonGlobalNPC.NPCTypes.Add(Type);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DemonEye);
			NPC.aiStyle = 86;
			NPC.lifeMax = 36;
			NPC.defense = 10;
			NPC.damage = 25;
			NPC.width = 34;
			NPC.height = 28;
			NPC.friendly = false;
			NPC.knockBackResist = 0.85f;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			target.AddBuff(BuffID.Confused, 50);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				new FlavorTextBestiaryInfoElement("These entities possess a higher standing within the Crimson heirarchy as they bear the knowledge of its secrets."),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.Vertebrae));
		}
		public override void AI() {
			NPC.FaceTarget();
			if (!NPC.HasValidTarget) NPC.direction = Math.Sign(NPC.velocity.X);
			NPC.spriteDirection = NPC.direction;
			if (++NPC.frameCounter > 7) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 32) % 128, 34, 32);
				NPC.frameCounter = 0;
			}
		}
	}
}
