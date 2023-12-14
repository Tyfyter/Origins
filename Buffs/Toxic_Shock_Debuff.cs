using Origins.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Toxic_Shock_Debuff : ModBuff {
		public const int stun_duration = 4;
		public const int default_duration = 60;
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			ID = Type;
			BuffID.Sets.GrantImmunityWith[Type] = new() {
				BuffID.Confused
			};
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<OriginPlayer>().toxicShock = true;
		}
		public override bool ReApply(NPC npc, int time, int buffIndex) {
			OriginGlobalNPC globalNPC = npc.GetGlobalNPC<OriginGlobalNPC>();
			if (globalNPC.toxicShockTime > Toxic_Shock_Debuff.stun_duration * 3) {
				globalNPC.toxicShockTime = 0;
			}
			return false;
		}
	}
	public class Toxic_Shock_Strengthen_Debuff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			ID = Type;
			Main.buffNoTimeDisplay[Type] = true;
			BuffID.Sets.GrantImmunityWith[Type] = new() {
				ModContent.BuffType<Toxic_Shock_Debuff>()
			};
		}
		public override void Update(Player player, ref int buffIndex) {
			if (player.HasBuff(Toxic_Shock_Debuff.ID)) {
				player.buffTime[buffIndex]++;
			}
		}
		public override void Update(NPC npc, ref int buffIndex) {
			if (npc.HasBuff(Toxic_Shock_Debuff.ID)) {
				npc.buffTime[buffIndex]++;
			}
		}
	}
}
