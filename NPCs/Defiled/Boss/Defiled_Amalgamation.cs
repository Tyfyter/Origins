using Microsoft.Xna.Framework.Graphics;
using Origins;
using Origins.Buffs;
using Origins.Dev;
using Origins.Graphics;
using Origins.Graphics.Primitives;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Other.LootBags;
using Origins.Items.Pets;
using Origins.Items.Vanity.BossMasks;
using Origins.Items.Weapons.Magic;
using Origins.Journal;
using Origins.LootConditions;
using Origins.Music;
using Origins.Projectiles.Enemies;
using Origins.Tiles.BossDrops;
using Origins.Tiles.Defiled;
using Origins.Walls;
using Origins.World.BiomeData;
using PegasusLib;
using PegasusLib.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Creative;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Origins.NPCs.Defiled.Boss.Defiled_Spike_Indicator;

namespace Origins.NPCs.Defiled.Boss {
	[AutoloadBossHead]
	public class Defiled_Amalgamation : Glowing_Mod_NPC, IDefiledEnemy, ICustomWikiStat, IJournalEntrySource, IOutlineDrawer, IMinions {
		static AutoLoadingAsset<Texture2D> RightArmTexture = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Right_Arm";
		static AutoLoadingAsset<Texture2D> RightArmGlowTexture = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Right_Arm_Glow";
		static AutoLoadingAsset<Texture2D> LeftArmTexture = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Left_Arm";
		static AutoLoadingAsset<Texture2D> LeftArmGlowTexture = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Left_Arm_Glow";
		static PegasusLib.AutoLoadingAsset<Texture2D> torsoPath = bodyPartsPath + "Torso";
		const string bodyPartsPath = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Split_";

		public string CustomSpritePath => "DefiledAmalg";
		public AssimilationAmount? Assimilation => 0.05f;
		public static bool spawnDA = false;
		float rightArmRot = 0.25f;
		float leftArmRot = 0.25f;
		public float time = 0;
		int trappedTime = 0;
		int roars = 0;
		int armFrame = 0;
		DrawData[] outlineData;
		NPC torso;
		NPC arm;
		NPC shoulder;
		NPC leg1;
		NPC leg2;
		public static int SplitDuration => 60 * 15;
		public static int SplitRegroupDuration => 60 * 1;
		public static int DifficultyMult => Main.masterMode ? 3 : (Main.expertMode ? 2 : 1);
		public static int TripleDashCD {
			get {
				int inactiveTime = 505 - DifficultyMult * 100;
				if (DifficultyMult == 3) {
					inactiveTime += 30;
				}
				return inactiveTime;
			}
		}
		//public float SpeedMult => npc.frame.Y==510?1.6f:0.8f;
		//bool attacking = false;
		public string EntryName => "Origins/" + typeof(Defiled_Amalgamation_Entry).Name;
		public class Defiled_Amalgamation_Entry : JournalEntry {
			public override string TextKey => "Defiled_Amalgamation";
			public override JournalSortIndex SortIndex => new("The_Defiled", 13);
		}
		internal static IItemDropRule normalDropRule;

