using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.Items.Accessories;
using Origins.Items.Armor.Fiberglass;
using PegasusLib;
using System.Numerics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Exo_Weapon_Mount_Layer : PlayerDrawLayer {
		AutoLoadingAsset<Texture2D> texture = typeof(Exo_Weapon_Mount).GetDefaultTMLName("_Back_Real");
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.back == Exo_Weapon_Mount.BackID;
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.BackAcc);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Texture2D fakeTexture = TextureAssets.AccBack[Exo_Weapon_Mount.BackID].Value;
			Player player = drawInfo.drawPlayer;
			int buffTime = player.buffTime.GetIfInRange(player.FindBuffIndex(ModContent.BuffType<Exo_Weapon_Mount_Buff>()));
			int frame = (Exo_Weapon_Mount.BuffTime - buffTime) / 2;
			if (frame >= 7) frame = 0;
			for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--) {
				DrawData data = drawInfo.DrawDataCache[i];
				if (data.texture != fakeTexture) continue;
				data.texture = texture;
				data.sourceRect = texture.Frame(verticalFrames: 7, frameY: frame);
				data.effect ^= SpriteEffects.FlipHorizontally;
				data.origin = data.effect.ApplyToOrigin(new(-4, 9), data.sourceRect.Value);
				drawInfo.DrawDataCache[i] = data;

				if (frame == 0) break;
				Item lastItem = player.inventory[player.OriginPlayer().exoWeaponMountLastWeapon];
				Rectangle itemDrawFrame = player.GetItemDrawFrame(lastItem.type);
				Vector2 origin = new(itemDrawFrame.Width * 0.5f - itemDrawFrame.Width * 0.5f * player.direction, itemDrawFrame.Height);
				if (player.gravDir == -1f) {
					origin.Y = itemDrawFrame.Height - origin.Y;
				}
				switch (lastItem.useStyle) {
					default:
					return;
					case ItemUseStyleID.Shoot:
					Vector2 vector10 = Main.DrawPlayerItemPos(player.gravDir, lastItem.type);
					if (player.direction == -1) {
						origin = new Vector2(-vector10.X, itemDrawFrame.Height / 2);
					} else {
						origin = new Vector2(itemDrawFrame.Width + vector10.X, itemDrawFrame.Height / 2);
					}
					break;
				}
				//origin += vector;
				Vector2 position = data.position + (frame switch {
					0 => new(28, -9),
					1 => new(28, -7),
					2 => new(26, 2),
					3 => new(28, 6),
					4 => new(30, 9),
					5 => new(28, 6),
					6 => new(28, -7),
					_ => new Vector2(28, -9)
				}).Apply(data.effect, default).RotatedBy(drawInfo.rotation);
				drawInfo.DrawDataCache.Add(new DrawData(
					TextureAssets.Item[lastItem.type].Value,
					position,
					itemDrawFrame,
					lastItem.GetAlpha(drawInfo.colorArmorBody),
					-MathHelper.PiOver2 * player.direction,
					origin,
					lastItem.scale,
					data.effect
				));
				break;
			}
		}
	}
}
