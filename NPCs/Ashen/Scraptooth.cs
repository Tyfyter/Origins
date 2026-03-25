using Origins.Dev;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace Origins.NPCs.Ashen {
	public class Scraptooth : Glowing_Mod_NPC, IAshenEnemy, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 28, 26);
		public int AnimationFrames => 6;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 6;
		}
		public override void FindFrame(int frameHeight) {
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				IsWet = true
			};
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
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath44;
			AnimationType = NPCID.CorruptGoldfish;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon
			);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore1");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore2");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore3");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore4");
				for (int i = 0; i < 2; i++) {
					Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
				}
			} else if (Main.rand.NextBool(5)) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
			}
		}
	}
}
