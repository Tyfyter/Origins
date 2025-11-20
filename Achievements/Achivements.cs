using Origins.NPCs.Ashen.Boss;
using Origins.NPCs.Brine.Boss;
using Origins.NPCs.Defiled.Boss;
using Origins.NPCs.Fiberglass;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.NPCs.Riven.World_Cracker;
using PegasusLib.Networking;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace Origins.Achievements {
	public class Killimanjaro : SlayerAchivement<Defiled_Amalgamation> {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public override Position GetDefaultPosition() => new After("MASTERMIND");
	}
	public class Kaiju : SlayerAchivement<World_Cracker_Head> {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public override Position GetDefaultPosition() => new After("MASTERMIND");
		public override IEnumerable<Position> GetModdedConstraints() => [new After(ModContent.GetInstance<Killimanjaro>())];
	}
	public class Kill_Trenchmaker : SlayerAchivement<Trenchmaker> {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public override Position GetDefaultPosition() => new After("MASTERMIND");
		public override IEnumerable<Position> GetModdedConstraints() => [new After(ModContent.GetInstance<Kaiju>())];
	}
	public class It_Was_Watching : SlayerAchivement<Fiberglass_Weaver> {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public override Position GetDefaultPosition() => new After("BONED");
	}
	public class An_Eye_For_An_Eye : SlayerAchivement<Shimmer_Construct> {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public override Position GetDefaultPosition() => new Before("STILL_HUNGRY");
	}
	public class Kill_Lost_Diver : SlayerAchivement<Mildew_Carrion> {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public override Position GetDefaultPosition() => new After("STILL_HUNGRY");
	}
	public class Enough_Yap : ModAchievement {
		public override string TextureName => "Origins/Achievements/Template"; // temp, remove when has sprite
		public CustomFlagCondition Condition { get; private set; }
		public override void SetStaticDefaults() {
			Achievement.SetCategory(AchievementCategory.Challenger);
			Condition = AddCondition();
		}
	}
}
