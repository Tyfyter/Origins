using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Sapphire_Aura_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Extra_194";
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			ID = Type;
			Main.buffNoTimeDisplay[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {// buffs go here
			
		}
	}
}
