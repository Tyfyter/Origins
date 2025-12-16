using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins {
	public partial class Origins : Mod {
		public static class Music {
			public static int Fiberglass = MusicID.OtherworldlyJungle;

			public static int BrinePool = MusicID.OtherworldlyEerie;
			public static int LostDiver = MusicID.OtherworldlyInvasion;
			public static int MildewCarrion = MusicID.OtherworldlyPlantera;
			public static int CrownJewel = MusicID.OtherworldlySpace;
			public static int AncientBrinePool;

			public static int ShimmerConstruct = MusicID.OtherworldlyBoss2;
			public static int ShimmerConstructPhase3 = 0;

			public static int Dusk;

			public static int Defiled = MusicID.OtherworldlyMushrooms;
			public static int OtherworldlyDefiled = MusicID.OtherworldlyCorruption;
			public static int UndergroundDefiled = MusicID.OtherworldlyUGHallow;
			public static int DefiledBoss = MusicID.OtherworldlyBoss1;
			public static int AncientDefiled;

			public static int Riven = MusicID.OtherworldlyCrimson;
			public static int UndergroundRiven = MusicID.OtherworldlyIce;
			public static int RivenBoss = MusicID.Boss5;
			public static int RivenOcean = MusicID.OtherworldlyRain;
			public static int AncientRiven;

			public static int AshenFactory = MusicID.OtherworldlyNight;
			public static int AshenMines = MusicID.OtherworldlyNight;
			public static int AshenScrapyard = MusicID.OtherworldlyNight;
			public static int SmogStorm = MusicID.Sandstorm;
			public static int AshenBoss = MusicID.Boss2;
			public static int AncientAshen;

			public static int TheDive;
		}
		public static class Sounds {
			public static SoundStyle MuffledHitMale = new("Origins/Sounds/Custom/ModifiedPlayer/Male_Hit_Mask", 3, SoundType.Sound);
			public static SoundStyle MuffledHitFemale = new("Origins/Sounds/Custom/ModifiedPlayer/Male_Hit_Mask", 3, SoundType.Sound);
			// can we really muffle these?
			//public static SoundStyle MuffledHitDSTMale = new("Origins/Sounds/Custom/ModifiedPlayer/DST_Male_Hit_Mask", 3, SoundType.Sound);
			//public static SoundStyle MuffledHitDSTFemale = new("Origins/Sounds/Custom/ModifiedPlayer/DST_Male_Hit_Mask", 3, SoundType.Sound);

			public static SoundStyle MultiWhip = SoundID.Item153;
			public static SoundStyle Krunch = SoundID.Item36;
			public static SoundStyle HeavyCannon = SoundID.Item36;
			public static SoundStyle EnergyRipple = SoundID.Item8;
			public static SoundStyle PhaserCrash = SoundID.Item12;
			public static SoundStyle DeepBoom = SoundID.Item14;
			public static SoundStyle DefiledIdle = SoundID.Zombie1;
			public static SoundStyle DefiledHurt = SoundID.DD2_SkeletonHurt;
			public static SoundStyle DefiledKill => OriginsModIntegrations.CheckAprilFools() ? defiledKillAF : defiledKill;
			public static SoundStyle defiledKill = SoundID.NPCDeath1;
			public static SoundStyle defiledKillAF = SoundID.NPCDeath1;
			public static SoundStyle Amalgamation = SoundID.Zombie1;
			public static SoundStyle BeckoningRoar = SoundID.ForceRoar;
			public static SoundStyle PowerUp = SoundID.Item4;
			public static SoundStyle RivenBass = SoundID.Item4;
			public static SoundStyle ShrapnelFest = SoundID.Item144;
			public static SoundStyle IMustScream = SoundID.Roar;
			public static SoundStyle ShinedownLoop = SoundID.ForceRoar;
			public static SoundStyle WCHit = SoundID.ForceRoar;
			public static SoundStyle WCIdle = SoundID.ForceRoar;
			public static SoundStyle WCScream = SoundID.ForceRoar;

			public static SoundStyle Lightning = SoundID.Roar;
			public static SoundStyle LightningCharging = SoundID.Roar;
			public static SoundStyle LightningChargingSoft = SoundID.Roar;
			public static SoundStyle[] LightningSounds = [];
			public static SoundStyle LittleZap = SoundID.Roar;

			public static SoundStyle ShimmershotCharging = new("Origins/Sounds/Custom/SoftCharge", SoundType.Sound) {
				IsLooped = true
			};
			public static SoundStyle HawkenThruster = new("Origins/Sounds/Custom/HawkenThrusterDistant", SoundType.Sound) {
				IsLooped = true
			};
			public static SoundStyle ShimmerConstructAmbienceIntro = new("Origins/Sounds/Custom/Ambience/SCP3_Ambience_Start", SoundType.Ambient);
			public static SoundStyle ShimmerConstructAmbienceLoop = new("Origins/Sounds/Custom/Ambience/SCP3_Ambience_Mid", SoundType.Ambient);
			public static SoundStyle ShimmerConstructAmbienceOutro = new("Origins/Sounds/Custom/Ambience/SCP3_Ambience_End", SoundType.Ambient);

			public static SoundStyle Bonk = SoundID.Roar;
			public static SoundStyle BikeHorn = new("Origins/Sounds/Custom/BikeHorn", SoundType.Sound) {
				PitchVariance = 0.5f,
				MaxInstances = 0
			};
			public static SoundStyle BoneBreakBySoundEffectsFactory = new("Origins/Sounds/Custom/Bone_Break_By_SoundEffectsFactory", SoundType.Sound) {
				PitchVariance = 0.5f,
				MaxInstances = 0
			};
			public static SoundStyle BonkByMrSoundEffect = new("Origins/Sounds/Custom/Bonk", SoundType.Sound) {
				PitchVariance = 0.5f,
				MaxInstances = 0
			};
			public static SoundStyle Journal = SoundID.Roar;
			public static void Unload() {
				LightningSounds = null;
			}
			public static class LaserTag {
				public static SoundStyle Hurt = SoundID.DSTMaleHurt;
				public static SoundStyle Death = SoundID.DD2_KoboldDeath;
				public static SoundStyle Score = SoundID.DrumTamaSnare;
			}
		}
		public void AudioLoad() {
			Sounds.MultiWhip = new SoundStyle("Terraria/Sounds/Item_153", SoundType.Sound) {
				MaxInstances = 0,
				SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
				PitchVariance = 0f
			};
			Sounds.Krunch = new SoundStyle("Origins/Sounds/Custom/BurstCannon", SoundType.Sound);
			Sounds.HeavyCannon = new SoundStyle("Origins/Sounds/Custom/HeavyCannon", SoundType.Sound);
			Sounds.PowerUp = new SoundStyle("Origins/Sounds/Custom/PowerUp", SoundType.Sound);
			Sounds.EnergyRipple = new SoundStyle("Origins/Sounds/Custom/EnergyRipple", SoundType.Sound);
			Sounds.PhaserCrash = new SoundStyle("Origins/Sounds/Custom/PhaserCrash", SoundType.Sound);
			Sounds.DeepBoom = new SoundStyle("Origins/Sounds/Custom/DeepBoom", SoundType.Sound) {
				MaxInstances = 0
			};
			Sounds.DefiledIdle = new SoundStyle("Origins/Sounds/Custom/Defiled_Idle", new int[] { 2, 3 }, SoundType.Sound) {
				MaxInstances = 0,
				Volume = 0.4f,
				PitchRange = (0.9f, 1.1f)
			};
			Sounds.DefiledHurt = new SoundStyle("Origins/Sounds/Custom/Defiled_Hurt", new int[] { 1, 2 }, SoundType.Sound) {
				MaxInstances = 0,
				Volume = 0.4f,
				PitchRange = (0.9f, 1.1f)
			};
			Sounds.defiledKill = new SoundStyle("Origins/Sounds/Custom/Defiled_Kill1", SoundType.Sound) {
				MaxInstances = 0,
				Volume = 0.4f,
				PitchRange = (0.9f, 1.1f)
			};
			Sounds.defiledKillAF = new SoundStyle("Origins/Sounds/Custom/BlockusLand", SoundType.Sound) {
				MaxInstances = 0,
				Volume = 1.0f,
				PitchRange = (0.9f, 1.1f)
			};
			Sounds.Amalgamation = new SoundStyle("Origins/Sounds/Custom/ChorusRoar", SoundType.Sound) {
				MaxInstances = 0,
				Volume = 0.5f,
				PitchRange = (0.04f, 0.15f)
			};
			Sounds.BeckoningRoar = new SoundStyle("Origins/Sounds/Custom/BeckoningRoar", SoundType.Sound) {
				MaxInstances = 0,
				//Volume = 0.75f,
				PitchRange = (0.2f, 0.3f)
			};
			Sounds.IMustScream = new SoundStyle("Origins/Sounds/Custom/IMustScream", SoundType.Sound) {
				MaxInstances = 0,
				PitchRange = (0.2f, 0.3f)
			};
			Sounds.RivenBass = new SoundStyle("Origins/Sounds/Custom/RivenBass", SoundType.Sound) {
				MaxInstances = 0
			};
			Sounds.ShrapnelFest = new SoundStyle("Origins/Sounds/Custom/ShrapnelFest", SoundType.Sound) {
				MaxInstances = 5,
				SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
				Volume = 0.75f
			};
			Sounds.LaserTag.Hurt = new SoundStyle("Origins/Sounds/Custom/LaserTag/Hurt", SoundType.Sound) {
				MaxInstances = 0
			};
			Sounds.LaserTag.Death = new SoundStyle("Origins/Sounds/Custom/LaserTag/Death", SoundType.Sound) {
				MaxInstances = 0
			};
			Sounds.LaserTag.Score = new SoundStyle("Origins/Sounds/Custom/LaserTag/Score", SoundType.Sound) {
				MaxInstances = 0
			};
			Sounds.Lightning = new SoundStyle("Origins/Sounds/Custom/ThunderShot", SoundType.Sound) {
				MaxInstances = 0
			}.WithPitchRange(-0.1f, 0.1f).WithVolume(0.75f);
			Sounds.LightningSounds = [
				Sounds.Lightning,
				new SoundStyle($"Terraria/Sounds/Thunder_0", SoundType.Sound).WithPitchRange(-0.1f, 0.1f).WithVolume(0.75f)
			];
			Sounds.LightningCharging = new SoundStyle("Origins/Sounds/Custom/ChargedUp", SoundType.Sound) {
				MaxInstances = 0,
				IsLooped = true
			};
			Sounds.LightningChargingSoft = new SoundStyle("Origins/Sounds/Custom/ChargedUpSmol", SoundType.Sound) {
				MaxInstances = 0,
				IsLooped = true
			};
			Sounds.LittleZap = new SoundStyle("Origins/Sounds/Custom/ChainZap", SoundType.Sound) {
				MaxInstances = 5,
				SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
				PitchVariance = 0.4f
			};
			Sounds.Bonk = new SoundStyle("Origins/Sounds/Custom/GrandSlam", SoundType.Sound) {
				MaxInstances = 0,
				PitchVariance = 0.4f
			};
			Sounds.HawkenThruster = new SoundStyle("Origins/Sounds/Custom/HawkenThrusterDistant", SoundType.Sound) {
				MaxInstances = 0
			};
			Sounds.Journal = new SoundStyle("Origins/Sounds/Custom/WritingFire", SoundType.Sound) {
				MaxInstances = 1,
				SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				PitchVariance = 0.4f
			};
			Sounds.ShinedownLoop = new SoundStyle("Origins/Sounds/Custom/BeckoningRoar", SoundType.Sound) {
				MaxInstances = 0,
				IsLooped = true,
				//Volume = 0.75f,
				PitchRange = (0.2f, 0.3f)
			};
			Sounds.WCHit = new SoundStyle("Origins/Sounds/Custom/WCHit", SoundType.Sound) {
				PitchVariance = 0.3f
			};
			Sounds.WCIdle = new SoundStyle("Origins/Sounds/Custom/WCIdle", SoundType.Sound) {
				PitchVariance = 0.3f
			};
			Sounds.WCScream = new SoundStyle("Origins/Sounds/Custom/WCScream", SoundType.Sound) {
				PitchVariance = 0.3f
			};
		}
	}
}
