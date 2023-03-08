using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Headphones_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			BuffID.Sets.IsAnNPCWhipDebuff[Type] = true;
			ID = Type;
		}
	}
}
