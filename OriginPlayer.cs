using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Accessories;
using Origins.Items.Armor.Riptide;
using Origins.Items.Armor.Vanity.Dev.PlagueTexan;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Fish;
using Origins.Items.Tools;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Journal;
using Origins.NPCs;
using Origins.Projectiles;
using Origins.Projectiles.Misc;
using Origins.Questing;
using Origins.Water;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Origins.OriginExtensions;

namespace Origins {
	public class OriginPlayer : ModPlayer {
		#region variables and defaults
		public const float rivenMaxMult = 0.3f;
		public float rivenMult => (1f - rivenMaxMult) + Math.Max((Player.statLife / (float)Player.statLifeMax2) * (rivenMaxMult * 2), rivenMaxMult);

		#region armor/set bonuses
		public bool fiberglassSet = false;
		public bool cryostenSet = false;
		public bool cryostenHelmet = false;
		public bool felnumSet = false;
		public float felnumShock = 0;
		public float oldFelnumShock = 0;
		public bool minerSet = false;
		public bool defiledSet = false;
		public bool rivenSet = false;
		public bool rivenSetBoost = false;
		public bool riftSet = false;
		public bool eyndumSet = false;
		public bool mimicSet = false;
		public bool riptideSet = false;
		public bool riptideLegs = false;
		public int riptideDashTime = 0;
		public bool necroSet = false;
		public bool novaSet = false;
		public bool tendonSet = false;
		public bool acridSet = false;
		public float necroSetAmount = 0f;
		public int mimicSetChoices = 0;
		public int setActiveAbility = 0;
		public int setAbilityCooldown = 0;
		#endregion armor/set bonuses

		#region accessories
		public bool bombHandlingDevice = false;
		public bool destructiveClaws = false;
		public bool dimStarlight = false;
		public int dimStarlightCooldown = 0;
		public bool madHand = false;
		public bool fiberglassDagger = false;
		public bool advancedImaging = false;
		public bool decayingScale = false;
		public bool lazyCloakVisible = false;
		public bool amebicVialVisible = false;
		public byte amebicVialCooldown = 0;
		public bool entangledEnergy = false;
		public bool asylumWhistle = false;
		public int asylumWhistleTarget = -1;
		public bool mitosis = false;
		public Item mitosisItem = null;
		public int mitosisCooldown = 0;
		public bool reshapingChunk = false;
		public float mysteriousSprayMult = 1;
		public bool protozoaFood = false;
		public int protozoaFoodCooldown = 0;
		public Item protozoaFoodItem = null;
		public bool symbioteSkull = false;
		public byte parasiticInfluenceCooldown = 0;
		public bool gunGlove = false;
		public Item gunGloveItem = null;
		public int gunGloveCooldown = 0;
		public bool guardedHeart = false;
		public bool razorwire = false;
		public Item razorwireItem = null;
		public bool unsoughtOrgan = false;
		public Item unsoughtOrganItem = null;
		public bool spiritShard = false;
		public bool ravel = false;
		public bool ravelEquipped = false;
		public bool spiderRavel = false;
		public bool ceilingRavel = false;
		public int spiderRavelTime;
		public int vanityRavel;
		public bool heliumTank = false;
		public bool heliumTankHit = false;
		public bool messyLeech = false;
		public bool noU = false;
		public bool donorWristband = false;
		public HashSet<Point> preHitBuffs;
		public bool plasmaPhial = false;
		public bool turboReel = false;
		public bool trapCharm = false;
		public bool rebreather = false;
		public float rebreatherCount = 0;
		public bool rebreatherCounting = false;
		public bool explosiveArtery = false;
		public int explosiveArteryCount = 0;
		public Item explosiveArteryItem = null;
		public bool graveDanger = false;
		public bool loversLeap = false;
		public Item loversLeapItem = null;
		public int loversLeapDashTime = 0;
		public int loversLeapDashDirection = 0;
		public float loversLeapDashSpeed = 0;
		public bool magicTripwire = false;
		public int lousyLiverCount = 0;
		public List<(int id, int duration)> lousyLiverDebuffs = new();
		public bool summonTagForceCrit = false;
		public bool rubyReticle = false;
		public bool taintedFlesh = false;
		public bool taintedFlesh2 = false;
		public bool focusCrystal = false;
		public int focusCrystalTime = 0;
		public int brineClover = 0;
		public Item brineCloverItem = null;
		public bool potatoBattery = false;
		public bool hasPotatOS = false;
		public int[] potatOSQuoteCooldown;
		public bool resinShield = false;
		public int resinShieldCooldown = 0;
		public bool ResinShield {
			get => resinShield;
			set => resinShield = value && resinShieldCooldown <= 0;
		}
		public bool thirdEyeActive = false;
		public int thirdEyeTime = 0;
		public int thirdEyeUseTime = 0;
		public int thirdEyeResetTime = 0;
		public bool sonarVisor = false;
		public bool solarPanel = false;
		#endregion

		#region explosive stats
		public StatModifier explosiveProjectileSpeed = StatModifier.Default;
		public StatModifier explosiveThrowSpeed = StatModifier.Default;
		public StatModifier explosiveSelfDamage = StatModifier.Default;
		public StatModifier explosiveBlastRadius = StatModifier.Default;
		public StatModifier explosiveFuseTime = StatModifier.Default;
		#endregion

		#region summon stats
		public StatModifier artifactDamage = StatModifier.Default;
		public float artifactManaCost = 1f;
		#endregion

		#region biomes
		public bool ZoneVoid { get; internal set; } = false;
		public float ZoneVoidProgress = 0;
		public float ZoneVoidProgressSmoothed = 0;

		public float ZoneDefiledProgress = 0;
		public float ZoneDefiledProgressSmoothed = 0;

		public float ZoneRivenProgress = 0;
		public float ZoneRivenProgressSmoothed = 0;

		public float ZoneBrineProgress = 0;
		public float ZoneBrineProgressSmoothed = 0;

		public bool ZoneFiberglass { get; internal set; } = false;
		public float ZoneFiberglassProgress = 0;
		public float ZoneFiberglassProgressSmoothed = 0;
		#endregion

		#region buffs
		public int rapidSpawnFrames = 0;
		public int rasterizedTime = 0;
		public bool toxicShock = false;
		public bool tornDebuff = false;
		public bool flaskBile = false;
		public bool flaskSalt = false;
		public int tornTime = 0;
		public int tornTargetTime = 180;
		public float tornTarget = 0.7f;
		#endregion

		#region keybinds
		public bool controlTriggerSetBonus = false;
		public bool releaseTriggerSetBonus = false;
		#endregion

		#region other items
		public int laserBladeCharge = 0;
		public bool boatRockerAltUse = false;
		#endregion

		public int quantumInjectors = 0;
		public int defiledWill = 0;

		public float statSharePercent = 0f;

		public bool journalUnlocked = false;
		public Item journalDye = null;

		public bool itemLayerWrench = false;
		public bool plagueSight = false;
		public bool plagueSightLight = false;

		public Ref<Item> eyndumCore = null;

		internal static bool ItemChecking = false;
		public int cryostenLifeRegenCount = 0;
		internal byte oldBonuses = 0;
		public const int minionSubSlotValues = 3;
		public float[] minionSubSlots = new float[minionSubSlotValues];
		public int wormHeadIndex = -1;
		public int heldProjectile = -1;
		public int lastMinionAttackTarget = -1;
		public int hookTarget = -1;
		bool rivenWet = false;
		public bool mountOnly = false;
		public bool changeSize = false;
		public int targetWidth;
		public int targetHeight;
		public int oldXSign = 0;
		public int oldYSign = 0;
		public bool collidingX = false;
		public bool collidingY = false;
		public HashSet<string> unlockedJournalEntries = new();
		public int dashDirection = 0;
		public int dashDirectionY = 0;
		public int dashDelay = 0;
		public int thornsVisualProjType = -1;
		public int timeSinceLastDeath = -1;
		public int oldBreath = 200;
		public override void ResetEffects() {
			oldBonuses = 0;
			if (fiberglassSet || fiberglassDagger) oldBonuses |= 1;
			if (felnumSet) oldBonuses |= 2;
			fiberglassSet = false;
			cryostenSet = false;
			cryostenHelmet = false;
			oldFelnumShock = felnumShock;
			if (!felnumSet) {
				felnumShock = 0;
			} else {
				if (felnumShock > Player.statLifeMax2) {
					if (Main.rand.NextBool(20)) {
						Vector2 pos = new Vector2(Main.rand.Next(4, Player.width - 4), Main.rand.Next(4, Player.height - 4));
						Projectile proj = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.position + pos, Main.rand.NextVector2CircularEdge(3, 3), Felnum_Shock_Leader.ID, (int)(felnumShock * 0.1f), 0, Player.whoAmI, pos.X, pos.Y);
						if (proj.ModProjectile is Felnum_Shock_Leader shock) {
							shock.Parent = Player;
							shock.OnStrike += () => felnumShock *= 0.9f;
						}
					}
					felnumShock -= (felnumShock - Player.statLifeMax2) / Player.statLifeMax2 * 5 + 1;
				}
			}
			if (donorWristband) {
				float healLogic = (1 - 0.375f) / (Player.pStone ? 0.75f : 1);

				Player.potionDelayTime = (int)(Player.potionDelayTime * healLogic);
				Player.restorationDelayTime = (int)(Player.restorationDelayTime * healLogic);
				Player.mushroomDelayTime = (int)(Player.mushroomDelayTime * healLogic);
			}

			felnumSet = false;
			minerSet = false;
			defiledSet = false;
			reshapingChunk = false;
			rivenSet = false;
			rivenSetBoost = false;
			riftSet = false;
			eyndumSet = false;
			mimicSet = false;
			riptideSet = false;
			riptideLegs = false;
			necroSet = false;
			novaSet = false;
			tendonSet = false;
			acridSet = false;
			if (necroSetAmount > 0) {
				necroSetAmount -= 1 + necroSetAmount * 0.01f;
			}
			setActiveAbility = 0;

