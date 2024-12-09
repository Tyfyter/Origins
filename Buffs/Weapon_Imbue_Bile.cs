using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Weapon_Imbue_Bile : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			BuffID.Sets.IsAFlaskBuff[Type] = true;
			Main.meleeBuff[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<OriginPlayer>().flaskBile = true;
		}
	}
}
