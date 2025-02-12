using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Armor.Riven;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Weapons.Summoner;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    public class Flagellant : Glowing_Mod_NPC, IRivenEnemy, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 56, 60);
		public int AnimationFrames => 24;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override Color? GetGlowColor(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
		public AssimilationAmount? Assimilation => 0.11f;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 4;
			ModContent.GetInstance<Riven_Hive.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.BloodJelly);
			NPC.lifeMax = 135;
			NPC.defense = 14;
			NPC.damage = 35;
			NPC.width = 33;
			NPC.height = 50;
			NPC.frame.Height = 58;
			NPC.value = 500;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type,
				ModContent.GetInstance<Underground_Riven_Hive_Biome>().Type,
				ModContent.GetInstance<Riven_Hive_Ocean>().Type
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;
			NPC.frameCounter += 1.0;
			if (NPC.frameCounter >= 24.0) {
				NPC.frameCounter = 0.0;
			}
			NPC.frame.Y = 60 * (int)(NPC.frameCounter / 6.0);
		}
        public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
            if (!spawnInfo.Water) return 0f;
            return Riven_Hive.SpawnRates.FlyingEnemyRate(spawnInfo) * Riven_Hive.SpawnRates.Flajelly;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Jam_Sandwich>(), 17));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Flagellash>(), 25));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Mask>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Coat>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Pants>(), 525));
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			OriginPlayer.InflictTorn(target, 1800, targetSeverity: 1f - 0.47f);// is this really supposed to last 30 seconds?
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(2, 4));
			} else {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
			}
		}
	}
}
