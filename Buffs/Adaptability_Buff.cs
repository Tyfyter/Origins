using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
    public class Adaptability_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Adaptability");
            Description.SetDefault("10% class stat share");
            ID = Type;
        }
		public override void Update(Player player, ref int buffIndex) {
            player.GetModPlayer<OriginPlayer>().statSharePercent += 0.5f;
		}
	}
}
