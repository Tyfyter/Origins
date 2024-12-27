using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Accessories;
using Origins.Items.Other.Consumables.Broths;
using Origins.Misc;
using Origins.NPCs.Defiled;
using Origins.Projectiles.Misc;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins {
	public partial class OriginPlayer : ModPlayer {
		public const float rivenMaxMult = 0.3f;
		public float rivenMult => (1f - rivenMaxMult) + Math.Max((Player.statLife / (float)Player.statLifeMax2) * (rivenMaxMult * 2), rivenMaxMult);

		#region assimilation
		public const float assimilation_max = 1f;
		float corruptionAssimilation = 0; // corruption
		float crimsonAssimilation = 0; // crimson
		float defiledAssimilation = 0; // defiled
		float rivenAssimilation = 0; // riven
									 // properties so that effects which change how much assimilation a player receives can be implemented more easily
		public float CorruptionAssimilation {
			get => corruptionAssimilation;
			set {
				corruptionAssimilation = value;
			}
		}
		public float CrimsonAssimilation {
			get => crimsonAssimilation;
			set {
				crimsonAssimilation = value;
			}
		}
		public float DefiledAssimilation {
			get => defiledAssimilation;
			set {
				defiledAssimilation = value;
			}
		}
		public float RivenAssimilation {
			get => rivenAssimilation;
			set {
				if (rivenAssimilation < value) timeSinceRivenAssimilated = 0;
				rivenAssimilation = value;
			}
		}
		public float corruptionAssimilationDebuffMult = 1f;
		public float crimsonAssimilationDebuffMult = 1f;
		public float defiledAssimilationDebuffMult = 1f;
		public float rivenAssimilationDebuffMult = 1f;
		public int timeSinceRivenAssimilated = 0;
		#endregion assimilation

		#region armor/set bonuses
		public bool ashenKBReduction = false;
		public bool fiberglassSet = false;
		public bool cryostenSet = false;
		public bool cryostenHelmet = false;
		public bool felnumSet = false;
		public float felnumShock = 0;
		public bool usedFelnumShock = false;
		public float oldFelnumShock = 0;
		public bool minerSet = false;
		public bool lostSet = false;
		public bool rivenSet = false;
		public bool rivenSetBoost = false;
		public bool bleedingObsidianSet = false;
		public bool eyndumSet = false;
		public bool mimicSet = false;
		public bool riptideSet = false;
		public bool riptideLegs = false;
		public int riptideDashTime = 0;
		public int meatDashTime = 0;
		public bool necroSet = false;
		public bool novaSet = false;
		public bool tendonSet = false;
		public bool acridSet = false;
		public float necroSetAmount = 0f;
		public bool soulhideSet = false;
		public int mimicSetChoices = 0;
		public int setActiveAbility = 0;
		public int setAbilityCooldown = 0;
		public bool scavengerSet = false;
		public bool amberSet = false;
		public bool sapphireSet = false;
		public bool blastSet = false;
		public float blastSetCharge = 0;
		public const int blast_set_charge_max = 200;
		public const float blast_set_charge_gain = 0.8f;
		public const float blast_set_charge_decay = 12;
		public bool blastSetActive = false;
		#endregion armor/set bonuses

		#region accessories
		public bool bombHandlingDevice = false;
		public bool destructiveClaws = false;
		public bool dimStarlight = false;
		public int dimStarlightCooldown = 0;
		public bool madHand = false;
		public bool fiberglassDagger = false;
		public bool advancedImaging = false;
		public bool venomFang = false;
		public bool lazyCloakVisible = false;
		public bool amebicVialVisible = false;
		public byte amebicVialCooldown = 0;
		public bool entangledEnergy = false;
		public bool asylumWhistle = false;
		public int asylumWhistleTarget = -1;
		public int mitosisCooldown = 0;
		public bool refactoringPieces;
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
		public bool retributionShield = false;
		public Item retributionShieldItem = null;
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
		public bool messyLeech = false;
		public bool magmaLeech = false;
		public bool noU = false;
		public const float donorWristbandMult = 0.52f;
		public bool donorWristband = false;
		public bool oldDonorWristband = false;
		public HashSet<Point> preHitBuffs;
		public int lastHitEnemy;
		public const float plasmaPhialMult = 0.7f;
		public bool plasmaPhial = false;
		public bool oldPlasmaPhial = false;
		public bool turboReel = false;
		public bool turboReel2 = false;
		public bool automatedReturnsHandler = false;
		public bool boomerangMagnet = false;
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
		public bool shineSpark = false;
		public bool shineSparkVisible = false;
		public Item shineSparkItem = null;
		public int shineSparkCharge = 0;
		public int shineSparkDashTime = 0;
		public Vector2 shineSparkDashDirection = default;
		public float shineSparkDashSpeed = 0;
		public bool magicTripwire = false;
		public int lousyLiverCount = 0;
		public List<(int id, int duration)> lousyLiverDebuffs = [];
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
		public Item protomindItem = null;
		public bool hasProtOS = false;
		public int[] potatOSQuoteCooldown;
		public int[] protOSQuoteCooldown;
		public int lastGravDir = 1;
		public int nearbyBoundNPCTime = 0;
		public int nearbyBoundNPCType = 0;
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
		public bool dangerBarrel = false;
		public bool pincushion = false;
		public Item meatScribeItem = null;
		public int meatDashCooldown = 0;
		public Item lotteryTicketItem = null;
		public StatModifier tornStrengthBoost = StatModifier.Default;
		public bool endlessExplosives = false;
		public Item cinderSealItem = null;
		public int cinderSealCount = 4;
		public bool dryadNecklace = false;
		public bool weakpointAnalyzer = false;
		public bool bindingBookVisual = false;
		public int bindingBookDye = 0;
		public Physics.Chain[] bindingBookChains = new Physics.Chain[3];
		public bool priorityMail = false;
		public bool emergencyBeeCanister = false;
		public bool blizzardwalkerJacket = false;
		public bool blizzardwalkerJacketVisual = false;
		public int blizzardwalkerActiveTime = 0;
		public bool cursedCrown = false;
		public bool cursedCrownVisual = false;
		public Item strangeToothItem = null;
		public int strangeToothCooldown = 0;
		public bool controlLocus = false;
		public int pickupRangeBoost = 0;
		//public bool isVoodooPickup = false;
		public bool primordialSoup = false;
		public bool bugZapper = false;
		public bool bombCharminIt = false;
		public bool cursedVoice = false;
		public Item cursedVoiceItem = null;
		public int cursedVoiceCooldown = 0;
		public bool futurephones = false;
		public bool coreGenerator = false;
		public Item coreGeneratorItem = null;
		public bool strangeComputer = false;
		public Color strangeComputerColor = Color.Blue;
		public bool scrapCompactor = false;
		public int scrapCompactorTimer = 0;
		public bool scrapBarrierCursed = false;
		public bool slagBucketCursed = false;
		public bool slagBucket = false;
		public bool scrapBarrierDebuff = false;
		public bool laserTagVest = false;
		public bool laserTagVestActive = false;
		public int laserTagPoints = 0;
		public int laserTagHits = 0;
		public int laserTagHP = 0;
		public int laserTagRespawnDelay = 0;
		#endregion

		#region explosive stats
		public StatModifier explosiveProjectileSpeed = StatModifier.Default;
		public StatModifier explosiveThrowSpeed = StatModifier.Default;
		public StatModifier explosiveSelfDamage = StatModifier.Default;
		public StatModifier explosiveBlastRadius = StatModifier.Default;
		public StatModifier explosiveFuseTime = StatModifier.Default;
		public int selfDamageRally = 0;
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
		public float tornCurrentSeverity = 0;
		public float tornSeverityRate = 0.3f / 180;
		public float tornSeverityDecayRate = 0.15f / 180;
		public float tornTarget = 0f;
		public bool hideTornTime = false;
		public Vector2 tornOffset = default;
		public bool swarmStatue = false;
		public bool focusPotion = false;
		public BrothBase broth = null;
		public bool cavitationDebuff = false;
		#endregion

		#region keybinds
		public bool controlTriggerSetBonus = false;
		public bool releaseTriggerSetBonus = false;
		#endregion

		#region other items
		public int laserBladeCharge = 0;
		public bool boatRockerAltUse = false;
		public int mojoFlaskCount = 5;
		public int mojoFlaskCountMax = 5;

		public int quantumInjectors = 0;
		public bool mojoInjection = false;
		public int defiledWill = 0;

		public int talkingPet = 0;
		public int talkingPetTime = 0;
		#endregion

		#region visuals
		public int shieldGlow = -1;
		#endregion visuals

		public float statSharePercent = 0f;

		public bool journalUnlocked = false;
		public Item journalDye = null;

		public bool itemLayerWrench = false;
		public bool plagueSight = false;
		public bool plagueSightLight = false;

		public Ref<Item> eyndumCore = null;

		internal static bool ItemChecking = false;
		public int cryostenLifeRegenCount = 0;
		public int bombCharminItLifeRegenCount = 0;
		internal byte oldBonuses = 0;
		public const int minionSubSlotValues = 3;
		public float[] minionSubSlots = new float[minionSubSlotValues];
		public int wormHeadIndex = -1;
		public int heldProjectile = -1;
		public IDrawOverArmProjectile heldProjOverArm = null;
		public int lastMinionAttackTarget = -1;
		public int hookTarget = -1;
		bool rivenWet = false;
		public bool mountOnly = false;
		public bool hideAllLayers = false;
		public bool disableUseItem = false;
		public bool changeSize = false;
		public int targetWidth;
		public int targetHeight;
		public int oldXSign = 0;
		public int oldYSign = 0;
		public bool collidingX = false;
		public bool collidingY = false;
		public HashSet<string> unlockedJournalEntries = new();
		public HashSet<string> startedQuests = new();
		public int dashDirection = 0;
		public int dashDirectionY = 0;
		public int dashDelay = 0;
		public int thornsVisualProjType = -1;
		public int timeSinceLastDeath = -1;
		public int oldBreath = 200;
		public float oldGravDir = 0;
		public float lifeRegenTimeSinceHit = 0;
		public int itemUseOldDirection = 0;
		public List<Vector2> oldVelocities = new();
		public Guid guid = Guid.Empty;
		public int voodooDollIndex = -1;
		public float manaShielding = 0f;
		public int doubleTapDownTimer = 0;
		public bool doubleTapDown = false;
		public bool forceDrown = false;
		public List<string> journalText = [];
		public override void ResetEffects() {
			oldBonuses = 0;
			if (fiberglassSet || fiberglassDagger) oldBonuses |= 1;
			if (felnumSet) oldBonuses |= 2;
			fiberglassSet = false;
			cryostenSet = false;
			cryostenHelmet = false;
			oldFelnumShock = felnumShock;
			if (!felnumSet || usedFelnumShock) {
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
			usedFelnumShock = false;
			if (donorWristband) {
				float healLogic = (1 - 0.375f) / (Player.pStone ? 0.75f : 1);

				Player.potionDelayTime = (int)(Player.potionDelayTime * healLogic);
				Player.restorationDelayTime = (int)(Player.restorationDelayTime * healLogic);
				Player.mushroomDelayTime = (int)(Player.mushroomDelayTime * healLogic);
			}

			ashenKBReduction = false;
			felnumSet = false;
			minerSet = false;
			lostSet = false;
			refactoringPieces = false;
			rivenSet = false;
			rivenSetBoost = false;
			bleedingObsidianSet = false;
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
			soulhideSet = false;
			scavengerSet = false;
			amberSet = false;
			sapphireSet = false;
			if (blastSetActive) {
				if (!blastSet || (blastSetCharge -= (blast_set_charge_decay / 60f)) <= 0) {
					blastSetActive = false;
					blastSetCharge = 0;
				}
			}
			if (blastSetCharge > blast_set_charge_max) blastSetCharge = blast_set_charge_max;
			blastSet = false;

			setActiveAbility = 0;
			if (setAbilityCooldown > 0) {
				if (--setAbilityCooldown == 0) {
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
			venomFang = false;
			lazyCloakVisible = false;
			amebicVialVisible = false;
			entangledEnergy = false;
			mysteriousSprayMult = 1;
			protozoaFood = false;
			protozoaFoodItem = null;
			symbioteSkull = false;
			taintedFlesh = false;
			taintedFlesh2 = false;
			tornStrengthBoost = StatModifier.Default;
			endlessExplosives = false;
			if (!Player.immune) cinderSealCount = cinderSealItem?.useAnimation ?? 4;
			cinderSealItem = null;
			if (toxicShock) {
				if (Player.breath > oldBreath) Player.breath = oldBreath;
				toxicShock = false;
			}
			gunGlove = false;
			gunGloveItem = null;
			guardedHeart = false;
			razorwire = false;
			retributionShield = false;
			razorwireItem = null;
			retributionShieldItem = null;
			unsoughtOrgan = false;
			unsoughtOrganItem = null;
			spiritShard = false;
			ravel = false;
			heliumTank = false;
			messyLeech = false;
			magmaLeech = false;
			noU = false;

			turboReel = false;
			turboReel2 = false;
			boomerangMagnet = false;

			Player.ApplyBuffTimeAccessory(oldPlasmaPhial, plasmaPhial, plasmaPhialMult, Main.debuff);
			oldPlasmaPhial = plasmaPhial;
			plasmaPhial = false;
			Player.ApplyBuffTimeAccessory(oldDonorWristband, donorWristband, donorWristbandMult, Main.debuff);
			oldDonorWristband = donorWristband;
			donorWristband = false;

			trapCharm = false;
			dangerBarrel = false;
			pincushion = false;
			meatScribeItem = null;
			if (meatDashCooldown > 0) {
				if (--meatDashCooldown <= 0) {
					for (int i = 0; i < 8; i++) {
						Dust.NewDust(
							Player.position,
							Player.width,
							Player.height,
							DustID.t_Flesh,
							Scale: 1.5f
						);
					}
					SoundEngine.PlaySound(SoundID.NPCDeath13.WithVolumeScale(0.75f), Player.position);
				}
			}
			lotteryTicketItem = null;

			
			spiderRavel = false;
			if (spiderRavelTime > 0) spiderRavelTime--;
			doubleTapDownTimer++;

			if (explosiveArtery) {
				explosiveArtery = false;
			} else {
				explosiveArteryCount = -1;
			}
			explosiveArteryItem = null;
			graveDanger = false;
			loversLeap = false;
			shineSpark = false;
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
			protomindItem = null;
			hasProtOS = false;
			potatOSQuoteCooldown ??= new int[(int)Potato_Battery.QuoteType.Count];
			for (int i = 0; i < (int)Potato_Battery.QuoteType.Count; i++) {
				if (potatOSQuoteCooldown[i] > 0) potatOSQuoteCooldown[i]--;
			}
			protOSQuoteCooldown ??= new int[(int)Protomind.QuoteType.Count];
			for (int i = 0; i < (int)Protomind.QuoteType.Count; i++) {
				if (protOSQuoteCooldown[i] > 0) protOSQuoteCooldown[i]--;
			}
			if (talkingPetTime > 0 && --talkingPetTime <= 0) talkingPet = -1;

			if (resinShieldCooldown > 0) resinShieldCooldown--;
			resinShield = false;
			if (thirdEyeTime < thirdEyeUseTime) {
				if (thirdEyeTime > 0 && !thirdEyeActive) thirdEyeTime--;
			} else if (++thirdEyeTime >= thirdEyeResetTime) {
				thirdEyeTime = 0;
			}
			sonarVisor = false;
			solarPanel = false;
			dryadNecklace = false;
			weakpointAnalyzer = false;
			bindingBookVisual = false;
			priorityMail = false;
			emergencyBeeCanister = false;
			if (!blizzardwalkerJacket) blizzardwalkerActiveTime = 0;
			blizzardwalkerJacket = false;
			blizzardwalkerJacketVisual = false;
			cursedCrown = false;
			cursedCrownVisual = false;
			strangeToothItem = null;
			controlLocus = false;
			pickupRangeBoost = 0;
			primordialSoup = false;
			bugZapper = false;
			bombCharminIt = false;
			cursedVoice = false;
			cursedVoiceItem = null;
			if (cursedVoiceCooldown > 0) cursedVoiceCooldown--;
			futurephones = false;
			coreGenerator = false;
			strangeComputer = false;
			scrapCompactor = false;
			scrapBarrierCursed = false;
			slagBucketCursed = false;
			slagBucket = false;
			scrapBarrierDebuff = false;
			if (laserTagVest) {
				if (laserTagRespawnDelay > 0) laserTagRespawnDelay--;
			} else {
				ResetLaserTag();
			}
			laserTagVest = false;

			flaskBile = false;
			flaskSalt = false;
			swarmStatue = false;
			focusPotion = false;
			cavitationDebuff = false;
			broth = null;

			boatRockerAltUse = false;

			manaShielding = 0f;

			corruptionAssimilationDebuffMult = 1f;
			crimsonAssimilationDebuffMult = 1f;
			defiledAssimilationDebuffMult = 1f;
			rivenAssimilationDebuffMult = 1f;
			timeSinceRivenAssimilated++;

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
			if (bombCharminItLifeRegenCount > 0)
				bombCharminItLifeRegenCount--;

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
			if (strangeToothCooldown > 0)
				strangeToothCooldown--;

			if (laserBladeCharge > 0 && !Player.ItemAnimationActive) laserBladeCharge--;

			if (rapidSpawnFrames > 0)
				rapidSpawnFrames--;
			if (!tornDebuff && tornCurrentSeverity > 0) {
				tornCurrentSeverity -= tornSeverityDecayRate;
				if (tornCurrentSeverity <= 0) {
					tornTarget = 0f;
				}
			}
			tornSeverityDecayRate = 0.15f / 180f;
			tornDebuff = false;
			int rasterized = Player.FindBuffIndex(Rasterized_Debuff.ID);
			if (rasterized >= 0) {
				rasterizedTime = Math.Min(Math.Min(rasterizedTime + 1, 8), Player.buffTime[rasterized] - 1);
			} else if (!Player.HasBuff<Defiled_Asphyxiator_Debuff_3>()) {
				rasterizedTime = 0;
			}
			plagueSight = false;
			plagueSightLight = false;
			mountOnly = false;
			hideAllLayers = false;
			disableUseItem = false;
			thornsVisualProjType = -1;
			changeSize = false;
			minionSubSlots = new float[minionSubSlotValues];
			if (timeSinceLastDeath < int.MaxValue) timeSinceLastDeath++;
			#region asylum whistle
			if (lastMinionAttackTarget != Player.MinionAttackTargetNPC) {
				if (asylumWhistle) {
					if (Player.MinionAttackTargetNPC == -1) {
						Player.MinionAttackTargetNPC = asylumWhistleTarget;
						asylumWhistleTarget = -1;
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
				} else if (Player.whoAmI == Main.myPlayer && Player.HeldItem.CountsAsClass(DamageClass.Summon)) {
					Vector2 center = possibleTarget.Center;
					float count = Player.miscCounter / 60f;
					float offset = MathHelper.TwoPi / 3f;
					for (int i = 0; i < 3; i++) {
						int dust = Dust.NewDust(center, 0, 0, DustID.WitherLightning, 0f, 0f, 100, default, 0.35f);
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
			if (Player.lifeRegenTime > lifeRegenTimeSinceHit) {
				lifeRegenTimeSinceHit = Player.lifeRegenTime;
			} else {
				lifeRegenTimeSinceHit += 1f;
				if (Player.usedAegisCrystal) {
					lifeRegenTimeSinceHit += 0.2f;
				}
				if (Player.honey) {
					lifeRegenTimeSinceHit += 2f;
				}
			}
			oldVelocities.Insert(0, Player.velocity);
			while (oldVelocities.Count > 20) oldVelocities.RemoveAt(20);
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
			if (voodooDollIndex >= 0) {
				Item voodooDoll = Main.item[voodooDollIndex];
				Player.wet |= forceWetCollision = voodooDoll.wet;
				Player.lavaWet |= forceLavaCollision = voodooDoll.lavaWet;
				Player.honeyWet |= forceHoneyCollision = voodooDoll.honeyWet;
				Player.shimmerWet |= forceShimmerCollision = voodooDoll.shimmerWet;
				voodooDollIndex = -1;
			}
			forceDrown = false;
			heldProjOverArm = null;
			shieldGlow = -1;
		}
		internal static bool forceWetCollision;
		internal static bool forceLavaCollision;
		internal static bool forceHoneyCollision;
		internal static bool forceShimmerCollision;
		public Vector2 AverageOldVelocity(int count = -1) {
			if (count == -1 || count > oldVelocities.Count) count = oldVelocities.Count;
			Vector2 value = Vector2.Zero;
			for (int i = 0; i < count; i++) {
				value += oldVelocities[i];
			}
			value /= count;
			return value;
		}
		public void SetTalkingPet(int index) {
			talkingPet = index;
			talkingPetTime = 2;
		}
	}
}
