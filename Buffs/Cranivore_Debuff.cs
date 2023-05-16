using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Cranivore_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Darkness;
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cranivore");
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.blind = true;
			player.lifeRegen -= 16;
		}
	}
}
