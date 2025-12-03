using CalamityMod.Enums;
using Origins.Buffs;
using Origins.Dusts;
using Origins.Items.Accessories;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Consumables.Broths;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Layers;
using Origins.Misc;
using Origins.NPCs.Defiled;
using Origins.NPCs.Riven;
using Origins.Projectiles.Misc;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace Origins {
	public partial class OriginPlayer : ModPlayer {
		public const float rivenMaxMult = 0.3f;
		public float rivenMult => (1f - rivenMaxMult) + Math.Max((Player.statLife / (float)Player.statLifeMax2) * (rivenMaxMult * 2), rivenMaxMult);

		#region assimilation
		public const float assimilation_max = 1f;
		public int timeSinceRivenAssimilated = 0;
		AssimilationInfo[] assimilationData = [];
		void ValidateAssimilations() {
			if (assimilationData.Length != AssimilationLoader.Debuffs.Count) {
				assimilationData = new AssimilationInfo[AssimilationLoader.Debuffs.Count];
				for (int i = 0; i < assimilationData.Length; i++) {
					assimilationData[i] = new(AssimilationLoader.Debuffs[i], Player);
				}
			}
		}
		public AssimilationInfo GetAssimilation(int type) {
			ValidateAssimilations();
			return assimilationData[type];
		}
		public AssimilationInfo GetAssimilation<TDebuff>() where TDebuff : AssimilationDebuff => GetAssimilation(ModContent.GetInstance<TDebuff>().AssimilationType);
		public IEnumerable<AssimilationInfo> IterateAssimilation() {
			ValidateAssimilations();
			return assimilationData;
		}
		#endregion assimilation

		#region armor/set bonuses
		public bool ashenKBReduction = false;
		public bool fiberglassHelmet = false;
		public bool fiberglassSet = false;
		public bool oldCryostenSet = false;
		public bool oldCryostenHelmet = false;
		public bool cryostenSet = false;
		public bool cryostenBody = false;
		public bool felnumSet = false;
		public bool felnumEnemiesFriendly = false;
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
		public bool necroSet2 = false;
		public float necroSetAmount = 0f;
		public bool novaSet = false;
		public bool tendonSet = false;
		public bool acridSet = false;
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
		public bool rainSet = false;
		public bool rubberBody = false;
		public int nearTrafficCone = 0;
		public bool extremophileSet = false;
		public int extremophileSetHits = 0;
		public int extremophileSetTime = 0;
		public bool luckyHatSet = false;
		public int luckyHatSetTime = 0;
		public bool LuckyHatSetActive => luckyHatSet && (luckyHatSetTime == -1 || luckyHatSetTime >= 90);
		public bool luckyHatGun = false;
		public int luckyHatGunTime = 0;
		public bool LuckyHatGunActive => luckyHatGun && (luckyHatGunTime == -1 || luckyHatGunTime >= 90);
		public bool mildewHead = false;
		public bool mildewSet = false;
		public bool chambersiteCommandoSet = false;
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
		public bool lightningRing = false;
		public bool lazyCloakHidden = false;
		public int[] lazyCloaksOffPlayer = ArmorIDs.Front.Sets.Factory.CreateIntSet();
		public bool amebicVialVisible = false;
		public byte amebicVialCooldown = 0;
		public bool entangledEnergy = false;
		public float entangledEnergyCount = 0;
		public bool asylumWhistle = false;
		public int asylumWhistleTarget = -1;
		public int mitosisCooldown = 0;
		public bool refactoringPieces = false;
		public int refactoringPiecesDashCooldown = 0;
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
		public Item barkShieldItem = null;
		public Item flakJacketItem = null;
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
		public bool heliumTankSqueak = false;
		public float heliumTankStrength = 1f;
		public bool messyLeech = false;
		public bool magmaLeech = false;
		public bool noU = false;
		public const float donorWristbandMult = 0.7f;
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
		public float lousyLiverRange = 256;
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
		public int lastDir = 1;
		public bool changedDir = false;
		public bool ChangedGravDir => Player.gravDir != oldGravDir;
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
		public bool abyssalAnchorVisual = false;
		public int abyssalAnchorDye = 0;
		public Physics.Chain abyssalAnchorChain = null;
		public Vector2 abyssalAnchorPosition;
		public Vector2 abyssalAnchorVelocity;
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
		public int bugZapperFlyTime = 0;
		public bool bombCharminIt = false;
		public int bombCharminItStrength = 24;
		public bool cursedVoice = false;
		public Item cursedVoiceItem = null;
		public int cursedVoiceCooldown = 0;
		public int cursedVoiceCooldownMax = 0;
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
		public float gunSpeedBonus = 0;
		public float meleeScaleMultiplier = 1;
		public bool eitriteGunMagazine = false;
		public bool fairyLotus = false;
		public bool abyssalAnchor = false;
		public bool mildewHeart = false;
		public float mildewHeartRegenMult = 1f;
		public float mildewHealth = 0;
		public PlayerDeathReason lastMildewDeathReason;
		public bool lastMildewDeathPvP = false;
		public bool faithBeads = false;
		public Item faithBeadsItem = null;
		public bool retaliatoryTendril = false;
		public Item retaliatoryTendrilItem = null;
		public float retaliatoryTendrilStrength = 0;
		public int retaliatoryTendrilCharge = 0;
		public bool mithrafin = false;
		public bool oldMithrafin = false;
		public const float mithrafinSelfMult = 0.8f;
		public bool fullSend = false;
		public bool fullSendHorseshoeBonus = false;
		public Item fullSendItem = null;
		public Vector2 fullSendStartPos;
		public Vector2 fullSendPos;
		public bool akaliegis = false;
		public List<(int type, Range duration)> dashHitDebuffs = [];
		public bool dashVase = false;
		public int vaseDashDirection = 0;
		public bool dashVaseVisual = false;
		public int dashVaseDye = 0;
		public float dashVaseFrameCount = 0;
		public bool goldenLotus = false;
		public Item goldenLotusItem = null;
		public int goldenLotusProj = -1;
		public bool resizingGlove = false;
		public float resizingGloveScale = 1f;
		public bool WishingGlass {
			get => wishingGlassEquipTime > 0;
			set => wishingGlassEquipTime = value.Mul(2);
		}
		int wishingGlassEquipTime = 0;
		public bool wishingGlassActive = false;
		public int wishingGlassCooldown = 0;
		public bool wishingGlassVisible = false;
		public int wishingGlassAnimation = 0;
		public int wishingGlassDye = -1;
		public Vector2 wishingGlassOffset = default;
		public bool shimmerShield = false;
		public int shimmerShieldDashTime = 0;
		public int? dashBaseDamage = 0;
		public bool airTank = false;
		public bool gasMask = false;
		public bool AnyGasMask => gasMask || filterBreather;
		public bool filterBreather = false;
		public int gasMaskDye = 0;
		public const float gasMaskMult = 0.75f;
		public bool crystalHeart = false;
		public int crystalHeartCounter = 0;
		public bool pacemaker = false;

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
		public float necromancyPrefixMana = 0;
		#endregion

		#region biomes
		public bool ZoneVoid { get; internal set; } = false;
		public float ZoneVoidProgress = 0;
		public float ZoneVoidProgressSmoothed = 0;

		public float ZoneDefiledProgress = 0;
		public float ZoneDefiledProgressSmoothed = 0;

		public float ZoneRivenProgress = 0;
		public float ZoneRivenProgressSmoothed = 0;

		public float ZoneAshenProgress = 0;
		public float ZoneAshenProgressSmoothed = 0;

		public float ZoneBrineProgress = 0;
		public float ZoneBrineProgressSmoothed = 0;

		public bool ZoneFiberglass { get; internal set; } = false;
		public float ZoneFiberglassProgress = 0;
		public float ZoneFiberglassProgressSmoothed = 0;
		#endregion

		#region buffs
		public int rapidSpawnFrames = 0;
		public int rasterizedTime = 0;
		public int? visualRasterizedTime = null;
		public int VisualRasterizedTime => visualRasterizedTime ?? rasterizedTime;
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
		public int staticBrothEffectCooldown = 0;
		public bool cavitationDebuff = false;
		public bool staticShock = false;
		public bool miniStaticShock = false;
		public bool staticShockDamage = false;
		public int staticShockTime = 0;
		public int relayRodStrength = 0;
		public bool weakShimmer = false;
		public bool compositeFrontArmWasEnabled = false;
		public bool walledDebuff = false;
		public bool medicinalAcid = false;
		public int preMedicinalAcidLife = 0;
		public int medicinalAcidLife = 0;
		public bool murkySludge = false;
		public bool miasma = false;
		public bool tetanus = false;

		public bool DisableBreathRestore => toxicShock || miasma;
		public bool sendBuffs = false;
		#endregion

		#region keybinds
		public bool controlTriggerSetBonus = false;
		public bool releaseTriggerSetBonus = false;
		#endregion

		#region other items
		public int laserBladeCharge = 0;
		public int tolrukCharge = 0;
		public bool boatRockerAltUse = false;
		public bool boatRockerAltUse2 = false;
		public int mojoFlaskChargesUsed = 0;
		public int mojoFlaskCountMax = 5;
		public int MojoFlaskCount => mojoFlaskCountMax - mojoFlaskChargesUsed;

		public int quantumInjectors = 0;
		public bool mojoInjection = false;
		public bool MojoInjectionEnabled {
			get => Player.BuilderToggleState<Mojo_Injection_Toggle>() == 0;
			set => Player.BuilderToggleState<Mojo_Injection_Toggle>() = (!value).ToInt();
		}
		public bool MojoInjectionActive => mojoInjection && MojoInjectionEnabled;
		public bool crownJewel = false;
		public bool CrownJewelEnabled {
			get => Player.BuilderToggleState<Crown_Jewel_Toggle>() == 0;
			set => Player.BuilderToggleState<Crown_Jewel_Toggle>() = (!value).ToInt();
		}
		public bool CrownJewelActive => crownJewel && CrownJewelEnabled;
		public int defiledWill = 0;

		public int talkingPet = 0;
		public int talkingPetTime = 0;
		public int nextActiveHarpoons = 0;
		public int currentActiveHarpoons = 0;
		public Vector2 nextActiveHarpoonAveragePosition = default;
		public Vector2 currentActiveHarpoonAveragePosition = default;
		public float aprilFoolsRubberDynamiteTracker = 0;
		public int crawdadNetworkCount = 0;
		public int neuralNetworkMisses = 0;
		public int keytarMode = 0;
		public float soulSnatcherTime = 0;
		public bool soulSnatcherActive = false;

		public bool shimmerGuardianMinion = false;
		public List<int> ownedLargeGems = [];
		public int amnesticRoseHoldTime = 0;
		public int amnesticRoseBloomTime = 0;
		public PolarVec2[] amnesticRoseJoints = [];
		/// <summary>
		/// Relative to Player.Bottom
		/// </summary>
		public Vector2 relativeTarget = Vector2.Zero;
		public float dreamcatcherAngle = 0;
		public float dreamcatcherRotSpeed = 0;
		public int dreamcatcherHoldTime = 0;
		public Vector2? dreamcatcherWorldPosition = null;
		public bool pocketDimensionMonolithActive = false;
		public bool InfoAccMechShowAshenWires = false;
		public int blastFurnaceCharges = 0;
		public List<int> unlockedPlantModes = [];
		List<ItemDefinition> unloadedPlantModes = [];
		#endregion

		#region visuals
		public int shieldGlow = -1;
		#endregion visuals

		public float statSharePercent = 0f;
		public StatModifier projectileSpeedBoost = StatModifier.Default;

		public bool journalUnlocked = false;
		public Item journalDye = null;

		public bool releaseAltUse = false;
		public bool itemLayerWrench = false;
		public int itemComboAnimationTime = 0;
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
		public bool onSlope = false;
		public HashSet<string> unlockedJournalEntries = [];
		public HashSet<string> unreadJournalEntries = [];
		public HashSet<string> startedQuests = [];
		public int dashDirection = 0;
		public int dashDirectionY = 0;
		public int dashDelay = 0;
		public int thornsVisualProjType = -1;
		public int timeSinceLastDeath = -1;
		public int oldBreath = 200;
		public float oldGravDir = 0;
		public float lifeRegenTimeSinceHit = 0;
		public int timeSinceHit = 0;
		public int itemUseOldDirection = 0;
		public List<Vector2> oldVelocities = [];
		public Guid guid = Guid.Empty;
		public Item voodooDoll = null;
		public float manaShielding = 0f;
		public int doubleTapDownTimer = 0;
		public bool doubleTapDown = false;
		public bool forceDrown = false;
		public bool forceFallthrough = false;
		public bool noFallThrough = false;
		public int timeSinceRainedOn = 0;
		/// <summary>
		/// not set to true by alt uses
		/// </summary>
		public bool realControlUseItem = false;
		public float oldNearbyActiveNPCs = 0;
		public List<string> journalText = [];
		public float moveSpeedMult = 1;
		public bool upsideDown = false;
		int[] minionCountByType = ProjectileID.Sets.Factory.CreateIntSet();
		public Speed_Booster.ConveyorBeltModifier conveyorBeltModifiers = null;
		public const int scytheBladeDetachCombo = 3;
		public const int maxScytheCombo = 10;
		public int scytheHitCombo = 0;
		public const int maxDangerTime = 5 * 60;
		public int dangerTime = 0;
		public bool InDanger { get; private set; }
		public override void ResetEffects() {
			Debugging.LogFirstRun(ResetEffects);
			oldBonuses = 0;
			if (fiberglassSet || fiberglassDagger) oldBonuses |= 1;
			if (felnumSet) oldBonuses |= 2;
			fiberglassHelmet = false;
			fiberglassSet = false;
			oldCryostenSet = false;
			oldCryostenHelmet = false;
			cryostenSet = false;
			cryostenBody = false;
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
			felnumEnemiesFriendly = false;
			minerSet = false;
			lostSet = false;
			refactoringPieces = false;
			if (refactoringPiecesDashCooldown > 0) {
				if (--refactoringPiecesDashCooldown <= 0) {
					for (int i = 0; i < 8; i++) {
						Dust.NewDust(
							Player.position,
							Player.width,
							Player.height,
							DustID.TintableDustLighted,
							Scale: 1.5f,
							newColor: Main.hslToRgb(Main.rand.NextFloat(6), 1, 0.5f)
						);
					}
					SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitch(-1f), Player.position);
				}
			}
			Player.GetJumpState<Latchkey_Jump_Refresh>().Enable();
			rivenSet = false;
			rivenSetBoost = false;
			bleedingObsidianSet = false;
			eyndumSet = false;
			mimicSet = false;
			riptideSet = false;
			riptideLegs = false;
			necroSet = false;
			necroSet2 = false;
			if (necroSetAmount > 0) {
				necroSetAmount -= 1 + necroSetAmount * 0.01f;
			}
			novaSet = false;
			tendonSet = false;
			acridSet = false;
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
			rainSet = false;
			rubberBody = false;
			if (nearTrafficCone > 0) nearTrafficCone--;
			if (extremophileSet) {
				if (extremophileSetTime > 0 && extremophileSetTime < 60 * 15) extremophileSetTime++;
				extremophileSet = false;
			} else {
				extremophileSetHits = 0;
				extremophileSetTime = 0;
			}
			if (luckyHatSet) {
				if (Player.ItemAnimationActive) {
					luckyHatSetTime = LuckyHatSetActive && Player.itemAnimation > 1 ? -1 : 0;
				} else if (Player.HeldItem.CountsAsClass(DamageClass.Ranged) || Player.HeldItem.CountsAsClass(DamageClasses.Explosive)) {
					if (luckyHatSetTime < 90) {
						luckyHatSetTime++;
						if (LuckyHatSetActive) {
							SoundEngine.PlaySound(SoundID.Camera.WithPitchRange(0.6f, 1f), Player.Center);
							SoundEngine.PlaySound(SoundID.Coins.WithPitchRange(0.6f, 1f), Player.Center);
						}
					}
				} else {
					luckyHatSetTime = 0;
				}
				luckyHatSet = false;
			} else {
				luckyHatSetTime = 0;
			}
			if (Player.HeldItem?.ModItem is Rattlesnake) {
				luckyHatGun = true;
				if (Player.ItemAnimationActive) {
					luckyHatGunTime = LuckyHatGunActive && Player.itemAnimation > 1 ? -1 : 0;
				} else {
					if (luckyHatGunTime < 90) {
						luckyHatGunTime++;
						if (LuckyHatGunActive) {
							SoundEngine.PlaySound(SoundID.Camera.WithPitchRange(0.6f, 1f), Player.Center);
							SoundEngine.PlaySound(SoundID.Coins.WithPitchRange(0.6f, 1f), Player.Center);
						}
					}
				}
			} else {
				luckyHatGun = false;
				luckyHatGunTime = 0;
			}
			mildewHead = false;
			mildewSet = false;
			chambersiteCommandoSet = false;

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
			lightningRing = false;
			lazyCloakHidden = false;
			for (int i = 0; i < lazyCloaksOffPlayer.Length; i++) lazyCloaksOffPlayer[i].Cooldown();
			amebicVialVisible = false;
			entangledEnergy = false;
			entangledEnergyCount.Warmup(60, Entangled_Energy.MaxSecondsPerSecond);
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
			toxicShock = false;
			gunGlove = false;
			gunGloveItem = null;
			guardedHeart = false;
			razorwire = false;
			retributionShield = false;
			razorwireItem = null;
			retributionShieldItem = null;
			barkShieldItem = null;
			flakJacketItem = null;
			unsoughtOrgan = false;
			unsoughtOrganItem = null;
			spiritShard = false;
			ravel = false;
			heliumTank = false;
			heliumTankSqueak = false;
			messyLeech = false;
			magmaLeech = false;
			noU = false;

			turboReel = false;
			turboReel2 = false;
			boomerangMagnet = false;

			try {
				Main.debuff[BuffID.PotionSickness] = false;
				Player.ApplyBuffTimeAccessory(oldPlasmaPhial, plasmaPhial, plasmaPhialMult, Main.debuff);
				oldPlasmaPhial = plasmaPhial;
				plasmaPhial = false;

				Player.ApplyBuffTimeAccessory(oldDonorWristband, donorWristband, donorWristbandMult, Main.debuff);
				oldDonorWristband = donorWristband;
				donorWristband = false;

				Player.ApplyBuffTimeAccessory(oldMithrafin, mithrafin, mithrafinSelfMult, Mithrafin.buffTypes);
				oldMithrafin = mithrafin;
				mithrafin = false;
			} finally {
				Main.debuff[BuffID.PotionSickness] = true;
			}
			fullSend = false;
			akaliegis = false;
			dashHitDebuffs.Clear();
			dashVase = false;
			dashVaseDye = 0;
			abyssalAnchorDye = 0;
			bindingBookDye = 0;
			goldenLotus = false;

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
			resizingGlove = false;
			wishingGlassEquipTime.Cooldown();
			if (wishingGlassCooldown.Cooldown()) {
				if (wishingGlassVisible) {

				} else {
					int dustType = ModContent.DustType<Following_Shimmer_Dust>();
					for (int i = 0; i < 20; i++) {
						Dust dust = Dust.NewDustDirect(
							Player.position,
							Player.width,
							Player.height,
							dustType,
							0f,
							0f,
							100,
							default,
							2.5f
						);
						dust.noGravity = true;
						dust.velocity *= 11f;
						dust.position -= dust.velocity * 12;
						dust.customData = new Following_Shimmer_Dust.FollowingDustSettings(Player, 1);

						dust = Dust.NewDustDirect(
							Player.position,
							Player.width,
							Player.height,
							dustType,
							0f,
							0f,
							100,
							default,
							1.5f
						);
						dust.noGravity = true;
						dust.velocity *= 7f;
						dust.position -= dust.velocity * 12;
						dust.customData = new Following_Shimmer_Dust.FollowingDustSettings(Player, 1);
					}
				}
			}
			wishingGlassActive = false;
			wishingGlassOffset -= Player.velocity * (wishingGlassAnimation > Wishing_Glass_Layer.CooldownEndAnimationDuration ? 0 : 0.5f);
			wishingGlassOffset *= wishingGlassAnimation > Wishing_Glass_Layer.CooldownEndAnimationDuration ? 0.7f : 0.85f;
			Wishing_Glass_Layer.UpdateAnimation(ref wishingGlassAnimation, wishingGlassCooldown);
			wishingGlassVisible = false;
			wishingGlassDye = 0;
			shimmerShield = false;
			dashBaseDamage = null;
			airTank = false;
			gasMask = false;
			gasMaskDye = 0;
			filterBreather = false;
			if (!crystalHeart.TrySet(false)) crystalHeartCounter = 0;
			pacemaker = false;
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
			lousyLiverRange = 256;
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
			currentActiveHarpoons = nextActiveHarpoons;
			currentActiveHarpoonAveragePosition = nextActiveHarpoonAveragePosition / currentActiveHarpoons;
			nextActiveHarpoons = 0;
			nextActiveHarpoonAveragePosition = Vector2.Zero;

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
			abyssalAnchorVisual = false;
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
			if (bugZapperFlyTime > 0) bugZapperFlyTime--;
			bombCharminIt = false;
			bombCharminItStrength = 24;
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
			gunSpeedBonus = 0;
			meleeScaleMultiplier = 1;
			eitriteGunMagazine = false;
			fairyLotus = false;
			abyssalAnchor = false;
			mildewHeart = false;
			mildewHeartRegenMult = 1f;
			faithBeads = false;
			if (!retaliatoryTendril) {
				retaliatoryTendrilStrength = 0;
				retaliatoryTendrilCharge = 0;
			}
			retaliatoryTendril = false;
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
			staticShock = false;
			miniStaticShock = false;
			staticShockDamage = false;
			if (!Player.HasBuff<Relay_Rod_Buff>()) relayRodStrength = 0;
			weakShimmer = false;
			compositeFrontArmWasEnabled = false;
			broth = null;
			if (staticBrothEffectCooldown > 0)
				staticBrothEffectCooldown--;

			mojoFlaskCountMax = 0;
			for (int i = 0; i < Player.inventory.Length; i++) {
				if (Player.inventory[i]?.ModItem is Mojo_Flask mojoFlask) mojoFlaskCountMax = mojoFlask.FlaskUseCount;
			}

			boatRockerAltUse = false;
			boatRockerAltUse2 = false;

			shimmerGuardianMinion = false;
			amnesticRoseHoldTime.Cooldown();
			if (amnesticRoseHoldTime <= 0) amnesticRoseBloomTime.Cooldown();
			Dream_Catcher.UpdateVisual(Player, ref dreamcatcherAngle, ref dreamcatcherRotSpeed);
			if (dreamcatcherHoldTime.Cooldown()) dreamcatcherWorldPosition = null;
			pocketDimensionMonolithActive = false;
			InfoAccMechShowAshenWires = false;
			if (blastFurnaceCharges > 0 && Player.HeldItem.ModItem is not Blast_Furnace) blastFurnaceCharges = 0;

			manaShielding = 0f;

			foreach (AssimilationInfo info in IterateAssimilation()) {
				info.ResetEffects();
			}
			timeSinceRivenAssimilated++;

			explosiveProjectileSpeed = StatModifier.Default;
			explosiveThrowSpeed = StatModifier.Default;
			explosiveSelfDamage = StatModifier.Default;
			explosiveBlastRadius = StatModifier.Default;
			explosiveFuseTime = StatModifier.Default;

			artifactDamage = StatModifier.Default;
			artifactManaCost = 1f;

			statSharePercent = 0f;
			projectileSpeedBoost = StatModifier.Default;

			if (itemComboAnimationTime > 0)
				itemComboAnimationTime--;

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
			if (tolrukCharge > 0 && !Player.ItemAnimationActive) tolrukCharge--;

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
			visualRasterizedTime = null;
			Soul_Snatcher.UpdateCharge(Player, ref soulSnatcherTime, ref soulSnatcherActive);
			plagueSight = false;
			plagueSightLight = false;
			mountOnly = false;
			hideAllLayers = false;
			disableUseItem = false;
			thornsVisualProjType = -1;
			if (changeSize) {
				Player.position.X -= (targetWidth - Player.width) / 2;
				Player.position.Y -= targetHeight - Player.height;
				Player.width = targetWidth;
				Player.height = targetHeight;
			} else if (targetWidth != 0) {
				Player.position.X -= (20 - Player.width) / 2;
				Player.position.Y -= 42 - Player.height;
				Player.width = 20;
				Player.height = 42;
				targetWidth = 0;
				targetHeight = 0;
			}
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
			timeSinceHit++;
			oldVelocities.Insert(0, Player.velocity);
			while (oldVelocities.Count > 20) oldVelocities.RemoveAt(20);
			if (tornCurrentSeverity >= 0.99f && Player.whoAmI == Main.myPlayer && !Player.dead && Player.statLifeMax2 <= 0) {
				mildewHealth = 0;
				Player.KillMe(PlayerDeathReason.ByCustomReason(TextUtils.LanguageTree.Find("Mods.Origins.DeathMessage.Torn").SelectFrom(Player.name).ToNetworkText()),
					9999, 0
				);
			}
			#region check if a dash should start
			dashDirection = 0;
			dashDirectionY = 0;
			if (dashDelay <= 0) {
				const int DashDown = 0;
				const int DashUp = 1;
				const int DashRight = 2;
				const int DashLeft = 3;
				if (Player.whoAmI == Main.myPlayer) {
					if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15) {
						dashDirection = 1;
					} else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15) {
						dashDirection = -1;
					}
					if (Player.controlUp && Player.releaseUp && Player.doubleTapCardinalTimer[DashUp] < 15) {
						dashDirectionY = -1;
					} else if (Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[DashDown] < 15) {
						dashDirectionY = 1;
					}
					if (dashDirection != 0 || dashDirectionY != 0) new Dash_Action(Player, dashDirection, dashDirectionY).Send();
				}
			} else {
				dashDelay--;
			}
			#endregion
			if (voodooDoll is not null) {
				Player.wet |= forceWetCollision = voodooDoll.wet;
				Player.lavaWet |= forceLavaCollision = voodooDoll.lavaWet;
				Player.honeyWet |= forceHoneyCollision = voodooDoll.honeyWet;
				Player.shimmerWet |= forceShimmerCollision = voodooDoll.shimmerWet;
			}
			changedDir = lastDir.TrySet(Player.direction);
			voodooDoll = null;
			forceDrown = false;
			heldProjOverArm = null;
			shieldGlow = -1;
			if (timeSinceRainedOn < int.MaxValue) timeSinceRainedOn++;
			moveSpeedMult = 1;
			Array.Clear(minionCountByType);
			upsideDown = false;
			conveyorBeltModifiers = null;
			walledDebuff = false;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc?.ModNPC is Goo_Wall gooWall && gooWall.InsideWall(Player)) {
					walledDebuff = true;
					break;
				}
			}
			medicinalAcid = false;
			murkySludge = false;
			miasma = false;
			tetanus = false;
			if (dangerTime <= 0) dangerTime = 0;
			else dangerTime--;
			InDanger = dangerTime > 0;
			if (scytheHitCombo > maxScytheCombo) scytheHitCombo = maxScytheCombo;
			if (!InDanger) scytheHitCombo = 0;
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
		public void UnlockJournalEntry(string entryNames) {
			if (!journalUnlocked) return;
			bool playSound = false;
			foreach (string entryName in entryNames.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) {
				if (Player.whoAmI == Main.myPlayer && unlockedJournalEntries.Add(entryName)) {
					unreadJournalEntries.Add(entryName);
					playSound = true;
				}
			}
			if (playSound) SoundEngine.PlaySound(Origins.Sounds.Journal);
		}
		bool necromanaUsedThisUse = false;
		public override void OnConsumeMana(Item item, int manaConsumed) {
			if (!necromanaUsedThisUse && (necroSet || necroSet2 || item.CountsAsClass(DamageClass.Summon)) && necromancyPrefixMana > 0) {
				int restoreMana = (int)Math.Min(necromancyPrefixMana, manaConsumed);
				necromancyPrefixMana -= restoreMana;
				Player.statMana += restoreMana;
				necromanaUsedThisUse = true;
			}
		}
		public override void OnMissingMana(Item item, int neededMana) {
			if (!necromanaUsedThisUse && (necroSet || necroSet2 || item.CountsAsClass(DamageClass.Summon)) && Player.statMana + necromancyPrefixMana >= neededMana) {
				int restoreMana = neededMana - Player.statMana;
				necromancyPrefixMana -= restoreMana;
				Player.statMana += restoreMana;
				necromanaUsedThisUse = true;
			}
		}
		internal int GetNewMinionIndexByType(int type) => minionCountByType[type]++;
	}
}
