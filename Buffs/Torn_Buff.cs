using Origins.NPCs;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
    public class Torn_Buff : ModBuff {
		public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Torn");
            Description.SetDefault("Your max life has been reduced!");
            Main.debuff[Type] = true;
            ID = Type;
        }
		public override void Update(Player player, ref int buffIndex) {
            player.GetModPlayer<OriginPlayer>().tornDebuff = true;
		}
		public override void Update(NPC npc, ref int buffIndex) {
            npc.GetGlobalNPC<OriginGlobalNPC>().tornDebuff = true;
        }
	}
}
