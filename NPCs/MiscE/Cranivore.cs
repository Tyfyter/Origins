using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
    public class Cranivore : ModNPC {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cranivore");
			Main.npcFrameCount[Type] = 2;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DemonEye);
			NPC.aiStyle = 85;
			NPC.lifeMax = 28;
			NPC.defense = 5;
			NPC.damage = 16;
			NPC.width = 32;
			NPC.height = 18;
			NPC.friendly = false;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				new FlavorTextBestiaryInfoElement("An uncommon lifeform known to be in the second stage of the Eater life cycle. This creature prefers to start its meals by the head."),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.RottenChunk, 3));
		}
		public override void AI() {
			NPCAimedTarget target = NPC.GetTargetData();
			NPC.rotation = NPC.AngleTo(target.Center) + MathHelper.PiOver2;
			if (Main.rand.NextBool(900)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(1f, 1.2f), NPC.Center);
			NPC.FaceTarget();
			if (!NPC.HasValidTarget) NPC.direction = Math.Sign(NPC.velocity.X);
			NPC.spriteDirection = NPC.direction;
		}
	}
}
