using Origins.NPCs;
using Origins.NPCs.Defiled;
using PegasusLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Electrified_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Electrified;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.GrantImmunityWith[Type] = [
				BuffID.Electrified
			];
			Buff_Hint_Handler.ModifyTip(Type, 20, this.GetLocalization("EffectDescription").Key);
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().electrified = true;
			int zappiness = 1 + npc.wet.ToInt() + (npc.ModNPC is IDefiledEnemy).ToInt();
			if (Main.rand.Next(4) < zappiness) {
				Vector2 offset = Main.rand.NextVector2FromRectangle(npc.Hitbox) - npc.Center;
				Dust dust = Dust.NewDustPerfect(
					npc.Center - offset,
					DustID.Electric,
					Vector2.Zero,
					Scale: 0.5f
				);
				dust.velocity += npc.velocity;
				dust.noGravity = true;
			}
		}
	}
}
