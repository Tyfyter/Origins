using Origins.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Empowerments;

namespace Origins.Buffs {
	public class Toxic_Shock_Debuff : ModBuff {
		public const int stun_duration = 4;
		public const int default_duration = 60;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<OriginPlayer>().toxicShock = true;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.lifeRegen -= 15;
			npc.defense -= (int)(npc.defense * 0.2f);
			if (!npc.buffImmune[BuffID.Confused] && Main.rand.NextBool(400)) {// roughly 15% chance each second
				npc.GetGlobalNPC<OriginGlobalNPC>().toxicShockStunTime = Toxic_Shock_Debuff.stun_duration;
			}
		}
	}
	public class Toxic_Shock_Strengthen_Debuff : ModBuff {
		public static int ID { get; private set; }
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
			if (Main.rand.NextBool(1200)) {// roughly 5% chance each second
				npc.GetGlobalNPC<OriginGlobalNPC>().toxicShockStunTime = Toxic_Shock_Debuff.stun_duration;
			}
		}
	}
}
