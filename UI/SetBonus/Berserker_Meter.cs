using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace Origins.UI.SetBonus {
	public class Berserker_Meter : SwitchableUIState {
		float alpha = 1f;
		public AutoLoadingAsset<Texture2D> texture = typeof(Berserker_Meter).GetDefaultTMLName();
		public override bool IsActive() {
			if (OriginPlayer.LocalOriginPlayer is null) return false;
			if (OriginPlayer.LocalOriginPlayer.setActiveAbility == SetActiveAbility.blast_armor) {
				return true;
			}
			alpha = 0f;
			return false;
		}
		public Berserker_Meter() : base() {
			OverrideSamplerState = SamplerState.PointClamp;
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			const int meterWidth = 52;// width of the meter itself in the spritesheet
			const int meterXOffset = -meterWidth / 2;// the offset necessary to center the meter
			const int barMaxWidth = 18;// the width of the empty area
			const int barStartXOffset = 14;// the starting x coordinate of the empty area
			const int frameHeight = 14;// the height of the frames
			const int activeYOffset = frameHeight + 2;// the y coordinate of the "in use" frames

			OriginPlayer originPlayer = OriginPlayer.LocalOriginPlayer;
			if (originPlayer.blastSetCharge > 0) {
				alpha = 1f;
			} else {
				if (alpha <= 0) return;
				alpha -= 0.1f - alpha * alpha * 0.097f;
			}
			Player player = Main.LocalPlayer;
			Color white = Color.White * alpha;
			Vector2 pos = player.MountedCenter - Main.screenPosition;
			pos.X += meterXOffset;
			pos.Y += player.height * 0.5f + 8;

			Main.UIScaleMatrix.Decompose(out Vector3 scale, out _, out _);
			pos.X = ((int)pos.X) / scale.X;
			pos.Y = ((int)pos.Y) / scale.Y;
			int frameOffset = originPlayer.blastSetActive ? activeYOffset : 0;
			spriteBatch.Draw(
				texture,
				pos + new Vector2(barStartXOffset, 0),
				new Rectangle(meterWidth + 2, frameOffset, 2, frameHeight),
				white,
				0,
				default,
				new Vector2((int)((originPlayer.blastSetCharge / OriginPlayer.blast_set_charge_max) * barMaxWidth), 1),
				0,
			0);
			spriteBatch.Draw(
				texture,
				pos,
				new Rectangle(0, frameOffset, meterWidth, frameHeight),
				white,
				0,
				default,
				1,
				0,
			0);
		}
	}
}
