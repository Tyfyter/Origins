using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Voidsight_Buff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			if (player.OriginPlayer().chambersiteCommandoSet) player.buffTime[buffIndex] = 18000;
			player.nightVision = true;
			Lighting.AddLight(player.Center, 2.7f, 1.5f, 3f);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams) {
			Main.buffNoTimeDisplay[Type] = Main.LocalPlayer.OriginPlayer().chambersiteCommandoSet;
			return true;
		}
	}
}
