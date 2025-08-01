using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Accessories.Eyndum_Cores;
using PegasusLib;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Fallacious_Vase_Dash_Layer : PlayerDrawLayer {
		AutoLoadingAsset<Texture2D> texture = typeof(Fallacious_Vase).GetDefaultTMLName() + "_Use";
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.OriginPlayer().dashVaseVisual;
		public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.Skin, PlayerDrawLayers.Leggings);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 Position = drawInfo.Position + drawPlayer.Size * 0.5f - Main.screenPosition;
			OriginPlayer originPlayer = drawPlayer.OriginPlayer();
			Rectangle frame = texture.Frame(verticalFrames: 6, frameY: (int)originPlayer.dashVaseFrameCount);

			DrawData item = new(texture, Position, frame, Color.White, drawPlayer.bodyRotation, frame.Size() * 0.5f, 1f, drawInfo.playerEffect) {
				shader = originPlayer.dashVaseDye
			};
			drawInfo.DrawDataCache.Add(item);
			if (drawInfo.projectileDrawPosition != -1) drawInfo.projectileDrawPosition = drawInfo.DrawDataCache.Count;
			originPlayer.dashVaseFrameCount += 1f / 2;
			if (originPlayer.dashVaseFrameCount >= 6) originPlayer.dashVaseFrameCount = 0;
		}
	}
}