			if (setAbilityCooldown > 0) {
				setAbilityCooldown--;
				if (setAbilityCooldown == 0) {
					SoundEngine.PlaySound(SoundID.MaxMana.WithPitch(-1).WithVolume(0.5f));
					for (int i = 0; i < 5; i++) {
						int dust = Dust.NewDust(Player.position, Player.width, Player.height, DustID.PortalBoltTrail, 0f, 0f, 255, Color.Black, (float)Main.rand.Next(20, 26) * 0.1f);
						Main.dust[dust].noLight = true;
						Main.dust[dust].noGravity = true;
						Main.dust[dust].velocity *= 0.5f;
					}
				}
			}

			bombHandlingDevice = false;
			destructiveClaws = false;
			dimStarlight = false;
			madHand = false;
			fiberglassDagger = false;
			advancedImaging = false;
			decayingScale = false;
			lazyCloakVisible = false;
			amebicVialVisible = false;
			mitosis = false;
			mitosisItem = null;
			entangledEnergy = false;
			mysteriousSprayMult = 1;
			protozoaFood = false;
			protozoaFoodItem = null;
			symbioteSkull = false;
			taintedFlesh = false;
			taintedFlesh2 = false;
			if (toxicShock) {
				if (Player.breath > oldBreath) Player.breath = oldBreath;
				toxicShock = false;
			}
			gunGlove = false;
			gunGloveItem = null;
			razorwire = false;
			razorwireItem = null;
			unsoughtOrgan = false;
			unsoughtOrganItem = null;
			spiritShard = false;
			ravel = false;
			heliumTank = false;
			messyLeech = false;
			noU = false;
			plasmaPhial = false;
			turboReel = false;
			donorWristband = false;
			trapCharm = false;

			if (!ravelEquipped && Player.mount.Active && Ravel_Mount.RavelMounts.Contains(Player.mount.Type)) {
				Player.mount.Dismount(Player);
			}
			ravelEquipped = false;
			spiderRavel = false;
			if (spiderRavelTime > 0) spiderRavelTime--;
			vanityRavel = -1;

			if (explosiveArtery) {
				explosiveArtery = false;
			} else {
				explosiveArteryCount = -1;
			}
			explosiveArteryItem = null;
			graveDanger = false;
			loversLeap = false;
			magicTripwire = false;
			lousyLiverCount = 0;
			if (lousyLiverDebuffs.Count > 0) lousyLiverDebuffs.Clear();
			summonTagForceCrit = false;
			rubyReticle = false;
			if (focusCrystal) {
				focusCrystal = false;
				if (Math.Abs(Player.velocity.X) < 0.05f && Math.Abs(Player.velocity.Y) < 0.05f) {
					focusCrystalTime = Math.Min(focusCrystalTime + 1, 180);
				} else {
					focusCrystalTime = Math.Max(focusCrystalTime - 2, 0);
				}
			} else {
				focusCrystalTime = 0;
			}
			brineClover = 0;
			brineCloverItem = null;
			potatoBattery = false;
			hasPotatOS = false;
			potatOSQuoteCooldown ??= new int[(int)Potato_Battery.QuoteType.Count];
			for (int i = 0; i < (int)Potato_Battery.QuoteType.Count; i++) {
				if (potatOSQuoteCooldown[i] > 0) potatOSQuoteCooldown[i]--;
			}
			if (resinShieldCooldown > 0) resinShieldCooldown--;
			resinShield = false;
			if (thirdEyeTime < thirdEyeUseTime) {
				if (thirdEyeTime > 0 && !thirdEyeActive) thirdEyeTime--;
			} else if (++thirdEyeTime >= thirdEyeResetTime) {
				thirdEyeTime = 0;
			}
			sonarVisor = false;
			solarPanel = false;

			flaskBile = false;
			flaskSalt = false;

			boatRockerAltUse = false;

			explosiveProjectileSpeed = StatModifier.Default;
			explosiveThrowSpeed = StatModifier.Default;
			explosiveSelfDamage = StatModifier.Default;
			explosiveBlastRadius = StatModifier.Default;
			explosiveFuseTime = StatModifier.Default;

			artifactDamage = StatModifier.Default;
			artifactManaCost = 1f;

			statSharePercent = 0f;
			if (cryostenLifeRegenCount > 0)
				cryostenLifeRegenCount--;

			if (dimStarlightCooldown > 0)
				dimStarlightCooldown--;
			if (amebicVialCooldown > 0)
				amebicVialCooldown--;
			if (protozoaFoodCooldown > 0)
				protozoaFoodCooldown--;
			if (parasiticInfluenceCooldown > 0)
				parasiticInfluenceCooldown--;
			if (gunGloveCooldown > 0)
				gunGloveCooldown--;
			if (mitosisCooldown > 0)
				mitosisCooldown--;

			if (laserBladeCharge > 0 && !Player.ItemAnimationActive) laserBladeCharge--;

			if (rapidSpawnFrames > 0)
				rapidSpawnFrames--;
			if (!tornDebuff && tornTime > 0 && --tornTime <= 0) {
				tornTargetTime = 180;
				tornTarget = 0.7f;
			}
			tornDebuff = false;
			int rasterized = Player.FindBuffIndex(Rasterized_Debuff.ID);
			if (rasterized >= 0) {
				rasterizedTime = Math.Min(Math.Min(rasterizedTime + 1, 8), Player.buffTime[rasterized] - 1);
			}
			if (Player.breath > Player.breathMax) {
				Player.breath = Player.breathMax;
			}
			Player.breathMax = 200;
			plagueSight = false;
			plagueSightLight = false;
			mountOnly = false;
			thornsVisualProjType = -1;
			changeSize = false;
			minionSubSlots = new float[minionSubSlotValues];
			if (timeSinceLastDeath < int.MaxValue) timeSinceLastDeath++;
			#region asylum whistle
			if (lastMinionAttackTarget != Player.MinionAttackTargetNPC) {
				if (asylumWhistle) {
					if (Player.MinionAttackTargetNPC == -1) {
						Player.MinionAttackTargetNPC = asylumWhistleTarget;
					} else {
						asylumWhistleTarget = lastMinionAttackTarget;
					}
				}
				lastMinionAttackTarget = Player.MinionAttackTargetNPC;
			}
			if (!asylumWhistle) {
				asylumWhistleTarget = -1;
			} else if (asylumWhistleTarget > -1) {
				NPC possibleTarget = Main.npc[asylumWhistleTarget];
				if (!possibleTarget.CanBeChasedBy() || possibleTarget.Hitbox.Distance(Player.Center) > 3000f) {
					asylumWhistleTarget = -1;
				} else if (Player.HeldItem.CountsAsClass(DamageClass.Summon)) {
					Vector2 center = possibleTarget.Center;
					float count = Player.miscCounter / 60f;
					float offset = MathHelper.TwoPi / 3f;
					for (int i = 0; i < 3; i++) {
						int dust = Dust.NewDust(center, 0, 0, DustID.WitherLightning, 0f, 0f, 100, default(Color), 0.35f);
						Main.dust[dust].noGravity = true;
						Main.dust[dust].velocity = Vector2.Zero;
						Main.dust[dust].noLight = true;
						Main.dust[dust].position = center + (count * MathHelper.TwoPi + offset * i).ToRotationVector2() * 6f;
					}
				}
			}
			asylumWhistle = false;
			#endregion
			oldBreath = Player.breath;

