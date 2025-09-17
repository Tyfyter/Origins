using Origins.NPCs;
using PegasusLib.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Silenced_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Silenced;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.GrantImmunityWith[Type] = [
				BuffID.Silenced
			];
			Buff_Hint_Handler.ModifyTip(Type, 0, this.GetLocalization("EffectDescription").Key);
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().silencedDebuff = true;
			if (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type]) npc.buffTime[buffIndex] -= 9;
		}
	}
}
