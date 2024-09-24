using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.Tiles.Other;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI;
using Terraria.UI;
using ReLogic.Content;
using System.Text;
using Terraria.GameContent;
using System;

namespace Origins.UI {
	public class Laser_Tag_Rules_UI : UIState {
		public static SoundStyle ClickSound => SoundID.Tink.WithVolumeScale(0.75f);
		public override void OnInitialize() {
			UIElement panel = new UIPanel();
			panel.Width.Set(0f, 0.875f);
			panel.MaxWidth.Set(900f, 0f);
			panel.MinWidth.Set(700f, 0f);
			panel.Top.Set(190f, 0f);
			panel.Height.Set(-310f, 1f);
			panel.HAlign = 0.5f;
			Append(panel);
			UIList list = [..Laser_Tag_Console.LaserTagRules.GetUIElements()];
			list.Width.Set(-10, 1f);
			list.Height.Set(-36, 1f);
			panel.HAlign = 0.5f;
			panel.Append(list);
			UIButton<LocalizedText> startButton = new(Language.GetOrRegister("Mods.Origins.Laser_Tag.StartButton"));
			startButton.Width.Set(-10, 1f);
			startButton.Height.Set(32, 0f);
			startButton.Top.Set(-32, 1f);
			startButton.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) => {
				IngameFancyUI.Close();
				if (Main.netMode == NetmodeID.SinglePlayer) return;
				ModPacket packet = Origins.instance.GetPacket();
				packet.Write(Origins.NetMessageType.start_laser_tag);
				Laser_Tag_Console.LaserTagRules.Write(packet);
				packet.Send(-1, Main.myPlayer);
			};
			panel.Append(startButton);
		}
	}
	public delegate ref bool ButtonTogglenessGetter();
	public class UI_Toggle_Button(ButtonTogglenessGetter variable, LocalizedText text) : UIPanel {
		static AutoLoadingAsset<Texture2D> toggleTexture = "Terraria/Images/UI/Settings_Toggle";
		public override void LeftClick(UIMouseEvent evt) {
			variable() ^= true;
			SoundEngine.PlaySound(Laser_Tag_Rules_UI.ClickSound);
		}
		public new Color BorderColor = Color.Black;
		public new Color BackgroundColor = new Color(63, 82, 151) * 0.7f;
		public Color HoverBorderColor = new Color(33, 33, 33) * 0.9f;
		public Color HoverBackgroundColor = new Color(93, 113, 187) * 0.9f;
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Rectangle bounds = GetDimensions().ToRectangle();
			if (bounds.Contains(Main.mouseX, Main.mouseY)) {
				base.BorderColor = HoverBorderColor;
				base.BackgroundColor = HoverBackgroundColor;
			} else {
				base.BorderColor = BorderColor;
				base.BackgroundColor = BackgroundColor;
			}
			base.DrawSelf(spriteBatch);

			Utils.DrawBorderString(spriteBatch, text.Value, bounds.Left() + Vector2.UnitX * 8, Color.White, 1, 0f, 0.4f, -1);
			Texture2D value = toggleTexture;
			Rectangle sourceRectangle = new(variable() ? ((value.Width - 2) / 2 + 2) : 0, 0, (value.Width - 2) / 2, value.Height);
			spriteBatch.Draw(toggleTexture, bounds.Right() - Vector2.UnitX * 8 - sourceRectangle.Size() * new Vector2(1, 0.5f), sourceRectangle, Color.White);
		}
	}
	public delegate ref int ButtonTimenessGetter();
	public class UI_Time_Button(ButtonTimenessGetter variable, LocalizedText text, (int radix, string format, bool alwaysShow)[] radices, int increment = 1, int indefiniteThreshold = 0, LocalizedText indefiniteText = null, int maxValue = int.MaxValue) : UIPanel {
		public new Color BorderColor = Color.Black;
		public new Color BackgroundColor = new Color(63, 82, 151) * 0.7f;
		public Color HoverBorderColor = new Color(33, 33, 33) * 0.9f;
		public Color HoverBackgroundColor = new Color(93, 113, 187) * 0.9f;
		string FormatTime(int time) {
			if (indefiniteText is not null && time < indefiniteThreshold * increment) return indefiniteText.Value;
			StringBuilder builder = new();
			float textTime = time / 60f;
			for (int i = 1; i <= radices.Length; i++) {
				(int radix, string format, bool alwaysShow) = radices[^i];
				if (textTime == 0 && !alwaysShow) break;
				float useTime = i == radices.Length ? textTime : textTime % radix;
				builder.Insert(0, string.Format(format, useTime));
				textTime = (int)(textTime / radix);
			}
			return builder.ToString();
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Rectangle bounds = GetDimensions().ToRectangle();
			base.DrawSelf(spriteBatch);
			int time = variable();

			Utils.DrawBorderString(spriteBatch, text.Value, bounds.Left() + Vector2.UnitX * 8, Color.White, 1f, 0f, 0.4f, -1);

			Texture2D value = UICommon.ButtonUpDownTexture.Value;
			Rectangle sourceRectangle = value.Frame(verticalFrames: 2);
			Rectangle destinationRectangle = bounds;
			destinationRectangle.Width = destinationRectangle.Height;
			destinationRectangle.X = bounds.Right - destinationRectangle.Width;

			Utils.DrawBorderString(spriteBatch, FormatTime(time), destinationRectangle.Left() - Vector2.UnitX * 8, Color.White, 1f, 1f, 0.4f, -1);

			bool click = Main.mouseLeft && Main.mouseLeftRelease;
			bool indefinite = time < indefiniteThreshold * increment;
			bool maxed = time >= maxValue;

			destinationRectangle.Height /= 2;
			Color color = new(211, 211, 211);
			if (maxed) {
				color = Color.Gray;
			} else if (destinationRectangle.Contains(Main.mouseX, Main.mouseY)) {
				color = Color.White;
				if (click) {
					if (indefinite) variable() = indefiniteThreshold * increment;
					else variable() += increment;
					if (time > maxValue) time = maxValue;
					SoundEngine.PlaySound(Laser_Tag_Rules_UI.ClickSound);
				}
			}
			spriteBatch.Draw(UICommon.ButtonUpDownTexture.Value, destinationRectangle, sourceRectangle, color);

			destinationRectangle.Y += destinationRectangle.Height;
			sourceRectangle.Y += sourceRectangle.Height;
			color = new(211, 211, 211);
			if (indefinite) {
				color = Color.Gray;
			} else if (destinationRectangle.Contains(Main.mouseX, Main.mouseY)) {
				color = Color.White;
				if (click) {
					variable() -= increment;
					if (time < indefiniteThreshold) time = indefiniteThreshold * increment - increment;
					SoundEngine.PlaySound(Laser_Tag_Rules_UI.ClickSound);
				}
			}
			spriteBatch.Draw(UICommon.ButtonUpDownTexture.Value, destinationRectangle, sourceRectangle, color);
		}
	}
	public delegate ref int ButtonHealthnessGetter();
	public class UI_HP_Button(ButtonHealthnessGetter variable, LocalizedText text, Texture2D texture) : UIPanel {
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Rectangle bounds = GetDimensions().ToRectangle();
			base.DrawSelf(spriteBatch);

			Vector2 pos = bounds.Left() + Vector2.UnitX * 8;
			Utils.DrawBorderString(spriteBatch, text.Value, pos, Color.White, 1f, 0f, 0.4f, -1);
			pos += FontAssets.MouseText.Value.MeasureString(text.Value) * new Vector2(1, 0f) + Vector2.UnitX * 8;
			bounds.Inflate(-4, -4);
			Vector2 size = texture.Size() * (bounds.Height / (float)texture.Height);
			int hp = variable();
			Rectangle destinationRectangle = new((int)pos.X, bounds.Y, (int)size.X, (int)size.Y);
			int paddedWidth = (destinationRectangle.Width + 2);
			int maxDisplayed = (bounds.Right - destinationRectangle.X) / paddedWidth;
			bool hovered = new Rectangle(destinationRectangle.X, destinationRectangle.Y - 2, maxDisplayed * paddedWidth, destinationRectangle.Height + 4).Contains(Main.mouseX, Main.mouseY);
			if (hovered) {
				int hoverIndex = -1;
				for (int i = maxDisplayed - 1; i >= 0; i--) {
					destinationRectangle.X = (int)pos.X + paddedWidth * i;
					if (i < hp) {
						spriteBatch.Draw(
							texture,
							destinationRectangle,
							Color.Black
						);
					}
					if (hoverIndex == -1 && destinationRectangle.Contains(Main.mouseX, Main.mouseY, 2, 4)) {
						hoverIndex = i;
					}
					if (hoverIndex != -1) {
						spriteBatch.Draw(
							texture,
							destinationRectangle,
							new Color(255, 255, 255, 0)
						);
					}
				}
				if (hoverIndex != -1 && Main.mouseLeft && Main.mouseLeftRelease) {
					variable() = hoverIndex + 1;
					SoundEngine.PlaySound(Laser_Tag_Rules_UI.ClickSound);
				}
			} else {
				for (int i = hp - 1; i >= 0; i--) {
					destinationRectangle.X = (int)pos.X + paddedWidth * i;
					spriteBatch.Draw(
						texture,
						destinationRectangle,
						Color.White
					);
				}
			}
		}
	}
}