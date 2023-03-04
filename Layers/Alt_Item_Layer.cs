using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Alt_Item_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.ItemAnimationActive
				&& !drawInfo.drawPlayer.JustDroppedAnItem
				&& drawInfo.shadow == 0
				&& drawInfo.heldItem.useStyle == Terraria.ID.ItemUseStyleID.Shoot
				&& drawInfo.heldItem.ModItem is ICustomDrawItem;
		}
		public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.HeldItem, PlayerDrawLayers.ArmOverItem);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player drawPlayer = drawInfo.drawPlayer;
			Item item = drawPlayer.HeldItem;
			Texture2D itemTexture = TextureAssets.Item[item.type].Value;
			ICustomDrawItem aItem = (ICustomDrawItem)item.ModItem;
			int drawXPos = 0;
			Vector2 itemCenter = new Vector2(itemTexture.Width / 2, itemTexture.Height / 2);
			Vector2 drawItemPos = Main.DrawPlayerItemPos(drawPlayer.gravDir, item.type);
			drawXPos = (int)drawItemPos.X;
			itemCenter.Y = drawItemPos.Y;
			Vector2 drawOrigin = new Vector2(drawXPos, itemTexture.Height / 2);
			if (drawPlayer.direction == -1) {
				drawOrigin = new Vector2(itemTexture.Width + drawXPos, itemTexture.Height / 2);
			}
			drawOrigin.X -= drawPlayer.width / 2;
			Color lightColor = Lighting.GetColor((int)(drawInfo.Position.X + drawInfo.drawPlayer.width * 0.5) / 16, (int)((drawInfo.Position.Y + drawInfo.drawPlayer.height * 0.5) / 16.0)); ;//drawInfo.colorBodySkin.ToVector4() / drawPlayer.skinColor.ToVector4();
			aItem.DrawInHand(itemTexture, ref drawInfo, itemCenter, lightColor, drawOrigin);
		}
	}
}
