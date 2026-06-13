using Origins.NPCs;
using PegasusLib.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Chilled_Debuff : ModBuff {
		public static int FreezeThreshold => 240;
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Chilled;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.GrantImmunityWith[Type] = [
				BuffID.Slow,
				BuffID.Chilled
			];
			Buff_Hint_Handler.ModifyTip(Type, 0, "BuffDescription.Slow");
			ID = Type;
		}
		public override bool ReApply(NPC npc, int time, int buffIndex) {
			npc.buffTime[buffIndex] += time;
			return true;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			if (npc.buffTime[buffIndex] >= FreezeThreshold) {
				if (npc.HasBuff(Frozen_Debuff.ID)) {
					Min(ref npc.buffTime[buffIndex], FreezeThreshold - 1);
				} else {
					npc.buffType[buffIndex] = Frozen_Debuff.ID;
					npc.buffTime[buffIndex] = FreezeThreshold;
				}
			}
		}
		public static float CalculateSlow(int buffTime) => Utils.Remap(buffTime, 0, FreezeThreshold - 1, 1, 0.3f);
	}
	public class Frozen_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Frozen;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.GrantImmunityWith[Type] = [
				BuffID.Chilled,
				ModContent.BuffType<Chilled_Debuff>(),
				BuffID.Frozen,
			];
			Buff_Hint_Handler.ModifyTip(Type, 0, "BuffDescription.Slow");
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			Min(ref npc.buffTime[buffIndex], Chilled_Debuff.FreezeThreshold);
			npc.GetGlobalNPC<OriginGlobalNPC>().frozenDebuff = true;
		}
	}
}
