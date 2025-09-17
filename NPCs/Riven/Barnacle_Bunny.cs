using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using ThoriumMod.Empowerments;

namespace Origins.NPCs.Riven {
	public class Barnacle_Bunny : Glowing_Mod_NPC, IRivenEnemy, IWikiNPC {
		public Rectangle DrawRect => new(-2, 6, 32, 32);
		public int AnimationFrames => 28;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override Color GetGlowColor(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 7;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.CrimsonBunny);
			NPC.damage = 21;
			NPC.lifeMax = 75;
			NPC.value = 500f;
			AnimationType = NPCID.CrimsonBunny;
			AIType = NPCID.CrimsonBunny;
        }
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                this.GetBestiaryFlavorText(),
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon
            });
		}
		public override void HitEffect(NPC.HitInfo hit) {
			//spawn gore if npc is dead after being hit
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(2, 4));
			}
		}
	}
}
