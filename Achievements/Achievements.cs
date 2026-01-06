using Newtonsoft.Json.Linq;
using Origins.Gores.NPCs;
using Origins.NPCs.Ashen.Boss;
using Origins.NPCs.Brine.Boss;
using Origins.NPCs.Defiled.Boss;
using Origins.NPCs.Fiberglass;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.NPCs.Riven.World_Cracker;
using Origins.Questing;
using System;
using System.Collections.Generic;
using Terraria;
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
		public class Tracker {
			readonly bool[,] coverage = new bool[Player.defaultWidth / 2, Player.defaultHeight / 2];
			public void Update() {
				RIVEN_WORMS instance = ModContent.GetInstance<RIVEN_WORMS>();
				if (instance.Achievement.IsCompleted) return;
				Array.Clear(coverage);
				for (int i = 0; i < Riven_Blood_Coating.activeDusts.Count; i++) {
					if (Main.dust[Riven_Blood_Coating.activeDusts[i]].customData is not (Player player, Vector2 playerOffset) || player.whoAmI != Main.myPlayer) continue;
					playerOffset *= new Vector2(player.direction, 1);
					playerOffset.X += Player.defaultWidth * 0.5f;
					playerOffset.Y += Player.defaultHeight * 0.5f;
					playerOffset /= 2;
					for (int x = -2; x < 3; x++) {
						int xIndex = (int)playerOffset.X + x;
						if (xIndex < 0 || xIndex >= coverage.GetLength(0)) continue;
						for (int y = -2; y < 3; y++) {
							int yIndex = (int)playerOffset.Y + y;
							if (yIndex < 0 || yIndex >= coverage.GetLength(1)) continue;
							coverage[xIndex, yIndex] = true;
						}
					}
				}
				float coveragePercent = 0;
				foreach (bool item in coverage) {
					if (item) coveragePercent++;
				}
				coveragePercent /= coverage.Length;
				if (coveragePercent >= 0.8f) instance.Condition.Complete();
			}
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
