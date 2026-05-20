using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Greater_Summoning_Buff : ModBuff, ICustomWikiStat {
		public static int ID { get; private set; }
		public string[] Categories => [
			"SummonBoostBuff"
		];
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.maxMinions += 2;
		}
		public override void Load() {
			On_Player.QuickBuff_ShouldBotherUsingThisBuff += On_Player_QuickBuff_ShouldBotherUsingThisBuff;
			On_Player.AddBuff_RemoveOldMeleeBuffsOfMatchingType += On_Player_AddBuff_RemoveOldMeleeBuffsOfMatchingType;
		}
		static bool On_Player_QuickBuff_ShouldBotherUsingThisBuff(On_Player.orig_QuickBuff_ShouldBotherUsingThisBuff orig, Player self, int attemptedType) {
			if (!orig(self, attemptedType)) return false;
			if (attemptedType != BuffID.Summoning) return true;
			for (int i = 0; i < Player.MaxBuffs; i++) {
				if (self.buffType[i] == ID) return false;
			}
			return true;
		}
		static void On_Player_AddBuff_RemoveOldMeleeBuffsOfMatchingType(On_Player.orig_AddBuff_RemoveOldMeleeBuffsOfMatchingType orig, Player self, int type) {
			orig(self, type);
			if (type == BuffID.Summoning || type == ID) {
				for (int i = self.buffType.Length - 1; i >= 0; i--) {
					if (self.buffType[i] != type && (self.buffType[i] == BuffID.Summoning || self.buffType[i] == ID)) {
						self.DelBuff(i);
						break;
					}
				}
			}
		}
	}
}
