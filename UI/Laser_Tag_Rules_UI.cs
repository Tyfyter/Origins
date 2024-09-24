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
			SoundEngine.PlaySound(SoundID.Tink);
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
}