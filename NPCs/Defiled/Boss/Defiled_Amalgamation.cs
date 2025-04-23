using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework.Graphics;
using Origins;
using Origins.Buffs;
using Origins.Dev;
using Origins.Dusts;
using Origins.Gores;
using Origins.Gores.NPCs;
using Origins.Graphics;
using Origins.Graphics.Primitives;
using Origins.Items.Accessories;
using Origins.Items.Armor.Vanity.BossMasks;
using Origins.Items.Materials;
using Origins.Items.Other.LootBags;
using Origins.Items.Pets;
using Origins.Items.Tools;
using Origins.Items.Weapons.Magic;
using Origins.Journal;
using Origins.LootConditions;
using Origins.Music;
using Origins.NPCs;
using Origins.NPCs.Defiled.Boss;
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
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using ThoriumMod.Empowerments;
using ThoriumMod.Items.Misc;
using static Defiled_Spike_Indicator;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Origins.NPCs.Defiled.Boss {
	[AutoloadBossHead]
	public class Defiled_Amalgamation : Glowing_Mod_NPC, IDefiledEnemy, ICustomWikiStat, IJournalEntrySource, IOutlineDrawer {
		static AutoLoadingAsset<Texture2D> RightArmTexture = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Right_Arm";
		static AutoLoadingAsset<Texture2D> RightArmGlowTexture = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Right_Arm_Glow";
		static AutoLoadingAsset<Texture2D> LeftArmTexture = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Left_Arm";
		static AutoLoadingAsset<Texture2D> LeftArmGlowTexture = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Left_Arm_Glow";
		static PegasusLib.AutoLoadingAsset<Texture2D> torsoPath = bodyPartsPath + "Torso";
		static string bodyPartsPath = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Split_";

		public string CustomSpritePath => "DefiledAmalg";
		public AssimilationAmount? Assimilation => 0.06f;
		public static bool spawnDA = false;
		float rightArmRot = 0.25f;
		float leftArmRot = 0.25f;
		public float time = 0;
		int trappedTime = 0;
		int roars = 0;
		int armFrame = 0;
		DrawData[] outlineData;
		int laserDuration = 60 * 9;
		DA_Body_Part torso;
		DA_Body_Part arm;
		DA_Body_Part shoulder;
		DA_Body_Part leg1;
		DA_Body_Part leg2;
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
		public const int state_projectiles = 2;
		public const int state_triple_dash = 3;
		public const int state_sidestep_dash = 4;
		public const int state_summon_roar = 5;
		public const int state_ground_spikes = 6;
		public const int state_magic_missile = 7;
		public const int state_laser_rotate = 8;
		public const int state_split_amalgamation_active = 9;
		public const int state_split_amalgamation_start = 10;
		public int AIState { get => (int)NPC.ai[0]; set => NPC.ai[0] = value; }
		public DrawData[] OutlineDrawDatas { get => outlineData; }
		public int OutlineSteps { get => 8; }
		public float OutlineOffset { get => MathF.Sin((float)Main.timeForVisualEffects * 0.3f) * 3; }

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
									new(state_projectiles, 1f),
									new(state_laser_rotate, MathHelper.Lerp(0.1f,1f,NPC.life / (float)(NPC.lifeMax)) * difficultyMult),
									new(state_triple_dash, 0.35f),
									new(state_sidestep_dash, 0.45f + (0.05f * difficultyMult)),
									new(state_summon_roar, 0f),
									new(state_ground_spikes, 1f),
									new(state_magic_missile, 1f),
									new(state_split_amalgamation_start,1f)
									]
								);
								int lastUsedAttack = -AIState;

								if (lastUsedAttack > 0) {
									rand.elements[lastUsedAttack] = new(rand.elements[lastUsedAttack].Item1, rand.elements[lastUsedAttack].Item2 / 3f);
									if (Main.masterMode && lastUsedAttack == state_triple_dash) {
										rand.elements[state_single_dash] = new(rand.elements[state_single_dash].Item1, 0);
										rand.elements[state_triple_dash] = new(rand.elements[state_triple_dash].Item1, 0);
									}
								}

								if (!Collision.CanHitLine(NPC.targetRect.TopLeft(), NPC.targetRect.Width, NPC.targetRect.Height, NPC.Center, 16, 16)) {
									rand.elements[0] = new(rand.elements[0].Item1, rand.elements[0].Item2 / 3f);
									rand.elements[1] = new(rand.elements[1].Item1, rand.elements[1].Item2 * 6f);
									rand.elements[2] = new(rand.elements[2].Item1, rand.elements[2].Item2 / 3f);
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

								if (AIState == state_single_dash) {
									SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(-0.9f), NPC.Center);
								}
								if (AIState == state_sidestep_dash) {
									SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(-0.5f), NPC.Center);
								}
							}
						}
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
						} else if (NPC.ai[1] > 80) {
							AIState = -state_single_dash;
							NPC.ai[1] = 0;
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

						switch ((int)NPC.ai[1]) {
							case 10:
							case 15:
							case 20:
							case 60:
							case 70:
							case 80:
							case 85:
							case 95:
							break;
							case 99:
							SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(-0.6f, -0.4f), NPC.Center);
							Vector2 randomPosAroundTarget = NPC.targetRect.Center() + Main.rand.NextVector2CircularEdge(250, 250);
							for (int i = 0; i < 160; i++) 
							{
								float progress = i / 160f;
								Dust.NewDustDirect(
									Vector2.Lerp(NPC.Center,
									randomPosAroundTarget + new Vector2(0,-250f) * Utils.PingPongFrom01To010(MathF.Sqrt(1f - MathF.Pow(progress - 1f, 2f))),
									progress),
									16,16,
									DustID.AncientLight
									).noGravity = true;

							}
							if (Main.netMode != NetmodeID.MultiplayerClient) {
								float realDifficultyMult = Math.Min(ContentExtensions.DifficultyDamageMultiplier, 3.666f);
								Projectile.NewProjectileDirect(
									NPC.GetSource_FromAI(),
									randomPosAroundTarget,
									Vector2.Zero,
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
								AIState = -state_projectiles;
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
							} else if (NPC.ai[1] % cycleLength < 18 - (difficultyMult * 2)) {
								float speed = 8 + (4 * difficultyMult);
								OriginExtensions.LinearSmoothing(ref NPC.velocity, (new Vector2(NPC.ai[2], NPC.ai[3]) - NPC.Center).WithMaxLength(speed), 8f);
								NPC.oldVelocity = NPC.velocity;
							} else if (NPC.ai[1] % cycleLength > dashLength || NPC.collideX || NPC.collideY) {
								NPC.ai[2] = NPC.targetRect.Center().X;
								NPC.ai[3] = NPC.targetRect.Center().Y;
								goto default;
							}
						} else {
							NPC.velocity.X *= 0.97f;
							if (NPC.velocity.Y < 0) NPC.velocity.Y *= 0.97f;
							//SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitch(-1.2f).WithVolume(0.05f), NPC.Center);
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
										SoundEngine.PlaySound(SoundID.Item62.WithPitch(2f), NPC.Center);
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
						/* April Fools' DAb
						leftArmTarget = 0.15f;
						rightArmTarget = -0.15f;
						*/
						leftArmTarget = 0.6f;
						rightArmTarget = 0.7f;
						armSpeed = 0.2f;
						break;
					}

					case state_magic_missile: {
						CheckTrappedCollision();
						if (NPC.ai[1] < 5) {
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

					// split attack
					case state_split_amalgamation_start: 
					{
						time++;
						NPC.ai[1]++;
						if (NPC.ai[1] < 60) 
						{
							NPC.velocity *= 0.95f;
							armSpeed = 0.1f;
							leftArmTarget = -(0.1f - MathHelper.Pi + MathHelper.PiOver4);
							rightArmTarget = -(0.1f - MathHelper.Pi + MathHelper.PiOver4);
						}
						if (NPC.ai[1] == 60) 
						{
							for (int i = 0; i < 128; i++) {
								Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(100, 100);
								Dust.NewDustPerfect(pos, DustID.AncientLight, NPC.Center.DirectionTo(pos) * 20);

							}
							SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(-1f), NPC.Center);
							SoundEngine.PlaySound(Origins.Sounds.EnergyRipple.WithPitch(-1f), NPC.Center);
							if (Main.netMode != NetmodeID.MultiplayerClient) 
							{
								leg1 = NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DA_Body_Part>(), 0, (int)DA_Body_Part.Part.leg1, NPC.whoAmI).ModNPC as DA_Body_Part;
								leg2 = NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DA_Body_Part>(), 0, (int)DA_Body_Part.Part.leg2, NPC.whoAmI).ModNPC as DA_Body_Part;
								arm = NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DA_Body_Part>(), 0, (int)DA_Body_Part.Part.arm, NPC.whoAmI).ModNPC as DA_Body_Part;
								shoulder = NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DA_Body_Part>(), 0, (int)DA_Body_Part.Part.shoulder, NPC.whoAmI).ModNPC as DA_Body_Part;
								torso = NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DA_Body_Part>(), 0, (int)DA_Body_Part.Part.torso, NPC.whoAmI).ModNPC as DA_Body_Part;


							}
							NPC.dontTakeDamage = true;
							NPC.ShowNameOnHover = false;
							AIState = state_split_amalgamation_active;
							NPC.ai[1] = 0;


						}



						break;
					}
					case state_split_amalgamation_active: {
						time += MathHelper.Lerp(2, 0, MathHelper.Clamp(NPC.ai[1] / 180f,0,1));
						NPC.ai[1]++;
						NPC.Center = NPC.targetRect.Center() + new Vector2(0,-250);


						// parts regroup takes 1 second
						if (NPC.ai[1] == 60 * 16) 
						{
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
					// laser attack *UNUSED/SCRAPPED*
					case state_laser_rotate: {
						float targetHeight = 96 + (float)(Math.Sin(++time * 0.02f) + 0.5f) * 32;
						float targetX = 320 + (float)Math.Sin(++time * 0.01f) * 32;
						float speed = MathHelper.Lerp(15,1, MathHelper.Clamp(NPC.ai[1] / (60 * 3),0f,1f)) ;
						float diffY = NPC.Bottom.Y - (NPC.targetRect.Center().Y - targetHeight);
						float diffX = NPC.Center.X - NPC.targetRect.Center().X;
						diffX -= Math.Sign(diffX) * targetX;
						OriginExtensions.LinearSmoothing(ref NPC.velocity.Y, Math.Clamp(-diffY, -speed, speed), 0.4f);
						OriginExtensions.LinearSmoothing(ref NPC.velocity.X, Math.Clamp(-diffX, -speed, speed), 0.4f);
						leftArmTarget = -(0.1f - MathHelper.Pi + MathHelper.PiOver4);


						rightArmTarget = -(0.1f - MathHelper.Pi + MathHelper.PiOver4);
						if (NPC.ai[1] <= 15)
							NPC.Center = Vector2.Lerp(NPC.Center, NPC.targetRect.Center() + new Vector2(0,-250), NPC.ai[1] / 15f);

						if (NPC.ai[1] == 15) 
						{
							// laser automatically fires after a 1/3 of its max timeleft has passed, Max time left is set in ai[1]
							if (Main.netMode != NetmodeID.MultiplayerClient)
								Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DA_Laser>(), NPC.damage, 0, -1,NPC.whoAmI,60 * 15);


						}

						if (NPC.ai[1] > 60 * 5) 
						{
							armSpeed = 0.1f;
							leftArmTarget = -( MathHelper.PiOver4);
							rightArmTarget = -(MathHelper.PiOver4);
							leftArmRot += Main.rand.NextFloat(-0.1f, 0.2f);
							rightArmRot += Main.rand.NextFloat(-0.1f, 0.2f);

						} else
						{
							leftArmRot += Main.rand.NextFloat(-0.025f, 0.025f);
							rightArmRot += Main.rand.NextFloat(-0.025f, 0.025f);
						}

						if (++NPC.ai[1] == 60 * 16) 
						{

							AIState = -AIState;
							NPC.ai[1] = 0;

						}




						break;
					}
				}
				OriginExtensions.AngularSmoothing(ref rightArmRot, rightArmTarget, armSpeed);
				OriginExtensions.AngularSmoothing(ref leftArmRot, leftArmTarget, armSpeed * 1.5f);
			} else {
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
			if (!NPC.Hitbox.OverlapsAnyTiles()) {
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
				if (AIState == state_split_amalgamation_active) 
				{
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
		public void ResetFrameSize() 
		{

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
			for (int releasedWisps = 0; releasedWisps < 5; releasedWisps++) {
				NPC.NewNPC(npc.GetSource_Death(), (int)npc.position.X + Main.rand.Next(npc.width), (int)npc.position.Y + Main.rand.Next(npc.height), ModContent.NPCType<Defiled_Wisp>());
			}
		}
		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {
			switch (AIState) {
				case 2:
				case 3:
				break;
				default: {
					Rectangle highHitbox = NPC.Hitbox;
					highHitbox.Height /= 4;

					Rectangle lowHitbox = NPC.Hitbox;
					lowHitbox.Y += highHitbox.Height;
					lowHitbox.Height -= highHitbox.Height;
					lowHitbox.Width /= 2;
					lowHitbox.X += lowHitbox.Width / 2;

					if (!highHitbox.Intersects(projectile.Hitbox) && !lowHitbox.Intersects(projectile.Hitbox)) {
						modifiers.DefenseEffectiveness *= 1 + DifficultyMult;
					}
				}
				break;
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			if (DifficultyMult >= 2) {
				if (Main.rand.NextBool(2 * DifficultyMult, 9)) {
					target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), (DifficultyMult - 1) * 46);
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
			if(AIState == state_split_amalgamation_active) 
			{
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

			if(AIState == state_laser_rotate && NPC.ai[1] > 60 * 3)
				this.DrawOutline();

			if (AIState == state_split_amalgamation_active) 
			{
				return false;
			}

			return true;
		}
		public Color SetOutlineColor(float progress) => Color.Lerp(Color.White,Color.Black,MathF.Sin((float)Main.timeForVisualEffects * 0.1f ));

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
	public class DA_Laser : ModProjectile, ITangelaHaver
	{

		public override string Texture => "Origins/Items/Weapons/Magic/Infusion_P";
		public const int LASER_LENGTH = 2500;
		public ref float DA => ref Projectile.ai[0];
		public ref float maxTimeleft => ref Projectile.ai[1];
		public ref float Timer => ref Projectile.ai[2];

		public int? TangelaSeed { get => -1; set; }

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;

		}
		public bool indicatorState = true;

		public override void OnSpawn(IEntitySource source) {
			NPC DA_boss = Main.npc[(int)DA];
			Projectile.rotation = DA_boss.Center.DirectionTo(DA_boss.targetRect.Center()).ToRotation();
			Projectile.timeLeft = (int)maxTimeleft;
		}
		public override void AI() {

			Projectile.Center = Main.npc[(int)DA].Center - Projectile.velocity;
			Vector2 targetCenter = Main.npc[(int)DA].targetRect.Center();
			Projectile.rotation = Projectile.rotation.AngleTowards(Projectile.Center.DirectionTo(targetCenter).ToRotation(),0.005f);
			indicatorState = Projectile.timeLeft > maxTimeleft - maxTimeleft / 3f;
			if (!indicatorState) Timer++;
			if(Projectile.timeLeft == maxTimeleft / 2f) {


				for(int i = 0; i < LASER_LENGTH; i += LASER_LENGTH / 7) 
				{

					if (i == 0 || i > LASER_LENGTH / 2)
						continue;
					//right arc

					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center + Projectile.rotation.ToRotationVector2() * i,
						Vector2.Zero,
						ModContent.ProjectileType<DA_Arc_Bolt>(),
						Projectile.damage / 2,
						0,
						-1,
						i,
						DA);

					//left arc

					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center + Projectile.rotation.ToRotationVector2() * i,
						Vector2.Zero,
						ModContent.ProjectileType<DA_Arc_Bolt>(),
						Projectile.damage / 2,
						0,
						-1,
						-i,
						DA);


				}

			}

			if (!Main.npc[(int)DA].active)
				Projectile.Kill();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CollisionExtensions.PolygonIntersectsRect([(Projectile.Center, Projectile.Center + Projectile.rotation.ToRotationVector2() * LASER_LENGTH)], targetHitbox) && !indicatorState;


		public override bool PreDraw(ref Color lightColor) {



			if (renderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return false;
			}
			Origins.shaderOroboros.Capture();
			float laserAnimationProgress = MathHelper.Lerp(1, 0, Timer / (float)(maxTimeleft / 2f));
			if (indicatorState)
				default(DefiledIndicator).Draw(
				[Projectile.Center, Projectile.Center + Projectile.rotation.ToRotationVector2() * LASER_LENGTH],
				[Projectile.rotation - MathHelper.Pi, Projectile.rotation - MathHelper.Pi],
				MathHelper.Lerp(0, 64, (Projectile.timeLeft - maxTimeleft / 2) / (maxTimeleft / 2)), 0.5f, 0.5f
				);
			else
				default(DefiledLaser).Draw(
					[Projectile.Center, Projectile.Center + Projectile.rotation.ToRotationVector2() * LASER_LENGTH],
					[Projectile.rotation - MathHelper.Pi, Projectile.rotation - MathHelper.Pi],
					MathHelper.Lerp(0, 64, Projectile.timeLeft <= 120 ? MathHelper.Lerp(0f, 1f, Projectile.timeLeft / 120f) : 1f),
					laserAnimationProgress
					);
			Origins.shaderOroboros.DrawContents(renderTarget);
			Origins.shaderOroboros.Reset(default);
			Vector2 center = renderTarget.Size() * 0.5f;


			SceneFiltersIgnoredDrawing.DrawWithoutFilters(renderTarget,
				center,
				null,
				Color.White,
				0,
				center,
				Vector2.One / Main.GameViewMatrix.Zoom,
				SpriteEffects.None);

			return false;
		}

		public override void OnKill(int timeLeft) {
			if (renderTarget is not null) {
				Main.QueueMainThreadAction(renderTarget.Dispose);
				Main.OnResolutionChanged -= Resize;
			}
		}
		internal RenderTarget2D renderTarget;
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			renderTarget.Dispose();
			SetupRenderTargets();
		}
		void SetupRenderTargets() {
			if (renderTarget is not null && !renderTarget.IsDisposed) return;
			renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}
}
	public class DA_Arc_Bolt : ModProjectile {

		public override string Texture => "Origins/Items/Weapons/Magic/Infusion_P";

	public override void SetStaticDefaults() {
		ProjectileID.Sets.TrailCacheLength[Type] = 40;
		ProjectileID.Sets.TrailingMode[Type] = 3;

	}
	public override void SetDefaults() {
		Projectile.width = 16;
		Projectile.height = 16;
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.aiStyle = -1;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 300;
		Projectile.tileCollide = false;
	}

	public override void AI() {
		if (Projectile.ai[1] != -1)
			Projectile.Center = Projectile.Center.RotatedBy((MathHelper.PiOver2 * MathF.Sign(Projectile.ai[0])) / (60f), Main.npc[(int)Projectile.ai[1]].Center);

		Projectile.rotation = Projectile.oldPosition.DirectionTo(Projectile.Center).ToRotation() + MathHelper.PiOver2;
	}

	public override bool PreDraw(ref Color lightColor) {
		if (renderTarget is null) {
			Main.QueueMainThreadAction(SetupRenderTargets);
			Main.OnResolutionChanged += Resize;
			return false;
		}
		Origins.Origins.shaderOroboros.Capture();
		default(DefiledBolt).Draw(Projectile.oldPos, Projectile.oldRot, Projectile.timeLeft / 300f);
		Origins.Origins.shaderOroboros.DrawContents(renderTarget);
		Origins.Origins.shaderOroboros.Reset(default);
		Vector2 center = renderTarget.Size() * 0.5f;


		SceneFiltersIgnoredDrawing.DrawWithoutFilters(renderTarget,
			center,
			null,
			Color.White,
			0,
			center,
			Vector2.One / Main.GameViewMatrix.Zoom,
			SpriteEffects.None);

		return false;
	}

	public override void OnKill(int timeLeft) {
		if (renderTarget is not null) {
			Main.QueueMainThreadAction(renderTarget.Dispose);
			Main.OnResolutionChanged -= Resize;
		}
	}
	internal RenderTarget2D renderTarget;
	public void Resize(Vector2 _) {
		if (Main.dedServ) return;
		renderTarget.Dispose();
		SetupRenderTargets();
	}
	void SetupRenderTargets() {
		if (renderTarget is not null && !renderTarget.IsDisposed) return;
		renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
	}

}

public class DA_Flan : ModProjectile, ITangelaHaver
{

	public const int tick_motion = 8;
	public override string Texture => "Origins/Projectiles/Weapons/Seam_Beam_P";
	public override void SetStaticDefaults() {
		const int max_length = 1200;
		ProjectileID.Sets.TrailCacheLength[Type] = max_length / tick_motion;
		ProjectileID.Sets.DrawScreenCheckFluff[Type] = max_length + 16;
		Origins.Origins.HomingEffectivenessMultiplier[Type] = 25f;
		Mitosis_P.aiVariableResets[Type][1] = true;
	}
	public override void SetDefaults() {
		Projectile.DamageType = DamageClass.Magic;
		Projectile.width = 10;
		Projectile.height = 10;
		Projectile.friendly = false;
		Projectile.hostile = true;
		Projectile.penetrate = -1;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = -1;
		Projectile.tileCollide = true;
		Projectile.extraUpdates = 25;
	}

	public override void ModifyDamageHitbox(ref Rectangle hitbox) {
		hitbox.Inflate(2, 2);
	}
	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
		for (int i = 1; i < Projectile.ai[1] && i < Projectile.oldPos.Length; i++) {
			Vector2 pos = Projectile.oldPos[^i];
			if (pos == default) {
				break;
			} else if (Origins.OriginExtensions.Recentered(projHitbox,pos).Intersects(targetHitbox)) {
				return true;
			}
		}
		return null;
	}
	protected Vector2? target = null;
	protected int startupDelay = 2;
	protected float randomArcing = 0.3f;
	public override void AI() {
		target ??= Projectile.Center + Projectile.velocity * 25 * (10 - Projectile.ai[2]);
		if (Projectile.numUpdates == -1 && ++Projectile.ai[2] >= 20) {
			Projectile.Kill();
			return;
		}
		if (Projectile.ai[0] != 1) {
			if ((Projectile.numUpdates + 1) % 5 == 0 && startupDelay <= 0) {
				float speed = Projectile.velocity.Length();
				if (speed != 0) Projectile.velocity = (target.Value - Projectile.Center).SafeNormalize(Projectile.velocity / speed).RotatedByRandom(randomArcing) * speed;
			}
			if (startupDelay > 0) {

				startupDelay--;
			} else {
				if (++Projectile.ai[1] > ProjectileID.Sets.TrailCacheLength[Type]) {
					StopMovement();
				} else {
					int index = (int)Projectile.ai[1];
					Projectile.oldPos[^index] = Projectile.Center;
					Projectile.oldRot[^index] = Projectile.velocity.ToRotation();
				}
			}
		}
	}
	public override bool OnTileCollide(Vector2 oldVelocity) {
		Vector2 direction = oldVelocity.SafeNormalize(default);
		if (direction != default) {
			float[] samples = new float[3];
			Collision.LaserScan(
				Projectile.Center,
				direction,
				5,
				32,
				samples
			);
			if (samples.Average() > tick_motion * 0.5f) {
				Projectile.Center += direction * tick_motion;
				int index = Math.Min((int)++Projectile.ai[1], Projectile.oldPos.Length);
				Projectile.oldPos[^index] = Projectile.Center;
				Projectile.oldRot[^index] = oldVelocity.ToRotation();
			}
		}
		StopMovement();
		return false;
	}
	protected void StopMovement() {
		Projectile.velocity = Vector2.Zero;
		Projectile.ai[0] = 1;
		Projectile.extraUpdates = 0;
	}
	public int? TangelaSeed { get; set; }
	public override bool PreDraw(ref Color lightColor) {
		if (renderTarget is null) {
			Main.QueueMainThreadAction(SetupRenderTargets);
			Main.OnResolutionChanged += Resize;
			return false;
		}
		Origins.Origins.shaderOroboros.Capture();
		Nerve_Flan_P_Drawer.Draw(Projectile);
		Origins.Origins.shaderOroboros.DrawContents(renderTarget);
		Origins.Origins.shaderOroboros.Reset(default);
		Vector2 center = renderTarget.Size() * 0.5f;
		TangelaVisual.DrawTangela(
			this,
			renderTarget,
			center,
			null,
			0,
			center,
			Vector2.One / Main.GameViewMatrix.Zoom,
			SpriteEffects.None,
			Main.screenPosition
		);
		return false;
	}
	public override void OnKill(int timeLeft) {
		if (renderTarget is not null) {
			Main.QueueMainThreadAction(renderTarget.Dispose);
			Main.OnResolutionChanged -= Resize;
		}
	}
	internal RenderTarget2D renderTarget;
	public void Resize(Vector2 _) {
		if (Main.dedServ) return;
		renderTarget.Dispose();
		SetupRenderTargets();
	}
	void SetupRenderTargets() {
		if (renderTarget is not null && !renderTarget.IsDisposed) return;
		renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
	}

}

public class DA_Body_Part : ModNPC, IOutlineDrawer
{
	static string bodyPartsPath = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Split_";
	static PegasusLib.AutoLoadingAsset<Texture2D> torsoPath = bodyPartsPath + "Torso";
	static PegasusLib.AutoLoadingAsset<Texture2D> armPath = bodyPartsPath + "Arm";
	static PegasusLib.AutoLoadingAsset<Texture2D> leg1Path = bodyPartsPath + "Leg1";
	static PegasusLib.AutoLoadingAsset<Texture2D> leg2Path = bodyPartsPath + "Leg2";
	static PegasusLib.AutoLoadingAsset<Texture2D> shoulderPath = bodyPartsPath + "Shoulder";
	static PegasusLib.AutoLoadingAsset<Texture2D> torsoGlowPath = bodyPartsPath + "Torso_Glow";

	static PegasusLib.AutoLoadingAsset<Texture2D> armGlowPath = bodyPartsPath + "Arm_Glow";
	static PegasusLib.AutoLoadingAsset<Texture2D> leg1GlowPath = bodyPartsPath + "Leg1_Glow";
	static PegasusLib.AutoLoadingAsset<Texture2D> leg2GlowPath = bodyPartsPath + "Leg2_Glow";
	static PegasusLib.AutoLoadingAsset<Texture2D> shoulderGlowPath = bodyPartsPath + "Shoulder_Glow";
	static PegasusLib.AutoLoadingAsset<Texture2D> RightArmPath = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Right_Arm";
	static PegasusLib.AutoLoadingAsset<Texture2D> RightArmGlowPath = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Right_Arm_Glow";
	static PegasusLib.AutoLoadingAsset<Texture2D> LeftArmPath = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Left_Arm";
	static PegasusLib.AutoLoadingAsset<Texture2D> LeftArmGlowPath = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Left_Arm_Glow";
	ref float PartType => ref NPC.ai[0];
	Defiled_Amalgamation DA;
	int maxFrames = -1;
	int currentFrame = 0;
	int frameHeight = 0;
	public override string Texture => bodyPartsPath + "Torso";
	public override void SetDefaults() {
		
		NPC.friendly = false;
		NPC.aiStyle = -1;
		NPC.width = NPC.height = 32;
		NPC.lifeMax = 1000;
		NPC.noGravity = true;
		NPC.knockBackResist = 0L;
		NPC.noTileCollide = true;
		NPC.damage = 0;
	}
	public enum Part : byte 
	{
			
		torso = 0,
		arm = 1,
		leg1 = 2,
		leg2 = 3,
		shoulder = 4

	}

	public void SetupPart() 
	{
		switch ((Part)(int)PartType) 
		{
			// only the torso can take damage
			case Part.torso:
			maxFrames = 4;
			NPC.frame = new Rectangle(0,0,80,60);
			frameHeight = 60;
			break;

			case Part.arm:
			maxFrames = 4;
			NPC.frame = new Rectangle(0, 0, 20, 50);
			frameHeight = 50;
			break;

			case Part.leg1:
			maxFrames = 5;
			NPC.frame = new Rectangle(0, 0, 40, 90);
			frameHeight = 90;
			NPC.noTileCollide = false;
			NPC.noGravity = false;

			break;		
			
			case Part.leg2:
			maxFrames = 5;
			NPC.frame = new Rectangle(0, 0, 50, 380 / 5);
			frameHeight = 380 / 5;
			NPC.noTileCollide = false;
			NPC.noGravity = false;
			break;

			case Part.shoulder:
			maxFrames = 5;
			NPC.frame = new Rectangle(0,0,44,46);
			frameHeight = 46;
			break;
		}


			

		

	}

	public override void OnSpawn(IEntitySource source) {
		if(Main.npc[(int)NPC.ai[1]].ModNPC is Defiled_Amalgamation DA) 
		{
			this.DA = DA;

		}

		SetupPart();
	}

	ref float Timer => ref NPC.ai[3];

	public DrawData[] OutlineDrawDatas => 
	[
		new(RightArmPath, NPC.Center, new Rectangle(0, (384 / 4) * currentFrame, 30, ((384 / 4))), Color.White, NPC.rotation - MathHelper.PiOver2, NPC.frame.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0)
		
	];

	public int OutlineSteps => 4;

	public float OutlineOffset => 15 * ((Timer - 60f) / 60f);

	public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
		UpdateDAHealth(hit);
	}

	public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
		UpdateDAHealth(hit);
	}

	public void UpdateDAHealth(NPC.HitInfo hit) 
	{
		int damageDone = DA.NPC.StrikeNPC(hit, fromNet: false, false);
		if (Main.netMode != NetmodeID.SinglePlayer)
			NetMessage.SendStrikeNPC(DA.NPC, hit);
	}

	public override void AI() {
		NPC.lifeMax = DA.NPC.lifeMax;
		NPC.life = DA.NPC.life;
		NPC.target = Main.maxPlayers;
		NPC.TargetClosest(true);
		if (!NPC.HasValidTarget) 
		{
			NPC.position.Y += 10;
			NPC.EncourageDespawn(2);
			return;
		}

		if ((Part)PartType != Part.leg1 && (Part)PartType != Part.leg2 && ++NPC.frameCounter % 7 == 0)
			if (++currentFrame == maxFrames - 1)
				currentFrame = 0;

		NPC.ai[2]++;
		if (NPC.ai[2] < 200) 
		{
			NPC.Center = Vector2.Lerp(NPC.Center,NPC.targetRect.Center() + 
				new Vector2(MathF.Sin(DA.time * 0.1f + (float.Tau * 0.2f * PartType)) * 400,
				MathF.Cos(DA.time * 0.1f + (float.Tau * 0.2f * PartType)) * 200), NPC.ai[2] / 160f);
			NPC.velocity = Vector2.Zero;
			return;
		}

		NPC.damage = DA.NPC.damage;

		//regroup
		if (NPC.ai[2] >= 60 * 15) 
		{
			float progress = (NPC.ai[2] - 60 * 15) / 60f;
			NPC.Center = Vector2.Lerp(NPC.Center, DA.NPC.Center,progress);
			NPC.damage = 0;


			if (progress >= 1) 
			{
				NPC.active = false;
				
			}


			return;
		};
		NPC.spriteDirection = NPC.velocity.X > 0 ? -1 : 1;

		switch ((Part)(int)PartType) {

			case Part.leg1:
			case Part.leg2:
			LegsAI();
			break;
			case Part.arm:
			ArmAI();
			break;
			case Part.shoulder:
			ShoulderAI();
			break;
			case Part.torso:
			TorsoAI();
			break;
		}

		NPC.width = NPC.frame.Height;
		NPC.height = NPC.frame.Height;
	}
	public void TorsoAI() 
	{

		NPC.velocity = NPC.Center.DirectionTo(NPC.targetRect.Center()) * 3;
	}

	public void ShoulderAI() 
	{
		Timer++;
		NPC.velocity *= 0.96f;
		if (Timer < 120) 
		{

			NPC.rotation += MathHelper.Lerp(0, 1f, Timer / 120);
		}
		if (Timer == 120) 
		{

			NPC.velocity = NPC.Center.DirectionTo(NPC.targetRect.Center()) * 35;
			NPC.rotation = NPC.velocity.ToRotation();
		}

		if (Timer == 160) 
		{

			NPC.velocity = Vector2.Zero;
			NPC.ai[3] = 0;

		}

	}
	bool charging = false;
	public void ArmAI() 
	{

		Timer++;
		NPC.velocity.Y = MathF.Sin(Timer * 0.05f) * 2;
		if(Timer >= 60 && !charging) 
		{
			if (Main.rand.NextBool()) 
			{
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.rotation.ToRotationVector2() * 15, ModContent.ProjectileType<DA_Arc_Bolt>(), NPC.damage, 0, -1, -1, -1, -1);
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.rotation.ToRotationVector2().RotatedBy(0.1f) * 15, ModContent.ProjectileType<DA_Arc_Bolt>(), NPC.damage, 0, -1, -1, -1, -1);
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.rotation.ToRotationVector2().RotatedBy(-0.1f) * 15, ModContent.ProjectileType<DA_Arc_Bolt>(), NPC.damage, 0, -1, -1, -1, -1);
				Timer = 0;


			} else if (Main.rand.NextBool()) {

				charging = true;

			}
			else
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.rotation.ToRotationVector2() * 25, ModContent.ProjectileType<Low_Signal_Hostile>(), NPC.damage, 0, -1, 0, 0, 0);

		
		}

		if (charging)
		{
			NPC.rotation = NPC.rotation.AngleTowards(NPC.targetRect.Center().DirectionFrom(NPC.Center).ToRotation(), 0.005f);

			if (Timer >= 120) {
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.rotation.ToRotationVector2() * 25, ModContent.ProjectileType<DA_Flan>(), NPC.damage, 0, -1, 0, 0, 0);
				charging = false;
			}
		}

		if (!charging) 
		{
			NPC.Center = Vector2.Lerp(NPC.Center - new Vector2(5, 0).RotatedBy(NPC.rotation), NPC.Center + new Vector2(10, 0).RotatedBy(NPC.rotation), Timer / 60f);
			NPC.rotation = NPC.rotation.AngleTowards(NPC.targetRect.Center().DirectionFrom(NPC.Center).ToRotation(), 0.02f);

			if (Timer >= 60)
				Timer = 0;
		}



	}
	public void LegsAI() 
	{

		// tried anti-stuck it, tho idk
		if (Timer == -1) {

			NPC.Center = Vector2.Lerp(NPC.Center, NPC.targetRect.Center() - new Microsoft.Xna.Framework.Vector2(0, 500), 0.2f);
			if (NPC.collideX || NPC.collideY)
				return;
			Timer = 0;
		}

		Timer++;



		if(Timer >= 20 && (NPC.collideY || NPC.collideX)) 
		{
			Timer = 0;
			if((Part)PartType == Part.leg1)
				NPC.velocity = NPC.targetRect.X > NPC.Center.X ? new Vector2(14,-7) : new Vector2(-14,-7);
			else
				NPC.velocity = NPC.targetRect.X > NPC.Center.X ? new Vector2(7, -14) : new Vector2(-7, -14);

		}

		if (NPC.collideY && NPC.velocity.Y > 0) 
		{

			NPC.velocity.Y = 0;
		}

		NPC.rotation = NPC.velocity.Y * NPC.spriteDirection * 0.1f;
		}
	
	public override void FindFrame(int frameHeight) {
		NPC.frame.Y = currentFrame * ((this.frameHeight * (maxFrames - 1)) / (maxFrames - 1));
	}



	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		Texture2D texture = torsoPath;
		Texture2D glowTexture = torsoGlowPath;
		switch ((Part)(int)PartType) 
		{
		
			case Part.leg1:
			texture = leg1Path;
			glowTexture = leg1GlowPath;
			break;
			case Part.leg2:
			texture = leg2Path;
			glowTexture = leg2GlowPath;

			break;
			case Part.shoulder:
			texture = shoulderPath;
			glowTexture = shoulderGlowPath;
			// assumes that the maximum amount of frames is same as the wings 
			spriteBatch.Draw(LeftArmPath, NPC.Center - Main.screenPosition, new Rectangle(0, (384 / 4) * currentFrame, 30, ((384 / 4))), drawColor, NPC.rotation - MathHelper.PiOver2, NPC.frame.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			spriteBatch.Draw(LeftArmGlowPath, NPC.Center - Main.screenPosition, new Rectangle(0, (384 / 4) * currentFrame, 30, ((384 / 4))), Color.White, NPC.rotation - MathHelper.PiOver2, NPC.frame.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			break;
			case Part.arm:
			texture = armPath;
			glowTexture = armGlowPath;
			if(Timer > 60)
				this.DrawOutline();
			// assumes that the maximum amount of frames is same as the wings 
			spriteBatch.Draw(RightArmPath, NPC.Center - Main.screenPosition, new Rectangle(0, (384 / 4) * currentFrame, 30, ((384 / 4))), drawColor, NPC.rotation - MathHelper.PiOver2, NPC.frame.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			spriteBatch.Draw(RightArmGlowPath, NPC.Center - Main.screenPosition, new Rectangle(0, (384 / 4) * currentFrame, 30, ((384 / 4))), Microsoft.Xna.Framework.Color.White, NPC.rotation - MathHelper.PiOver2, NPC.frame.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			break;


		}


		spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally | SpriteEffects.FlipHorizontally,0);
		spriteBatch.Draw(glowTexture, NPC.Center - Main.screenPosition, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically,0);

		if (charging) 
		{

			default(DefiledIndicator).Draw([NPC.Center, NPC.Center + NPC.rotation.ToRotationVector2() * 1500], [NPC.rotation, NPC.rotation + MathHelper.Pi],MathHelper.Lerp(15,1,(Timer - 60f) / 60f),0f,0.5f);
			
		}

		return false;
	}

	public Color SetOutlineColor(float progress) => Color.Lerp(Color.Green,Color.Purple, progress);
}
	public class Bendy_Indicator : ModProjectile {
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
					ModContent.ProjectileType<Defiled_Spike_Explosion_Hostile_Unused>(),
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
				ModContent.ProjectileType<Defiled_Spike_Explosion_Hostile_Unused>(),
				Projectile.damage,
				0,
				Projectile.owner,
			7).localNPCImmunity.AsSpan());
		}
	}
	public class Defiled_Spike_Explosion_Hostile_Unused : ModProjectile {
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
			if (Projectile.ai[0] > 0) {
				Projectile.ai[0]--;
				Projectile.NewProjectileDirect(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					(Vector2)new PolarVec2(Main.rand.NextFloat(8, 16), Projectile.ai[1]++),
					ModContent.ProjectileType<DA_Bendy_Spikes>(),
					Projectile.damage,
					0,
					Projectile.owner,
					10,
					ai1: Projectile.whoAmI,
					20
				);
			}
		}
	}
	public class Defiled_Spike_Explosion_Spike_Hostile_Unused : ModProjectile {
		public static int DifficultyMult => Main.masterMode ? 3 : (Main.expertMode ? 2 : 1);
		public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";
		public static int ID { get; private set; }
		Vector2 realPosition;
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
			Projectile.npcProj = true;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Default;
		}
		public override void OnSpawn(IEntitySource source) {
		}
		public Projectile ParentProjectile => Main.projectile[(int)Projectile.ai[1]];
		public float movementFactor {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public override void AI() {
			Projectile.Center = ParentProjectile.Center - Projectile.velocity;
			if (movementFactor == 0f) {
				movementFactor = 1f;
				//if(projectile.timeLeft == 25)projectile.timeLeft = projOwner.itemAnimationMax-1;
				Projectile.netUpdate = true;
			}
			if (Projectile.timeLeft > 18) {
				movementFactor += 1f;
			}
			Projectile.position += Projectile.velocity * movementFactor;
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
					target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), DifficultyMult * 67);
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			float totalLength = Projectile.velocity.Length() * movementFactor;
			int avg = (lightColor.R + lightColor.G + lightColor.B) / 3;
			lightColor = Color.Lerp(lightColor, new Color(avg, avg, avg), 0.5f);
			Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 18, System.Math.Min(58, (int)totalLength)), lightColor, Projectile.rotation, new Vector2(9, 0), Projectile.scale, SpriteEffects.None, 0);
			totalLength -= 58;
			Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.Zero) * 58;
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
	public class DA_Bendy_Spikes : ModProjectile {
		float progress = 0;
		public int maxTimeleft = 20 + (int)(10 * Origins.ContentExtensions.DifficultyDamageMultiplier);
		public override string Texture => PegasusLib.PegasusExt.GetDefaultTMLName(typeof(DA_Spike));
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
		Projectile parentProj => Main.projectile[(int)Projectile.ai[1]];

		public override void AI() {
			if(Projectile.timeLeft <= maxTimeleft)
				progress = Utils.GetLerpValue(0f,1f, Utils.PingPongFrom01To010((((Projectile.timeLeft) / (float)maxTimeleft))),true);
			Projectile.Center = parentProj.Center - Projectile.velocity;
			float maxGrowth = 128 * Origins.ContentExtensions.DifficultyDamageMultiplier;
			if (Projectile.ai[0] < maxGrowth) {
				int diff = (int)(Math.Min(Projectile.ai[0] + 16, maxGrowth) - Projectile.ai[0]);
				Projectile.ai[0] += diff;
				Projectile.Center -= Projectile.rotation.ToRotationVector2() * diff;
			}
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			hitbox.Height += (int)Projectile.ai[0];
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override void OnKill(int timeLeft) {
			
		}
		/// <summary>
		/// Call a programmer if this method lasts longer than four hours
		/// </summary>
		/// <returns></returns>
		public (Vector2 pos, Vector2 perpendicular)[] GetCurve(float progress) {
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
			return Origins.CollisionExtensions.PolygonIntersectsRect(_lines.ToArray(), targetHitbox);
		}
		public static float InExpo(float t, float strength) => (float)Math.Pow(2, strength * (t - 1));
		public static float OutExpo(float t, float strength) => 1 - InExpo(1 - t, strength);
		public static float InOutExpo(float t, float strength) {
			if (t < 0.5) return InExpo(t * 2, strength) * .5f;
			return 1 - InExpo((1 - t) * 2, strength) * .5f;
		}
		public override bool PreDraw(ref Color lightColor) {

			var curve = GetCurve(1f);

			Vector2[] positions = new Vector2[curve.Length];
			float[] rotations = new float[curve.Length];

			for (int i = 0; i < curve.Length; i++) 
			{

				positions[i] = curve[i].pos;
				rotations[i] = curve[i].perpendicular.ToRotation() + MathHelper.PiOver2;
				

			}


			if(progress > 0) {
				default(DefiledPortal).Draw(curve[0].pos - Main.screenPosition, Projectile.velocity.ToRotation() + MathHelper.PiOver2, new Vector2(128, 64), Utils.GetLerpValue(0, 1, (MAX_TIMELEFT + Projectile.timeLeft) / (float)MAX_TIMELEFT));
				default(DefiledSpikeStrip).Draw(TextureAssets.Projectile[Type], positions, rotations, progress, Projectile.ai[0]);

			}
			return false;
		}
	}
	public class Defiled_Spike_Indicator : ModProjectile {

		public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";


		public const int INDICATOR_DURATION = 40;
		public const int MAX_TIMELEFT = 120;
		public Vector2 startingPosition;
		public float curveAmount = 0;
		public int segments = 15;
		public DA_Bendy_Spikes childSpike;
		public int fadeInOutTimer = 0;
		public override void OnSpawn(IEntitySource source) {
			if (Projectile.ai[2] != 0) Projectile.scale = Projectile.ai[2];
			startingPosition = Projectile.Center;
			Projectile.rotation = Projectile.Center.DirectionTo(Main.player[(int)Projectile.ai[1]].Center).RotatedByRandom(1).ToRotation();
			curveAmount = Main.rand.NextFloat() * MathHelper.Pi * (Main.rand.NextBool() ? -1 : 1);
			childSpike = Projectile.NewProjectileDirect(
				Projectile.GetSource_FromThis(),
				Projectile.Center,
				(Vector2)Projectile.rotation.ToRotationVector2() * 92,
				ModContent.ProjectileType<DA_Bendy_Spikes>(),
				Projectile.damage,
				0,
				Projectile.owner,
				segments * 32f,
				Projectile.whoAmI, curveAmount / 2f / (segments)
			).ModProjectile as DA_Bendy_Spikes;
		}
		public override void SetDefaults() {
			Projectile.timeLeft = MAX_TIMELEFT;
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

		public override bool PreDraw(ref Color lightColor) {
			(Vector2 pos, Vector2 per)[] curve = childSpike.GetCurve(1f);
			Vector2[] pos = new Vector2[segments * 32];
			float[] rot = new float[segments * 32];
			for (int i = 0; i < curve.Length; i++) {

				pos[i] = curve[i].pos;
				rot[i] = curve[i].per.ToRotation() + MathHelper.PiOver2;

			}

			if (Projectile.timeLeft > MAX_TIMELEFT - 10)
				return false;


			default(DefiledIndicator).Draw(
				pos, rot, 16f, Utils.GetLerpValue(0f, 1f, Utils.PingPongFrom01To010((Projectile.timeLeft) / (float)MAX_TIMELEFT)), ((Projectile.timeLeft) / (float)MAX_TIMELEFT));


			return false;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override void AI() {

			if (Projectile.ai[2] != 0) Projectile.scale = Projectile.ai[2];
			fadeInOutTimer++;
		}
		public override void OnKill(int timeLeft) {

		}


		public struct DefiledIndicator {
			private static VertexStrip vertexStrip = new VertexStrip();

			public void Draw(Vector2[] positions, float[] rotations, float width, float alpha, float progress) {
				MiscShaderData shader = GameShaders.Misc["Origins:DefiledIndicator"];
				shader.UseImage1(TextureAssets.Extra[193]);
				shader.UseColor(Color.Black);
				shader.UseSecondaryColor(Color.Green);
				shader.UseShaderSpecificData(new Vector4(alpha, progress, 0, 0));
				shader.Apply();
				vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, (p) => Color.White, (p) => width, -Main.screenPosition,false);
				vertexStrip.DrawTrail();

			}
		}

		public struct DefiledSpikeStrip {
			private static VertexStrip vertexStrip = new VertexStrip();

			public void Draw(Asset<Texture2D> tex,Vector2[] positions, float[] rotations, float progress, float length) {
				MiscShaderData shader = GameShaders.Misc["Origins:DefiledSpike"];
				shader.UseImage1(tex);
				shader.UseImage2(ModContent.Request<Texture2D>("Origins/NPCs/Defiled/Boss/DA_Spike_Base"));
				shader.UseSamplerState(SamplerState.AnisotropicClamp);
				shader.UseColor(Color.Beige);
				shader.UseSecondaryColor(Color.Pink);
				shader.UseShaderSpecificData(new Vector4(progress, length, 0, 0));
				shader.Apply();
				vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, (p) => Color.White, (p) => 16, -Main.screenPosition, false);
				vertexStrip.DrawTrail();

			}
		}

		public struct DefiledPortal {
			private static VertexRectangle rect = new VertexRectangle();

			public void Draw(Vector2 position, float rotation, Vector2 size, float progress) {
				MiscShaderData shader = GameShaders.Misc["Origins:DefiledPortal"];
				shader.UseImage1(TextureAssets.Extra[193]);
				shader.UseSamplerState(SamplerState.PointWrap);
				shader.UseColor(Color.Cyan);
				shader.UseSecondaryColor(Color.Purple);
				shader.UseShaderSpecificData(new Vector4(0,0,Main.LocalPlayer.Center.X,Main.LocalPlayer.Center.Y));
				shader.Apply();
				rect.Draw(position,Color.White,((progress)) * size,rotation,position);

			}
		} 

		public struct DefiledLaser 
		{
			private static VertexStrip vertexStrip = new VertexStrip();

			public void Draw(Vector2[] positions, float[] rotations, float width, float progress) {
				MiscShaderData shader = GameShaders.Misc["Origins:DefiledLaser"];
				shader.UseImage1(TextureAssets.Extra[193]);
				shader.UseColor(Color.Black);
				shader.UseSecondaryColor(Color.Green);
				shader.UseShaderSpecificData(new Vector4(progress, 0, 0, 0));
				shader.Apply();
				vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, (p) => Color.White, (p) => width, -Main.screenPosition, false);
				vertexStrip.DrawTrail();

			}

		}

		public struct DefiledBolt 
		{
		private static VertexStrip vertexStrip = new VertexStrip();

		public void Draw(Vector2[] positions, float[] rotations, float progress) {
			MiscShaderData shader = GameShaders.Misc["Origins:DefiledLaser"];
			shader.UseImage1(TextureAssets.Extra[193]);
			shader.UseColor(Color.Black);
			shader.UseSecondaryColor(Color.Green);
			shader.UseShaderSpecificData(new Vector4(progress, 0, 0, 0));
			shader.Apply();
			vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, (p) => Color.White, (p) => MathHelper.Lerp(MathHelper.Lerp(64,0,1f - p),1,p), -Main.screenPosition, false);
			vertexStrip.DrawTrail();

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
		if (Projectile.ai[0] > 0) {
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
	Vector2 realPosition;
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
	public float movementFactor {
		get => Projectile.ai[0];
		set => Projectile.ai[0] = value;
	}
	public override void AI() {
		Projectile.scale = ParentProjectile.scale;
		Projectile.Center = ParentProjectile.Center - Projectile.velocity;
		if (movementFactor == 0f) {
			movementFactor = 1f;
			//if(projectile.timeLeft == 25)projectile.timeLeft = projOwner.itemAnimationMax-1;
			Projectile.netUpdate = true;
		}
		if (Projectile.timeLeft > 18) {
			movementFactor += 1f;
		}
		Projectile.position += Projectile.velocity * movementFactor * Projectile.scale;
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
		float totalLength = Projectile.velocity.Length() * movementFactor;
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





