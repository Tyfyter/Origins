using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Tools;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Riven_Mimic : Glowing_Mod_NPC, IRivenEnemy, IWikiNPC, ICustomWikiStat {
		public Rectangle DrawRect => new(0, 4, 60, 50);
		public int AnimationFrames => 1;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override Color GetGlowColor(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
		public override void Load() => this.AddBanner(25);
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
			AnimationType = NPCID.BigMimicCrimson;
			SpawnModBiomes = [
				ModContent.GetInstance<Underground_Riven_Hive_Biome>().Type
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.OneFromOptions(1,
				ModContent.ItemType<Dew_Justice>(),
				ModContent.ItemType<Dart_Crossbow>(),
				ModContent.ItemType<Amoebash>(),
				ModContent.ItemType<Tainted_Flesh>(),
				ModContent.ItemType<Amoeba_Hook>()
			));
			npcLoot.Add(ItemDropRule.Common(ItemID.GreaterHealingPotion, 1, 5, 10));
			npcLoot.Add(ItemDropRule.Common(ItemID.GreaterManaPotion, 1, 5, 15));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (Main.netMode == NetmodeID.Server) return;
			if (NPC.life <= 0) {
                for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
                Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(2, 4));
            } else {
                Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
            }
		}
		public override void OnKill() {
			Boss_Tracker.Instance.downedRivenMimic = true;
			NetMessage.SendData(MessageID.WorldData);
		}
	}
}
