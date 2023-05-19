using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
    public class Corrupt_Assimilation_Debuff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Corrupt Assimilation");
			Description.SetDefault("You're being assimilated by the Corruption");
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			// custom effects per percent
		}
	}
	public class Crimson_Assimilation_Debuff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crimson Assimilation");
			Description.SetDefault("You're being assimilated by the Crimson");
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			// custom effects per percent
		}
	}
	public class Defiled_Assimilation_Debuff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Defiled Assimilation");
			Description.SetDefault("You're being assimilated by the Defiled");
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			// custom effects per percent
		}
	}
	public class Riven_Assimilation_Debuff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Riven Assimilation");
			Description.SetDefault("You're being assimilated by the Riven");
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			// custom effects per percent
		}
	}
}
