using Newtonsoft.Json.Linq;
using Origins.NPCs.Ashen.Boss;
using Origins.NPCs.Brine.Boss;
using Origins.NPCs.Defiled.Boss;
using Origins.NPCs.Fiberglass;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.NPCs.Riven.World_Cracker;
using Origins.Questing;
using System.Collections.Generic;
using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace Origins.Achievements {
	public class Killimanjaro : SlayerAchievement<Defiled_Amalgamation> {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public override Position GetDefaultPosition() => new After("MASTERMIND");
	}
	public class Kaiju : SlayerAchievement<World_Cracker_Head> {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public override Position GetDefaultPosition() => new After("MASTERMIND");
		public override IEnumerable<Position> GetModdedConstraints() => [new After(ModContent.GetInstance<Killimanjaro>())];
	}
	public class Kill_Trenchmaker : SlayerAchievement<Trenchmaker> {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public override Position GetDefaultPosition() => new After("MASTERMIND");
		public override IEnumerable<Position> GetModdedConstraints() => [new After(ModContent.GetInstance<Kaiju>())];
	}
	public class It_Was_Watching : SlayerAchievement<Fiberglass_Weaver> {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public override Position GetDefaultPosition() => new After("BONED");
	}
	public class An_Eye_For_An_Eye : SlayerAchievement<Shimmer_Construct> {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public override Position GetDefaultPosition() => new Before("STILL_HUNGRY");
	}
	public class Kill_Lost_Diver : SlayerAchievement<Mildew_Carrion> {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public override Position GetDefaultPosition() => new After("ITS_HARD");
	}
	public class Enough_Yap : ModAchievement {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public CustomFlagCondition Condition { get; private set; }
		public override bool Hidden => !ModContent.GetInstance<Killimanjaro>().Achievement.IsCompleted;
		public override void SetStaticDefaults() {
			Achievement.SetCategory(AchievementCategory.Challenger);
			Condition = AddCondition();
		}
	}
	public class Mildew_Death : ModAchievement {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public CustomFlagCondition Condition { get; private set; }
		public override void SetStaticDefaults() {
			Achievement.SetCategory(AchievementCategory.Challenger);
			Condition = AddCondition();
		}
	}
	public class Peat_Obsession : ModAchievement {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public CustomFlagCondition Condition { get; private set; }
		public override void SetStaticDefaults() {
			Achievement.SetCategory(AchievementCategory.Challenger);
			Condition = AddCondition();
		}
	}
	public class Cheat_Code : ModAchievement {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public CustomFlagCondition Condition { get; private set; }
		public override void SetStaticDefaults() {
			Achievement.SetCategory(AchievementCategory.Challenger);
			Condition = AddCondition();
		}
	}
	public class Cloning_Factory : ModAchievement {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public CustomIntCondition Condition { get; private set; }
		public override void SetStaticDefaults() {
			Achievement.SetCategory(AchievementCategory.Challenger);
			Condition = AddCondition(CustomIntCondition.AddIntCondition(500));
		}
	}
	public class Whimsical : ModAchievement {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public CustomFlagCondition Condition { get; private set; }
		public override void SetStaticDefaults() {
			Achievement.SetCategory(AchievementCategory.Challenger);
			Condition = AddCondition();
		}
	}
	public class Different_Game : ModAchievement {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public CustomFlagCondition Condition { get; private set; }
		public override void SetStaticDefaults() {
			Achievement.SetCategory(AchievementCategory.Challenger);
			Condition = AddCondition();
		}
	}
	public class Auto_Immune_Disease : ModAchievement {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public CustomIntCondition Condition { get; private set; }
		public override void SetStaticDefaults() {
			Achievement.SetCategory(AchievementCategory.Collector);
			Condition = AddCondition(CustomIntCondition.AddIntCondition(20));
		}
	}
	public class Whole_Blend_Evil : ModAchievement {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public CustomIntCondition Condition { get; private set; }
		public override void SetStaticDefaults() {
			Achievement.SetCategory(AchievementCategory.Collector);
			Condition = AddCondition(CustomIntCondition.AddIntCondition(29));
		}
	}
	public class Going_Places : ModAchievement {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public CustomIntCondition Condition { get; private set; }
		public override void SetStaticDefaults() {
			Achievement.SetCategory(AchievementCategory.Collector);
			Condition = AddCondition(CustomIntCondition.AddIntCondition(3));
		}
	}
	public class True_Hero : ModAchievement {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public CustomIntCondition Condition { get; private set; }
		public override void SetStaticDefaults() {
			Achievement.SetCategory(AchievementCategory.Challenger);
			Condition = AddCondition(CustomIntCondition.AddIntCondition(Quest_Registry.Quests.Count));
		}
	}
	public class RIVEN_WORMS : ModAchievement {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public CustomFlagCondition Condition { get; private set; }
		public override void SetStaticDefaults() {
			Achievement.SetCategory(AchievementCategory.Challenger);
			Condition = AddCondition();
		}
	}
	public class Slow_Loading_Bar : ModAchievement {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public CustomIntCondition Condition { get; private set; }
		public override void SetStaticDefaults() {
			Achievement.SetCategory(AchievementCategory.Challenger);
			Condition = AddCondition(CustomIntCondition.AddIntCondition(1000));
		}
	}
	public class Cracked : ModAchievement {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public CustomFlagCondition Condition { get; private set; }
		public override bool Hidden => !ModContent.GetInstance<Kaiju>().Achievement.IsCompleted;
		public override void SetStaticDefaults() {
			Achievement.SetCategory(AchievementCategory.Challenger);
			Condition = AddCondition();
		}
	}
	public class Martyrdom : ModAchievement {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public CustomFlagCondition Condition { get; private set; }
		public override void SetStaticDefaults() {
			Achievement.SetCategory(AchievementCategory.Challenger);
			Condition = AddCondition();
		}
	}
	public class CustomIntCondition(string name, int maxValue) : Terraria.GameContent.Achievements.CustomIntCondition(name, maxValue) {
		/// <inheritdoc cref="Terraria.GameContent.Achievements.CustomIntCondition"/>
		public static CustomIntCondition AddIntCondition(int maxValue) => new("Condition", maxValue);

		/// <inheritdoc cref="Terraria.GameContent.Achievements.CustomIntCondition"/>
		public static CustomIntCondition AddIntCondition(string key, int maxValue) => new(key, maxValue);
		public override void Load(JObject state) {
			if (state["Value"] is null) state["Value"] = 0;
			base.Load(state);
		}
	}
}
