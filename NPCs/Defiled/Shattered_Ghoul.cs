using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Shattered_Ghoul : Glowing_Mod_NPC, IDefiledEnemy, IWikiNPC, ICustomWikiStat {
		public Rectangle DrawRect => new(0, 6, 36, 52);
		public int AnimationFrames => 16;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public AssimilationAmount? Assimilation => 0.5f;
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Velocity = 1
			});
			Main.npcFrameCount[NPC.type] = 8;
			ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DesertGhoulCorruption);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 280;
			NPC.defense = 30;
			NPC.knockBackResist = 0.5f;
			NPC.damage = 60;
			NPC.width = 20;
			NPC.height = 44;
			NPC.value = 700;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt;
			NPC.DeathSound = Origins.Sounds.DefiledKill;
			NPC.value = Item.buyPrice(silver: 6, copper: 50);
			Banner = Item.NPCtoBanner(NPCID.DesertGhoul);
			AnimationType = NPCID.DesertGhoulCorruption;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands_Underground_Desert>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public int MaxMana => 100;
		public int MaxManaDrain => 20;
		public float Mana { get; set; }
		public bool ForceSyncMana => true;
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			target.AddBuff(Rasterized_Debuff.ID, 36);
		}
		public void Regenerate(out int lifeRegen) {
			int factor = Main.rand.RandomRound((180f / NPC.life) * 8);
			lifeRegen = factor;
			Mana -= factor / 180f;
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!Main.hardMode) return 0;
			if (!spawnInfo.DesertCave) return 0;
			if (!spawnInfo.Player.InModBiome<Defiled_Wastelands>()) return 0;
			return Defiled_Wastelands.SpawnRates.Ghoul;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				this.GetBestiaryFlavorText(),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.AncientCloth, 10));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Black_Bile>(), 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Latchkey>(), 5, 3, 7));
			npcLoot.Add(ItemDropRule.Common(ItemID.DarkShard, 15));
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
