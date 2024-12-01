using Origins.Dev;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Maelstrom_Buff_Damage : ModBuff, ICustomWikiStat {
		public bool CanExportStats => false;
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
	}
	public class Maelstrom_Buff_Zap : Maelstrom_Buff_Damage {
		public static new int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
	}
}
