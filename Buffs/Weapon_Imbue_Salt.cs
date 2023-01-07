using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
    public class Weapon_Imbue_Salt : ModBuff {
		public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Weapon Imbue: Alkahest");
            BuffID.Sets.IsAFlaskBuff[Type] = true;
            Main.meleeBuff[Type] = true;
            ID = Type;
        }
    }
}
