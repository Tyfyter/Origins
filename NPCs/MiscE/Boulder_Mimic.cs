using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
	// made seed-specific because not getting killed by boulders is how you're supposed to deal with them
	public class Boulder_Mimic : Glowing_Mod_NPC {
		public float SpeedMult => 1.15f;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 9;
			// seed-specific NPCs don't get bestiary entries
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
		}
		public override void Load() => this.AddBanner();
		public override void SetDefaults() {
			NPC.lifeMax = 120;
			NPC.defense = 22;
			NPC.damage = 40;
			NPC.width = 32;
			NPC.height = 32;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit41;
			NPC.DeathSound = SoundID.NPCDeath43;
			DrawOffsetY = 20;
			NPC.knockBackResist = 0.5f;
		}
		public override bool PreAI() {
			NPC.velocity /= SpeedMult;
			return true;
		}
		public override void AI() {
			if (NPC.frame.Y / 62 >= 6) {
				DrawOffsetY = 14;
				NPC.aiStyle = NPCAIStyleID.Granite_Elemental;
				float diff = NPC.GetTargetData().Center.X - NPC.Center.X;
				if (diff != 0) NPC.spriteDirection = diff > 0 ? 1 : -1;
			}
			NPC.velocity *= SpeedMult;
		}
		public override void FindFrame(int frameHeight) {
			bool expanded = NPC.frame.Y / 62 >= 6;
			NPC.DoFrames(expanded ? 6 : 9, (expanded ? 6 : 0)..Main.npcFrameCount[Type]);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.StoneBlock, 1, 10, 20));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0 || OriginsModIntegrations.CheckAprilFools()) {
				for (int i = 0; i < 12; i++) {
					Dust.NewDust(
						NPC.Center - new Vector2(16),
						32,
						32,
						DustID.Stone
					);
				}
				/*Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 4);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 4);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 5);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 5);*/
			}
		}
	}
}
