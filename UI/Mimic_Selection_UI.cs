using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.UI {
	public class Mimic_Selection_UI : UIState {
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
		public float StartX => Main.screenWidth - 64 - 14 - 142;
		public float StartY => (174 + (!Main.mapFullscreen && Main.mapStyle == 1 ? 204 : 0)) + (1 * 56) * 0.85f;
		public string GetAbilityTooltip(int level, int choice) {
			switch (level) {
				case 0:
				switch (choice) {
					case 0:
					return "BRO@dCAST: We $hare your p0tion effects";

					case 1:
					return "DR3aM: @dapt to any scenario";

					case 2:
					return "GR0w: Se3d the world 4 improved base s7ats";
				}
				break;

				case 1:
				switch (choice) {
					case 0:
					return "INJE%t: Help y0ur enemies d1scover their inner-$elves";

					case 1:
					return "D3fILE: We welc0me all, there !s alw@ys room";

					case 2:
					return "F0cUS: Seed th3 world to hone your d#structive p0tent!al";
				}
				break;

				case 2:
				switch (choice) {
					case 0:
					return "F1oAT: Grow y0ur wings";

					case 1:
					return "COM#aND: The anti-b0d!es are your army";

					case 2:
					return "EV0lVE: Se3d the w0rld for stronge3r equipment effects";
				}
				break;
			}
			return "";
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
			int currentLevel = OriginSystem.MimicSetLevel;
			int boxSize = (int)(32 * Main.inventoryScale);
			float posX = StartX;
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				posX -= 38;
			}
			for (int level = 0; level < currentLevel; level++) {
				float posY = StartY;
				for (int i = 0; i < 3; i++) {
					Rectangle rectangle = new((int)posX, (int)posY, boxSize, boxSize);
					bool glow = originPlayer.GetMimicSetChoice(level) == i + 1;
					if (rectangle.Contains(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
						Main.LocalPlayer.mouseInterface = true;
						if (!glow && Main.mouseLeft && Main.mouseLeftRelease) {
							originPlayer.SetMimicSetChoice(level, i + 1);
							SoundEngine.PlaySound(SoundID.MenuTick);
						} else if (glow && Main.mouseRight && Main.mouseRightRelease) {
							originPlayer.SetMimicSetChoice(level, 0);
							SoundEngine.PlaySound(SoundID.MenuTick);
						}
						Main.hoverItemName = GetAbilityTooltip(level, i);
						glow = true;
					}
					spriteBatch.Draw(Textures[i + level * 3], rectangle, glow ? Color.White : Color.LightSlateGray);
					posY += boxSize + 2;
				}
				posX -= boxSize + 4;
			}
		}
	}
}