using Microsoft.Xna.Framework;
using Origins.Items.Accessories;
using Origins.Dev;
using Origins.Items.Armor.Defiled;
using Origins.Items.Materials;
using Origins.Items.Weapons.Demolitionist;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Journal;

namespace Origins.NPCs.Defiled {
	public class Defiled_Brute : Glowing_Mod_NPC, IDefiledEnemy, IWikiNPC, IJournalEntrySource {
		public Rectangle DrawRect => new(0, 4, 74, 62);
		public int AnimationFrames => 16;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public AssimilationAmount? Assimilation => 0.08f;
		public const float speedMult = 0.75f;
		//public float SpeedMult => npc.frame.Y==510?1.6f:0.8f;
		//bool attacking = false;
		public string EntryName => "Origins/" + typeof(Defiled_Krusher_Entry).Name;
		public class Defiled_Krusher_Entry : JournalEntry {
			public override string TextKey => "Defiled_Krusher";
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 4;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 160;
			NPC.defense = 9;
			NPC.damage = 49;
			NPC.width = 33;
			NPC.height = 48;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt.WithPitchRange(0.5f, 0.75f);
			NPC.DeathSound = Origins.Sounds.DefiledKill.WithPitchRange(0.5f, 0.75f);
			NPC.value = 103;
			NPC.knockBackResist = 0.5f;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public int MaxMana => 200;
		public int MaxManaDrain => 24;
		public float Mana { get; set; }
		public void Regenerate(out int lifeRegen) {
			int factor = 37 / ((NPC.life / 40) + 2);
			lifeRegen = factor;
			Mana -= factor / 90f;// 3 mana for every 2 health regenerated
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.SpawnTileY > Main.worldSurface || spawnInfo.DesertCave) return 0;
			return Defiled_Wastelands.SpawnRates.LandEnemyRate(spawnInfo, false) * Defiled_Wastelands.SpawnRates.Brute;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bombardment>(), 48));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Lousy_Liver>(), 87));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Helmet>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Breastplate>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Greaves>(), 525));
		}

		public override bool? CanFallThroughPlatforms() => NPC.directionY == 1 && NPC.target >= 0 && NPC.targetRect.Bottom > NPC.position.Y + NPC.height + NPC.velocity.Y;
		public override bool PreAI() {
			if (Main.rand.NextBool(1000)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(0.5f, 0.75f), NPC.Center);
			//if(!attacking) {
			if (NPC.collideY && Math.Sign(NPC.velocity.X) == NPC.direction) NPC.velocity.X /= speedMult;
			//npc.Hitbox = new Rectangle((int)npc.position.X+(npc.oldDirection == 1 ? 70 : 52), (int)npc.position.Y, 56, npc.height);
			//}
			return true;
		}
		public override void AI() {
			NPC.TargetClosest();
			/*if(npc.localAI[3]<=0&&npc.targetRect.Intersects(new Rectangle((int)npc.position.X-(npc.direction == 1 ? 70 : 52), (int)npc.position.Y, 178, npc.height))) {
				if(!attacking) {
					npc.frame = new Rectangle(0, 680, 182, 170);
					npc.frameCounter = 0;
					npc.velocity.X*=0.25f;
				}
				attacking = true;
			}
			if(npc.localAI[3]>0)npc.localAI[3]--;*/
			if (NPC.HasPlayerTarget) {
				NPC.spriteDirection = NPC.direction;
			}
			/*if(attacking) {
				if(++npc.frameCounter>7) {
					//add frame height to frame y position and modulo by frame height multiplied by walking frame count
					if(npc.frame.Y>=1018) {
						if(npc.frameCounter>19) {
							npc.frame = new Rectangle(0, 0, 182, 170);
							npc.frameCounter = 0;
							attacking = false;
							npc.localAI[3] = 60;
						}
					} else {
						npc.frame = new Rectangle(0, (npc.frame.Y+170)%1190, 182, 170);
						npc.frameCounter = 0;
					}
				}
				if (npc.collideY) {
					npc.velocity.X*=0.5f;
				}
			//}else{*/
			if (NPC.collideY && Math.Sign(NPC.velocity.X) == NPC.direction) NPC.velocity.X *= speedMult;
			//}
		}
		public override void FindFrame(int frameHeight) {
			if (++NPC.frameCounter > 9) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 62) % 248, 74, 60);
				NPC.frameCounter = 0;
			}
		}
		public override void PostAI() {
			//if(!attacking) {
			//if(npc.collideY&&Math.Sign(npc.velocity.X)==npc.direction)npc.velocity.X*=SpeedMult;
			//npc.Hitbox = new Rectangle((int)npc.position.X+(npc.oldDirection == 1 ? 70 : 52), (int)npc.position.Y, 56, npc.height);
			//}
		}
		/*public void GetMeleeCollisionData(Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect, ref float knockbackMult) {
			bool flip = npc.direction == 1;
			//Rectangle armHitbox = new Rectangle((int)npc.position.X+(flip?0:108), (int)npc.position.Y, 70, npc.height);
			bool h = victimHitbox.Intersects(npcRect);
			if(attacking) {
				if(npc.frame.Y>=1018) {
					npcRect = new Rectangle(npcRect.Center.X+(npc.direction*63), npcRect.Y, 52, npc.height);
					if(npc.frameCounter<9&&victimHitbox.Intersects(npcRect)) {
						damageMultiplier = 3;
						knockbackMult = 2f;
						return;
					}
				}
			}
			/*if(victimHitbox.Intersects(armHitbox)) {
				npcRect = armHitbox;
				return;
			}
			npcRect = new Rectangle((int)npc.position.X+(flip?70:52), (int)npc.position.Y, 56, npc.height);* /
		}*/
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVectorIn(spawnbox), projectile.velocity, "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			int halfWidth = NPC.width / 2;
			int baseX = player.direction > 0 ? 0 : halfWidth;
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), hit.GetKnockbackFromHit(), "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
				for (int i = 0; i < 10; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
			}
		}
	}
}
