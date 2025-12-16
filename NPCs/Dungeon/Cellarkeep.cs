using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Summoner;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.Utilities;

namespace Origins.NPCs.Dungeon {
	public class Cellarkeep : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 4, 40, 56);
		public int AnimationFrames => 48;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public Range FrameRange => new(8, 48);
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 18;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.AngryBones);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 81;
			NPC.defense = 10;
			NPC.damage = 33;
			NPC.width = 20;
			NPC.height = 38;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit2;
			NPC.DeathSound = SoundID.NPCDeath24.WithPitch(0.6f);
			NPC.knockBackResist = 0.5f;
			NPC.value = 90;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!spawnInfo.HasRightDungeonWall(NPCExtensions.DungeonWallType.Slab)) return 0;
			return SpawnCondition.DungeonNormal.Chance * 0.1f;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheDungeon
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<Joint_Pop>(), 10, 5));
			npcLoot.Add(new DropBasedOnExpertMode(
				ItemDropRule.Common(ModContent.ItemType<Explosive_Barrel>(), 3, 15, 30),
				ItemDropRule.Common(ModContent.ItemType<Explosive_Barrel>(), 2, 20, 40)
			));
			npcLoot.Add(ItemDropRule.Common(ItemID.AncientNecroHelmet, 450));
			npcLoot.Add(ItemDropRule.Common(ItemID.ClothierVoodooDoll, 300));
			npcLoot.Add(ItemDropRule.Common(ItemID.BoneWand, 250)).OnFailedRoll(ItemDropRule.Common(ItemID.TallyCounter, 100)).OnFailedRoll(ItemDropRule.Common(ItemID.GoldenKey, 65)).OnFailedRoll(ItemDropRule.ByCondition(new Conditions.NotExpert(), ItemID.Bone, 1, 1, 3));
			npcLoot.Add(ItemDropRule.ByCondition(new Conditions.IsExpert(), ItemID.Bone, 1, 2, 6));
		}
		public override bool PreAI() {
			switch (NPC.aiAction) {
				case 0:
				if (NPC.collideY && NPC.HasValidTarget) {
					const int index = 0;
					NPC.localAI[index] += Main.rand.NextFloat(0.5f, 1);
					if (NPC.localAI[index] > 180) {
						NPC.aiAction = 1;
						NPC.localAI[index] = 0;
					}
				}
				break;
				case 1:
				int frame = (int)NPC.localAI[0]++;
				if (frame == 0 && Main.netMode != NetmodeID.MultiplayerClient) {
					NPC.localAI[3] = NPC.NewNPC(
						NPC.GetSource_FromThis(),
						(int)NPC.position.X,
						(int)NPC.position.Y,
						ModContent.NPCType<Cellarkeep_Barrel>(),
						ai0:NPC.whoAmI
					);
				}
				const int timePerFrame = 24;
				if (frame == timePerFrame) {
					NPC barrel = Main.npc[(int)NPC.localAI[3]];
					barrel.ai[0] = -1;
					barrel.velocity = new Vector2(NPC.direction * 4, -4);
					barrel.netUpdate = true;
					NPC.localAI[3] = -1;
				}
				if (NPC.collideY) NPC.velocity *= 0.95f;
				if (frame / timePerFrame > 2) {
					NPC.localAI[0] = 0;
					NPC.aiAction = 0;
					NPC.frame = new Rectangle(6, 0, 32, 56);
					DrawOffsetY = 0;
					return true;
				}
				DrawOffsetY = 12;
				NPC.frame = new Rectangle(0, 870 + 46 * (frame / timePerFrame), 40, 44);
				NPC.frameCounter = 0;
				return false;
			}
			return true;
		}
		public override void AI() {
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) {
				NPC.spriteDirection = NPC.direction;
			}
		}
		public override void FindFrame(int frameHeight) {
			if (NPC.aiAction != 1 && NPC.velocity.Y == 0 && ++NPC.frameCounter > 7) {
				NPC.frame = new Rectangle(6, (NPC.frame.Y + 58) % 870, 32, 56);
				NPC.frameCounter = 0;
			}
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life > 0) {
				for (int i = (int)(hit.Damage / (float)NPC.lifeMax * 50f); i-->0;) {
					Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Bone, hit.HitDirection, -1f);
				}
			} else {
				for (int i = 0; i < 20; i++) {
					Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Bone, 2.5f * hit.HitDirection, -2.5f);
				}

				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 42, NPC.scale);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 43, NPC.scale);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 43, NPC.scale);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 44, NPC.scale);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 44, NPC.scale);
			}
		}
	}
	public class Cellarkeep_Barrel : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 26, 26);
		public int AnimationFrames => 1;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new() {
				Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
			});
		}
		public override void SetDefaults() {
			NPC.aiStyle = -1;
			NPC.lifeMax = 40;
			NPC.defense = 0;
			NPC.damage = 67;
			NPC.height = NPC.width = 26;
			NPC.knockBackResist = 0;
			NPC.noGravity = true;
			NPC.chaseable = false;
		}
		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) {
			if (modifiers.DamageType?.CountsAsClass<Explosive>() ?? false) modifiers.SetInstantKill();
			modifiers.SourceDamage.Base += 8;
		}
		public override void AI() {
			if (NPC.ai[0] >= 0) {
				NPC thrower = Main.npc[(int)NPC.ai[0]];
				NPC.Bottom = thrower.Top;
				NPC.position.Y += 8;
				NPC.direction = thrower.direction;
			} else {
				NPC.noGravity = false;
				//NPC.velocity.X = NPC.direction * 4;
				if (++NPC.frameCounter > 4) {
					if (NPC.direction == 1) {
						switch ((NPC.spriteDirection, NPC.directionY)) {
							case (1, 1):
							NPC.spriteDirection = -1;
							break;
							case (-1, 1):
							NPC.directionY = -1;
							break;
							case (-1, -1):
							NPC.spriteDirection = 1;
							break;
							case (1, -1):
							NPC.directionY = 1;
							break;
						}
					} else {
						switch ((NPC.spriteDirection, NPC.directionY)) {
							case (1, 1):
							NPC.directionY = -1;
							break;
							case (1, -1):
							NPC.spriteDirection = -1;
							break;
							case (-1, -1):
							NPC.directionY = 1;
							break;
							case (-1, 1):
							NPC.spriteDirection = 1;
							break;
						}
					}
					NPC.frameCounter = 0;
				}
				if (NPC.collideX) {
					NPC.direction *= -1;
					NPC.velocity.X = -4 * System.Math.Sign(NPC.velocity.X);
					NPC.HitInfo hit = new() {
						Damage = 4,
						HideCombatText = true
					};
					NPC.StrikeNPC(hit, noPlayerInteraction: true);
					//NetMessage.SendStrikeNPC(NPC, hit);
				}
				/*if (NPC.collideX) {
					NPC.StrikeInstantKill();
				}*/
			}
		}
		public override void UpdateLifeRegen(ref int damage) {
			if (NPC.onFire || NPC.onFire2 || NPC.onFire3) {
				NPC.lifeRegen = NPC.lifeMax * 2;
				damage = NPC.lifeMax;
			}
			if (NPC.collideY) {
				//damage = 0;
				//NPC.lifeRegen -= 8;
			}
		}
		public override void OnKill() {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				Projectile.NewProjectile(
					NPC.GetSource_Death(),
					NPC.Center,
					default,
					ModContent.ProjectileType<Cellarkeep_Barrel_Explosion>(),
					NPC.damage,
					4,
					Main.myPlayer,
					ai1: NPC.ai[0]
				);
			}
		}
		public override void DrawBehind(int index) {
			Main.instance.DrawCacheNPCProjectiles.Add(index);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			SoundEngine.PlaySound((hit.Damage > NPC.lifeMax / 2 ? SoundID.NPCDeath63.WithPitchRange(0.6f, 1.0f) : SoundID.NPCHit3.WithVolumeScale(1.5f).WithPitchRange(-0.4f, 0.0f)), NPC.Center);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			SpriteEffects effects = SpriteEffects.None;
			if (NPC.spriteDirection == -1) effects |= SpriteEffects.FlipHorizontally;
			if (NPC.directionY == -1) effects |= SpriteEffects.FlipVertically;
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				NPC.position - screenPos,
				null,
				drawColor,
				0,
				default,
				1,
				effects,
			0);
			return false;
		}
	}
	public class Cellarkeep_Barrel_Explosion : ModProjectile, IIsExplodingProjectile {
		public override string Texture => "Origins/CrossMod/Thorium/Items/Weapons/Bard/Sonorous_Shredder_P";
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.friendly = true;
			Projectile.hostile = true;
			Projectile.trap = true;
			Projectile.hide = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item62);
				Projectile.ai[0] = 1;
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (target.whoAmI == Projectile.ai[1]) {
				void Modifiers_ModifyHitInfo(ref NPC.HitInfo info) {
					info.Damage = target.lifeMax;
				}
				modifiers.ModifyHitInfo += Modifiers_ModifyHitInfo;
			}
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding => true;
	}
}
