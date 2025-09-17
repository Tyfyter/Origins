using Origins.Dev;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Riven_Mummy : Glowing_Mod_NPC, IRivenEnemy, IWikiNPC, ICustomWikiStat {
		public Rectangle DrawRect => new(0, 6, 40, 48);
		public int AnimationFrames => 16;
		public int FrameDuration => 2;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override Color GetGlowColor(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
		public AssimilationAmount? Assimilation => 0.07f;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Velocity = 1
			});
			Main.npcFrameCount[NPC.type] = 16;
			ModContent.GetInstance<Riven_Hive.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 110;
			NPC.defense = 8;
			NPC.damage = 42;
			NPC.width = 20;
			NPC.height = 46;
			NPC.friendly = false;
			NPC.value = 7;
			AnimationType = NPCID.BloodMummy;
			AIType = NPCID.BloodMummy;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive_Desert>().Type,
				ModContent.GetInstance<Riven_Hive_Underground_Desert>().Type
			];
			//ModContent.Request<Microsoft.Xna.Framework.Graphics.Texture2D>("Origins/UI/MapBGs/Riven_Desert").Value.Size()
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			return spawnInfo.SpecificTilesEnemyRate([ModContent.TileType<Silica>()], true) * Riven_Hive.SpawnRates.Mummy / 3f;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void AI() {
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) {
				NPC.spriteDirection = NPC.direction;
			}
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.DarkShard, 10));
			npcLoot.Add(ItemDropRule.StatusImmunityItem(ItemID.Megaphone, 100));
			npcLoot.Add(ItemDropRule.StatusImmunityItem(ItemID.Blindfold, 100));
			npcLoot.Add(ItemDropRule.Common(ItemID.MummyMask, 75));
			npcLoot.Add(ItemDropRule.Common(ItemID.MummyShirt, 75));
			npcLoot.Add(ItemDropRule.Common(ItemID.MummyPants, 75));
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
