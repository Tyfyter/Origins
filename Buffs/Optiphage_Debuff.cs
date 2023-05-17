using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
    public class Optiphage_Debuff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Optiphage");
			Description.SetDefault("It's on your eyeballs!");
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.blind = true;
			player.lifeRegen -= 9;
		}
	}
}
