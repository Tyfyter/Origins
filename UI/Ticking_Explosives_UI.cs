using Microsoft.Xna.Framework.Graphics;
using Origins;
using Origins.Items.Weapons.Demolitionist;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI.Chat;

namespace Origins.UI {
	public abstract class Ticking_Explosives_UI(int maxSeconds, int defaultSeconds) : ItemModeHUD {
		AutoLoadingAsset<Texture2D> texture = typeof(Ticking_Explosives_UI).GetDefaultTMLName();
		public float rotation = float.Epsilon;
		public bool dragging = false;
		public int seconds = defaultSeconds;
		readonly int secondsLimit = maxSeconds - 1;
		public float TotalSeconds {
			get {
				float value = seconds + rotation / MathHelper.TwoPi;
				if (rotation == 0) value += 1;
				return value;
			}
		}
		public override bool ShouldToggle() {
			switch (PlayerInput.CurrentInputMode) {
				default:
				case InputMode.Keyboard or InputMode.KeyboardUI:
				return base.ShouldToggle();

				case InputMode.XBoxGamepad or InputMode.XBoxGamepadUI:
				return Main.mouseRight != isShowing;
			}
		}
		public float RotationControl {
			get {
				switch (PlayerInput.CurrentInputMode) {
					default:
					case InputMode.Keyboard or InputMode.KeyboardUI:
					return (Main.MouseScreen - activationPosition).ToRotation();

					case InputMode.XBoxGamepad or InputMode.XBoxGamepadUI:
					return PlayerInput.GamepadThumbstickRight.ToRotation();
				}
			}
		}
		public bool IsHovering {
			get {
				switch (PlayerInput.CurrentInputMode) {
					default:
					case InputMode.Keyboard or InputMode.KeyboardUI:
					return new Rectangle((int)activationPosition.X - 8, (int)activationPosition.Y - 48, 16, 56).Contains(Main.MouseScreen.RotatedBy(-rotation, activationPosition));

					case InputMode.XBoxGamepad or InputMode.XBoxGamepadUI:
					return true;
				}

			}
		}
		public bool IsTryingToDrag {
			get {
				switch (PlayerInput.CurrentInputMode) {
					default:
					case InputMode.Keyboard or InputMode.KeyboardUI:
					return Main.mouseLeft && Main.mouseLeftRelease;

					case InputMode.XBoxGamepad or InputMode.XBoxGamepadUI:
					return PlayerInput.GamepadThumbstickRight != default;
				}
			}
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Rectangle frame = texture.Value.Frame(verticalFrames: 3, sizeOffsetY: -2);
			switch (PlayerInput.CurrentInputMode) {
				case InputMode.XBoxGamepad or InputMode.XBoxGamepadUI:
				activationPosition = (Main.LocalPlayer.Top - Vector2.UnitY * 64).ToScreenPosition();
				break;
			}
			spriteBatch.Draw(
				texture,
				activationPosition,
				frame,
				Color.White,
				0,
				frame.Size() * 0.5f,
				1,
				0,
			0);
			frame.Y += frame.Height + 2;
			spriteBatch.Draw(
				texture,
				activationPosition,
				frame,
				Color.White,
				rotation,
				frame.Size() * 0.5f,
				1,
				0,
			0);
			if (dragging || IsHovering) {
				Main.LocalPlayer.mouseInterface = true;
				frame.Y += frame.Height + 2;
				spriteBatch.Draw(
					texture,
					activationPosition,
					frame,
					Main.OurFavoriteColor,
					rotation,
					frame.Size() * 0.5f,
					1,
					0,
				0);
				if (IsTryingToDrag) dragging = true;
			}
			if (dragging) {
				if (!Main.mouseLeft) dragging = false;
				float oldRotation = rotation;
				rotation = RotationControl + MathHelper.PiOver2;
				rotation = MathHelper.WrapAngle(rotation - MathHelper.Pi) + MathHelper.Pi;
				float fullRotFactor = MathHelper.WrapAngle(oldRotation + MathHelper.Pi) - MathHelper.WrapAngle(rotation + MathHelper.Pi);
				if (seconds <= 0 && oldRotation < MathHelper.TwoPi - 4 && rotation > 4) rotation = float.Epsilon;
				if (Math.Abs(fullRotFactor) > 4 && oldRotation != float.Epsilon) {
					seconds += Math.Sign(fullRotFactor);
					if (seconds < 0) {
						seconds = 0;
						rotation = float.Epsilon;
					} else if (seconds > secondsLimit) {
						seconds = secondsLimit;
						rotation = 0;
					} else {
						SoundEngine.PlaySound(SoundID.MenuTick);
					}
				}
			}
			string text = OriginExtensions.ToRomanNumerals((int)TotalSeconds);
			if (string.IsNullOrEmpty(text)) return;
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.CombatText[0].Value,
				text,
				activationPosition - Vector2.UnitY * 48,
				Color.White,
				0,
				FontAssets.CombatText[0].Value.MeasureString(text) * new Vector2(0.5f, 1),
				Vector2.One
			);
		}
		public override void DrawNearCursor(SpriteBatch spriteBatch) { }
	}
}
