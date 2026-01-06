using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Accessories;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Retool_Arm_Layer : PlayerDrawLayer {
		AutoLoadingAsset<Texture2D> baseArmTexture = typeof(Retool_Arm).GetDefaultTMLName("_Use");
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.OriginPlayer().retoolArm is not null;
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.ArmOverItem);
		public static Vector2 GetShoulder(Player player, Vector2 position) => position + new Vector2(player.width * (0.5f - 0.25f * player.direction), player.height * 0.35f);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player player = drawInfo.drawPlayer;
			OriginPlayer originPlayer = player.OriginPlayer();
			Retool_Arm retoolArm = originPlayer.retoolArm;
			drawInfo.DrawDataCache.Add(new(
				baseArmTexture,
				GetShoulder(player, drawInfo.Position).Floor() - Main.screenPosition,
				null,
				drawInfo.colorArmorHead,
				originPlayer.retoolArmBaseRotation - (player.direction * 0.558f) + MathHelper.PiOver2,
				new Vector2(3, 21).Apply(drawInfo.playerEffect, baseArmTexture.Value.Size()),
				retoolArm.Item.scale,
				drawInfo.playerEffect
				) {
				shader = originPlayer.retoolArmDye
			});
			retoolArm?.DrawArm(ref drawInfo);
		}
	}
}
