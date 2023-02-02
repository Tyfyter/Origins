using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
    public class Purification_Buff : ModBuff {
		public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Willed");
            Description.SetDefault("The evils of the world cannot manipulate you");
            ID = Type;
        }
		public override void Update(Player player, ref int buffIndex) {
            //player-immune: assimilation
        }
	}
}
