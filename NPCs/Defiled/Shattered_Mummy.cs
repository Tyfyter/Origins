using Origins.Dev;
using Origins.Tiles.Defiled;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Shattered_Mummy : Glowing_Mod_NPC, IDefiledEnemy, IWikiNPC {
		public Rectangle DrawRect => new(0, 4, 40, 56);
		public int AnimationFrames => 16;
		public int FrameDuration => 2;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public AssimilationAmount? Assimilation => 0.04f;
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Velocity = 1
			});
			Main.npcFrameCount[NPC.type] = 16;
			ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 180;
			NPC.defense = 18;
			NPC.knockBackResist = 0.5f;
			NPC.damage = 60;
			NPC.width = 20;
			NPC.height = 44;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt;
			NPC.DeathSound = Origins.Sounds.DefiledKill;
			NPC.value = 700;
			AnimationType = NPCID.DarkMummy;
			AIType = NPCID.DarkMummy;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands_Desert>().Type,
				ModContent.GetInstance<Defiled_Wastelands_Underground_Desert>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public int MaxMana => 100;
		public int MaxManaDrain => 20;
		public float Mana { get; set; }
		public void Regenerate(out int lifeRegen) {
			int factor = Main.rand.RandomRound((180f / NPC.life) * 8);
			lifeRegen = factor;
			Mana -= factor / 180f;
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			return spawnInfo.SpecificTilesEnemyRate([ModContent.TileType<Defiled_Sand>()], true) * Defiled_Wastelands.SpawnRates.Mummy / 3f;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.DarkShard, 10));
			npcLoot.Add(ItemDropRule.StatusImmunityItem(ItemID.Megaphone, 100));
			npcLoot.Add(ItemDropRule.StatusImmunityItem(ItemID.Blindfold, 100));
			npcLoot.Add(ItemDropRule.Common(ItemID.MummyMask, 75));
			npcLoot.Add(ItemDropRule.Common(ItemID.MummyShirt, 75));
			npcLoot.Add(ItemDropRule.Common(ItemID.MummyPants, 75));
		}
		public override void AI() {
			if (Main.rand.NextBool(800)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle, NPC.Center);
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) {
				NPC.spriteDirection = NPC.direction;
			}
		}
		public override void HitEffect(NPC.HitInfo hit) {
			//spawn gore if npc is dead after being hit
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
			}
		}
	}
}
