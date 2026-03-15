using Origins.Core;
using Origins.Dev;
using Origins.Items.Armor.Ashen;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.LootConditions;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.NPCs.Ashen {
	public class Hammerhand : Glowing_Mod_NPC, IWikiNPC, IAshenEnemy, IReplaceAITypeSounds {
		public Rectangle DrawRect => new(0, 0, 40, 60);
		public int AnimationFrames => 6;
		public int FrameDuration => 8;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 6;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			/*NPCID.Sets.DontDoHardmodeScaling[Type] = true;
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
			};*/
			GetInstance<Ashen_Biome.SpawnRates>().AddSpawn(Type, BiomeSpawnChance);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.lifeMax = 80;
			NPC.defense = 22;
			NPC.damage = 24;
			NPC.width = 32;
			NPC.height = 48;
			NPC.value = Item.buyPrice(0, 0, 2);
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.knockBackResist = 0.1f;
			AIType = NPCID.GoblinScout;
			SpawnModBiomes = [
				GetInstance<Ashen_Biome>().Type,
			];
		}
		public override bool PreAI() {
			float acc = 0.25f; // left as a variable for balance testing
			if (NPC.collideY && !NPC.collideX) NPC.velocity.X += acc * NPC.direction;
			NPC.spriteDirection = NPC.direction;
			return true;
		}
		public static float BiomeSpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerInTown) return 0;
			return Ashen_Biome.SpawnRates.PowerZombie;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void FindFrame(int frameHeight) {
			NPC.DoFrames(5, Math.Abs(NPC.velocity.X));
			if (!NPC.collideY && !NPC.IsABestiaryIconDummy && Math.Abs(NPC.velocity.Y) != 0) NPC.DoFrames(1, 0..1);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(new CommonDrop(ItemType<Biocomponent10>(), 1, 1, 3));
			npcLoot.Add(ScavengerBonus.Scrap(1, 1, 2, 5));
			npcLoot.Add(ItemDropRule.Common(ItemType<BBQ_Skewer>(), 19));
			npcLoot.Add(ItemDropRule.Common(ItemType<Ashen2_Helmet>(), 525));
			npcLoot.Add(ItemDropRule.Common(ItemType<Ashen2_Breastplate>(), 525));
			npcLoot.Add(ItemDropRule.Common(ItemType<Ashen2_Greaves>(), 525));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore1");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore2");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore3");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore4");
				for (int i = 0; i < 7; i++) {
					Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
				}
			} else if (Main.rand.NextBool(5)) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
			}
		}
		public bool PlaySound() => true;
	}
}
