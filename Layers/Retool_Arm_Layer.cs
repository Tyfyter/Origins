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
		public static Vector2 GetShoulder(Player player, Vector2 position, bool visual = false)
			=> position + new Vector2(player.width * (0.5f - 0.25f * player.direction), visual ? player.height * (0.5f - player.gravDir * 0.15f) : player.height * (0.35f - (player.gravDir - 1) * 0.5f));
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			const float sprite_rotation_offset = 0.558f;
			Player player = drawInfo.drawPlayer;
			OriginPlayer originPlayer = player.OriginPlayer();
			Retool_Arm retoolArm = originPlayer.retoolArm;
			float rotation = originPlayer.retoolArmBaseRotation - (player.direction * player.gravDir * sprite_rotation_offset) + MathHelper.PiOver2;
			if (player.gravDir == -1) rotation = (MathHelper.Pi - rotation) - 2 * player.direction;
			drawInfo.DrawDataCache.Add(new(
				baseArmTexture,
				GetShoulder(player, drawInfo.Position, true).Floor() - Main.screenPosition,
				null,
				drawInfo.colorArmorHead,
				rotation,
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
