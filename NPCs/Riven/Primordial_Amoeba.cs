#define tentacleDamage
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Armor.Vanity.BossMasks;
using Origins.Items.Materials;
using Origins.Items.Other.LootBags;
using Origins.Items.Tools;
using Origins.Items.Weapons.Summoner;
using Origins.LootConditions;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    [AutoloadBossHead]
    public class Primordial_Amoeba : ModNPC {
		public override string BossHeadTexture => "Origins/UI/BossMap/Map_Icon_DA";
		internal static int npcIndex;
        public static int DifficultyMult => Main.masterMode ? 3 : (Main.expertMode ? 2 : 1);
		internal static IItemDropRule normalDropRule;
		public override void Unload() {
			normalDropRule = null;
		}
		public override void SetStaticDefaults() {
            DisplayName.SetDefault("Primordial Amoeba");
            NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData {
                SpecificallyImmuneTo = new int[] {
                    BuffID.Confused
	            }
            };
            NPCID.Sets.DebuffImmunitySets[Type] = debuffData;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.Zombie);
            NPC.boss = true;
            NPC.BossBar = ModContent.GetInstance<Boss_Bar_PA>();
            NPC.aiStyle = NPCAIStyleID.None;
            NPC.lifeMax = 1800;
            NPC.defense = 14;
            NPC.damage = 23;
            NPC.width = 144;
            NPC.height = 148;
            NPC.friendly = false;
            NPC.HitSound = Origins.Sounds.PowerUp.WithPitchRange(0f, 0.25f);
            NPC.DeathSound = Origins.Sounds.EnergyRipple.WithPitchRange(-1f, -0.75f);
            NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.npcSlots = 200;
            Music = Origins.Music.RivenBoss;
            NPC.knockBackResist = 0f;// actually a multiplier
			NPC.value = Item.buyPrice(gold: 5);
		}
		public override void ScaleExpertStats(int numPlayers, float bossLifeScale) {
			switch (DifficultyMult) {
                case 1:
                NPC.lifeMax = (int)(1800 * bossLifeScale);
				NPC.damage = 23;
				break;

                case 2:
                NPC.lifeMax = (int)(2700 * bossLifeScale) / 2;
				NPC.damage = 36;
				break;

                case 3:
                NPC.lifeMax = (int)(3600 * bossLifeScale) / 3;
				NPC.damage = 49;
				break;
            }
		}

        public override void OnSpawn(IEntitySource source) {
            if (Main.netMode == NetmodeID.Server) {
                ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasAwoken", NPC.GetTypeNetName()), new Color(222, 222, 222));
            } else {
                if (Main.netMode == NetmodeID.SinglePlayer) {
                    Main.NewText(Language.GetTextValue("Announcement.HasAwoken", NPC.TypeName), 222, 222, 222);
                }
                SoundEngine.PlaySound(Origins.Sounds.EnergyRipple.WithPitch(-1));
            }
        }
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement("A murderous super-organism just trying to kill you."),
            });
		}
        public override void ModifyNPCLoot(NPCLoot npcLoot) {

			normalDropRule = new LeadingSuccessRule();

			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Encrusted_Ore_Item>(), 1, 140, 330));
			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Riven_Sample>(), 1, 40, 100));
			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Amoeba_Hook>(), 1));
			normalDropRule.OnSuccess(ItemDropRule.OneFromOptions(1, ModContent.ItemType<Teardown>(), ModContent.ItemType<Return_To_Sender>()));

			//normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<PA_Trophy_Item>(), 10));
			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<PA_Mask>(), 10));

			npcLoot.Add(new DropBasedOnExpertMode(
				normalDropRule,
				new DropLocalPerClientAndResetsNPCMoneyTo0(ModContent.ItemType<Primordial_Amoeba_Bag>(), 1, 1, 1, null)
			));
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Protozoa_Food>(), 4));
		}
		public override bool PreAI() {
			const bool useVanillaAI = false;
			int tentacleID = Primordial_Amoeba_Tentacle.ID;
			npcIndex = NPC.whoAmI;
			if (NPC.localAI[0] == 0f && Main.netMode != NetmodeID.MultiplayerClient) {
				NPC.localAI[0] = 1f;

				for (int i = 0; i < 7; i++) {
					NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, tentacleID, NPC.whoAmI);
				}
			}

			Vector2 center = NPC.Center;
			int difficultyMult = DifficultyMult;// just saving the value as a slight optimization
			bool runAway = false;
			NPC.TargetClosest();
			Player target = Main.player[NPC.target];
			if (target.dead) {
				runAway = true;
			}
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				int range = 6000;
				if (Math.Abs(center.X - target.Center.X) + Math.Abs(center.Y - target.Center.Y) > range) {
					NPC.active = false;
					NPC.life = 0;
					if (Main.netMode == NetmodeID.Server) {
						NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);
					}
				}
			}

			float targetDiffX = target.Center.X - center.X;
			float targetDiffY = target.Center.Y - center.Y;
			NPC.rotation = (float)Math.Atan2(targetDiffY, targetDiffX) + 1.57f;
			NPC.damage = NPC.GetAttackDamage_ScaledByStrength(NPC.defDamage);
			#region projectile attack
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				NPC.localAI[1] += 1f;
				if (NPC.life < NPC.lifeMax * 0.9f) {
					NPC.localAI[1] += 0.6f * difficultyMult;
				}
				if (NPC.life < NPC.lifeMax * 0.8f) {
					NPC.localAI[1] += 0.45f * difficultyMult;
				}
				if (NPC.life < NPC.lifeMax * 0.7f) {
					NPC.localAI[1] += 0.45f * difficultyMult;
				}
				if (NPC.life < NPC.lifeMax * 0.6f) {
					NPC.localAI[1] += 0.4f * difficultyMult;
				}
				if (runAway) {
					NPC.localAI[1] += 2f * difficultyMult;
				}
				if (Main.expertMode && NPC.justHit && Main.rand.NextBool(2)) {
					NPC.localAI[3] = 1f;
				}
				if (Main.getGoodWorld) {
					NPC.localAI[1] += 1f * difficultyMult;
				}
				if (NPC.localAI[1] > 900f) {
					NPC.localAI[1] = 0f;
					bool canShootAtTarget = Collision.CanHit(NPC.position, NPC.width, NPC.height, target.position, target.width, target.height);
					if (NPC.localAI[3] > 0f) {
						canShootAtTarget = true;
						NPC.localAI[3] = 0f;
					}
					if (canShootAtTarget) {
						Vector2 projPos = new Vector2(NPC.Center.X, NPC.Center.Y);
						float projSpeed = 10f + difficultyMult;
						Vector2 projVel = target.position + target.Size * 0.5f - projPos;
						float normFact = projVel.Length();
						normFact = projSpeed / normFact;
						projVel *= normFact;
						int projDamage = 13;
						int projType = Amoebic_Gel_P.ID;
						if (runAway) {
							projDamage *= 2;
						}
						projDamage = NPC.GetAttackDamage_ForProjectiles(projDamage, projDamage * 0.9f);
						projPos += projVel * 3f;
						for (int i = 0; i < 5; i++) {
							Projectile.NewProjectile(
								NPC.GetSource_FromAI(),
								projPos,
								projVel.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.85f, 1f),
								projType,
								projDamage,
								0f,
								Main.myPlayer
							);
						}
					}
				}
			}
			#endregion
			if (!NPC.HasValidTarget) {
				NPC.EncourageDespawn(10);
			}

			int[] tentacles = new int[7];
			float tentacleCenterX = 0f;
			float tentacleCenterY = 0f;
			int tentacleCount = 0;
			for (int i = 0; i < 200; i++) {
				if (Main.npc[i].active && Main.npc[i].type == tentacleID) {
					tentacleCenterX += Main.npc[i].Center.X;
					tentacleCenterY += Main.npc[i].Center.Y;
					tentacles[tentacleCount] = i;
					tentacleCount++;
					if (tentacleCount >= 7) {
						break;
					}
				}
			}
			NPC.dontTakeDamage = false;
			if (difficultyMult > 1 && tentacleCount > 0 && NPC.life < NPC.lifeMax / 2) {
				NPC.dontTakeDamage = true;
			}
			NPC.defense = 12 + (tentacleCount + 1) * 2 * difficultyMult;
			if (runAway) {
				NPC.defense *= 2;
				NPC.damage *= 2;
			}
			if (tentacleCount <= 0) {
				if (Collision.IsClearSpotTest(NPC.position + new Vector2(16), 16f, NPC.width - 32, NPC.height - 32, checkSlopes: true)) {
					NPC.noTileCollide = false;
					NPC.noGravity = false;
				} else {
					if (Math.Abs(targetDiffX) > Math.Abs(targetDiffY)) {
						NPC.velocity.X = Math.Sign(targetDiffX);
					} else {
						NPC.velocity.Y = Math.Sign(targetDiffY);
					}
				}
				return true;
			}
			NPC.noTileCollide = true;
			NPC.noGravity = true;
			if (tentacleCount < 7 && NPC.life > NPC.lifeMax && Main.netMode != NetmodeID.MultiplayerClient && difficultyMult > 1) {
				NPC.ai[2] += difficultyMult + 1;
				if (NPC.ai[2] > 1600) {
					NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, tentacleID, NPC.whoAmI);
					NPC.ai[2] = 0;
				}
			}
			tentacleCenterX /= tentacleCount;
			tentacleCenterY /= tentacleCount;
			float speedFactor = 2.75f;
			float moveAccelleration = 0.025f;
			if (NPC.life < NPC.lifeMax / 2) {
				speedFactor *= 1.15f;
				moveAccelleration *= 1.15f;
			}
			if (NPC.life < NPC.lifeMax / 4) {
				speedFactor *= 1.15f;
			}
			if (!target.InModBiome<Riven_Hive>()) {
				runAway = true;
				speedFactor += 6.4f;
				moveAccelleration *= 3f;
			}
			if (Main.expertMode) {
				speedFactor += 0.3f;
				speedFactor *= 1.05f;
				moveAccelleration += 0.01f;
				moveAccelleration *= 1.05f;
			}
			if (Main.getGoodWorld) {
				speedFactor *= 1.15f;
				moveAccelleration *= 1.15f;
			}
			Vector2 tentacleCenter = new Vector2(tentacleCenterX, tentacleCenterY);
			float tentacleDiffX = target.Center.X - tentacleCenter.X;
			float tentacleDiffY = target.Center.Y - tentacleCenter.Y;
			if (runAway || Math.Sqrt(targetDiffX * targetDiffX + targetDiffY * targetDiffY) < 128) {
				tentacleDiffY *= -1f;
				tentacleDiffX *= -1f;
				speedFactor += 8f;
			}
			float tentacleDist = (float)Math.Sqrt(tentacleDiffX * tentacleDiffX + tentacleDiffY * tentacleDiffY);
			int distFactor = 500;
			if (runAway) {
				distFactor += 350;
			}
			if (Main.expertMode) {
				distFactor += 100;
			}
			if (tentacleDist >= distFactor) {
				tentacleDist = distFactor / tentacleDist;
				tentacleDiffX *= tentacleDist;
				tentacleDiffY *= tentacleDist;
			}
			tentacleCenterX += tentacleDiffX;
			tentacleCenterY += tentacleDiffY;
			tentacleDiffX = tentacleCenterX - center.X;
			tentacleDiffY = tentacleCenterY - center.Y;
			tentacleDist = (float)Math.Sqrt(tentacleDiffX * tentacleDiffX + tentacleDiffY * tentacleDiffY);

			if (tentacleDist < speedFactor) {
				//
				tentacleDiffX = NPC.velocity.X;
				tentacleDiffY = NPC.velocity.Y;
			} else {
				tentacleDist = speedFactor / tentacleDist;
				tentacleDiffX *= tentacleDist;
				tentacleDiffY *= tentacleDist;

			}

			if (NPC.velocity.X < tentacleDiffX) {
				NPC.velocity.X += moveAccelleration;
				if (NPC.velocity.X < 0f && tentacleDiffX > 0f) {
					NPC.velocity.X += moveAccelleration * 2f;
				}
			} else if (NPC.velocity.X > tentacleDiffX) {
				NPC.velocity.X -= moveAccelleration;
				if (NPC.velocity.X > 0f && tentacleDiffX < 0f) {
					NPC.velocity.X -= moveAccelleration * 2f;
				}
			}
			if (NPC.velocity.Y < tentacleDiffY) {
				NPC.velocity.Y += moveAccelleration;
				if (NPC.velocity.Y < 0f && tentacleDiffY > 0f) {
					NPC.velocity.Y += moveAccelleration * 2f;
				}
			} else if (NPC.velocity.Y > tentacleDiffY) {
				NPC.velocity.Y -= moveAccelleration;
				if (NPC.velocity.Y > 0f && tentacleDiffY < 0f) {
					NPC.velocity.Y -= moveAccelleration * 2f;
				}
			}
			return useVanillaAI;
		}
		public override bool SpecialOnKill() {
			return false;
		}
		public override void OnKill() {
            NPC.downedBoss2 = true;
			int tentacleID = Primordial_Amoeba_Tentacle.ID;
			int tentacleCount = 0;
			for (int i = 0; i < 200; i++) {
				if (Main.npc[i].active && Main.npc[i].type == tentacleID) {
					Main.npc[i].StrikeNPCNoInteraction(9999, 0, 0);
					tentacleCount++;
					if (tentacleCount >= 7) {
						break;
					}
				}
			}
		}
		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) {
            Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
            for(int i = Main.rand.Next(3); i-->0;)Gore.NewGore(NPC.GetSource_OnHurt(projectile), Main.rand.NextVectorIn(spawnbox), projectile.velocity, Mod.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4)));
        }
        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit) {
            int halfWidth = NPC.width / 2;
            int baseX = player.direction > 0 ? 0 : halfWidth;
            for(int i = Main.rand.Next(3); i-->0;)Gore.NewGore(NPC.GetSource_OnHurt(player), NPC.position+new Vector2(baseX + Main.rand.Next(halfWidth),Main.rand.Next(NPC.height)), new Vector2(knockback*player.direction, -0.1f*knockback), Mod.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4)));
        }
        public override void HitEffect(int hitDirection, double damage) {
            if(NPC.life < 0) {
                for(int i = 0; i < 6; i++)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(Main.rand.Next(NPC.width),Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4)));
                for(int i = 0; i < 10; i++)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(Main.rand.Next(NPC.width),Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/R_Effect_Meat" + Main.rand.Next(1,4)));
            }
        }
	}
    public class Primordial_Amoeba_Tentacle : ModNPC {
		public override string Texture => "Origins/Items/Weapons/Summoner/Flagellash_P";
		public static int ID { get; private set; }
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Primordial Amoeba");
			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData {
				SpecificallyImmuneTo = new int[] {
					BuffID.Confused
				}
			};
			NPCID.Sets.DebuffImmunitySets[Type] = debuffData;
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.PlanterasHook);
			NPC.aiStyle = NPCAIStyleID.None;
			NPC.damage = 13;
			NPC.dontTakeDamage = false;
			NPC.lifeMax = 800;
			NPC.width = NPC.height = 24;
			NPC.defense = 8;
		}
		public override void ScaleExpertStats(int numPlayers, float bossLifeScale) {
			switch (Primordial_Amoeba.DifficultyMult) {
				case 1:
				NPC.lifeMax = (int)(800 * bossLifeScale);
				NPC.damage = 13;
				NPC.defense = 8;
				break;

				case 2:
				NPC.lifeMax = (int)(1000 * bossLifeScale) / 2;
				NPC.damage = 17;
				NPC.defense = 10;
				break;

				case 3:
				NPC.lifeMax = (int)(1200 * bossLifeScale) / 3;
				NPC.damage = 20;
				NPC.defense = 12;
				break;
			}
		}
		public override void OnSpawn(IEntitySource source) {
			int hitboxID = Primordial_Amoeba_Tentacle_Hitbox.ID;
			for (int i = 0; i < 14; i++) {
				 NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, hitboxID, NPC.whoAmI, NPC.whoAmI, (i + 1) / 15f);
			}
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven_Sample>(), 2, 2, 4));
			npcLoot.Add(ItemDropRule.ByCondition(new Conditions.PlayerNeedsHealing(), ItemID.Heart, 2));
		}
		public override bool PreKill() {
			//NPC.Center = (NPC.Center + Main.npc[Primordial_Amoeba.npcIndex].Center) * 0.5f;
			return true;
		}
		public override void AI() {
			bool targetLeftBiome = false;
			bool targetDead = false;
			NPC.timeLeft = 937;
			if (Primordial_Amoeba.npcIndex < 0) {
				NPC.StrikeNPCNoInteraction(9999, 0f, 0);
				NPC.netUpdate = true;
				return;
			}
			NPC bodyNPC = Main.npc[Primordial_Amoeba.npcIndex];
			if (!bodyNPC.active) {
				NPC.active = false;
				NPC.netUpdate = true;
				return;
			}
			Player targetPlayer = Main.player[bodyNPC.target];
			Vector2 center = NPC.Center;
			if (targetPlayer.dead) {
				targetDead = true;
			}
			if (!targetPlayer.InModBiome<Riven_Hive>() || targetDead) {
				NPC.localAI[0] -= 4f;
				targetLeftBiome = true;
			}
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				if (NPC.ai[0] == 0f) {
					NPC.ai[0] = (int)(NPC.Center.X / 16f);
				}
				if (NPC.ai[1] == 0f) {
					NPC.ai[1] = (int)(NPC.Center.X / 16f);
				}
			} else {
				if (NPC.ai[0] == 0f || NPC.ai[1] == 0f) {
					NPC.localAI[0] = 0f;
				}
				NPC.localAI[0] -= 1f;
				if (bodyNPC.life < bodyNPC.lifeMax / 2) {
					NPC.localAI[0] -= 2f;
				}
				if (bodyNPC.life < bodyNPC.lifeMax / 4) {
					NPC.localAI[0] -= 2f;
				}
				if (targetLeftBiome) {
					NPC.localAI[0] -= 6f;
				}
				if (!targetDead && NPC.localAI[0] <= 0f && NPC.ai[0] != 0f) {
					for (int i = 0; i < 200; i++) {
						if (i != NPC.whoAmI && Main.npc[i].active && Main.npc[i].type == Type && (Main.npc[i].velocity.X != 0f || Main.npc[i].velocity.Y != 0f)) {
							NPC.localAI[0] = Main.rand.Next(60, 300);
						}
					}
				}
				if (NPC.localAI[0] <= 0f) {
					NPC.localAI[0] = Main.rand.Next(300, 600);
					bool foundSpot = false;
					int tries = 0;
					Vector2 targetCenter = targetPlayer.Center;
					while (!foundSpot && tries <= 1000) {
						tries++;
						int checkBaseX = (int)(targetCenter.X / 16f);
						int checkBaseY = (int)(targetCenter.Y / 16f);
						if (NPC.ai[0] == 0f) {
							checkBaseX = (int)((targetCenter.X + bodyNPC.Center.X) / 32f);
							checkBaseY = (int)((targetCenter.Y + bodyNPC.Center.Y) / 32f);
						}
						if (targetDead) {
							checkBaseX = (int)bodyNPC.position.X / 16;
							checkBaseY = (int)(bodyNPC.position.Y + 400f) / 16;
						}
						int checkSize = 20;
						checkSize += (int)(tries / 10f);
						int checkX = checkBaseX + Main.rand.Next(-checkSize, checkSize + 1);
						int checkY = checkBaseY + Main.rand.Next(-checkSize, checkSize + 1);
						checkX = (int)MathHelper.Clamp(checkX, (bodyNPC.Center.X - 24 * 15) / 16, (bodyNPC.Center.X + 24 * 15) / 16);
						checkY = (int)MathHelper.Clamp(checkY, (bodyNPC.Center.Y - 24 * 15) / 16, (bodyNPC.Center.Y + 24 * 15) / 16);
						if (Main.rand.NextBool(6)) {
							NPC.TargetClosest();
							int playerCenterX = (int)(targetCenter.X / 16f);
							int playerCenterY = (int)(targetCenter.Y / 16f);
							if (Main.tile[playerCenterX, playerCenterY].WallType > 0) {
								checkX = playerCenterX;
								checkY = playerCenterY;
							}
						}
						try {
							if (WorldGen.InWorld(checkX, checkY) && (WorldGen.SolidTile(checkX, checkY) || (Main.tile[checkX, checkY].WallType > 0 && tries > 500))) {
								foundSpot = true;
								NPC.ai[0] = checkX;
								NPC.ai[1] = checkY;
								NPC.netUpdate = true;
							}
						} catch {
						}
					}
				}
			}
			if (NPC.ai[0] > 0f && NPC.ai[1] > 0f) {
				float moveSpeed = 6f;
				if (bodyNPC.life < bodyNPC.lifeMax / 2) {
					moveSpeed = 8f;
				}
				if (bodyNPC.life < bodyNPC.lifeMax / 4) {
					moveSpeed = 10f;
				}
				if (Main.expertMode) {
					moveSpeed += 1f;
				}
				if (Main.expertMode && bodyNPC.life < bodyNPC.lifeMax / 2) {
					moveSpeed += 1f;
				}
				if (targetLeftBiome) {
					moveSpeed *= 2f;
				}
				if (targetDead) {
					moveSpeed *= 2f;
				}
				float latchDiffX = NPC.ai[0] * 16f - 8f - center.X;
				float latchDiffY = NPC.ai[1] * 16f - 8f - center.Y;
				float latchDist = (float)Math.Sqrt(latchDiffX * latchDiffX + latchDiffY * latchDiffY);
				if (latchDist < 12f + moveSpeed) {
					NPC.velocity.X = latchDiffX;
					NPC.velocity.Y = latchDiffY;
				} else {
					latchDist = moveSpeed / latchDist;
					NPC.velocity.X = latchDiffX * latchDist;
					NPC.velocity.Y = latchDiffY * latchDist;
				}
				NPC.rotation = (bodyNPC.Center - center).ToRotation() - 1.57f;
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = TextureAssets.Npc[Type].Value;
			NPC bodyNPC = Main.npc[Primordial_Amoeba.npcIndex];
			Vector2 center = NPC.Center - NPC.netOffset;
			float diffX = bodyNPC.Center.X - center.X;
			float diffY = bodyNPC.Center.Y - center.Y;
			float rotation2 = (float)Math.Atan2(diffY, diffX) - 1.57f;
			bool drawCont = true;
			int frameY = 0;
			while (drawCont) {
				int height = 176;
				float dist = (float)Math.Sqrt(diffX * diffX + diffY * diffY);
				if (dist < height) {
					height = (int)dist;
					drawCont = false;
				}
				Color lightColor = Color.Lerp(Lighting.GetColor((int)center.X / 16, (int)(center.Y / 16f)), Color.White, 0.85f);
				spriteBatch.Draw(texture, new Vector2(center.X - screenPos.X, center.Y - screenPos.Y), new Rectangle(0, frameY, texture.Width, height), lightColor, rotation2, new Vector2(texture.Width * 0.5f, 0), 1f, SpriteEffects.None, 0f);

				frameY = 2;

				dist = height / dist;
				diffX *= dist;
				diffY *= dist;
				center.X += diffX;
				center.Y += diffY;
				diffX = bodyNPC.Center.X - center.X + bodyNPC.netOffset.X;
				diffY = bodyNPC.Center.Y - center.Y + bodyNPC.netOffset.Y;
			}
			return false;
		}
	}
	public class Primordial_Amoeba_Tentacle_Hitbox : ModNPC {
		public override string Texture => "Origins/Items/Weapons/Summoner/Flagellash_P";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Primordial Amoeba");
			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData {
				SpecificallyImmuneTo = new int[] {
					BuffID.Confused
				}
			};
			NPCID.Sets.DebuffImmunitySets[Type] = debuffData;
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.PlanterasHook);
			NPC.aiStyle = NPCAIStyleID.None;
			NPC.damage = 0;
			NPC.dontTakeDamage = false;
			NPC.lifeMax = 99999;
			NPC.defense = 0;
			NPC.width = NPC.height = 24;
			NPC.hide = true;
		}
		public override void ScaleExpertStats(int numPlayers, float bossLifeScale) {
			switch (Primordial_Amoeba.DifficultyMult) {
				case 1:
#if tentacleDamage
				NPC.damage = 13;
#endif
				NPC.defense = 8;
				break;

				case 2:
#if tentacleDamage
				NPC.damage = 17;
#endif
				NPC.defense = 10;
				break;

				case 3:
#if tentacleDamage
				NPC.damage = 20;
#endif
				NPC.defense = 12;
				break;
			}
		}
		public override void AI() {
			NPC tentacleNPC = Main.npc[(int)NPC.ai[0]];
			NPC.timeLeft = 937;
			if (!tentacleNPC.active) {
				NPC.active = false;
				NPC.netUpdate = true;
				return;
			}
			NPC.defense = 0;
			NPC.realLife = (int)NPC.ai[0];
			NPC.life = NPC.lifeMax;
			NPC bodyNPC = Main.npc[Primordial_Amoeba.npcIndex];
			Vector2 diff = (bodyNPC.Center - tentacleNPC.Center);
			diff -= diff.WithMaxLength(64);
			//diff = new Vector2(16);
			NPC.Center = tentacleNPC.Center + diff * NPC.ai[1];
		}
		public override bool? CanBeHitByProjectile(Projectile projectile) {
			return
				(!projectile.usesLocalNPCImmunity && !projectile.usesIDStaticNPCImmunity && Main.npc[(int)NPC.ai[0]].immune[projectile.owner] == 0) ||
				(projectile.usesLocalNPCImmunity && projectile.localNPCImmunity[(int)NPC.ai[0]] == 0) ||
				(projectile.usesIDStaticNPCImmunity && Projectile.IsNPCIndexImmuneToProjectileType(projectile.type, (int)NPC.ai[0]));
		}
		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) {
			NPC tentacleNPC = Main.npc[(int)NPC.ai[0]];
			if (tentacleNPC.immune[projectile.owner] < NPC.immune[projectile.owner]) {
				tentacleNPC.immune[projectile.owner] = NPC.immune[projectile.owner];
			}
			if (projectile.localNPCImmunity[tentacleNPC.whoAmI] < projectile.localNPCImmunity[NPC.whoAmI] || projectile.localNPCImmunity[NPC.whoAmI] < 0) {
				projectile.localNPCImmunity[tentacleNPC.whoAmI] = projectile.localNPCImmunity[NPC.whoAmI];
			}
			if (Projectile.perIDStaticNPCImmunity[projectile.type][tentacleNPC.whoAmI] < Projectile.perIDStaticNPCImmunity[projectile.type][NPC.whoAmI]) {
				Projectile.perIDStaticNPCImmunity[projectile.type][tentacleNPC.whoAmI] = Projectile.perIDStaticNPCImmunity[projectile.type][NPC.whoAmI];
			}
		}
	}
	public class Amoebic_Gel_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Summoner/Minions/Amoeba_Bubble";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Amoebic Gel");
			Main.projFrames[Projectile.type] = 4;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowHostile);
			Projectile.timeLeft = 3600;
			Projectile.aiStyle = ProjAIStyleID.Arrow;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 0;
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.scale = 0.85f;
			Projectile.ignoreWater = true;
		}
		public override Color? GetAlpha(Color lightColor) {
			return Color.Lerp(lightColor, Color.White, 0.85f);
		}
		public override void Kill(int timeLeft) {
			if (timeLeft < 3590) SoundEngine.PlaySound(SoundID.NPCDeath1.WithPitch(0.15f).WithVolumeScale(0.5f));
		}
	}
	public class Boss_Bar_PA : ModBossBar {
        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
            return Asset<Texture2D>.Empty;
        }
	}
}
