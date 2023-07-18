using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Safe_Buff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Safe From Yourself");
			// Description.SetDefault("Impervious to self-inflicted explosive damage");
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage = new StatModifier(0, 0);
		}
	}
}
