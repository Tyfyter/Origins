using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    public class Bottomfeeder : ModNPC, IRivenEnemy {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 6;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.CorruptGoldfish);
			NPC.lifeMax = 120;
			NPC.defense = 7;
			NPC.damage = 38;
			NPC.width = 28;
			NPC.height = 26;
			NPC.frame.Height = 28;
			NPC.value = 500;
			AnimationType = NPCID.CorruptGoldfish;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				this.GetBestiaryFlavorText("Goldfish naturally evolve into dangerous, insolveable carnivores in the Riven Hive which is only further catalyzed by the Blood Moon."),
			});
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life < 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(2, 4));
			} else {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
			}
		}
	}
}
