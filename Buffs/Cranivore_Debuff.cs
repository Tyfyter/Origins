using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
    public class Cranivore_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			AssimilationLoader.AddDebuffAssimilation<Corrupt_Assimilation>(Type, 0.012f / 60);
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.blind = true;
			player.lifeRegen -= 16;
			//player.GetAssimilation<Corrupt_Assimilation>().Percent += 0.0002f;
		}
	}
}
