using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
    public class Weapon_Imbue_Bile : ModBuff {
        public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Weapon Imbue Bile");
            ID = Type;
        }
        public override void Update(Player player, ref int buffIndex) {
            player.GetModPlayer<OriginPlayer>().bileFlask = true;
        }
    }
}
