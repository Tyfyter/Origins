using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Proj_Over_Arm_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.OriginPlayer().heldProjOverArm is not null;
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.ArmOverItem);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			drawInfo.DrawDataCache.Add(drawInfo.drawPlayer.OriginPlayer().heldProjOverArm.GetDrawData());
		}
	}
}
