using Origins.Dev;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
    public class Power_Suit_Zombie : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 6, 34, 46);
		public int AnimationFrames => 24;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void Load() {
			On_NPC.ScaleStats_ApplyExpertTweaks += (orig, self) => {
				orig(self);
				OriginsSets.NPCs.CustomExpertScaling.GetIfInRange(self.type)?.Invoke(self);
			};
		}
		public override void SetStaticDefaults() {
			NPCID.Sets.ShimmerTransformToNPC[NPC.type] = NPCID.UndeadMiner;//maybe undead viking instead?
			Main.npcFrameCount[NPC.type] = 7;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			NPCID.Sets.DontDoHardmodeScaling[Type] = true;
			OriginsSets.NPCs.CustomExpertScaling[Type] = npc => {
				if (Main.hardMode) {
					int strength = npc.damage + 6 + npc.lifeMax / 4;
					if (strength == 0) strength = 1;
					int targetStrength = 80;
					if (NPC.downedPlantBoss) targetStrength += 20;
					if (strength < targetStrength) {
						float num3 = targetStrength / strength;
						npc.damage = (int)(npc.damage * num3 * 0.9);
						npc.defense = (int)(npc.defense * (num3 + 4) / 5);
						npc.lifeMax = (int)(npc.lifeMax * num3 * 1.1);
						npc.value = (int)(npc.value * num3 * 0.8);
					}
				}
			};
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.lifeMax = 45;
			NPC.defense = 28;
			NPC.damage = 14;
			NPC.width = 28;
			NPC.height = 44;
			NPC.value = 90;
			NPC.friendly = false;
			AIType = NPCID.Zombie;
			AnimationType = NPCID.Zombie;
			Banner = Item.NPCtoBanner(NPCID.Zombie);
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerInTown) return 0;
			if (spawnInfo.Player.ZoneGraveyard || !Main.dayTime) {
				if (spawnInfo.Player.InModBiome<Ashen_Biome>()) return Ashen_Biome.SpawnRates.PowerZombie;
				if (spawnInfo.Player.ZoneForest) return Ashen_Biome.SpawnRates.PowerZombie * 0.08f;
			}
			return 0;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.EmptyBucket, 15));
			npcLoot.Add(ItemDropRule.Common(ItemID.Diamond, 20));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0 || OriginsModIntegrations.CheckAprilFools()) {
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 4);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 4);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 5);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 5);
			}
		}
	}
}