		public static List<int> Minions = [];
		List<int> IMinions.BossMinions => Minions;
		public override void Unload() {
			normalDropRule = null;
		}
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 8;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<Rasterized_Debuff>()] = true;
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Scale = 0.75f,
				PortraitScale = 1f,
			};
			NPCID.Sets.BossBestiaryPriority.Add(Type);
			ID = Type;
			Origins.NPCOnlyTargetInBiome.Add(Type, ModContent.GetInstance<Defiled_Wastelands>());
			Origins.RasterizeAdjustment[Type] = (16, 0f, 0f);
		}
		public override void SetDefaults() {
			NPC.boss = true;
			NPC.BossBar = ModContent.GetInstance<Boss_Bar_DA>();
			NPC.aiStyle = NPCAIStyleID.None;
			NPC.lifeMax = 3250;
			NPC.defense = 14;
			NPC.damage = 62;
			NPC.width = 81;
			NPC.height = 96;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt.WithPitchRange(0f, 0.25f);
			NPC.DeathSound = Origins.Sounds.DefiledKill.WithPitchRange(-1f, -0.75f);
			NPC.noGravity = true;
			NPC.npcSlots = 200;
			NPC.knockBackResist = 0; // actually a multiplier
			NPC.value = Item.sellPrice(gold: 1);
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type
			];
		}
		public bool ForceSyncMana => false;
		public float Mana { get => 1; set { } }
		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
			float terriblyPlacedHookMult = 1;
			if (Main.GameModeInfo.IsJourneyMode) {
				CreativePowers.DifficultySliderPower power = CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
				if (power != null && power.GetIsUnlocked()) {
					if (power.StrengthMultiplierToGiveNPCs > 2) {
						terriblyPlacedHookMult /= 3;
					} else if (power.StrengthMultiplierToGiveNPCs > 1) {
						terriblyPlacedHookMult /= 2;
					}
				}
			}
			switch (DifficultyMult) {
				case 2:
				NPC.lifeMax = (int)(5200 * balance * terriblyPlacedHookMult);
				// NPC.defense = 13;
				NPC.damage = (int)(73 * terriblyPlacedHookMult);
				break;

				case 3:
				NPC.lifeMax = (int)(8320 * balance * terriblyPlacedHookMult);
				// NPC.defense = 15;
				NPC.damage = (int)(99 * terriblyPlacedHookMult);
				break;
			}
		}

		public override void OnSpawn(IEntitySource source) {
			spawnDA = false;
			if (Main.netMode == NetmodeID.Server) {
				ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasAwoken", NPC.GetTypeNetName()), new Color(222, 222, 222));
			} else {
				if (Main.netMode == NetmodeID.SinglePlayer) {
					Main.NewText(Language.GetTextValue("Announcement.HasAwoken", NPC.TypeName), 222, 222, 222);
				}
				SoundEngine.PlaySound(
					new SoundStyle("Origins/Sounds/Custom/Defiled_Kill1") {
						Pitch = -1,
						Volume = 0.66f
					}, NPC.Center
				);
			}
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			normalDropRule = new LeadingSuccessRule();

			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Lost_Ore_Item>(), 1, 140, 330));
			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Undead_Chunk>(), 1, 40, 100));
			normalDropRule.OnSuccess(ItemDropRule.OneFromOptions(1, ModContent.ItemType<Low_Signal>(), ModContent.ItemType<Return_To_Sender>()));

			normalDropRule.OnSuccess(ItemDropRule.Common(TrophyTileBase.ItemType<Defiled_Amalgamation_Trophy>(), 10));
			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Defiled_Amalgamation_Mask>(), 10));

			npcLoot.Add(new DropBasedOnExpertMode(
				normalDropRule,
				new DropLocalPerClientAndResetsNPCMoneyTo0(ModContent.ItemType<Defiled_Amalgamation_Bag>(), 1, 1, 1, null)
			));
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Mysterious_Spray>(), 4));
			npcLoot.Add(ItemDropRule.MasterModeCommonDrop(RelicTileBase.ItemType<Defiled_Amalgamation_Relic>()));
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Blockus_Tube>(), 4));
		}
		public const int state_single_dash = 1;
		public const int state_spike_field = 2;
		public const int state_triple_dash = 3;
		public const int state_sidestep_dash = 4;
		public const int state_summon_roar = 5;
		public const int state_ground_spikes = 6;
		public const int state_magic_missile = 7;
		public const int state_split_amalgamation_active = 8;
		public const int state_split_amalgamation_start = 9;
		public const int state_projectiles = 10;
		public int AIState { get => (int)NPC.ai[0]; set => NPC.ai[0] = value; }
		public int AttacksSinceTripleDash { get => (int)NPC.localAI[0]; set => NPC.localAI[0] = value; }
		public int AttacksSinceSplit { get => (int)NPC.localAI[1]; set => NPC.localAI[1] = value; }
		public DrawData[] OutlineDrawDatas { get => outlineData; }
		public int OutlineSteps { get => 8; }
		public float OutlineOffset { get => MathF.Sin((float)Main.timeForVisualEffects * 0.3f) * 3; }

		List<Vector2> oldPositions = [];
		public override void AI() {
			if (Main.rand.NextBool(650)) SoundEngine.PlaySound(Origins.Sounds.Amalgamation, NPC.Center);
			NPC.target = Main.maxPlayers;
			NPC.TargetClosest();
			if (NPC.HasValidTarget) {
				float leftArmTarget = 0.5f;
				float rightArmTarget = 0.25f;
				float armSpeed = 0.03f;

				int difficultyMult = DifficultyMult;// just saving the value as a slight optimization 

				int tickCount = 10 - difficultyMult * 2;
				int tickSize = NPC.lifeMax / tickCount;
				float currentTick = NPC.life / tickSize;

				switch (AIState) {
					//default state, uses default case so that negative values can be used for which action was taken last
					default: {
						CheckTrappedCollision();
						float targetHeight = 96 + (float)(Math.Sin(++time * (0.04f + (0.01f * difficultyMult))) + 0.5f) * 32 * difficultyMult;
						float targetX = 256 + (float)Math.Sin(++time * (0.02f + (0.01f * difficultyMult))) * 48 * difficultyMult;
						float speed = 5;
						float accelerationMult = 1f;

						float diffY = NPC.Bottom.Y - (NPC.targetRect.Center().Y - targetHeight);
						float diffX = NPC.Center.X - NPC.targetRect.Center().X;
						diffX -= Math.Sign(diffX) * targetX;
						float dist = NPC.DistanceSQ(NPC.targetRect.Center());
						if (dist > 640 * 640) {
							accelerationMult = 1 + ((dist / (640 * 640)) - 1) * 3;
							speed *= accelerationMult;
						}
						OriginExtensions.LinearSmoothing(ref NPC.velocity.Y, Math.Clamp(-diffY, -speed, speed), (Math.Abs(NPC.velocity.Y) > 16 ? 4 : 0.4f) * accelerationMult);
						OriginExtensions.LinearSmoothing(ref NPC.velocity.X, Math.Clamp(-diffX, -speed, speed), (Math.Abs(NPC.velocity.X) > 16 ? 4 : 0.4f) * accelerationMult);

						if (AIState <= 0) {
							NPC.ai[1] += 0.75f + (0.25f * difficultyMult);
							NPC.ai[1] += 0.5f * (difficultyMult - 1) * (1f - (currentTick / tickCount));
							if (NPC.ai[1] > 300) {
								WeightedRandom<int> rand = new(
									Main.rand,
									[
									new(0, 0f),
									new(state_single_dash, 0.9f),
									new(state_spike_field, 0.9f),
									new(state_triple_dash, 0.1f * Math.Clamp(AttacksSinceTripleDash - 1, 0, 5)),
									new(state_sidestep_dash, 0.5f + (0.05f * difficultyMult)),
									new(state_summon_roar, 0f),
									new(state_ground_spikes, 0.9f),
									new(state_magic_missile, 1f),
									new(state_split_amalgamation_start, NPC.AnyNPCs(ModContent.NPCType<Defiled_Swarmer>()) ? 0 : 0.12f * Math.Clamp(AttacksSinceSplit - 1, 0, 5)),// swapped to make state_split_amalgamation_active weight state_split_amalgamation_start
									new(state_split_amalgamation_active, 0f),
									new(state_projectiles, 0.9f)
									]
								);
								int lastUsedAttack = -AIState;

								if (!Collision.CanHitLine(NPC.targetRect.TopLeft(), NPC.targetRect.Width, NPC.targetRect.Height, NPC.Center, 16, 16)) {
									rand.elements[0] = new(rand.elements[0].Item1, rand.elements[0].Item2 / 3f);
									rand.elements[1] = new(rand.elements[1].Item1, rand.elements[1].Item2 * 6f);
									rand.elements[2] = new(rand.elements[2].Item1, rand.elements[2].Item2 / 3f);
								}

								if (lastUsedAttack > 0) {
									rand.elements[lastUsedAttack] = new(rand.elements[lastUsedAttack].Item1, rand.elements[lastUsedAttack].Item2 * (0.04 + 0.04 * ContentExtensions.DifficultyDamageMultiplier));
									if (Main.masterMode && lastUsedAttack == state_triple_dash) {
										rand.elements[state_single_dash] = new(rand.elements[state_single_dash].Item1, 0);
										rand.elements[state_sidestep_dash] = new(rand.elements[state_single_dash].Item1, 0);
										rand.elements[state_triple_dash] = new(rand.elements[state_triple_dash].Item1, 0);
									}
								}

								AIState = rand.Get();
								NPC.ai[2] = NPC.targetRect.Center().X;
								NPC.ai[3] = NPC.targetRect.Center().Y;
								NPC.ai[1] = 0;

								int roarCount = difficultyMult;
								int roarHP = NPC.lifeMax / (roarCount + 1);

								if (roarCount - roars > NPC.life / roarHP) {
									AIState = state_summon_roar;
									roars++;
								}
								if (AIState == state_triple_dash) {
									AttacksSinceTripleDash = 0;
								} else {
									AttacksSinceTripleDash++;
								}
								if (AIState == state_split_amalgamation_start) {
									AttacksSinceSplit = 0;
								} else {
									AttacksSinceSplit++;
								}
								if (AIState == state_projectiles) {
									NPC.ai[2] = 0;
								}

								if (AIState == state_single_dash) {
									SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(-0.9f), NPC.Center);
								}
								if (AIState == state_sidestep_dash) {
									SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(-0.5f), NPC.Center);
								}
							}
						}
						oldPositions.Clear();
						break;
					}

					//single dash
					case state_single_dash: {
						NPC.ai[1]++;
						NPC.velocity = NPC.oldVelocity;
						if (NPC.ai[1] < 30 - difficultyMult * 5) {
							float speed = 5;
							OriginExtensions.LinearSmoothing(ref NPC.velocity, (NPC.Center - new Vector2(NPC.ai[2], NPC.ai[3])).WithMaxLength(speed), 1.8f);
							NPC.oldVelocity = NPC.velocity;
						} else if (NPC.ai[1] < 35) {
							float speed = 7 + (5 * difficultyMult);
							OriginExtensions.LinearSmoothing(ref NPC.velocity, (new Vector2(NPC.ai[2], NPC.ai[3]) - NPC.Center).WithMaxLength(speed), 3f);
							NPC.oldVelocity = NPC.velocity;
							oldPositions.Add(NPC.position);
						} else if (NPC.ai[1] > 80) {
							AIState = -state_single_dash;
							NPC.ai[1] = 0;
						} else {
							oldPositions.Add(NPC.position);
						}
						break;
					}

					//spike field
					case state_spike_field: {
						CheckTrappedCollision();
						NPC.ai[1] += 1;
						float targetHeight = 96 + (float)(Math.Sin(++time * 0.02f) + 0.5f) * 32;
						float targetX = 320 + (float)Math.Sin(++time * 0.01f) * 32;
						float speed = 3;

						float diffY = NPC.Bottom.Y - (NPC.targetRect.Center().Y - targetHeight);
						float diffX = NPC.Center.X - NPC.targetRect.Center().X;
						diffX -= Math.Sign(diffX) * targetX;
						OriginExtensions.LinearSmoothing(ref NPC.velocity.Y, Math.Clamp(-diffY, -speed, speed), 0.4f);
						OriginExtensions.LinearSmoothing(ref NPC.velocity.X, Math.Clamp(-diffX, -speed, speed), 0.4f);
						leftArmTarget = -0.75f;
						rightArmTarget = -0.75f;
						armSpeed = 0.1f;

						switch ((int)NPC.ai[1]) {
							case 10:
							case 15:
							case 20:
							case 60:
							case 70:
							case 80:
							case 85:
							case 95:
							SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(1).WithVolume(0.14f), NPC.Center);

							Vector2 randomPosAroundTarget = NPC.targetRect.Center() + Main.rand.NextVector2CircularEdge(360, 360) * Main.rand.NextFloat(0.75f, 1f);
							if (Main.netMode != NetmodeID.MultiplayerClient) {
								float realDifficultyMult = Math.Min(ContentExtensions.DifficultyDamageMultiplier, 3.666f);
								Projectile.NewProjectileDirect(
									NPC.GetSource_FromAI(),
									randomPosAroundTarget,
									randomPosAroundTarget.DirectionTo(NPC.targetRect.Center()).RotatedByRandom(1),
									ModContent.ProjectileType<Defiled_Spike_Indicator>(),
									(int)((24 - (realDifficultyMult * 3)) * realDifficultyMult), // for some reason NPC projectile damage is just arbitrarily doubled
									0f,
									Main.myPlayer,
									1,
									NPC.target
								).tileCollide = Collision.CanHitLine(NPC.targetRect.TopLeft(), NPC.targetRect.Width, NPC.targetRect.Height, NPC.Center, 8, 8);
							}
							break;
							case 12:
							case 17:
							case 65:
							if (difficultyMult > 1) {
								goto case 10;
							}
							break;
							default:
							if (NPC.ai[1] > 100) {
								AIState = -state_spike_field;
								NPC.ai[1] = 0;
							}
							break;
						}
						break;
					}

					//triple dash and downtime after
					case state_triple_dash: {
						NPC.ai[1]++;
						int cycleLength = 100 - (difficultyMult * 4);
						int dashLength = 60 - (difficultyMult * 2);
						int activeLength = cycleLength * 2 + dashLength;
						if (NPC.ai[1] < activeLength) {
							if (NPC.ai[1] % cycleLength is < 2 and >= 1) {
								SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(-1), NPC.Center);
							}
							NPC.velocity = NPC.oldVelocity;
							if (NPC.ai[1] % cycleLength < 10 - (difficultyMult * 3)) {
								float speed = 4;
								OriginExtensions.LinearSmoothing(ref NPC.velocity, (NPC.Center - new Vector2(NPC.ai[2], NPC.ai[3])).WithMaxLength(speed), 1.8f);
								NPC.oldVelocity = NPC.velocity;
								oldPositions.Add(NPC.position);
							} else if (NPC.ai[1] % cycleLength < 18 - (difficultyMult * 2)) {
								float speed = 8 + (4 * difficultyMult);
								OriginExtensions.LinearSmoothing(ref NPC.velocity, (new Vector2(NPC.ai[2], NPC.ai[3]) - NPC.Center).WithMaxLength(speed), 8f);
								NPC.oldVelocity = NPC.velocity;
								oldPositions.Add(NPC.position);
							} else if (NPC.ai[1] % cycleLength > dashLength || NPC.collideX || NPC.collideY) {
								NPC.ai[2] = NPC.targetRect.Center().X;
								NPC.ai[3] = NPC.targetRect.Center().Y;
								oldPositions.Clear();
								goto default;
							} else {
								oldPositions.Add(NPC.position);
							}
						} else {
							oldPositions.Clear();
							NPC.velocity.X *= 0.97f;
							if (NPC.velocity.Y < 0) NPC.velocity.Y *= 0.97f;
							//SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitch(-1f).WithVolume(0.05f), NPC.Center);
							NPC.collideX = NPC.collideY = false;
							CheckTrappedCollision();
							if (NPC.ai[1] - activeLength < TripleDashCD) {
								NPC.velocity.Y += 0.12f;
								leftArmTarget = 0;
								rightArmTarget = 0;
								armSpeed *= 3;
							} else {
								AIState = -state_triple_dash;
								NPC.ai[1] = 100 * difficultyMult;
							}
						}
						break;
					}

					//"sidestep" dash
					case state_sidestep_dash: {
						if ((int)NPC.ai[1] == 0) {
							NPC.ai[2] = NPC.targetRect.Center().X - Math.Sign(NPC.Center.X - NPC.targetRect.Center().X) * 288;
							NPC.ai[3] = NPC.targetRect.Center().Y - 128;
						}
						NPC.ai[1]++;
						float targetHeight = (float)(Math.Sin(++time * 0.05f) + 0.5f) * 32;
						float targetX = (float)Math.Sin(++time * 0.03f) * 48;
						float speed = 5 * difficultyMult;

						float diffX = NPC.Center.X - NPC.ai[2];
						float diffY = NPC.Bottom.Y - (NPC.ai[3] - targetHeight);
						OriginExtensions.LinearSmoothing(ref NPC.velocity.Y, Math.Clamp(-diffY, -speed, speed), 0.4f);
						OriginExtensions.LinearSmoothing(ref NPC.velocity.X, Math.Clamp(-diffX, -speed * 4, speed * 4), 2.4f);
						if (Math.Abs(diffX) < 64 || NPC.ai[1] > 25) {
							AIState = -state_sidestep_dash;
							NPC.ai[1] = 160 + (difficultyMult * 40);
						}
						oldPositions.Add(NPC.position);
						NPC.noTileCollide = true;
						break;
					}

					//"beckoning roar"
					case state_summon_roar: {
						Main.instance.CameraModifiers.Add(new CameraShakeModifier(
							NPC.Center, 10f, 16f, 60, 5000f, 1f, nameof(Defiled_Amalgamation)
						));
						NPC.ai[1]++;
						NPC.velocity *= 0.9f;
						SoundEngine.PlaySound(Origins.Sounds.BeckoningRoar.WithPitchRange(0.1f, 0.2f).WithVolumeScale(0.25f), NPC.Center, (sound) => {
							sound.Position = NPC.Center;
							return true;
						});
						if (NPC.ai[1] < 40) {
							leftArmTarget = 0;
							rightArmTarget = 0;
							armSpeed *= 0.5f;
						} else if (NPC.ai[1] > 60) {
							AIState = 0;
							NPC.ai[1] = -40 + (difficultyMult * 20);
						} else if (NPC.ai[1] >= 45) {
							NPC.velocity = new Vector2(0, -4);
							if ((int)NPC.ai[1] == 45) {
								if (Main.netMode != NetmodeID.MultiplayerClient) {
									for (int i = 3 + (difficultyMult * NPC.statsAreScaledForThisManyPlayers); i-- > 0;) {
										Projectile.NewProjectileDirect(
											NPC.GetSource_FromAI(),
											NPC.targetRect.Center() - new Vector2(Main.rand.Next(80, 640) * (Main.rand.Next(2) * 2 - 1), 640),
											new Vector2(0, 8),
											ModContent.ProjectileType<Defiled_Enemy_Summon>(),
											0,
											0f,
											Main.myPlayer
										);
									}
								}
							}
							leftArmTarget = -1.25f;
							rightArmTarget = -1.25f;
							armSpeed *= 5f;
						}
						break;
					}

					//ground spikes
					case state_ground_spikes: {
						Main.instance.CameraModifiers.Add(new CameraShakeModifier(
							NPC.Center, 10f, 16f, 60, 1200f, 1f, nameof(Defiled_Amalgamation)
						));
						CheckTrappedCollision();
						NPC.ai[1]++;
						float targetHeight = 96 + (float)(Math.Sin(++time * 0.02f) + 0.5f) * 32;
						float targetX = 0;// 320 + (float)Math.Sin(++time * 0.01f) * 32;
						float speed = 2;
						float acceleration = 0.4f;
						targetHeight += NPC.ai[1] * 2;
						if (NPC.ai[1] >= 60) {
							if ((int)NPC.ai[1] == 60) {
								NPC.velocity.Y += 8;
								SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(-1f, -0.8f), NPC.Center);
								SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(-1), NPC.Center);
							}
							speed = 16 + 8 * ContentExtensions.DifficultyDamageMultiplier;
							targetHeight = (NPC.Bottom.Y + 24) - NPC.targetRect.Center().Y;
							if (NPC.collideY || NPC.ai[1] > 120) {
								if (Main.netMode != NetmodeID.MultiplayerClient) {
									float realDifficultyMult = Math.Min(ContentExtensions.DifficultyDamageMultiplier, 3.666f);
									int count = Main.rand.Next(6, 8) + Main.rand.RandomRound(realDifficultyMult * 2);
									for (int i = count; i-- > 0;) {
										SoundEngine.PlaySound(SoundID.Item62.WithPitch(1f), NPC.Center);
										Projectile.NewProjectileDirect(
											NPC.GetSource_FromAI(),
											NPC.targetRect.Center() - new Vector2((i - count * 0.5f) * (56 - realDifficultyMult * 8 + 34 + 24), 640),
											new Vector2(0, 8),
											ModContent.ProjectileType<DA_Spike_Summon>(),
											0,
											0f,
											Main.myPlayer,
											ai2: NPC.targetRect.Center().Y
										);
									}
								}
								NPC.velocity = Vector2.Zero;
								AIState = -AIState;
							}
						}

						float diffY = NPC.Bottom.Y - (NPC.targetRect.Center().Y - targetHeight);
						float diffX = NPC.Center.X - NPC.targetRect.Center().X;
						diffX -= Math.Sign(diffX) * targetX;
						OriginExtensions.LinearSmoothing(ref NPC.velocity.Y, Math.Clamp(-diffY, -speed, speed), acceleration);
						OriginExtensions.LinearSmoothing(ref NPC.velocity.X, Math.Clamp(-diffX, -speed, speed), acceleration);
						
						if (OriginsModIntegrations.CheckAprilFools()) {// April Fools' DAb
							leftArmTarget = 0.15f;
							rightArmTarget = -0.15f;
						} else {
							leftArmTarget = 0.6f;
							rightArmTarget = 0.7f;
						}
						armSpeed = 0.2f;
						break;
					}

					//magic missile
					case state_magic_missile: {
						CheckTrappedCollision();
						if (NPC.ai[1] < 5 || NPC.ai[2] is not 0 and not 1) {
							NPC.ai[2] = 0;
						}
						NPC.ai[1] += Main.rand.NextFloat(0.9f, 1f);
						float targetHeight = 96 + (float)(Math.Sin(++time * 0.02f) + 0.5f) * 32;
						float targetX = 320 + (float)Math.Sin(++time * 0.01f) * 32;
						float speed = 1;

						float diffY = NPC.Bottom.Y - (NPC.targetRect.Center().Y - targetHeight);
						float diffX = NPC.Center.X - NPC.targetRect.Center().X;
						diffX -= Math.Sign(diffX) * targetX;
						OriginExtensions.LinearSmoothing(ref NPC.velocity.Y, Math.Clamp(-diffY, -speed, speed), 0.4f);
						OriginExtensions.LinearSmoothing(ref NPC.velocity.X, Math.Clamp(-diffX, -speed, speed), 0.4f);
						leftArmTarget = -0.75f;
						rightArmTarget = -0.75f;
						armSpeed = 0.1f;

						if (NPC.ai[1] > 20 && NPC.ai[2] == 0) {
							SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(-1f), NPC.Center);
							SoundEngine.PlaySound(Origins.Sounds.EnergyRipple.WithPitch(-1f), NPC.Center);
							if (Main.netMode != NetmodeID.MultiplayerClient) {
								float realDifficultyMult = Math.Min(ContentExtensions.DifficultyDamageMultiplier, 3.666f);
								Projectile.NewProjectileDirect(
									NPC.GetSource_FromAI(),
									NPC.Center,
									Vector2.Normalize(NPC.targetRect.Center() - NPC.Center).RotatedByRandom(0.15f) * (10 + difficultyMult * 2) * Main.rand.NextFloat(0.9f, 1.1f),
									ModContent.ProjectileType<DA_Guided_Missile>(),
									(int)((24 - (realDifficultyMult * 3)) * realDifficultyMult),
									0f,
									ai0: NPC.whoAmI
								);
							}
							NPC.ai[2] = 1;
						} else if (NPC.ai[1] > 60 * 7.5f) {
							AIState = -AIState;
							NPC.ai[1] = 0;
						}
						break;
					}

					//split attack
					case state_split_amalgamation_start: {
						time++;
						NPC.ai[1]++;
						if (NPC.ai[1] < 60) {
							NPC.velocity *= 0.95f;
							armSpeed = 0.1f;
							leftArmTarget = -(0.1f - MathHelper.Pi + MathHelper.PiOver4);
							rightArmTarget = -(0.1f - MathHelper.Pi + MathHelper.PiOver4);
						}
						if (NPC.ai[1] == 60) {
							for (int i = 0; i < 128; i++) {
								Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(100, 100);
								Dust.NewDustPerfect(pos, DustID.AncientLight, NPC.Center.DirectionTo(pos) * 20);
							}
							SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(-1), NPC.Center);
							SoundEngine.PlaySound(Origins.Sounds.PowerUp.WithPitch(-1f), NPC.Center);
							SoundEngine.PlaySound(SoundID.Item123.WithPitch(1f), NPC.Center);

							if (Main.netMode != NetmodeID.MultiplayerClient) {
								int staticShock = NPC.FindBuffIndex(Static_Shock_Debuff.ID);
								if (staticShock >= 0) NPC.DelBuff(staticShock);
								leg1 = NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DA_Body_Part>(), 0, (int)DA_Body_Part.Part.leg1, NPC.whoAmI);
								leg2 = NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DA_Body_Part>(), 0, (int)DA_Body_Part.Part.leg2, NPC.whoAmI);
								arm = NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DA_Body_Part>(), 0, (int)DA_Body_Part.Part.arm, NPC.whoAmI);
								shoulder = NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DA_Body_Part>(), 0, (int)DA_Body_Part.Part.shoulder, NPC.whoAmI);
								torso = NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DA_Body_Part>(), 0, (int)DA_Body_Part.Part.torso, NPC.whoAmI);
							}
							NPC.dontTakeDamage = true;
							NPC.ShowNameOnHover = false;
							AIState = state_split_amalgamation_active;
							NPC.ai[1] = 0;
						}


						break;
					}
					case state_split_amalgamation_active: {
						time += MathHelper.Lerp(2, 0, MathHelper.Clamp(NPC.ai[1] / 180f, 0, 1));
						NPC.ai[1]++;
						NPC.Center = NPC.targetRect.Center() + new Vector2(0, -250);

						// parts regroup takes 1 second
						if (NPC.ai[1] >= SplitDuration + SplitRegroupDuration) {
							SoundEngine.PlaySound(SoundID.Item103.WithPitch(-1f), NPC.Center);
							SoundEngine.PlaySound(SoundID.NPCHit42.WithPitch(-0.4f).WithVolume(0.5f), NPC.Center);
							SoundEngine.PlaySound(Origins.Sounds.ShrapnelFest.WithPitch(-1f), NPC.Center);
							SoundEngine.PlaySound(Origins.Sounds.Amalgamation.WithPitch(-0.2f), NPC.Center);
							NPC.ShowNameOnHover = true;
							NPC.dontTakeDamage = false;
							AIState = -AIState;
							NPC.ai[1] = 0;
							ResetFrameSize();
							for (int i = 0; i < 128; i++) {
								Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(100, 100);
								Dust.NewDustPerfect(pos, DustID.AncientLight, NPC.Center.DirectionTo(pos) * 20);
							}
						}

						break;
					}

					//projectile spray
					case state_projectiles: {
						CheckTrappedCollision();
						NPC.ai[1] += Main.rand.NextFloat(0.9f, 1f);
						float targetHeight = 96 + (float)(Math.Sin(++time * 0.02f) + 0.5f) * 32;
						float targetX = 320 + (float)Math.Sin(++time * 0.01f) * 32;
						float speed = 3;

						float diffY = NPC.Bottom.Y - (NPC.targetRect.Center().Y - targetHeight);
						float diffX = NPC.Center.X - NPC.targetRect.Center().X;
						diffX -= Math.Sign(diffX) * targetX;
						OriginExtensions.LinearSmoothing(ref NPC.velocity.Y, Math.Clamp(-diffY, -speed, speed), 0.4f);
						OriginExtensions.LinearSmoothing(ref NPC.velocity.X, Math.Clamp(-diffX, -speed, speed), 0.4f);
						leftArmTarget = -0.75f;
						rightArmTarget = -0.75f;
						armSpeed = 0.1f;
						int projectilesToHaveFired = 0;
						if (NPC.ai[1] >= 10) projectilesToHaveFired++;
						if (NPC.ai[1] >= 12 && difficultyMult > 1) projectilesToHaveFired++;
						if (NPC.ai[1] >= 15) projectilesToHaveFired++;
						if (NPC.ai[1] >= 17 && difficultyMult > 1) projectilesToHaveFired++;
						if (NPC.ai[1] >= 20) projectilesToHaveFired++;

						if (NPC.ai[1] >= 60) projectilesToHaveFired++;
						if (NPC.ai[1] >= 65 && difficultyMult > 1) projectilesToHaveFired++;
						if (NPC.ai[1] >= 70) projectilesToHaveFired++;
						if (NPC.ai[2] < projectilesToHaveFired) {
							NPC.ai[2]++;
							SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(-0.6f, -0.4f), NPC.Center);
							if (Main.netMode != NetmodeID.MultiplayerClient) {
								Projectile.NewProjectileDirect(
									NPC.GetSource_FromAI(),
									NPC.Center,
									Vector2.Normalize(NPC.targetRect.Center() - NPC.Center).RotatedByRandom(0.15f) * (10 + difficultyMult * 2) * Main.rand.NextFloat(0.9f, 1.1f),
									ModContent.ProjectileType<Low_Signal_Hostile>(),
									22 - (difficultyMult * 3), // for some reason NPC projectile damage is just arbitrarily doubled
									0f,
									Main.myPlayer
								).tileCollide = Collision.CanHitLine(NPC.targetRect.TopLeft(), NPC.targetRect.Width, NPC.targetRect.Height, NPC.Center, 8, 8);
							}
						}
						if (NPC.ai[1] > 100) {
							AIState = -state_projectiles;
							NPC.ai[1] = 0;
						}
					}
					break;
				}
				OriginExtensions.AngularSmoothing(ref rightArmRot, rightArmTarget, armSpeed);
				OriginExtensions.AngularSmoothing(ref leftArmRot, leftArmTarget, armSpeed * 1.5f);
			} else {
				if (AIState == state_split_amalgamation_active) {
					NPC.Center = NPC.targetRect.Center() + new Vector2(0, -250);
				}
				NPC.EncourageDespawn(300);
				if (++trappedTime > 30) {
					NPC.noTileCollide = true;
				}
				float leftArmTarget = 0f;
				float rightArmTarget = 0f;
				float armSpeed = 0.09f;
				OriginExtensions.AngularSmoothing(ref rightArmRot, rightArmTarget, armSpeed);
				OriginExtensions.AngularSmoothing(ref leftArmRot, leftArmTarget, armSpeed * 1.5f);
			}
			NPC.alpha = NPC.noTileCollide ? 75 : 0;
		}
		public void CheckTrappedCollision() {
			if (NPC.position.Y > Main.UnderworldLayer * 16 && NPC.HasValidTarget) {
				NPC.noTileCollide = false;
				trappedTime = 30;
				return;
			}
			Rectangle hitbox = NPC.Hitbox;
			hitbox.Inflate(-1, -1);
			if (!hitbox.OverlapsAnyTiles()) {
				NPC.noTileCollide = false;
			} else if (AIState == state_triple_dash) {
				NPC.velocity.Y = -4;
			}
			if (NPC.collideX || NPC.collideY) {
				if (++trappedTime > 30) {
					NPC.noTileCollide = true;
					NPC.collideX = NPC.collideY = false;
				}
			} else if (trappedTime > 0) {
				trappedTime--;
			}
		}
		public override bool? CanFallThroughPlatforms() {
			switch (AIState) {
				case state_triple_dash:
				int cycleLength = 100 - (DifficultyMult * 4);
				int dashLength = 60 - (DifficultyMult * 2);
				int activeLength = cycleLength * 2 + dashLength;
				return NPC.ai[1] <= activeLength;

				case state_ground_spikes:
				return NPC.BottomLeft.Y < NPC.targetRect.Center.Y;
			}
			return true;
		}

		public override void FindFrame(int frameHeight) {
			if (!NPC.HasValidTarget && !NPC.IsABestiaryIconDummy) {
				NPC.frame = new Rectangle(0, (frameHeight * 7) % (frameHeight * 8), 122, frameHeight);
				NPC.velocity.Y += 0.12f;
				if (NPC.direction == 0) NPC.direction = 1;
				return;
			}
			int cycleLength = 100 - (DifficultyMult * 4);
			int dashLength = 60 - (DifficultyMult * 2);
			int activeLength = cycleLength * 2 + dashLength;
			if (AIState == state_triple_dash && NPC.ai[1] > activeLength) {
				NPC.frame = new Rectangle(0, (frameHeight * (int)(Math.Pow((NPC.ai[1] - activeLength) / TripleDashCD, 3) * 5) + frameHeight * 7) % (frameHeight * 8), 122, frameHeight);
				armFrame = 3;
			} else if (++NPC.frameCounter > 7) {
				NPC.frameCounter = 0;
				if (AIState == state_split_amalgamation_active) {
					int frameHeightTorso = 240 / 4;
					NPC.frame = new Rectangle(0, (NPC.frame.Y + frameHeightTorso) % (frameHeightTorso * 3), 80, frameHeightTorso);
					return;
				}

				NPC.frame = new Rectangle(0, (NPC.frame.Y + frameHeight) % (frameHeight * 3) + frameHeight * 4, 122, frameHeight);
				armFrame = (armFrame + 1) % 3;
			}
		}
		public void Regenerate(out int lifeRegen) {
			lifeRegen = 0;
			if (AIState != state_triple_dash) {
				int tickSize = NPC.lifeMax / (10 - DifficultyMult * 2);
				int threshold = (((NPC.life - 1) / tickSize) + 1) * tickSize;
				if (NPC.life < threshold) {
					lifeRegen = 6 + (DifficultyMult * 2);
				}
			}
		}
		public void ResetFrameSize() {
			if (AIState == state_split_amalgamation_active)
				NPC.frame = new Rectangle(0, 0, 122, 928 / 8);
			else
				NPC.frame = new Rectangle(0, 0, 80, 240 / 4);
		}
		public override void OnKill() {
			if (!NPC.downedBoss2 || Main.rand.NextBool(2)) WorldGen.spawnMeteor = true;
			NPC.SetEventFlagCleared(ref NPC.downedBoss2, GameEventClearedID.DefeatedEaterOfWorldsOrBrainOfChtulu);
		}
		public void SpawnWisp(NPC npc) {
			if (AIState is state_split_amalgamation_active or state_split_amalgamation_start) {
				NPC.NewNPC(npc.GetSource_Death(), (int)torso.position.X + Main.rand.Next(torso.width), (int)torso.position.Y + Main.rand.Next(torso.height), ModContent.NPCType<Defiled_Wisp>());
				NPC.NewNPC(npc.GetSource_Death(), (int)arm.position.X + Main.rand.Next(arm.width), (int)arm.position.Y + Main.rand.Next(arm.height), ModContent.NPCType<Defiled_Wisp>());
				NPC.NewNPC(npc.GetSource_Death(), (int)shoulder.position.X + Main.rand.Next(shoulder.width), (int)shoulder.position.Y + Main.rand.Next(shoulder.height), ModContent.NPCType<Defiled_Wisp>());
				NPC.NewNPC(npc.GetSource_Death(), (int)leg1.position.X + Main.rand.Next(leg1.width), (int)leg1.position.Y + Main.rand.Next(leg1.height), ModContent.NPCType<Defiled_Wisp>());
				NPC.NewNPC(npc.GetSource_Death(), (int)leg2.position.X + Main.rand.Next(leg2.width), (int)leg2.position.Y + Main.rand.Next(leg2.height), ModContent.NPCType<Defiled_Wisp>());
			} else {
				for (int releasedWisps = 0; releasedWisps < 5; releasedWisps++) {
					NPC.NewNPC(npc.GetSource_Death(), (int)npc.position.X + Main.rand.Next(npc.width), (int)npc.position.Y + Main.rand.Next(npc.height), ModContent.NPCType<Defiled_Wisp>());
				}
			}
		}
		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {
			if (AIState <= 0) {
				Rectangle highHitbox = NPC.Hitbox;
				highHitbox.Height /= 4;

				Rectangle lowHitbox = NPC.Hitbox;
				lowHitbox.Y += highHitbox.Height;
				lowHitbox.Height -= highHitbox.Height;
				lowHitbox.Width /= 2;
				lowHitbox.X += lowHitbox.Width / 2;

				if (!highHitbox.Intersects(projectile.Hitbox) && !lowHitbox.Intersects(projectile.Hitbox)) {
					modifiers.DefenseEffectiveness *= 1 + DifficultyMult * 0.5f;
				}
			}
		}
		public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
			if (AIState is state_split_amalgamation_start or state_split_amalgamation_active) {
				npcHitbox = default;
				return false;
			}
			if (npcHitbox.Intersects(victimHitbox)) return base.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);
			Rectangle hitbox = npcHitbox;
			for (int i = 0; i < oldPositions.Count; i++) {
				hitbox.X = (int)oldPositions[i].X;
				hitbox.Y = (int)oldPositions[i].Y;
				if (hitbox.Intersects(victimHitbox)) {
					npcHitbox = hitbox;
					/// checks the same counter as the default -1, but is distinguishable in <see cref="ModifyHitPlayer"></see> and <see cref="OnHitPlayer"></see>
					immunityCooldownSlot = -2;
					return true;
				}
			}
			return base.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			if (modifiers.CooldownCounter == -2) {
				modifiers.Knockback *= 0;
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			if (hurtInfo.CooldownCounter == -2) {
				target.immune = true;
				target.immuneTime = (hurtInfo.Damage == 1) ? (target.longInvince ? 40 : 20) : (target.longInvince ? 80 : 40);
			} else {
				if (DifficultyMult >= 2 && Main.rand.NextBool(2 * DifficultyMult, 9)) {
					target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), DifficultyMult * 23);
				}
			}
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			if (AIState == state_magic_missile) {
				NPC.ai[1] += projectile.knockBack;
			}

			Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_OnHurt(projectile), Main.rand.NextVectorIn(spawnbox), projectile.velocity, "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			if (AIState == state_magic_missile) {
				NPC.ai[1] += player.GetWeaponKnockback(item);
			}

			int halfWidth = NPC.width / 2;
			int baseX = player.direction > 0 ? 0 : halfWidth;
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_OnHurt(player), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), hit.GetKnockbackFromHit(), "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				SpawnGore(NPC.Center + new Vector2(NPC.spriteDirection * -30, -20), 1);
				SpawnGore(NPC.Center + new Vector2(NPC.spriteDirection * 15, 18), 2);
				SpawnGore(NPC.Center + new Vector2(NPC.spriteDirection * -4, -22), 3);
				for (int i = 0; i < 6; i++)
					Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
				for (int i = 0; i < 10; i++)
					Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
			}
		}
		void SpawnGore(Vector2 position, int num) {
			Gore gore = Main.gore[Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), position, NPC.velocity, $"Gores/NPCs/DA{num}_Gore")];
			gore.Frame = new SpriteFrame(2, 1) {
				CurrentColumn = (NPC.spriteDirection == 1) ? (byte)0 : (byte)1
			};
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (AIState == state_split_amalgamation_active) {
				return;
			}

			base.PostDraw(spriteBatch, screenPos, drawColor);
			drawColor *= (255 - NPC.alpha) / 255f;
			bool dir = NPC.spriteDirection == 1;
			Rectangle armsFrame = new(0, armFrame * 96, 30, 94);

			spriteBatch.DrawGlowingNPCPart(RightArmTexture, RightArmGlowTexture,
				NPC.Center - new Vector2(-46 * NPC.spriteDirection, 12) * NPC.scale - screenPos,
				armsFrame,
				drawColor, Color.White,
				rightArmRot * NPC.spriteDirection,
				new Vector2(dir ? 7 : 23, 19),
				NPC.scale,
				dir ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			);

			spriteBatch.DrawGlowingNPCPart(LeftArmTexture, LeftArmGlowTexture,
				NPC.Center - new Vector2(36 * NPC.spriteDirection, 0) * NPC.scale - screenPos,
				armsFrame,
				drawColor, Color.White,
				-leftArmRot * NPC.spriteDirection,
				new Vector2(dir ? 23 : 7, 19),
				NPC.scale,
				dir ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {

			bool dir = NPC.spriteDirection == 1;
			Rectangle armsFrame = new(0, armFrame * 96, 30, 94);
			outlineData =
			[
					new(TextureAssets.Npc[Type].Value,NPC.Center + NPC.velocity - new Vector2(0,4),NPC.frame,Color.White,NPC.rotation,NPC.frame.Size() / 2f, NPC.scale, dir ? SpriteEffects.FlipHorizontally : SpriteEffects.None),

					new(LeftArmTexture,
					NPC.Center - new Vector2(36 * NPC.spriteDirection, 0) * NPC.scale + NPC.velocity,
					armsFrame,
					Color.White,
					-leftArmRot * NPC.spriteDirection,
					new Vector2(dir ? 23 : 7, 19),
					NPC.scale,
					dir ? SpriteEffects.None : SpriteEffects.FlipHorizontally),

					new(RightArmTexture,
					NPC.Center - new Vector2(-46 * NPC.spriteDirection, 12) * NPC.scale + NPC.velocity,
					armsFrame,
					Color.White,
					rightArmRot * NPC.spriteDirection,
					new Vector2(dir ? 7 : 23, 19),
					NPC.scale,
					dir ? SpriteEffects.None : SpriteEffects.FlipHorizontally)
			];

			if (AIState == state_split_amalgamation_active) {
				return false;
			}

			return true;
		}
		public Color? SetOutlineColor(float progress) => Color.Lerp(Color.White, Color.Black, MathF.Sin((float)Main.timeForVisualEffects * 0.1f));

		public class Spawn : SpawnPool {
			public override string Name => $"{nameof(Defiled_Amalgamation)}_{base.Name}";
			public override void SetStaticDefaults() {
				Priority = SpawnPoolPriority.EventHigh;
				AddSpawn<Defiled_Amalgamation>(spawnInfo => spawnInfo.PlayerFloorY < Main.worldSurface && Main.tile[spawnInfo.PlayerFloorX, spawnInfo.PlayerFloorY].WallType != ModContent.WallType<Defiled_Stone_Wall>() ? 99999999 : 0);
			}
			public override bool IsActive(NPCSpawnInfo spawnInfo) => spawnDA && spawnInfo.Player.InModBiome<Defiled_Wastelands>();
		}
	}
	public class Boss_Bar_DA : ModBossBar {
		bool isDead = false;
		float lastTickPercent = 1f;
		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
			return Asset<Texture2D>.Empty;
		}
		AutoLoadingAsset<Texture2D> tickTexture = typeof(Boss_Bar_DA).GetDefaultTMLName() + "_Tick";
		public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax) {
			NPC owner = Main.npc[info.npcIndexToAimAt];
			if (owner.type != Defiled_Amalgamation.ID || (lastTickPercent < 0 && isDead)) return false;
			if (!owner.active || owner.life <= 0) {
				isDead = true;
				life = 0;
			}
			if (owner.life > 0 && owner.active) {
				isDead = false;
				life = owner.life;
				lifeMax = owner.lifeMax;
				shield = owner.life;
				shieldMax = lifeMax;
			}

			int tickCount = 10 - Defiled_Amalgamation.DifficultyMult * 2;
			float tickSize = lifeMax / tickCount;
			float lifeTarget = MathF.Ceiling((life - 1) / tickSize) / tickCount;
			OriginExtensions.LinearSmoothing(ref lastTickPercent, lifeTarget, 0.015f);
			life = lastTickPercent * lifeMax;
			return life > 0;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) {
			Point barSize = new(456, 22); //Size of the bar
			Point topLeftOffset = new(32, 24); //Where the top left of the bar starts
			int frameCount = 6;

			Rectangle bgFrame = drawParams.BarTexture.Frame(verticalFrames: frameCount, frameY: 3);
			bgFrame.X += topLeftOffset.X;
			bgFrame.Y += topLeftOffset.Y;
			bgFrame.Width = 2;
			bgFrame.Height = barSize.Y;

			int shieldScale = (int)(barSize.X * drawParams.Life / drawParams.LifeMax);
			shieldScale -= shieldScale % 2;

			Rectangle barPosition = Utils.CenteredRectangle(drawParams.BarCenter, barSize.ToVector2());
			Vector2 barTopLeft = barPosition.TopLeft();

			SpriteBatchState state = spriteBatch.GetState();
			try {
				spriteBatch.Restart(state, SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);
				ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(TangelaVisual.ShaderID, Main.LocalPlayer);
				FastRandom random = new(npc.whoAmI);
				shader.Shader.Parameters["uOffset"]?.SetValue(new Vector2(random.NextFloat(), random.NextFloat()) * 512);
				bgFrame.Width = shieldScale;
				DrawData data = new(
					drawParams.BarTexture,
					barTopLeft,
					bgFrame,
					Color.White,
					0,
					Vector2.Zero,
					Vector2.One,
					SpriteEffects.None
				);
				shader.Apply(null, data);
				data.Draw(spriteBatch);
			} finally {
				spriteBatch.Restart(state);
			}
			return true;
		}
		public override void PostDraw(SpriteBatch spriteBatch, NPC npc, BossBarDrawParams drawParams) {
			int tickCount = 10 - Defiled_Amalgamation.DifficultyMult * 2;
			Vector2 barSize = new(456, 22);
			Vector2 barPos = drawParams.BarCenter - barSize * new Vector2(0.5f, 0);
			Vector2 origin = tickTexture.Value.Size() / 2;
			float tickPercent = 1f / tickCount;
			float lifePercentToShow = drawParams.Life / drawParams.LifeMax;
			for (float f = 0; f < lifePercentToShow; f += tickPercent) {
				if (f == 0f) continue;
				float animFactor = Math.Min((lifePercentToShow - f) / tickPercent, 1);
				spriteBatch.Draw(
					tickTexture,
					barPos + barSize * new Vector2(f, 0),
					null,
					new Color(animFactor, animFactor, animFactor, animFactor),
					0f,
					origin,
					2f - animFactor,
					SpriteEffects.None,
				0f);
			}
		}
	}
	public class DA_Music_Scene_Effect : BossMusicSceneEffect<Defiled_Amalgamation> {
		public override int Music => Origins.Music.DefiledBoss;
	}
	public class DA_Bendy_Spikes : ModProjectile {
		float progress = 0;
		public int maxTimeleft = 20 + (int)(10 * ContentExtensions.DifficultyDamageMultiplier);
		public override string Texture => "Origins/NPCs/Defiled/Boss/DA_Spike_Rotated";
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
			Projectile.width = 34;
			Projectile.height = 0;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = true;
			Projectile.hide = true;
			Projectile.timeLeft = maxTimeleft + 60;
			Projectile.knockBack = 0;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if (Projectile.timeLeft <= maxTimeleft)
				progress = Utils.GetLerpValue(0f, 1f, Utils.PingPongFrom01To010(Projectile.timeLeft / (float)maxTimeleft), true);
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCs.Add(index);
		}
		/// <summary>
		/// Call a programmer if this method lasts longer than four hours
		/// </summary>
		/// <returns></returns>
		public (Vector2 pos, Vector2 perpendicular)[] GetCurve(float progress) {
			if (Projectile.ai[0] < 0) return [];
			if (Projectile.localAI[2] != Projectile.ai[0]) {
				const int precision = 16;
				_curve = new (Vector2 pos, Vector2 perpendicular)[(int)Math.Ceiling((Projectile.ai[0] * progress) / precision)];
				Vector2 pos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.Pi) / 16f * Projectile.ai[0];
				Vector2 mov = -Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.Pi) * precision;
				int index = 0;
				for (int i = 0; i < Math.Round(Projectile.ai[0] * progress); i += precision) {
					_curve[index++] = (pos, mov.RotatedBy(MathHelper.PiOver2) / precision);
					pos += mov;
					mov = mov.RotatedBy(Projectile.ai[2]);
				}
			}
			return _curve;
		}
		(Vector2 pos, Vector2 perpendicular)[] _curve = [];
		public (Vector2 pos, Vector2 perpendicular)[] Curve {
			get {
				return _curve;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			(Vector2 pos, Vector2 perpendicular)[] curve = GetCurve(progress);
			if (curve.Length <= 0)
				return false;
			Vector2 lastPos0 = curve[0].pos - curve[0].perpendicular * 17;
			Vector2 lastPos1 = curve[0].pos + curve[0].perpendicular * 17;
			Vector2 nextPos0 = default, nextPos1 = default;
			List<(Vector2 start, Vector2 end)> _lines = [(lastPos1, lastPos0)];
			for (int i = 0; i < curve.Length; i++) {
				Vector2 nextPos = curve[i].pos;
				if (nextPos == default) break;
				nextPos0 = nextPos - curve[i].perpendicular * 17;
				nextPos1 = nextPos + curve[i].perpendicular * 17;

				_lines.Add((lastPos0, nextPos0));
				_lines.Add((nextPos1, lastPos1));
				lastPos0 = nextPos0;
				lastPos1 = nextPos1;
			}
			_lines.Add((nextPos0, nextPos1));
			return CollisionExtensions.PolygonIntersectsRect(_lines.ToArray(), targetHitbox);
		}
		public static float InExpo(float t, float strength) => (float)Math.Pow(2, strength * (t - 1));
		public static float OutExpo(float t, float strength) => 1 - InExpo(1 - t, strength);
		public static float InOutExpo(float t, float strength) {
			if (t < 0.5) return InExpo(t * 2, strength) * .5f;
			return 1 - InExpo((1 - t) * 2, strength) * .5f;
		}
		public override bool PreDraw(ref Color lightColor) {

			(Vector2 pos, Vector2 perpendicular)[] curve = GetCurve(1f);

			Vector2[] worldPositions = new Vector2[curve.Length];
			Vector2[] positions = new Vector2[curve.Length];
			float[] rotations = new float[curve.Length];

			for (int i = 0; i < curve.Length; i++) {
				worldPositions[i] = curve[i].pos;
				rotations[i] = (curve[i].perpendicular.ToRotation() + MathHelper.PiOver2) * Main.Transform.Up.Y;
				positions[i] = Vector2.Transform(worldPositions[i] - Main.screenPosition, Main.Transform);
			}

			if (progress > 0) {
				DrawDefiledPortal(curve[0].pos, Projectile.velocity.ToRotation() + MathHelper.PiOver2, new Vector2(128, 64), Utils.GetLerpValue(0, 1, (max_lifetime + Projectile.timeLeft) / (float)max_lifetime));
				DrawDefiledSpikeStrip(TextureAssets.Projectile[Type], positions, worldPositions, rotations, progress - 1, Projectile.ai[0]);
			}
			return false;
		}
		private static readonly VertexStrip vertexStrip = new();
		private static readonly VertexRectangle rect = new();
		public static void DrawDefiledSpikeStrip(Asset<Texture2D> tex, Vector2[] positions, Vector2[] lightPositions, float[] rotations, float progress, float length) {
			MiscShaderData shader = GameShaders.Misc["Origins:DefiledSpike"];
			shader.UseImage1(tex);
			shader.UseColor(Color.Black);
			shader.UseSecondaryColor(Color.Green);
			shader.UseSamplerState(SamplerState.PointClamp);
			shader.UseShaderSpecificData(new Vector4(progress, length, 0, 0));
			shader.Apply();
			vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, (p) => Lighting.GetColor(lightPositions[(int)Math.Round(p * (lightPositions.Length - 1))].ToTileCoordinates()), (p) => 16, Vector2.Zero, false);
			vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}

		public static void DrawDefiledPortal(Vector2 position, float rotation, Vector2 size, float progress) {
			MiscShaderData shader = GameShaders.Misc["Origins:DefiledPortal"];
			shader.UseImage1(TextureAssets.Extra[193]);
			shader.UseSamplerState(SamplerState.PointWrap);
			shader.UseColor(Color.Cyan);
			shader.UseSecondaryColor(Color.Purple);
			shader.UseShaderSpecificData(new Vector4(0, 0, Main.LocalPlayer.Center.X, Main.LocalPlayer.Center.Y));
			shader.Apply();
			rect.Draw(position, Color.White, progress * size, rotation, position);
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}
	}
	public class Defiled_Spike_Indicator : ModProjectile {
		public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";

		public const int indicator_duration = 40;
		public const int max_lifetime = 120;
		public Vector2 dustStartingPosition;
		public int segments = 15;
		public DA_Bendy_Spikes childSpike;
		public int fadeInOutTimer = 0;
		public override bool ShouldUpdatePosition() => false;
		public override void OnSpawn(IEntitySource source) {
			if (Projectile.ai[2] != 0) Projectile.scale = Projectile.ai[2];
			if (source is EntitySource_Parent sourceParent) {
				dustStartingPosition = sourceParent.Entity.Center;
			} else {
				dustStartingPosition = Projectile.Center;
			}
			float curveAmount = MathF.Pow(Main.rand.NextFloat(), 2 / ContentExtensions.DifficultyDamageMultiplier) * MathHelper.Pi * (Main.rand.NextBool() ? -1 : 1) * (3 + ContentExtensions.DifficultyDamageMultiplier) * 0.25f;
			float maxGrowth = 120 + 120 * ContentExtensions.DifficultyDamageMultiplier;
			float segs = maxGrowth / 32;
			segments = (int)MathF.Ceiling(segs);
			childSpike = Projectile.NewProjectileDirect(
				Projectile.GetSource_FromThis(),
				Projectile.Center,
				Projectile.velocity.SafeNormalize(default) * 92,
				ModContent.ProjectileType<DA_Bendy_Spikes>(),
				Projectile.damage,
				0,
				Projectile.owner,
				maxGrowth,
				Projectile.whoAmI,
				curveAmount / 2f / segs
			).ModProjectile as DA_Bendy_Spikes;
		}
		public override void SetDefaults() {
			Projectile.timeLeft = max_lifetime;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.hide = true;
			Projectile.tileCollide = false;
			Projectile.npcProj = true;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Default;
		}
		public override bool? CanHitNPC(NPC target) => false;
		public override bool CanHitPlayer(Player target) => false;
		public override bool CanHitPvp(Player target) => false;

		public override bool PreDraw(ref Color lightColor) {
			if (Projectile.timeLeft > max_lifetime - 10)
				return false;

			(Vector2 pos, Vector2 per)[] curve = childSpike.GetCurve(1f);
			Vector2[] pos = new Vector2[segments * 32];
			float[] rot = new float[segments * 32];
			for (int i = 0; i < curve.Length && i < pos.Length; i++) {
				pos[i] = Vector2.Transform(curve[i].pos - Main.screenPosition, Main.Transform) + Main.screenPosition;
				rot[i] = (curve[i].per.ToRotation() + MathHelper.PiOver2) * Main.Transform.Up.Y;
			}

			float progress = Projectile.timeLeft / (float)max_lifetime;
			float alpha;
			if (childSpike.Projectile.timeLeft < childSpike.maxTimeleft) {
				//alpha = 0;
				alpha = childSpike.Projectile.timeLeft / (float)childSpike.maxTimeleft;
				alpha *= alpha * alpha * alpha;
			} else {
				alpha = Utils.GetLerpValue(0f, 1f, Utils.PingPongFrom01To010(progress));
			}

			Draw(
				pos,
				rot,
				16f,
				alpha,
				progress
			);

			return false;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCs.Add(index);
		}
		public override void AI() {
			if (fadeInOutTimer == 0) {
				for (int i = 0; i < 160; i++) {
					float progress = i / 160f;
					Dust.NewDustDirect(
						Vector2.Lerp(
							dustStartingPosition,
							Projectile.Center + new Vector2(0, -250f) * Utils.PingPongFrom01To010(MathF.Sqrt(1f - MathF.Pow(progress - 1f, 2f))),
							progress
						),
						16, 16,
						DustID.AncientLight
					).noGravity = true;
				}
			}
			if (Projectile.ai[2] != 0) Projectile.scale = Projectile.ai[2];
			fadeInOutTimer++;
		}
		private static readonly VertexStrip vertexStrip = new();
		public static void Draw(Vector2[] positions, float[] rotations, float width, float alpha, float progress) {
			MiscShaderData shader = GameShaders.Misc["Origins:DefiledIndicator"];
			shader.UseImage1(TextureAssets.Extra[193]);
			shader.UseColor(Color.Black);
			shader.UseSecondaryColor(Color.Green);
			shader.UseShaderSpecificData(new Vector4(alpha, progress, 0, 0));
			shader.Apply();
			vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, (p) => Color.White * alpha, (p) => width, -Main.screenPosition, false);
			vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.WriteVector2(dustStartingPosition);
			writer.Write((ushort)segments);
			writer.Write((ushort)childSpike.Projectile.identity);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			dustStartingPosition = reader.ReadVector2();
			segments = reader.ReadUInt16();
			int spikeType = ModContent.ProjectileType<DA_Bendy_Spikes>();
			int spikeIdentity = reader.ReadUInt16();
			foreach (Projectile child in Main.ActiveProjectiles) {
				if (child.type == spikeType && child.identity == spikeIdentity) {
					childSpike = child.ModProjectile as DA_Bendy_Spikes;
					break;
				}
			}
		}
	}
	public class Low_Signal_Hostile : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Magic/Infusion_P";

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.DamageType = DamageClass.Default;
			Projectile.friendly = false;
			if (Main.masterMode || Main.expertMode) {
				Projectile.hostile = true;
			}
			Projectile.timeLeft = 40;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 60;
			Projectile.aiStyle = 0;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.penetrate = 1;
			Projectile.hide = true;
		}
		public override void AI() {
			Dust.NewDustPerfect(Projectile.Center, DustID.AncientLight, default, newColor: Color.White, Scale: 0.5f + (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.15f);
			if (Projectile.timeLeft % 15 == 0) {
				Projectile.localNPCImmunity.CopyTo(Projectile.NewProjectileDirect(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					Vector2.Zero,
					ModContent.ProjectileType<Defiled_Spike_Explosion_Hostile>(),
					Projectile.damage,
					0,
					Projectile.owner,
					7,
					ai2: 0.5f
				).localNPCImmunity.AsSpan());
			}
		}
		public override void OnKill(int timeLeft) {
			Projectile.localNPCImmunity.CopyTo(Projectile.NewProjectileDirect(
				Projectile.GetSource_FromThis(),
				Projectile.Center,
				Vector2.Zero,
				ModContent.ProjectileType<Defiled_Spike_Explosion_Hostile>(),
				Projectile.damage,
				0,
				Projectile.owner,
			7).localNPCImmunity.AsSpan());
		}
	}
	public class Defiled_Spike_Explosion_Hostile : ModProjectile {
		public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";
		public override void SetDefaults() {
			Projectile.timeLeft = 600;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.hide = true;
			Projectile.rotation = Main.rand.NextFloatDirection();
			Projectile.tileCollide = false;
			Projectile.npcProj = true;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Default;
		}
		public override bool? CanHitNPC(NPC target) => false;
		public override bool CanHitPlayer(Player target) => false;
		public override bool CanHitPvp(Player target) => false;
		public override void AI() {
			if (Projectile.ai[2] != 0) Projectile.scale = Projectile.ai[2];
			if (Projectile.ai[0] > 0 && Main.netMode != NetmodeID.MultiplayerClient) {
				Projectile.ai[0]--;
				Projectile.NewProjectileDirect(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					(Vector2)new PolarVec2(Main.rand.NextFloat(8, 16), Projectile.ai[1]++),
					Defiled_Spike_Explosion_Spike_Hostile.ID,
					Projectile.damage,
					0,
					Projectile.owner,
					ai1: Projectile.whoAmI
				);
			}
		}
	}
	public class Defiled_Spike_Explosion_Spike_Hostile : ModProjectile {
		public static int DifficultyMult => Main.masterMode ? 3 : (Main.expertMode ? 2 : 1);
		public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Projectile.type;
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.timeLeft = Main.rand.Next(22, 25);
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 0;
			Projectile.hide = true;
			Projectile.npcProj = false;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Magic;
		}
		public Projectile ParentProjectile => Main.projectile[(int)Projectile.ai[1]];
		public float MovementFactor {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public override void AI() {
			Projectile.scale = ParentProjectile.scale;
			Projectile.Center = ParentProjectile.Center - Projectile.velocity;
			if (MovementFactor == 0f) {
				MovementFactor = 1f;
				//if(projectile.timeLeft == 25)projectile.timeLeft = projOwner.itemAnimationMax-1;
				Projectile.netUpdate = true;
			}
			if (Projectile.timeLeft > 18) {
				MovementFactor += 1f;
			}
			Projectile.position += Projectile.velocity * MovementFactor * Projectile.scale;
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.rotation += MathHelper.PiOver2;
			ParentProjectile.timeLeft = 7;
		}
		public override bool? CanHitNPC(NPC target) {
			if (ParentProjectile.localNPCImmunity[target.whoAmI] == 0) {
				return null;
			}
			return false;
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			ParentProjectile.localNPCImmunity[target.whoAmI] = -1;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if (DifficultyMult >= 2) {
				if (Main.rand.NextBool(2 * DifficultyMult, 9)) {
					target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), (DifficultyMult - 1) * 15);
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			float totalLength = Projectile.velocity.Length() * MovementFactor;
			int avg = (lightColor.R + lightColor.G + lightColor.B) / 3;
			lightColor = Color.Lerp(lightColor, new Color(avg, avg, avg), 0.5f);
			Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 18, System.Math.Min(58, (int)totalLength)), lightColor, Projectile.rotation, new Vector2(9, 0), Projectile.scale, SpriteEffects.None, 0);
			totalLength -= 58;
			Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.Zero) * 58 * Projectile.scale;
			Texture2D texture = Mod.Assets.Request<Texture2D>("Projectiles/Weapons/Dismay_Mid").Value;
			int c = 0;
			Vector2 pos;
			for (int i = (int)totalLength; i > 0; i -= 58) {
				c++;
				pos = (Projectile.Center - Main.screenPosition) - (offset * c);
				//lightColor = Projectile.GetAlpha(new Color(Lighting.GetColor((pos + Projectile.velocity * 2).ToTileCoordinates()).ToVector4()));
				Main.EntitySpriteDraw(texture, pos, new Rectangle(0, 0, 18, Math.Min(58, i)), lightColor, Projectile.rotation, new Vector2(9, 0), Projectile.scale, SpriteEffects.None, 0);
			}
			return false;
		}
	}
}