using PegasusLib.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Oiled_Debuff : ModBuff {
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.GrantImmunityWith[Type] = [
				BuffID.Oiled
			];
			Buff_Hint_Handler.ModifyTip(Type, 0, $"Mods.{nameof(PegasusLib)}.BuffTooltip.Oiled");
		}
		public override void Update(Player player, ref int buffIndex) {
			player.OriginPlayer().oiled = true;
			if (player.burned) player.AddBuff(BuffID.OnFire, 60);
		}
	}
}
