using Origins.Dev;
using Origins.Items.Armor.Defiled;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Weapons.Ranged;
using Origins.Journal;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Ancient_Defiled_Cyclops : ModNPC, IMeleeCollisionDataNPC, IDefiledEnemy, IWikiNPC, IJournalEntrySource {
		public const float speedMult = 1f;
		bool attacking = false;
		public Rectangle DrawRect => new(-3, 6, 90, 118);
		public int AnimationFrames => 1;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public float ZapWeakness => 0.5f;
		public class Ancient_Defiled_Cyclops_Entry : JournalEntry {
			public override string TextKey => "Ancient_Defiled_Cyclops";
			public override JournalSortIndex SortIndex => new("The_Defiled", 1);
		}
		public string EntryName => "Origins/" + typeof(Ancient_Defiled_Cyclops_Entry).Name;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 7;
			ItemID.Sets.KillsToBanner[Type] *= 3;
			ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 300;
			NPC.defense = 12;
			NPC.damage = 63;
			NPC.width = 45;
			NPC.height = 114;
			NPC.friendly = false;
			NPC.value = 10000;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type,
				ModContent.GetInstance<Underground_Defiled_Wastelands_Biome>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public bool ForceSyncMana => false;
		public float Mana { get; set; }
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.DesertCave) return 0;
			return Defiled_Wastelands.SpawnRates.LandEnemyRate(spawnInfo, false) * Defiled_Wastelands.SpawnRates.AncientCyclops;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Latchkey>(), 5, 3, 7));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ancient_Kruncher>()));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Helmet>(), 14));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Breastplate>(), 14));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Greaves>(), 14));
		}
		public override bool? CanFallThroughPlatforms() => NPC.directionY == 1 && NPC.target >= 0 && NPC.targetRect.Bottom > NPC.position.Y + NPC.height + NPC.velocity.Y;
		public override void AI() {
			NPC.TargetClosest();
			if (NPC.Hitbox.Intersects(NPC.targetRect)) {
				if (!attacking) {
					NPC.frame = new Rectangle(0, 120 * 4, 96, 120);
					NPC.frameCounter = 0;
					NPC.velocity.X *= 0.25f;
				}
				attacking = true;
			}
			if (NPC.HasPlayerTarget) {
				NPC.spriteDirection = NPC.direction;
			}
			if (attacking) {
				if (++NPC.frameCounter > 7) {
					//add frame height to frame y position and modulo by frame height multiplied by walking frame count
					if (NPC.frame.Y >= 720) {
						if (NPC.frameCounter > 19) {
							NPC.frame = new Rectangle(0, 0, 96, 120);
							NPC.frameCounter = 0;
							attacking = false;
						}
					} else {
						NPC.frame = new Rectangle(0, (NPC.frame.Y + 120) % 840, 96, 120);
						NPC.frameCounter = 0;
					}
				}
			} else {
				if (NPC.collideY && Math.Sign(NPC.velocity.X) == NPC.direction) NPC.velocity.X /= speedMult;
				if (++NPC.frameCounter > 7) {
					//add frame height to frame y position and modulo by frame height multiplied by walking frame count
					NPC.frame = new Rectangle(0, (NPC.frame.Y + 120) % (840 - 120 * 3), 96, 120);
					NPC.frameCounter = 0;
				}
			}
		}
		public override void PostAI() {
			if (NPC.collideY && Math.Sign(NPC.velocity.X) == NPC.direction) NPC.velocity.X *= speedMult;
		}

		public void GetMeleeCollisionData(Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect, ref float knockbackMult) {
			if (attacking) {
				if (NPC.frame.Y >= 720) {
					int hitboxWidth = 32;
					int hitboxHeight = 64;

					Rectangle armHitbox = new((int)NPC.Center.X + ((NPC.width / 2) * NPC.direction) - hitboxWidth / 2, (int)NPC.Center.Y - hitboxHeight / 2, hitboxWidth, hitboxHeight);
					if (NPC.frameCounter < 9 && victimHitbox.Intersects(armHitbox)) {
						damageMultiplier = 2;
						knockbackMult = 1.5f;
						npcRect = armHitbox;
						return;
					}
				}
			}
			npcRect.Width /= 2;
			npcRect.X += npcRect.Width / 2;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
			}
		}
	}
}