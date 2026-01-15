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
			int frame = (Exo_Weapon_Mount.BuffTime - buffTime) / 3;
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
				if (frame > 5) break;
				Item lastItem = player.inventory[player.OriginPlayer().exoWeaponMountLastWeapon];
				Texture2D lastItemTexture = TextureAssets.Item[lastItem.type].Value;
				Rectangle itemDrawFrame = player.GetItemDrawFrame(lastItem.type);
				Vector2 origin = new(itemDrawFrame.Width * 0.5f - itemDrawFrame.Width * 0.5f * player.direction, itemDrawFrame.Height);
				if (player.gravDir == -1f) {
					origin.Y = itemDrawFrame.Height - origin.Y;
				}
				float rotation = -MathHelper.PiOver2;
				rotation -= frame * 0.1f;
				switch (lastItem.useStyle) {
					default:
					return;
					case ItemUseStyleID.Guitar: {
						const int offset = 12;
						if (player.direction == -1) {
							origin = new Vector2(itemDrawFrame.Width - offset, offset);
						} else {
							origin = new Vector2(offset, offset);
						}
						rotation -= frame * 0.2f;
						rotation += MathHelper.Pi + 0.4f;
						break;
					}
					case ItemUseStyleID.Rapier:
					case ItemUseStyleID.Swing: {
						const int offset = 12;
						if (player.direction == -1) {
							origin = new Vector2(offset, itemDrawFrame.Height - offset);
						} else {
							origin = new Vector2(itemDrawFrame.Width - offset, itemDrawFrame.Height - offset);
						}
						rotation -= frame * 0.1f;
						break;
					}
					case ItemUseStyleID.Thrust: {
						const int offset = 12;
						origin = new Vector2(itemDrawFrame.Width * 0.5f, itemDrawFrame.Height - offset);
						rotation -= frame * 0.1f;
						break;
					}
					case ItemUseStyleID.RaiseLamp: {
						const int offset = 12;
						origin = new Vector2(itemDrawFrame.Width * 0.5f, offset);
						rotation = 0;
						break;
					}
					case ItemUseStyleID.Shoot: {
						if (lastItem.CountsAsClass(DamageClasses.Incantation)) {
							origin = new Vector2(5 - 2 * drawInfo.drawPlayer.direction, 8);
							rotation = frame * -0.075f;
							return;
						}
						Vector2 offset = Main.DrawPlayerItemPos(player.gravDir, lastItem.type);
						if (player.direction == -1) {
							origin = new Vector2(-offset.X, itemDrawFrame.Height / 2);
						} else {
							origin = new Vector2(itemDrawFrame.Width + offset.X, itemDrawFrame.Height / 2);
						}
						if (lastItem.useAmmo == AmmoID.Arrow) {
							rotation += MathHelper.PiOver2;
							origin.X = itemDrawFrame.Width - origin.X;
							origin.Y -= 12;
						}
						break;
					}
				}
				rotation *= player.direction;
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
					lastItemTexture,
					position,
					itemDrawFrame,
					lastItem.GetAlpha(drawInfo.colorArmorBody),
					rotation,
					origin,
					lastItem.scale,
					data.effect
				));
				break;
			}
		}
	}
}
