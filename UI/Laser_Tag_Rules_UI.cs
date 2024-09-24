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

namespace Origins.UI {
	public class Laser_Tag_Rules_UI : UIState {
		static AutoCastingAsset<Texture2D>[] Textures;
		public class TextureLoader : ILoadable {
			public void Load(Mod mod) {
				Textures = [
					Origins.instance.Assets.Request<Texture2D>("UI/Broadcast_Icon"),
					Origins.instance.Assets.Request<Texture2D>("UI/Dream_Icon"),
					Origins.instance.Assets.Request<Texture2D>("UI/Grow_Icon"),
					Origins.instance.Assets.Request<Texture2D>("UI/Inject_Icon"),
					Origins.instance.Assets.Request<Texture2D>("UI/Defile_Icon"),
					Origins.instance.Assets.Request<Texture2D>("UI/Focus_Icon"),
					Origins.instance.Assets.Request<Texture2D>("UI/Float_Icon"),
					Origins.instance.Assets.Request<Texture2D>("UI/Command_Icon"),
					Origins.instance.Assets.Request<Texture2D>("UI/Evolve_Icon")
				];
			}

			public void Unload() {
				Textures = null;
			}
		}
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
			SoundEngine.PlaySound(SoundID.Tink.WithVolumeScale(0.75f));
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
	public class UI_Time_Button(ButtonTimenessGetter variable, LocalizedText text, (int radix, string format, string suffix)[] radices, int increment = 1, int indefiniteThreshold = 0, LocalizedText indefiniteText = null, int maxValue = int.MaxValue) : UIPanel {
		public new Color BorderColor = Color.Black;
		public new Color BackgroundColor = new Color(63, 82, 151) * 0.7f;
		public Color HoverBorderColor = new Color(33, 33, 33) * 0.9f;
		public Color HoverBackgroundColor = new Color(93, 113, 187) * 0.9f;
		string FormatTime(int time) {
			if (indefiniteText is not null && time < indefiniteThreshold) return indefiniteText.Value;
			StringBuilder builder = new();
			float textTime = time / 60f;
			for (int i = 1; i <= radices.Length; i++) {
				(int radix, string format, string suffix) = radices[^i];
				float useTime = i == radices.Length ? textTime : textTime % radix;
				builder.Insert(0, string.Format(format, useTime) + suffix);
				textTime = (int)(textTime / radix);
				if (textTime == 0) break;
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
			bool indefinite = time < indefiniteThreshold;
			bool maxed = time >= maxValue;

			destinationRectangle.Height /= 2;
			Color color = new(211, 211, 211);
			if (maxed) {
				color = Color.Gray;
			} else if (destinationRectangle.Contains(Main.mouseX, Main.mouseY)) {
				color = Color.White;
				if (click) {
					if (indefinite) variable() = indefiniteThreshold;
					else variable() += increment;
					if (time > maxValue) time = maxValue;
					SoundEngine.PlaySound(SoundID.Tink.WithVolumeScale(0.75f));
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
					if (time < indefiniteThreshold) time = indefiniteThreshold - increment;
					SoundEngine.PlaySound(SoundID.Tink.WithVolumeScale(0.75f));
				}
			}
			spriteBatch.Draw(UICommon.ButtonUpDownTexture.Value, destinationRectangle, sourceRectangle, color);
		}
	}
}