			Player.statManaMax2 += quantumInjectors * Quantum_Injector.mana_per_use;
			#region check if a dash should start
			dashDirection = 0;
			dashDirectionY = 0;
			if (dashDelay <= 0) {
				const int DashDown = 0;
				const int DashUp = 1;
				const int DashRight = 2;
				const int DashLeft = 3;
				if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15) {
					dashDirection = 1;
				} else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15) {
					dashDirection = -1;
				} else if (Player.controlUp && Player.releaseUp && Player.doubleTapCardinalTimer[DashUp] < 15) {
					dashDirectionY = -1;
				} else if (Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[DashDown] < 15) {
					dashDirectionY = 1;
				}
			} else {
				dashDelay--;
			}
			#endregion
		}
		#endregion
		public const float explosive_defense_factor = 1f;
		public override void PreUpdateMovement() {
			if (riptideLegs && Player.wet) {
				Player.velocity *= 1.0048f;
				Player.ignoreWater = true;
			}
			if (riptideSet && !Player.mount.Active) {
				Player.dashType = 0;
				Player.dashTime = 0;
				const int riptideDashDuration = 12;
				const float riptideDashSpeed = 9f;
				if (dashDirection != 0 && (Player.velocity.X * dashDirection < riptideDashSpeed)) {
					Player.dashDelay = -1;
					Player.dash = 2;
					riptideDashTime = riptideDashDuration * dashDirection;
					Player.timeSinceLastDashStarted = 0;
					int gravDir = Math.Sign(Player.gravity);
					if (Player.velocity.Y * gravDir > Player.gravity * gravDir) {
						Player.velocity.Y = Player.gravity;
					}
					Projectile.NewProjectile(
						Player.GetSource_Misc("riptide_dash"),
						Player.Center + new Vector2(Player.width * dashDirection, 0),
						new Vector2(dashDirection * riptideDashSpeed, 0),
						Riptide_Dash_P.ID,
						25,
						riptideDashSpeed + 3,
						Player.whoAmI
					);
				}
				if (riptideDashTime != 0) {
					Player.velocity.X = riptideDashSpeed * Math.Sign(riptideDashTime);
					riptideDashTime -= Math.Sign(riptideDashTime);
					dashDelay = 25;
				}
			}
			if (loversLeap) {
				Player.dashType = 0;
				Player.dashTime = 0;
				const int loversLeapDuration = 6;
				const float loversLeapSpeed = 12f;
				if (collidingX || collidingY) {
					if ((dashDirection != 0 && (Player.velocity.X * dashDirection < loversLeapSpeed)) || (dashDirectionY != 0 && (Player.velocity.Y * dashDirectionY < loversLeapSpeed))) {
						//Player.dashDelay = -1;
						//Player.dash = 2;
						loversLeapDashTime = loversLeapDuration;
						loversLeapDashSpeed = loversLeapSpeed;
						Player.timeSinceLastDashStarted = 0;
						if (dashDirectionY == -1) {
							loversLeapDashDirection = 0;
						} else if (dashDirectionY == 1) {
							loversLeapDashDirection = 1;
						} else if (dashDirection == 1) {
							loversLeapDashDirection = 2;
						} else if (dashDirection == -1) {
							loversLeapDashDirection = 3;
						}
					}
				}
				if (loversLeapDashTime > 0) {
					if (loversLeapDashTime > 1) {
						switch (loversLeapDashDirection) {
							case 0:
							Player.velocity.Y = -loversLeapDashSpeed;
							break;
							case 1:
							Player.velocity.Y = loversLeapDashSpeed;
							break;
							case 2:
							Player.velocity.X = loversLeapDashSpeed;
							break;
							case 3:
							Player.velocity.X = -loversLeapDashSpeed;
							break;
						}
						loversLeapDashTime--;
						dashDelay = 30;
					}
					if ((loversLeapDashTime == 1 || loversLeapDashDirection == 2 || loversLeapDashDirection == 3) && loversLeapDashSpeed > 0) {
						bool bounce = false;
						bool canBounce = true;
						if (Math.Abs(Player.velocity.X) > Math.Abs(Player.velocity.Y)) {
							loversLeapDashDirection = Math.Sign(Player.velocity.X) == 1 ? 2 : 3;
						} else {
							loversLeapDashDirection = Math.Sign(Player.velocity.Y) == 1 ? 1 : 0;
						}
						Rectangle loversLeapHitbox = default;
						int hitDirection = 0;
						switch (loversLeapDashDirection) {
							case 0:
							canBounce = false;
							break;
							case 1:
							loversLeapHitbox = Player.Hitbox;
							loversLeapHitbox.Inflate(4, 4);
							loversLeapHitbox.Offset(0, 8);
							break;
							case 2:
							loversLeapHitbox = new Rectangle((int)(Player.position.X + Player.width), (int)(Player.position.Y - 4), 8, Player.height + 8);
							hitDirection = 1;
							break;
							case 3:
							loversLeapHitbox = new Rectangle((int)(Player.position.X - 8), (int)(Player.position.Y - 4), 8, Player.height + 8);
							hitDirection = -1;
							break;
						}
						if (canBounce) {
							int baseDamage = Player.GetWeaponDamage(loversLeapItem);
							float baseKnockback = Player.GetWeaponKnockback(loversLeapItem);
							int crit = Player.GetWeaponCrit(loversLeapItem);
							for (int i = 0; i < Main.maxNPCs; i++) {
								NPC npc = Main.npc[i];
								if (npc.active && !npc.dontTakeDamage) {
									if (!npc.friendly || (npc.type == NPCID.Guide && Player.killGuide) || (npc.type == NPCID.Clothier && Player.killClothier)) {
										if (loversLeapHitbox.Intersects(npc.Hitbox)) {
											bounce = true;
											int damage = baseDamage;
											int bannerBuff = Item.NPCtoBanner(Main.npc[i].BannerID());
											if (bannerBuff > 0 && Player.HasNPCBannerBuff(bannerBuff)) {
												ItemID.BannerEffect bannerEffect = ItemID.Sets.BannerStrength[Item.BannerToItem(bannerBuff)];
												damage = (int)(Main.expertMode ? damage * bannerEffect.ExpertDamageDealt : damage * bannerEffect.NormalDamageDealt);
											}
											damage = Main.DamageVar(damage, Player.luck) + npc.checkArmorPenetration(Player.GetWeaponArmorPenetration(loversLeapItem));
											npc.StrikeNPC(damage, baseKnockback, hitDirection, Main.rand.Next(1, 101) <= crit);
										}
									}
								}
							}
							if (bounce) {
								loversLeapDashDirection ^= 1;
								loversLeapDashTime = 2;
								loversLeapDashSpeed = Math.Min(loversLeapDashSpeed - 0.5f, 9f);
							}
							switch (loversLeapDashDirection) {
								case 2:
								case 3:
								if (bounce) {
									Player.velocity.Y -= 4;
									loversLeapDashSpeed -= 2f;
								}
								break;
								case 0:
								case 1:
								if (collidingX || collidingY) {
									loversLeapDashTime = 0;
								}
								break;
							}
						}
					} else if(loversLeapDashTime == 1) {
						loversLeapDashTime = 0;
					}
				}
			}
			if (rebreather && Player.breath < Player.breathMax) {
				if (Player.breathCD == 0 || rebreatherCounting) {
					rebreatherCounting = true;
					const float maxCount = 8f;
					const float maxSpeed = 16f;
					float speed = Math.Min(Player.velocity.Length() / maxSpeed, 1);
					if ((rebreatherCount += speed) >= maxCount) {
						rebreatherCounting = false;
						rebreatherCount -= maxCount;
						Player.breath++;
					}
				}
			}
			if (hookTarget >= 0) {//ropeVel.HasValue&&
				Player.fallStart = (int)(Player.position.Y / 16f);
				Projectile projectile = Main.projectile[hookTarget];
				if (projectile.type == Amoeba_Hook_Projectile.ID) {
					Vector2 diff = Player.Center - projectile.Center;
					Vector2 normDiff = diff.SafeNormalize(default);
					float dot = Vector2.Dot(normDiff, Player.velocity.SafeNormalize(default));
					Player.velocity = Vector2.Lerp(normDiff * -16, Player.velocity, 0.85f + dot * 0.1f);
					if (diff.LengthSquared() > 64) {
						Player.GoingDownWithGrapple = true;
					}
					Player.RefreshMovementAbilities();
				}
			}
			if (changeSize) {
				Player.position.X -= (targetWidth - Player.width) / 2;
				Player.position.Y -= targetHeight - Player.height;
				Player.width = targetWidth;
				Player.height = targetHeight;
			}
			oldXSign = Math.Sign(Player.velocity.X);
			oldYSign = Math.Sign(Player.velocity.Y);
			//endCustomMovement:
			hookTarget = -1;
		}
		public override void PreUpdate() {
			if (rivenWet) {
				Player.gravity = 0.25f;
			}
			if (ravel && spiderRavel) {
				ceilingRavel = false;
				if (collidingX) {
					Player.gravity = 0;
					Player.velocity.Y *= 0.9f;
					if (Player.controlUp) {
						Player.velocity.Y -= 0.35f;
					}
					if (Player.controlDown) {
						Player.velocity.Y += 0.35f;
					}
				} else {
					bool colliding = false;
					float halfSpiderWidth = Player.width / 2 - 1;
					float halfSpiderHeight = Player.height / 2 + 4;
					for (int i = -1; i < 2; i++) {
						Tile currentTile = Main.tile[(Player.Center - Player.velocity + new Vector2(halfSpiderWidth * i, -halfSpiderHeight)).ToTileCoordinates()];
						if (currentTile.HasTile && Main.tileSolid[currentTile.TileType] && !Main.tileSolidTop[currentTile.TileType]) {
							colliding = true;
							break;
						}
					}
					if (colliding) {
						ceilingRavel = true;
						spiderRavelTime = 10;
					}
					if (Player.controlDown) {
						spiderRavelTime = 0;
					}
					if (spiderRavelTime > 0 && Player.controlUp) {
						Player.gravity = 0f;
						Player.velocity.Y -= 0.35f;
						for (int i = -1; i < 2; i++) {
							Tile currentTile = Main.tile[(Player.Center - Player.velocity + new Vector2(9 * i, -30)).ToTileCoordinates()];
							if (currentTile.HasTile && Main.tileSolid[currentTile.TileType] && !Main.tileSolidTop[currentTile.TileType]) {
								Player.velocity.Y -= 1;
								break;
							}
						}
						Collision.StepUp(ref Player.position, ref Player.velocity, Player.width, Player.height, ref Player.stepSpeed, ref Player.gfxOffY, -1, true);
					}
				}
			}
		}
		public override void PostUpdate() {
			heldProjectile = -1;
			if (rasterizedTime > 0) {
				Player.velocity = Vector2.Lerp(Player.velocity, Player.oldVelocity, rasterizedTime * 0.06f);
				Player.position = Vector2.Lerp(Player.position, Player.oldPosition, rasterizedTime * 0.06f);
			}
			Player.oldVelocity = Player.velocity;
			rivenWet = false;
			if ((Player.wet || WaterCollision(Player.position, Player.width, Player.height)) && !(Player.lavaWet || Player.honeyWet)) {
				ModWaterStyle waterStyle = LoaderManager.Get<WaterStylesLoader>().Get(Main.waterStyle);
				if (waterStyle is Riven_Water_Style) {
					rivenWet = true;
					int duration = 30;
					int targetTime = 1440;
					float targetSeverity = 0f;
					bool hadTorn = Player.HasBuff(Torn_Buff.ID);
					Player.AddBuff(Torn_Buff.ID, duration);
					if (hadTorn || targetSeverity < tornTarget) {
						tornTargetTime = targetTime;
						tornTarget = targetSeverity;
					}
					Player.velocity.X *= 0.975f;
				} else if (waterStyle is Brine_Water_Style) {
					Player.AddBuff(Toxic_Shock_Debuff.ID, 30);
				}
			}
		}
		public override void PostUpdateEquips() {
			if (eyndumSet) {
				ApplyEyndumSetBuffs();
				if (eyndumCore?.Value?.ModItem is ModItem equippedCore) {
					equippedCore.UpdateEquip(Player);
				}
			}
			Player.buffImmune[Rasterized_Debuff.ID] = Player.buffImmune[BuffID.Cursed];
			if (tornDebuff) {
				if (tornTime < tornTargetTime) {
					tornTime++;
				}
			}
			if (tornTime > 0) {
				Player.statLifeMax2 = (int)(Player.statLifeMax2 * (1 - ((1 - tornTarget) * (tornTime / (float)tornTargetTime))));
				if (Player.statLifeMax2 <= 0) {
					Player.KillMe(PlayerDeathReason.ByOther(0), 1, 0);
				}
			}
			if (explosiveArtery) {
				if (explosiveArteryCount == -1) {
					explosiveArteryCount = CombinedHooks.TotalUseTime(explosiveArteryItem.useTime, Player, explosiveArteryItem);
				}
				if (explosiveArteryCount > 0) {
					explosiveArteryCount--;
				} else {
					const float maxDist = 512 * 512;
					for (int i = 0; i < Main.maxNPCs; i++) {
						NPC currentTarget = Main.npc[i];
						if (Main.rand.NextBool(explosiveArteryItem.useAnimation, explosiveArteryItem.reuseDelay)) {
							if (currentTarget.CanBeChasedBy() && currentTarget.HasBuff(BuffID.Bleeding)) {
								Vector2 diff = currentTarget.Center - Player.MountedCenter;
								if (diff.LengthSquared() < maxDist) {
									Projectile.NewProjectileDirect(
										Player.GetSource_Accessory(explosiveArteryItem),
										currentTarget.Center,
										new Vector2(Math.Sign(diff.X), 0),
										explosiveArteryItem.shoot,
										Player.GetWeaponDamage(explosiveArteryItem),
										Player.GetWeaponKnockback(explosiveArteryItem),
										Player.whoAmI
									);
								}
							}
						}
					}
					explosiveArteryCount = -1;
				}
			}
			if (lousyLiverCount > 0) {
				const float maxDist = 256 * 256;
				List<(NPC target, float dist)> targets = new(lousyLiverCount);
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC currentTarget = Main.npc[i];
					if (currentTarget.CanBeChasedBy()) {
						float dist = (currentTarget.Center - Player.MountedCenter).LengthSquared();
						if (dist < maxDist) {
							int index = Math.Min(targets.Count, lousyLiverCount) + 1;
							for (;index-->0;) {
								if (index == 0 || targets[index - 1].dist < dist) {
									targets.Insert(index, (currentTarget, dist));
									break;
								}
							}
						}
					}
				}
				for (int i = 0; i < Math.Min(targets.Count, lousyLiverCount); i++) {
					for (int j = 0; j < lousyLiverDebuffs.Count; j++) {
						targets[i].target.AddBuff(lousyLiverDebuffs[j].id, lousyLiverDebuffs[j].duration);
					}
				}
				explosiveArteryCount = -1;
			}
			if (solarPanel) {
				static Vector3 DoTileCalcs(int x, int top, Vector3 sunLight, Vector3 waterFactor) {
					for (int y = top; y > 0; y--) {
						Tile tile = Framing.GetTileSafely(x, y);
						if (tile.HasUnactuatedTile && Main.tileBlockLight[tile.TileType]) {
							sunLight = Vector3.Zero;
						} else if (tile.LiquidAmount > 0) {
							switch (tile.LiquidType) {
								case 0:
								sunLight *= waterFactor;
								sunLight -= waterFactor * 0.01f;
								break;
								case 1:
								sunLight = Vector3.Zero;
								break;
								case 2:
								sunLight *= new Vector3(0.25f, 0.25f, 0f);
								sunLight -= new Vector3(0.0025f, 0.0025f, 0f);
								break;
							}
						}
						if (sunLight.X < 0) {
							sunLight.X = 0;
						}
						if (sunLight.Y < 0) {
							sunLight.Y = 0;
						}
						if (sunLight.Z < 0) {
							sunLight.Z = 0;
						}
						if (sunLight == Vector3.Zero) {
							break;
						}
					}
					return sunLight;
				}
				Vector3 sunLight = new Vector3(1f, 1f, 0.67f);
				float sunFactor = 0;
				int top = (int)Player.TopLeft.Y / 16;
				Vector3 waterFactor = Vector3.One;
				switch (Main.waterStyle) {
					case WaterStyleID.Purity:
					case WaterStyleID.Lava:
					case WaterStyleID.Underground:
					case WaterStyleID.Cavern:
					waterFactor = new Vector3(0.88f, 0.96f, 1.015f);
					break;

					case WaterStyleID.Corrupt:
					waterFactor = new Vector3(0.94f, 0.85f, 1.01f);
					break;
					case WaterStyleID.Jungle:
					waterFactor = new Vector3(0.84f, 0.95f, 1.015f);
					break;
					case WaterStyleID.Hallow:
					waterFactor = new Vector3(0.90f, 0.86f, 1.01f);
					break;
					case WaterStyleID.Snow:
					waterFactor = new Vector3(0.64f, 0.99f, 1.01f);
					break;
					case WaterStyleID.Desert:
					waterFactor = new Vector3(0.93f, 0.83f, 0.98f);
					break;
					case WaterStyleID.Bloodmoon:
					waterFactor = new Vector3(1f, 0.88f, 0.84f);
					break;
					case WaterStyleID.Crimson:
					waterFactor = new Vector3(0.83f, 1f, 1f);
					break;
					case WaterStyleID.UndergroundDesert:
					waterFactor = new Vector3(0.95f, 0.98f, 0.85f);
					break;

					case WaterStyleID.Honey:
					waterFactor = new Vector3(0f);
					break;

					default:
					LoaderManager.Get<WaterStylesLoader>().Get(Main.waterStyle).LightColorMultiplier(ref waterFactor.X, ref waterFactor.Y, ref waterFactor.Z);
					break;
				}
				waterFactor /= 1.015f;

				sunFactor += DoTileCalcs((int)(Player.TopLeft.X + 1) / 16, top, sunLight, waterFactor).Length() / 1.56f;
				sunFactor += DoTileCalcs((int)Player.Top.X / 16, top, sunLight, waterFactor).Length() / 1.56f;
				sunFactor += DoTileCalcs((int)(Player.TopRight.X - 1) / 16, top, sunLight, waterFactor).Length() / 1.56f;

				Player.chatOverhead.NewMessage(sunFactor + "", 5);
				Player.manaRegenCount += (int)(sunFactor * 12);
			}
		}
		public override void PostUpdateMiscEffects() {
			if (cryostenHelmet) {
				if (Player.statLife != Player.statLifeMax2 && (int)Main.time % (cryostenLifeRegenCount > 0 ? 5 : 15) == 0) {
					for (int i = 0; i < 10; i++) {
						int num6 = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Frost);
						Main.dust[num6].noGravity = true;
						Main.dust[num6].velocity *= 0.75f;
						int num7 = Main.rand.Next(-40, 41);
						int num8 = Main.rand.Next(-40, 41);
						Main.dust[num6].position.X += num7;
						Main.dust[num6].position.Y += num8;
						Main.dust[num6].velocity.X = -num7 * 0.075f;
						Main.dust[num6].velocity.Y = -num8 * 0.075f;
					}
				}
			}
			if (tendonSet) {
				Player.moveSpeed *= 5f;
				Player.jumpSpeedBoost *= 5f;
				Player.runAcceleration *= 5f;
			}
			if (protozoaFood && protozoaFoodCooldown <= 0 && Player.ownedProjectileCounts[Mini_Protozoa_P.ID] < Player.maxMinions) {
				Item item = protozoaFoodItem;
				int damage = Player.GetWeaponDamage(item);
				Projectile.NewProjectileDirect(
					Player.GetSource_Accessory(item),
					Player.Center,
					OriginExtensions.Vec2FromPolar(Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi), Main.rand.NextFloat(1, 8)),
					Mini_Protozoa_P.ID,
					damage,
					Player.GetWeaponKnockback(item),
					Player.whoAmI
				).originalDamage = damage;
				protozoaFoodCooldown = item.useTime;
			}
			if (statSharePercent != 0f) {
				foreach (DamageClass damageClass in DamageClasses.All) {
					if (damageClass == DamageClass.Generic) {
						continue;
					}
					Player.GetArmorPenetration(DamageClass.Generic) += Player.GetArmorPenetration(damageClass) * statSharePercent;
					Player.GetArmorPenetration(damageClass) -= Player.GetArmorPenetration(damageClass) * statSharePercent;

					Player.GetDamage(DamageClass.Generic) = Player.GetDamage(DamageClass.Generic).CombineWith(Player.GetDamage(damageClass).Scale(statSharePercent));
					Player.GetDamage(damageClass) = Player.GetDamage(damageClass).Scale(1f - statSharePercent);

					Player.GetAttackSpeed(DamageClass.Generic) += (Player.GetAttackSpeed(damageClass) - 1) * statSharePercent;
					Player.GetAttackSpeed(damageClass) -= (Player.GetAttackSpeed(damageClass) - 1) * statSharePercent;

					float crit = Player.GetCritChance(damageClass) * statSharePercent;
					Player.GetCritChance(DamageClass.Generic) += crit;
					Player.GetCritChance(damageClass) -= crit;
				}
			}
			if (hookTarget >= 0) {
				Projectile projectile = Main.projectile[hookTarget];
				if (projectile.type == Amoeba_Hook_Projectile.ID) {
					if (projectile.ai[1] < 5 || Player.controlJump || (Player.Center - projectile.Center).LengthSquared() > 2304) {
						Player.GoingDownWithGrapple = true;
					}
				}
			}
			if (graveDanger) {
				if (Player.difficulty != 2) {
					//1 minute (3600 ticks), decays over the latter 45 seconds (2700 ticks)
					float factor = Math.Min((3600 - timeSinceLastDeath) / 2700f, 1);
					if (factor > 0) {
						Player.statDefense += (int)Math.Ceiling(Player.statDefense * factor * 0.25f);
						if (Main.rand.NextFloat(1.25f) < factor + 0.1f) {
							Dust dust = Dust.NewDustDirect(Player.position - new Vector2(8, 0), Player.width + 16, Player.height, DustID.Smoke, newColor: new Color(0.1f, 0.1f, 0.2f));
							dust.velocity *= 0.4f;
							dust.alpha = 150;
						}
					}
				} else {
					Player.endurance += 0.15f;
				}
			}
		}
		public override void UpdateDead() {
			timeSinceLastDeath = -1;
		}
		public override void UpdateDyes() {
			if (Ravel_Mount.RavelMounts.Contains(Player.mount.Type)) {
				for (int i = Player.SupportedSlotsArmor; i < Player.SupportedSlotsArmor + Player.SupportedSlotsAccs; i++) {
					if (Player.armor[i].ModItem is Ravel) {
						Player.cMount = Player.dye[i].dye;
					}
					if (Player.armor[i + 10].ModItem is Ravel) {
						Player.cMount = Player.dye[i].dye;
						break;
					}
				}
			}
		}
		public override void PostUpdateBuffs() {
			if (Player.whoAmI == Main.myPlayer) {
				foreach (var quest in Quest_Registry.Quests) {
					if (!quest.SaveToWorld && quest.PreUpdateInventoryEvent is not null) {
						quest.PreUpdateInventoryEvent();
					}
				}
			}
		}
		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			tornTime = 0;
			tornTargetTime = 180;
			tornTarget = 0.7f;
			if (hasPotatOS) {
				Potato_Battery.PlayRandomMessage(Potato_Battery.QuoteType.Death, potatOSQuoteCooldown, Player.Top);
			}
		}
		public override void ProcessTriggers(TriggersSet triggersSet) {
			releaseTriggerSetBonus = !controlTriggerSetBonus;
			controlTriggerSetBonus = Origins.SetBonusTriggerKey.Current;
			if (controlTriggerSetBonus && releaseTriggerSetBonus) {
				TriggerSetBonus();
			}
		}
		public void ApplyEyndumSetBuffs() {
			#region movement
			float speedMult = (Player.moveSpeed - 1) * 0.5f;
			Player.runAcceleration += (Player.runAcceleration / Player.moveSpeed) * speedMult;
			Player.maxRunSpeed += (Player.maxRunSpeed / Player.moveSpeed) * speedMult;
			Player.extraFall += Player.extraFall / 2;
			Player.wingTimeMax += Player.wingTimeMax / 2;
			Player.jumpSpeedBoost += Player.jumpSpeedBoost * 0.5f;
			if (Player.spikedBoots == 1) Player.spikedBoots = 2;
			#endregion
			#region defense
			Player.statLifeMax2 += (Player.statLifeMax2 - Player.statLifeMax) / 2;
			Player.statDefense += Player.statDefense / 2;
			Player.endurance += Player.endurance * 0.5f;
			Player.lifeRegen += Player.lifeRegen / 2;
			Player.thorns += Player.thorns * 0.5f;
			Player.lavaMax += Player.lavaMax / 2;
			#endregion
			#region damage
			foreach (DamageClass damageClass in DamageClasses.All) {
				Player.GetArmorPenetration(damageClass) += Player.GetArmorPenetration(damageClass) * 0.5f;

				Player.GetDamage(damageClass) = Player.GetDamage(damageClass).Scale(1.5f);

				Player.GetAttackSpeed(damageClass) += (Player.GetAttackSpeed(damageClass) - 1) * 0.5f;
			}

			Player.arrowDamage = Player.arrowDamage.Scale(1.5f);
			Player.bulletDamage = Player.bulletDamage.Scale(1.5f);
			Player.rocketDamage = Player.rocketDamage.Scale(1.5f);

			explosiveBlastRadius = explosiveBlastRadius.Scale(1.5f);

			//explosiveDamage += (explosiveDamage - 1) * 0.5f;
			//explosiveThrowSpeed += (explosiveThrowSpeed - 1) * 0.5f;
			//explosiveSelfDamage += (explosiveSelfDamage - 1) * 0.5f;
			#endregion
			#region resources
			Player.statManaMax2 += (Player.statManaMax2 - Player.statManaMax) / 2;
			Player.manaCost += (Player.manaCost - 1) * 0.5f;
			Player.maxMinions += (Player.maxMinions - 1) / 2;
			Player.maxTurrets += (Player.maxTurrets - 1) / 2;
			Player.manaRegenBonus += Player.manaRegenBonus / 2;
			Player.manaRegenDelayBonus += Player.manaRegenDelayBonus / 2;
			#endregion
			#region utility
			Player.wallSpeed += (Player.wallSpeed - 1) * 0.5f;
			Player.tileSpeed += (Player.tileSpeed - 1) * 0.5f;
			Player.pickSpeed *= (Player.pickSpeed - 1) * 0.5f;
			Player.aggro += Player.aggro / 2;
			Player.blockRange += Player.blockRange / 2;
			#endregion
		}
		public void TriggerSetBonus() {
			if (setAbilityCooldown > 0) return;
			switch (setActiveAbility) {
				case 1: {
					Vector2 speed = Vector2.Normalize(Main.MouseWorld - Player.MountedCenter) * 14;
					int type = ModContent.ProjectileType<Infusion_P>();
					for (int i = -5; i < 6; i++) {
						Projectile.NewProjectile(Player.GetSource_FromThis(), Player.MountedCenter + speed.RotatedBy(MathHelper.PiOver2) * i * 0.25f + speed * (5 - Math.Abs(i)) * 0.75f, speed, type, 40, 7, Player.whoAmI);
					}
					setAbilityCooldown = 30;
					if (Player.manaRegenDelay < 60) Player.manaRegenDelay = 60;
				}
				break;
				case 2: {
					if (Player.CheckMana((int)(40 * Player.manaCost), true)) {
						setAbilityCooldown = 1800;
						Player.AddBuff(Mimic_Buff.ID, 600);
						Player.AddBuff(BuffID.Heartreach, 30);
					}
				}
				break;
				case 3:
				break;
				default:
				break;
			}
		}
		public override void UpdateLifeRegen() {
			if (cryostenHelmet) Player.lifeRegenCount += cryostenLifeRegenCount > 0 ? 180 : 1;
			if (focusCrystal) {
				float factor = Player.dpsDamage / 200f;
				int rounded = (int)factor;
				Player.lifeRegenCount += rounded;
				Player.lifeRegenTime += (int)((factor - rounded) * 50);
			}
		}
		public override void UpdateBadLifeRegen() {
			if (plasmaPhial && Player.bleed) {
				Player.lifeRegen -= 12;
			}
		}
		#region attacks
		public override void ModifyManaCost(Item item, ref float reduce, ref float mult) {
			if (Origins.ArtifactMinion[item.shoot]) {
				mult *= artifactManaCost;
			}
		}
		public override bool? CanAutoReuseItem(Item item) {
			if (destructiveClaws && item.CountsAsClass(DamageClasses.Explosive)) return true;
			return null;
		}
		public override void MeleeEffects(Item item, Rectangle hitbox) {
			if (flaskBile) {
				Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, DustID.BloodWater, newColor: Color.Black);
			} else if (flaskSalt) {
				Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, DustID.GoldFlame, newColor: Color.Lime);
			}
			if (gunGlove && gunGloveCooldown <= 0) {
				if (Player.PickAmmo(gunGloveItem, out int projToShoot, out float speed, out int damage, out float knockback, out int usedAmmoItemId, ItemID.Sets.gunProj[gunGloveItem.type])) {
					int manaCost = Player.GetManaCost(gunGloveItem);
					if (CombinedHooks.CanShoot(Player, gunGloveItem) && Player.CheckMana(manaCost, true)) {
						if (manaCost > 0) {
							Player.manaRegenDelay = (int)Player.maxRegenDelay;
						}
						Vector2 position = Player.itemLocation;
						Vector2 velocity = Vec2FromPolar(Player.direction == 1 ? Player.itemRotation : Player.itemRotation + MathHelper.Pi, speed);

						CombinedHooks.ModifyShootStats(Player, gunGloveItem, ref position, ref velocity, ref projToShoot, ref damage, ref knockback);
						EntitySource_ItemUse_WithAmmo source = (EntitySource_ItemUse_WithAmmo)Player.GetSource_ItemUse_WithPotentialAmmo(gunGloveItem, usedAmmoItemId);
						if (CombinedHooks.Shoot(Player, gunGloveItem, source, position, velocity, projToShoot, damage, knockback)) {
							Projectile.NewProjectile(source, position, velocity, projToShoot, damage, knockback, Player.whoAmI);
							SoundEngine.PlaySound(gunGloveItem.UseSound, position);
						}
					}
					gunGloveCooldown = CombinedHooks.TotalUseTime(gunGloveItem.useTime, Player, gunGloveItem);
				}
			}
		}
		public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit) {
			//enemyDefense = NPC.GetDefense;
			if (felnumShock > 29) {
				damage += (int)(felnumShock / 15);
				felnumShock = 0;
				SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), target.Center);
			}
			if (item.CountsAsClass(DamageClasses.Explosive)) {
				damage -= (int)Math.Max((target.defense - Player.GetWeaponArmorPenetration(item)) * (explosive_defense_factor - 0.5f), 0);
			}
			if (target.HasBuff(BuffID.Bleeding)) {
				target.lifeRegen -= 1;
			}
			if (acridSet) {
				target.AddBuff(Toxic_Shock_Debuff.ID, 300);
			}
		}
		public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (advancedImaging) {
				velocity *= 1.38f;
			}
			if (item.CountsAsClass(DamageClasses.Explosive)) {
				StatModifier velocityModifier = explosiveProjectileSpeed;
				if (item.useAmmo == 0 && item.CountsAsClass(DamageClass.Throwing)) {
					velocityModifier = velocityModifier.CombineWith(explosiveThrowSpeed);
				}
				float baseSpeed = velocity.Length();
				velocity *= velocityModifier.ApplyTo(baseSpeed) / baseSpeed;
			}
			if (item.shoot > ProjectileID.None && felnumShock > 29) {
				Projectile p = new();
				p.SetDefaults(type);
				OriginGlobalProj.felnumEffectNext = true;
				if (
					(p.CountsAsClass(DamageClass.Melee) ||
					p.CountsAsClass(DamageClass.Summon) ||
					ProjectileID.Sets.IsAWhip[type] ||
					Origins.DamageModOnHit[type] ||
					p.aiStyle == ProjAIStyleID.WaterJet) &&
					!Origins.ForceFelnumShockOnShoot[type]) return;
				damage += (int)(felnumShock / 15);
				felnumShock = 0;
				SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), position);
			}
		}
		public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (item.CountsAsClass(DamageClasses.Explosive)) {
				if (riftSet) {
					Fraction dmg = new Fraction(2, 2);
					int c = (madHand ? 1 : 0) + (Main.rand.NextBool(2) ? 1 : 0);
					dmg.D += c;
					damage *= dmg;
					double rot = Main.rand.NextBool(2) ? -0.1 : 0.1;
					Vector2 _position;
					Vector2 _velocity;
					int _type;
					int _damage;
					float _knockBack;
					for (int i = c; i-- > 0;) {
						_position = position;
						_velocity = velocity.RotatedBy(rot);
						_type = type;
						_damage = damage;
						_knockBack = knockback;
						if (ItemLoader.Shoot(item, Player, source, _position, _velocity, _type, _damage, _knockBack)) {
							Projectile.NewProjectile(source, _position, _velocity, _type, _damage, _knockBack, Player.whoAmI);
						}
						rot = -rot;
					}
				}
				if (novaSet) {
					Fraction dmg = new Fraction(3, 3);
					int c = (madHand ? 1 : 0) + (Main.rand.NextBool(4) ? 0 : 1);
					dmg.D += c;
					damage *= dmg;
					double rot = Main.rand.NextBool(2) ? -0.1 : 0.1;
					Vector2 _position;
					Vector2 _velocity;
					int _type;
					int _damage;
					float _knockBack;
					for (int i = c; i-- > 0;) {
						_position = position;
						_velocity = velocity.RotatedBy(rot);
						_type = type;
						_damage = damage;
						_knockBack = knockback;
						if (ItemLoader.Shoot(item, Player, source, _position, _velocity, _type, _damage, _knockBack)) {
							Projectile.NewProjectile(source, _position, _velocity, _type, _damage, _knockBack, Player.whoAmI);
						}
						rot = -rot;
					}
				}
			}
			return true;
		}
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			if (Origins.DamageModOnHit[proj.type]) {
				bool shouldReplace = Origins.ExplosiveBaseDamage.TryGetValue(proj.type, out int dam);
				float baseDamage = Player.GetTotalDamage(proj.DamageType).ApplyTo(shouldReplace ? dam : damage);
				damage = shouldReplace ? Main.DamageVar(baseDamage) : (int)baseDamage;
			}
			if ((proj.CountsAsClass(DamageClass.Melee) || proj.CountsAsClass(DamageClass.Summon) || ProjectileID.Sets.IsAWhip[proj.type]) && felnumShock > 29) {
				damage += (int)(felnumShock / 15);
				felnumShock = 0;
				SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), proj.Center);
			}
			if (proj.minion && rivenSet) {
				damage = (int)(damage * rivenMult);
			}
			if (proj.CountsAsClass(DamageClasses.Explosive)) {
				damage -= (int)Math.Max((target.defense - proj.ArmorPenetration) * (explosive_defense_factor - 0.5f), 0);
			}
		}
		public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit) {
			OnHitNPCGeneral(item, target, damage, knockback, crit);
			if (entangledEnergy && item.ModItem is IElementalItem elementalItem && (elementalItem.Element & Elements.Fiberglass) != 0) {
				Projectile.NewProjectile(
					Player.GetSource_OnHit(target),
					target.Center,
					default,
					ModContent.ProjectileType<Entangled_Energy_Lifesteal>(),
					0,
					0,
					Player.whoAmI,
					ai1: damage / 10
				);
			}
			if (item.CountsAsClass(DamageClass.Melee)) {//flasks
				if (flaskBile) {
					target.AddBuff(Rasterized_Debuff.ID, Rasterized_Debuff.duration * 2);
				}
				if (flaskSalt) {
					OriginGlobalNPC.InflictTorn(target, 300, 180, 0.8f, this);
				}
			}
			if (item.CountsAsClass(DamageClasses.Explosive)) {
				if (madHand) {
					target.AddBuff(BuffID.Oiled, 600);
					target.AddBuff(BuffID.OnFire, 600);
				}
			}
		}
		public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) {
			OnHitNPCGeneral(proj, target, damage, knockback, crit);
			if (proj.CountsAsClass(DamageClass.Melee) || ProjectileID.Sets.IsAWhip[proj.type]) {//flasks
				if (flaskBile) {
					target.AddBuff(Rasterized_Debuff.ID, Rasterized_Debuff.duration * 2);
				}
				if (flaskSalt) {
					OriginGlobalNPC.InflictTorn(target, 300, 180, 0.8f, this);
				}
			}
		}
		public void OnHitNPCGeneral(Entity entity, NPC target, int damage, float knockback, bool crit) {
			Entity sourceEntity = entity is Projectile ? entity : Player;
			if (crit) {
				if (dimStarlight && dimStarlightCooldown < 1) {
					Item.NewItem(sourceEntity.GetSource_OnHit(target, "Accessory"), target.position, target.width, target.height, ItemID.Star);
					dimStarlightCooldown = 300;
				}
			}
			if (symbioteSkull) {
				OriginGlobalNPC.InflictTorn(target, Main.rand.Next(50, 70), 60, 0.9f, this);
			}
			if (decayingScale) {
				target.AddBuff(Toxic_Shock_Debuff.ID, Toxic_Shock_Debuff.default_duration);
			}
			if (messyLeech) {
				target.AddBuff(BuffID.Bleeding, 480);
			}
			if (target.life <= 0) {
				foreach (var quest in Quest_Registry.Quests) {
					if (!quest.SaveToWorld && quest.KillEnemyEvent is not null) {
						quest.KillEnemyEvent(target);
					}
				}
				if (necroSet) {
					necroSetAmount += target.lifeMax;
				}
			}
			if (hasPotatOS && Main.rand.NextBool(10)) {
				Potato_Battery.PlayRandomMessage(
					Potato_Battery.QuoteType.Combat,
					potatOSQuoteCooldown,
					Player.Top,
					new Vector2(Math.Sign(target.Center.X - Player.Center.X) * 7f, -2f + Main.rand.NextFloat() * -2f)
				);
			}
		}

		public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit) {
			if (trapCharm && proj.trap) {
				damage /= 2;
				Player.buffImmune[BuffID.Poisoned] = true;
			}
			if (proj.owner == Player.whoAmI && proj.friendly && proj.CountsAsClass(DamageClasses.Explosive)) {
				/*float damageVal = damage;
                if(minerSet) {
                    explosiveSelfDamage-=0.2f;
                    float inverseDamage = Player.GetDamage(DamageClasses.Explosive).ApplyTo(damage);
                    damageVal -= inverseDamage - damage;
                    //damage = (int)(damage/explosiveDamage);
                    //damage-=damage/5;
                }
                damage = (int)(damageVal * explosiveSelfDamage);*/
			}
		}
		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (entangledEnergy && item.ModItem is IElementalItem elementalItem && (elementalItem.Element & Elements.Fiberglass) != 0) {
				damage.Flat += Player.statDefense / 2;
			}
			if (Origins.ArtifactMinion[item.shoot]) damage = damage.CombineWith(artifactDamage);
			if (focusCrystal) {
				damage *= 1 + (focusCrystalTime / 360f);
			}
			damage.Base *= Origins.FlatDamageMultiplier[item.type];
			damage.Flat *= Origins.FlatDamageMultiplier[item.type];
		}
		public override void ModifyWeaponCrit(Item item, ref float crit) {
			if (rubyReticle) {
				crit += Player.GetWeaponDamage(item) * 0.15f;
			}
		}
		public override void OnHitByNPC(NPC npc, int damage, bool crit) {
			if (!Player.noKnockback && damage != 0) {
				Player.velocity.X *= MeleeCollisionNPCData.knockbackMult;
			}
			if (preHitBuffs is not null)
				for (int i = 0; i < Player.MaxBuffs; i++) {
					if (!preHitBuffs.Contains(new Point(Player.buffType[i], Player.buffTime[i]))) {
						int buffType = Player.buffType[i];
						if (noU) {
							bool immune = npc.buffImmune[buffType];
							npc.buffImmune[buffType] = false;
							npc.AddBuff(buffType, Player.buffTime[i]);
							npc.buffImmune[buffType] = immune;

							Player.DelBuff(i--);
						} else if (plasmaPhial) {
							if (Main.debuff[buffType]) {
								Player.buffTime[i] /= 2;
							}
						} else if (donorWristband) {
							if (Main.debuff[buffType]) {
								Player.buffTime[i] -= (int)(Player.buffTime[i] * 0.375f);
							}
						}
					}
				}
			MeleeCollisionNPCData.knockbackMult = 1f;
		}
		public override void OnHitByProjectile(Projectile proj, int damage, bool crit) {
			if (preHitBuffs is not null)
				for (int i = 0; i < Player.MaxBuffs; i++) {
					if (!preHitBuffs.Contains(new Point(Player.buffType[i], Player.buffTime[i]))) {
						int buffType = Player.buffType[i];
						if (plasmaPhial) {
							if (Main.debuff[buffType]) {
								Player.buffTime[i] /= 2;
							}
						} else if (donorWristband) {
							if (Main.debuff[buffType]) {
								Player.buffTime[i] -= (int)(Player.buffTime[i] * 0.375f);
							}
						}
					}
				}
		}
		/// <param name="target">the potential target</param>
		/// <param name="targetPriorityMultiplier"></param>
		/// <param name="isPriorityTarget">whether or not this npc is a "priority" target (i.e. a manually selected target)</param>
		/// <param name="foundTarget">whether or not a target has already been found</param>
		public delegate void Minion_Selector(NPC target, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget);
		public bool GetMinionTarget(Minion_Selector selector) {
			bool foundTarget = false;
			if (Player.MinionAttackTargetNPC > -1) selector(Main.npc[Player.MinionAttackTargetNPC], 1f, true, ref foundTarget);
			if (asylumWhistleTarget > -1) selector(Main.npc[asylumWhistleTarget], 1f, true, ref foundTarget);
			if (!foundTarget) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					selector(Main.npc[i], 1f, false, ref foundTarget);
				}
			}
			return foundTarget;
		}
		#endregion
		internal static FastFieldInfo<PlayerDeathReason, int> _sourcePlayerIndex;
		static FastFieldInfo<PlayerDeathReason, int> SourcePlayerIndex => _sourcePlayerIndex ??= new("_sourcePlayerIndex", BindingFlags.NonPublic);
		internal static FastFieldInfo<PlayerDeathReason, int> _sourceProjectileIndex;
		static FastFieldInfo<PlayerDeathReason, int> SourceProjectileIndex => _sourceProjectileIndex ??= new("_sourceProjectileIndex", BindingFlags.NonPublic);
		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter) {
			if (Player.HasBuff(Toxic_Shock_Debuff.ID) && Main.rand.Next(9) < 3) {
				crit = true;
			}
			heliumTankHit = false;
			if (heliumTank && playSound) {
				if (!Player.stoned && !Player.frostArmor && !Player.boneArmor) {
					heliumTankHit = true;
					playSound = false;
				}
			}
			if (SourcePlayerIndex.GetValue(damageSource) == Player.whoAmI) {
				Projectile sourceProjectile = Main.projectile[SourceProjectileIndex.GetValue(damageSource)];
				if (sourceProjectile.owner == Player.whoAmI && sourceProjectile.CountsAsClass(DamageClasses.Explosive)) {
					float damageVal = damage;
					if (minerSet) {
						explosiveSelfDamage -= 0.2f;
						float inverseDamage = Player.GetDamage(DamageClasses.Explosive).ApplyTo(damage);
						damageVal -= inverseDamage - damage;
						if (damageVal < 0) {
							damageVal = 0;
						}
						//damage = (int)(damage/explosiveDamage);
						//damage-=damage/5;
					}
					if (resinShield) {
						explosiveSelfDamage = new StatModifier();
						resinShieldCooldown = (int)explosiveFuseTime.Scale(5).ApplyTo(300);
						if (Player.shield == Resin_Shield.ShieldID) {
							for (int i = Main.rand.Next(4, 8); i-->0;) {
								Dust.NewDust(Player.MountedCenter + new Vector2(12 * Player.direction - 6, -12), 8, 32, DustID.GemAmber, Player.direction * 2, Alpha: 100);
							}
						}
					}
					damage = (int)explosiveSelfDamage.ApplyTo(damageVal);
					if (Math.Sign(damage) != Math.Sign(damageVal)) {
						damage = 0;
					}
				}
			}
			if (defiledSet) {
				float manaDamage = damage;
				float costMult = 3;
				float costMult2 = reshapingChunk ? 0.25f : 0.15f;
				float costMult3 = (float)Math.Pow(reshapingChunk ? 0.25f : 0.15f, Player.manaCost);
				if (Player.magicCuffs) {
					costMult = 1;
					Player.magicCuffs = false;
				}
				if (Player.statMana < manaDamage * costMult * costMult2) {
					manaDamage = Player.statMana / (costMult * costMult2);
				}
				if (manaDamage * costMult * costMult2 >= 1f) {
					Player.ManaEffect((int)-(manaDamage * costMult * costMult2));
				}
				Player.CheckMana((int)Math.Floor(manaDamage * costMult * costMult2), true);
				damage = (int)(damage - (manaDamage * costMult3));
				Player.AddBuff(ModContent.BuffType<Defiled_Exhaustion_Buff>(), 50);
			} else if (reshapingChunk) {
				damage -= damage / 20;
			}
			if (toxicShock) {
				damage += Player.statDefense / 10;
			}
			return damage > 0;
		}
		public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) {
			if (heliumTankHit) {
				if ((Player.wereWolf || Player.forceWerewolf) && !Player.hideWolf) {
					SoundEngine.PlaySound(SoundID.NPCHit6.WithPitch(1), Player.position);
				} else if (Main.dontStarveWorld) {
					SoundStyle style = (Player.Male ? SoundID.DSTMaleHurt : SoundID.DSTFemaleHurt).WithPitch(1);
					SoundEngine.PlaySound(in style, Player.position);
				} else {
					SoundEngine.PlaySound((Player.Male ? SoundID.PlayerHit : SoundID.FemaleHit).WithPitch(1), Player.position);
				}
				Player.eyeHelper.BlinkBecausePlayerGotHurt();
			}
			if (guardedHeart) {
				Player.AddBuff(Guarded_Heart_Buff.ID, 60 * 8);
			}
			if (razorwire) {
				const float maxDist = 240 * 240;
				double totalDamage = damage * 0.67f;
				List<(int id, float weight)> targets = new();
				NPC npc;
				for (int i = 0; i < Main.maxNPCs; i++) {
					npc = Main.npc[i];
					if (npc.active && npc.damage > 0 && !npc.friendly) {
						Vector2 currentPos = npc.Hitbox.ClosestPointInRect(Player.MountedCenter);
						Vector2 diff = currentPos - Player.MountedCenter;
						float dist = diff.LengthSquared();
						if (dist > maxDist) continue;
						float currentWeight = (1.5f - Vector2.Dot(npc.velocity, diff.SafeNormalize(default))) * (dist / maxDist);
						if (totalDamage / 3 > npc.life) {
							currentWeight = 0;
						}
						if (targets.Count >= 3) {
							for (int j = 0; j < 3; j++) {
								if (targets[j].weight < currentWeight) {
									targets.Insert(j, (i, currentWeight));
									break;
								}
							}
						} else {
							targets.Add((i, currentWeight));
						}
					}
				}
				for (int i = 0; i < 3; i++) {
					if (i >= targets.Count) break;
					Vector2 currentPos = Main.npc[targets[i].id].Hitbox.ClosestPointInRect(Player.MountedCenter);
					Projectile.NewProjectile(
						Player.GetSource_Accessory(razorwireItem),
						Player.MountedCenter,
						(currentPos - Player.MountedCenter).WithMaxLength(12),
						razorwireItem.shoot,
						(int)(totalDamage / targets.Count) + 1,
						10,
						Player.whoAmI
					);
				}
			}
			if (unsoughtOrgan) {
				const float maxDist = 240 * 240;
				double totalDamage = damage * 0.5f;
				List<(int id, float weight)> targets = new();
				NPC npc;
				for (int i = 0; i < Main.maxNPCs; i++) {
					npc = Main.npc[i];
					if (npc.active && npc.damage > 0 && !npc.friendly) {
						Vector2 currentPos = npc.Hitbox.ClosestPointInRect(Player.MountedCenter);
						Vector2 diff = currentPos - Player.MountedCenter;
						float dist = diff.LengthSquared();
						if (dist > maxDist) continue;
						float currentWeight = (1.5f - Vector2.Dot(npc.velocity, diff.SafeNormalize(default))) * (dist / maxDist);
						if (totalDamage / 3 > npc.life) {
							currentWeight = 0;
						}
						if (targets.Count >= 3) {
							for (int j = 0; j < 3; j++) {
								if (targets[j].weight < currentWeight) {
									targets.Insert(j, (i, currentWeight));
									break;
								}
							}
						} else {
							targets.Add((i, currentWeight));
						}
					}
				}
				for (int i = 0; i < 3; i++) {
					if (i >= targets.Count) break;
					Vector2 currentPos = Main.npc[targets[i].id].Hitbox.ClosestPointInRect(Player.MountedCenter);
					Projectile.NewProjectile(
						Player.GetSource_Accessory(unsoughtOrganItem),
						Player.MountedCenter,
						(currentPos - Player.MountedCenter).WithMaxLength(12),
						unsoughtOrganItem.shoot,
						(int)(totalDamage / targets.Count) + 1,
						10,
						Player.whoAmI
					);
				}
			}
			preHitBuffs = new();
			for (int i = 0; i < Player.MaxBuffs; i++) {
				preHitBuffs.Add(new Point(Player.buffType[i], Player.buffTime[i]));
			}
			if (thornsVisualProjType >= 0) {
				Projectile.NewProjectile(
					Player.GetSource_Misc("thorns_visual"),
					Player.position,
					default,
					thornsVisualProjType,
					0,
					0,
					Player.whoAmI
				);
			}
		}
		public override void PostSellItem(NPC vendor, Item[] shopInventory, Item item) {
			if (vendor.type == NPCID.Demolitionist && item.type == ModContent.ItemType<Peat_Moss>()) {
				OriginSystem originWorld = ModContent.GetInstance<OriginSystem>();
				if (originWorld.peatSold < 999) {
					if (item.stack >= 999 - originWorld.peatSold) {
						item.stack -= 999 - originWorld.peatSold;
						originWorld.peatSold = 999;
						int nextSlot = 0;
						for (; ++nextSlot < shopInventory.Length && !shopInventory[nextSlot].IsAir;) ;
						if (nextSlot < shopInventory.Length) shopInventory[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Grenade>());
						if (nextSlot < shopInventory.Length) shopInventory[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Bomb>());
						if (nextSlot < shopInventory.Length) shopInventory[nextSlot].SetDefaults(ModContent.ItemType<Impact_Dynamite>());
					} else {
						originWorld.peatSold += item.stack;
						item.TurnToAir();
					}
				}
			}
		}
		public bool DisplayJournalTooltip(IJournalEntryItem journalItem) {
			if (!journalUnlocked) {
				return true;
			}
			bool unlockedEntry = unlockedJournalEntries.Contains(journalItem.EntryName);
			if (Origins.InspectItemKey.JustPressed) {
				if (!unlockedEntry) unlockedJournalEntries.Add(journalItem.EntryName);
				if (OriginClientConfig.Instance.OpenJournalOnUnlock) {
					Origins.OpenJournalEntry(journalItem.EntryName);
				}
				return false;
			}
			return !unlockedEntry;
		}
		public static void InflictTorn(Player player, int duration, int targetTime = 180, float targetSeverity = 0.7f) {
			player.AddBuff(Torn_Buff.ID, duration);
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (targetSeverity < originPlayer.tornTarget) {
				originPlayer.tornTargetTime = targetTime;
				originPlayer.tornTarget = targetSeverity;
			}
		}

		public override void LoadData(TagCompound tag) {
			if (tag.SafeGet<Item>("EyndumCore") is Item eyndumCoreItem) {
				eyndumCore = new Ref<Item>(eyndumCoreItem);
			}
			if (tag.SafeGet<int>("MimicSetSelection") is int mimicSetSelection) {
				mimicSetChoices = mimicSetSelection;
			}
			if (tag.SafeGet<Item>("JournalDye") is Item journalDyeItem) {
				journalDye = journalDyeItem;
			}
			if (tag.SafeGet<List<string>>("UnlockedJournalEntries") is List<string> journalEntries) {
				unlockedJournalEntries = journalEntries.ToHashSet();
			}
			if (tag.ContainsKey("journalUnlocked")) {
				journalUnlocked = tag.Get<bool>("journalUnlocked");
			}
			questsTag = tag.SafeGet<TagCompound>("Quests");
			if (tag.SafeGet<int>("TimeSinceLastDeath") is int timeSinceLastDeath) {
				this.timeSinceLastDeath = timeSinceLastDeath;
			}
		}
		TagCompound questsTag;
		public override void OnEnterWorld(Player player) {
			questsTag ??= new TagCompound();
			TagCompound worldQuestsTag = ModContent.GetInstance<OriginSystem>().questsTag ?? new TagCompound();
			foreach (var quest in Quest_Registry.Quests) {
				if (!quest.SaveToWorld) {
					quest.LoadData(questsTag.SafeGet<TagCompound>(quest.FullName) ?? new TagCompound());
				} else {
					quest.LoadData(worldQuestsTag.SafeGet<TagCompound>(quest.FullName) ?? new TagCompound());
				}
			}
		}
		public override void SaveData(TagCompound tag) {
			if (eyndumCore is not null) {
				tag.Add("EyndumCore", eyndumCore.Value);
			}
			tag.Add("MimicSetSelection", mimicSetChoices);
			tag.Add("journalUnlocked", journalUnlocked);
			if (journalDye is not null) {
				tag.Add("JournalDye", journalDye);
			}
			if (unlockedJournalEntries is not null) {
				tag.Add("UnlockedJournalEntries", unlockedJournalEntries.ToList());
			}
			TagCompound questsTag = new TagCompound();
			foreach (var quest in Quest_Registry.Quests) {
				if (!quest.SaveToWorld) {
					TagCompound questTag = new TagCompound();
					quest.SaveData(questTag);
					if (questTag.Count > 0) questsTag.Add(quest.FullName, questTag);
				}
			}
			if (questsTag.Count > 0) {
				tag.Add("Quests", questsTag);
			}
			tag.Add("TimeSinceLastDeath", timeSinceLastDeath);
		}
		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
			bool zoneDefiled = Player.InModBiome<Defiled_Wastelands>();
			bool zoneRiven = Player.InModBiome<Riven_Hive>();
			bool junk = (itemDrop >= ItemID.OldShoe && itemDrop < ItemID.MinecartTrack);
			if (zoneDefiled && zoneDefiled) {
				if (Main.rand.NextBool()) {
					zoneDefiled = false;
				} else {
					zoneRiven = false;
				}
			}
			if (zoneDefiled) {
				if (attempt.crate) {
					if (attempt.rare && !(attempt.veryrare || attempt.legendary)) {
						itemDrop = ModContent.ItemType<Chunky_Crate>();
					}
				} else if (attempt.legendary && Main.hardMode && Main.rand.NextBool(2)) {
					itemDrop = ModContent.ItemType<Knee_Slapper>();
				} else if (attempt.uncommon) {
					int prikish = ModContent.ItemType<Prikish>();
					if (attempt.questFish == prikish) {
						itemDrop = prikish;
					} else {
						itemDrop = ModContent.ItemType<Bilemouth>();
					}
				}
			} else if (zoneRiven) {
				if (attempt.crate) {
					if (attempt.rare && !(attempt.veryrare || attempt.legendary)) {
						itemDrop = ModContent.ItemType<Crusty_Crate>();
					}
				} else if (attempt.legendary && Main.hardMode && Main.rand.NextBool(2)) {
					itemDrop = ModContent.ItemType<Knee_Slapper>();
				} else if (attempt.uncommon) {
					itemDrop = ModContent.ItemType<Tearracuda>();
				}
			}
			if (junk) {
				if (Main.rand.NextBool(4)) {
					itemDrop = ModContent.ItemType<Tire>();
				}
			}
			if (Player.ZoneJungle && attempt.uncommon && !(attempt.rare || attempt.veryrare || attempt.legendary)) {
				if (Main.rand.NextBool(10)) {
					itemDrop = ModContent.ItemType<Messy_Leech>();
				}
			}
		}
		public override bool CanUseItem(Item item) {
			if (ravel) {
				return false;
			}
			return true;
		}
		public override bool PreItemCheck() {
			collidingX = oldXSign != 0 && Player.velocity.X == 0;
			collidingY = oldYSign != 0 && Player.velocity.Y == 0;
			ItemChecking = true;
			return true;
		}
		public override void PostItemCheck() {
			ItemChecking = false;
		}
		public override void HideDrawLayers(PlayerDrawSet drawInfo) {
			Item item = drawInfo.heldItem;
			if (
				(
					drawInfo.drawPlayer.ItemAnimationActive && (
						(item.useStyle == ItemUseStyleID.Shoot && item.ModItem is ICustomDrawItem) ||
						(item.useStyle == ItemUseStyleID.Swing && item.ModItem is AnimatedModItem)
					)
				)) PlayerDrawLayers.HeldItem.Hide();

			if (mountOnly && !drawInfo.headOnlyRender) {
				for (int i = 0; i < PlayerDrawLayerLoader.DrawOrder.Count; i++) {
					PlayerDrawLayer layer = PlayerDrawLayerLoader.DrawOrder[i];
					if (layer != PlayerDrawLayers.MountFront && layer != PlayerDrawLayers.MountBack) {
						layer.Hide();
					}
				}
			}
		}
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {

			if (plagueSight) drawInfo.colorEyes = IsDevName(Player.name, 1) ? new Color(43, 185, 255) : Color.Gold;
			if (mysteriousSprayMult != 1f) {
				float lightSaturationMult = (float)Math.Pow(mysteriousSprayMult, 2f);
				float saturationMult = 1f - (float)Math.Pow(1f - mysteriousSprayMult, 1.5f);
				drawInfo.colorArmorHead = OriginExtensions.Desaturate(drawInfo.colorArmorHead, lightSaturationMult);
				drawInfo.colorArmorBody = OriginExtensions.Desaturate(drawInfo.colorArmorBody, lightSaturationMult);
				drawInfo.colorArmorLegs = OriginExtensions.Desaturate(drawInfo.colorArmorLegs, lightSaturationMult);
				drawInfo.floatingTubeColor = OriginExtensions.Desaturate(drawInfo.floatingTubeColor, lightSaturationMult);
				drawInfo.itemColor = OriginExtensions.Desaturate(drawInfo.itemColor, lightSaturationMult);

				drawInfo.headGlowColor = OriginExtensions.Desaturate(drawInfo.headGlowColor, saturationMult);
				drawInfo.armGlowColor = OriginExtensions.Desaturate(drawInfo.armGlowColor, saturationMult);
				drawInfo.bodyGlowColor = OriginExtensions.Desaturate(drawInfo.bodyGlowColor, saturationMult);
				drawInfo.legsGlowColor = OriginExtensions.Desaturate(drawInfo.legsGlowColor, saturationMult);

				drawInfo.colorElectricity = OriginExtensions.Desaturate(drawInfo.colorElectricity, saturationMult);
				drawInfo.ArkhalisColor = OriginExtensions.Desaturate(drawInfo.ArkhalisColor, saturationMult);

				drawInfo.colorHair = OriginExtensions.Desaturate(drawInfo.colorHair, saturationMult);
				drawInfo.colorHead = OriginExtensions.Desaturate(drawInfo.colorHead, saturationMult);
				drawInfo.colorEyes = Color.Lerp(drawInfo.colorEyes, Color.White, 1f - saturationMult);
				drawInfo.colorEyeWhites = Color.Lerp(drawInfo.colorEyeWhites, Color.Black, 1f - saturationMult);
				drawInfo.colorBodySkin = OriginExtensions.Desaturate(drawInfo.colorBodySkin, saturationMult);

			}
			if (drawInfo.drawPlayer.shield == Resin_Shield.ShieldID && resinShieldCooldown > 0) {
				drawInfo.drawPlayer.shield = (sbyte)Resin_Shield.InactiveShieldID;
			}
		}
		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			//return;
			if (Main.netMode == NetmodeID.SinglePlayer) return;
			ModPacket packet = Mod.GetPacket();
			packet.Write(Origins.NetMessageType.sync_player);
			packet.Write((byte)Player.whoAmI);
			packet.Write((byte)quantumInjectors);
			packet.Write((byte)defiledWill);
			packet.Send(toWho, fromWho);
		}
		public void ReceivePlayerSync(BinaryReader reader) {
			quantumInjectors = reader.ReadByte();
			defiledWill = reader.ReadByte();
		}
		public override void FrameEffects() {
			for (int i = 13; i < 18 + Player.extraAccessorySlots; i++) {
				if (Player.armor[i].type == Plague_Texan_Sight.ID) Plague_Texan_Sight.ApplyVisuals(Player);
			}
		}
		public void SetMimicSetChoice(int level, int choice) {
			mimicSetChoices = (mimicSetChoices & ~(3 << level * 2)) | ((choice & 3) << level * 2);
		}
		public int GetMimicSetChoice(int level) {
			return (mimicSetChoices >> level * 2) & 3;
		}
	}
}
