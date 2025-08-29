using Origins.Questing;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Hair;
public class QuestUnlockHair<T> : ModHair where T : Quest {
	public override bool AvailableDuringCharacterCreation => false;
	public override IEnumerable<Condition> GetUnlockConditions() {
		yield return Quest.QuestCondition<T>();
	}
}
public class Mantis_Shrimpatouille_Hair : QuestUnlockHair<Comb_Quest> { }
public class Pandora_Hair : QuestUnlockHair<Comb_Quest> { }
public class Bonus_Hair_1 : QuestUnlockHair<Magic_Hair_Spray_Quest> { }
public class Bonus_Hair_2 : QuestUnlockHair<Magic_Hair_Spray_Quest> { }
public class Bonus_Hair_3 : QuestUnlockHair<Magic_Hair_Spray_Quest> { }
public class Bonus_Hair_4 : QuestUnlockHair<Magic_Hair_Spray_Quest> { }
public class Dio_Hair : ModHair, IBrokenContent {
	public override bool AvailableDuringCharacterCreation => true;
	public override Gender RandomizedCharacterCreationGender => Gender.Male;
	public string BrokenReason => "spritesheet issue";
}