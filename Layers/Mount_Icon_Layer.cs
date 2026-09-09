using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Layers;
[ReinitializeDuringResizeArrays]
public class Mount_Icon_Layer : PlayerDrawLayer {
	public static Asset<Texture2D>[] IconTexture = MountID.Sets.Factory.CreateCustomSet<Asset<Texture2D>>(null);
	public override bool IsHeadLayer => true;
	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
		return drawInfo.headOnlyRender && !drawInfo.drawPlayer.dead && drawInfo.drawPlayer.mount.Active && IconTexture[drawInfo.drawPlayer.mount.Type] is not null;
	}
	public override Position GetDefaultPosition() => new Between();
	protected override void Draw(ref PlayerDrawSet drawInfo) {
		Texture2D texture = IconTexture[drawInfo.drawPlayer.mount.Type].Value;
		drawInfo.DrawDataCache.Add(new(
			texture,
			drawInfo.Position - Main.screenPosition,
			null,
			drawInfo.drawPlayer.GetImmuneAlphaPure(new Color(1f, 1f, 1f, 1f), drawInfo.shadow),
			0,
			texture.Size() * 0.5f,
			1,
			drawInfo.playerEffect
		) {
			shader = drawInfo.drawPlayer.cMount
		});
	}
}
