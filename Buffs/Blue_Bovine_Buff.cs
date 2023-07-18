using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Blue_Bovine_Buff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Bovine Flight");
			// Description.SetDefault("Blue Bovine allowed flight and slow fall");
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			if (player.wings == 0) {
				player.wings = 10;
				if (player.cWings == 0) {
					player.cWings = 36;
				}
			}
			if (player.wingsLogic == 0 || player.wingTimeMax <= 15) {
				player.wingsLogic = 13;
				player.wingTimeMax = 15;
				player.wingTimeMax += 30;
			}
			player.noFallDmg = true;
		}
	}
}
