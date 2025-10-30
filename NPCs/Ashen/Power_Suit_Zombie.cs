using Origins.Dev;
using Origins.Items.Armor.Ashen;
using Origins.Items.Materials;
using Origins.Journal;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.NPCs.Ashen {
	public class Power_Suit_Zombie : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 34, 46);
		public int AnimationFrames => 7;
		public int FrameDuration => 5;
		public override void Load() {
			/*On_NPC.ScaleStats_ApplyExpertTweaks += (orig, self) => {
				orig(self);
				OriginsSets.NPCs.CustomExpertScaling.GetIfInRange(self.type)?.Invoke(self);
			};*/
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 7;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			NPCID.Sets.DontDoHardmodeScaling[Type] = true;
			OriginsSets.NPCs.CustomExpertScaling[Type] = npc => {
				if (Main.hardMode) {
					int strength = npc.damage + 6 + npc.lifeMax / 4;
					if (strength == 0) strength = 1;
					int targetStrength = 80;
					if (NPC.downedPlantBoss) targetStrength += 40;
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
			NPC.defense = 14;
			NPC.damage = 18;
			NPC.width = 24;
			NPC.height = 38;
			NPC.value = Item.buyPrice(0, 0, 2);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			AIType = NPCID.Zombie;
			Banner = Item.NPCtoBanner(NPCID.Zombie);
			SpawnModBiomes = [
				GetInstance<Ashen_Biome>().Type,
			];
		}
		public override bool PreAI() {
			float acc = 0.5f; // left as a variable for balance testing
			if (NPC.collideY && !NPC.collideX) NPC.velocity.X += acc * NPC.direction;
			NPC.spriteDirection = OriginsModIntegrations.CheckAprilFools() ? -NPC.direction : NPC.direction;
			return true;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerInTown) return 0;
			if (spawnInfo.Player.ZoneGraveyard || !Main.dayTime) {
				if (spawnInfo.Player.InModBiome<Ashen_Biome>()) return Ashen_Biome.SpawnRates.PowerZombie;
				else return Ashen_Biome.SpawnRates.PowerZombie * 0.08f;
			}
			return 0;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime
			);
		}
		public override void FindFrame(int frameHeight) {
			if (Math.Abs(NPC.velocity.X) > 0) NPC.DoFrames(5);
			if (!NPC.collideY && !NPC.IsABestiaryIconDummy) NPC.DoFrames(1, 3..3);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.Amber, 20));
			npcLoot.Add(ItemDropRule.ByCondition(new Journal_Entry_Condition(Journal_Registry.GetJournalEntryByTextKey(GetInstance<Worn_Paper_They_Found_Us>().PaperName)), ItemType<Worn_Paper_They_Found_Us>(), 40));
			npcLoot.Add(new CommonDrop(ItemType<Ashen2_Helmet>(), 300, 1, 1, 11));
			npcLoot.Add(new CommonDrop(ItemType<Ashen2_Breastplate>(), 300, 1, 1, 11));
			npcLoot.Add(new CommonDrop(ItemType<Ashen2_Greaves>(), 300, 1, 1, 11));
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
