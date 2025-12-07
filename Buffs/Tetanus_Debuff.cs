using Origins.NPCs;
using PegasusLib.UI;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Tetanus_Debuff : ModBuff {
		public static int DPS => 40;
		public static int ID { get; private set; }
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Venom;
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.GrantImmunityWith[Type] = [
				BuffID.Venom
			];
			Buff_Hint_Handler.ModifyTip(Type, DPS);
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.OriginPlayer().tetanus = true;
			DoBubbles(player);
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().tetanus = true;
			DoBubbles(npc);
		}
		public static void DoBubbles(Entity entity) {
			if (!Main.rand.NextBool(8)) return;
			Dust dust = Dust.NewDustDirect(new Vector2(entity.position.X - 2f, entity.position.Y - 2f), entity.width + 4, entity.height + 4, DustID.MinecartSpark, entity.velocity.X * 0.3f, entity.velocity.Y * 0.3f, 100, default, 1.75f);
			dust.noGravity = true;
			dust.velocity *= 0.75f;
			dust.velocity.X *= 0.75f;
			dust.velocity.Y -= 1f;
			if (Main.rand.NextBool(4)) {
				dust.noGravity = false;
				dust.scale *= 0.5f;
			}
		}
	}
}
