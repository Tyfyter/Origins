using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Back_Glow_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return Origins.BackGlowMasks.ContainsKey(drawInfo.drawPlayer.back);
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.BackAcc);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			drawInfo.DrawDataCache.Add(drawInfo.DrawDataCache[^1] with {
				texture = Origins.BackGlowMasks[drawInfo.drawPlayer.back],
				color = Color.White
			});
		}
	}
}
