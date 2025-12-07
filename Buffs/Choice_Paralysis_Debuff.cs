using PegasusLib.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Choice_Paralysis_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Confused;
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.GrantImmunityWith[Type] = [
				BuffID.Confused
			];
			Buff_Hint_Handler.ModifyTip(Type, 0, this.GetLocalization("EffectDescription").Key);
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.confused = player.buffTime[buffIndex] % 2 == 0;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.confused = npc.buffTime[buffIndex] % 2 == 0;
		}
	}
}
