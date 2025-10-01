using Origins.Dev;
using Origins.Items.Mounts;
using Origins.Items.Tools;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.NPCs.Defiled;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Ashen {
	public class Trash_Compactor_Mimic : ModNPC, IWikiNPC, ICustomWikiStat {
		public override string Texture => typeof(Defiled_Mimic).GetDefaultTMLName();
		public Rectangle DrawRect => new(0, 0, 60, 50);
		public int AnimationFrames => 1;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 14;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire3] = true;
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.width = 28;
			NPC.height = 44;
			NPC.aiStyle = NPCAIStyleID.Biome_Mimic;
			NPC.damage = 90;
			NPC.defense = 34;
			NPC.lifeMax = 3500;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.value = 30000f;
			NPC.knockBackResist = 0.1f;
			NPC.rarity = 5;
			AnimationType = NPCID.BigMimicCorruption;
			SpawnModBiomes = [
				ModContent.GetInstance<Underground_Ashen_Biome>().Type,
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public bool ForceSyncMana => false;
		public float Mana { get; set; }
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.OneFromOptions(1,
				ModContent.ItemType<Communion>(),
				ModContent.ItemType<Incision>(),
				ModContent.ItemType<Ziptie>(),
				ModContent.ItemType<Ravel>(),
				ModContent.ItemType<Tangela_Tether>()
			));
			npcLoot.Add(ItemDropRule.Common(ItemID.GreaterHealingPotion, 1, 5, 10));
			npcLoot.Add(ItemDropRule.Common(ItemID.GreaterManaPotion, 1, 5, 15));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
			}
		}
		public override void OnKill() {
			Boss_Tracker.Instance.downedTrashCompactorMimic = true;
			NetMessage.SendData(MessageID.WorldData);
		}
	}
}
