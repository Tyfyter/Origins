using Origins.NPCs;
using PegasusLib.UI;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Cavitation_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Buff_Hint_Handler.ModifyTip(Type, 33, this.GetLocalization("EffectDescription").Key);
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.OriginPlayer().cavitationDebuff = true;
			DoBubbles(player);
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().cavitationDebuff = true;
			DoBubbles(npc);
		}
		public static void DoBubbles(Entity entity) {
			Dust dust = Dust.NewDustDirect(new Vector2(entity.position.X - 2f, entity.position.Y - 2f), entity.width + 4, entity.height + 4, DustID.BreatheBubble, entity.velocity.X * 0.3f, entity.velocity.Y * 0.3f, 100, default, 1.75f);
			dust.noGravity = true;
			dust.velocity *= 0.75f;
			dust.velocity.X *= 0.75f;
			dust.velocity.Y -= 1f;
			if (Main.rand.NextBool(4)) {
				dust.noGravity = false;
				dust.scale *= 0.5f;
			}
			if (Main.rand.NextBool(4)) {
				SoundEngine.PlaySound(SoundID.Item54.WithPitchRange(-1f, -0.6f).WithVolumeScale(0.5f), entity.Center);
			}
			if (Main.rand.NextBool(8)) {
				SoundEngine.PlaySound(SoundID.Drown.WithPitchRange(0f, 0.6f).WithVolumeScale(0.5f) with { MaxInstances = 0 }, entity.Center);
			}
		}
	}
}
