using Origins.Dev;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Flagellash_Buff_0 : ModBuff, ICustomWikiStat {
		public bool CanExportStats => false;
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
	}
	public class Flagellash_Buff_1 : Flagellash_Buff_0 {
		public static new int ID { get; private set; }
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
	}
	public class Flagellash_Buff_2 : Flagellash_Buff_0 {
		public static new int ID { get; private set; }
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
	}
}
