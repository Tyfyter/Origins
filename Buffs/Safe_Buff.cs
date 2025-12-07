using Origins.Dev;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Safe_Buff : ModBuff {
		public static int ID { get; private set; }
		public string[] Categories => [
			WikiCategories.ExplosiveBoostBuff
		];
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage = new StatModifier(0, 0);
		}
	}
}
