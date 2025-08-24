using MonoMod.Cil;
using Origins.NPCs;
using PegasusLib.Reflection;
using PegasusLib.UI;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Toxic_Shock_Debuff : ModBuff {
		public const int stun_duration = 30;
		public const int default_duration = 60;
		public LocalizedText StunDescription;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			StunDescription = this.GetLocalization(nameof(StunDescription));
			Buff_Hint_Handler.ModifyTip(Type, 7.5f, this.GetLocalization("EffectDescription").Key);
			Buff_Hint_Handler.CombineBuffHintModifiers(Type, modifyBuffTip: (lines, _, player) => {
				if (!player) lines.Add(StunDescription.Value);
			});
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<OriginPlayer>().toxicShock = true;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			if (!npc.buffImmune[BuffID.Confused] && Main.rand.NextBool(400)) {// roughly 15% chance each second
				npc.GetGlobalNPC<OriginGlobalNPC>().toxicShockStunTime = Toxic_Shock_Debuff.stun_duration;
			}
		}
		internal static void IL_Player_CheckDrowning(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.After,
				i => i.MatchAdd(),
				i => i.MatchStfld<Player>(nameof(Player.breath))
			);
			c.Index--;
			MonoModMethods.SkipPrevArgument(c);
			c.Index--;
			c.MoveAfterLabels();
			if (c.Prev.Operand is ILLabel label) {
				c.EmitLdarg0();
				c.EmitDelegate((Player player) => player.OriginPlayer().toxicShock);
				c.EmitBrtrue(label);
			} else {
				Origins.LogLoadingWarning(Language.GetOrRegister("Mods.Origins.Warnings.ToxicShockILEditFail"));
			}
		}
	}
	public class Toxic_Shock_Strengthen_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
			Main.buffNoTimeDisplay[Type] = true;
			BuffID.Sets.GrantImmunityWith[Type] = new() {
				Toxic_Shock_Debuff.ID
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
