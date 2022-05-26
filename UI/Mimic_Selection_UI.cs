using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using System;
using System.Linq;
using static Tyfyter.Utils.UITools;
using Origins.Items.Accessories.Eyndum_Cores;
using Origins.World;

namespace Origins.UI {
	public class Mimic_Selection_UI : UIState {
		public float StartX => Main.screenWidth - 64 - 14 - 142;
		public float StartY => (174 + (!Main.mapFullscreen && Main.mapStyle == 1 ? 204 : 0)) + (1 * 56) * 0.85f;
		public string GetAbilityTooltip(int level, int choice) {
			switch (level) {
				case 0:
				switch (choice) {
					case 0:
					return "Gives wings";

					case 1:
					break;

					case 2:
					break;
				}
				break;

				case 1:
				switch (choice) {
					case 0: 
					return "Active ability: consume 40 mana to shoot spines in an arc";
					
					case 1:
					return "Active ability: consume 40 mana to gain several buffs for a short time\n30 second cooldown";

					case 2:
					break;
				}
				break;

				case 2:
				switch (choice) {
					case 0:
					return "Gives an extra accessory slot";

					case 1:
					return "Greatly increases movement speed";

					case 2:
					return "Increases damage and efficiency while in the air";
				}
				break;
			}
			return "";
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
			int currentLevel = 3;//(int)((OriginWorld.totalDefiled * 3) / (float)WorldGen.totalSolid);
			Texture2D[] textures = new Texture2D[] {
				ModContent.GetTexture("Origins/UI/Defiled_Buff_Choice_Generic_1"),
				ModContent.GetTexture("Origins/UI/Defiled_Buff_Choice_Generic_2"),
				ModContent.GetTexture("Origins/UI/Defiled_Buff_Choice_Generic_3")
			};
			int boxSize = (int)(32 * Main.inventoryScale);
			float posX = StartX;
			for (int level = 0; level < currentLevel; level++) {
				float posY = StartY;
				for (int i = 0; i < 3; i++) {
					Rectangle rectangle = new Rectangle((int)posX, (int)posY, boxSize, boxSize);
					bool glow = originPlayer.GetMimicSetChoice(level) == i + 1;
					if (rectangle.Contains(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
						Main.LocalPlayer.mouseInterface = true;
						if (!glow && Main.mouseLeft && Main.mouseLeftRelease) {
							originPlayer.SetMimicSetChoice(level, i + 1);
							Main.PlaySound(SoundID.MenuTick);
						}else if(glow && Main.mouseRight && Main.mouseRightRelease) {
							originPlayer.SetMimicSetChoice(level, 0);
							Main.PlaySound(SoundID.MenuTick);
						}
						Main.hoverItemName = GetAbilityTooltip(level, i);
						glow = true;
					}
					spriteBatch.Draw(textures[i], rectangle, glow ? Color.White : Color.LightSlateGray);
					posY += boxSize + 2;
				}
				posX -= boxSize + 4;
			}
		}
	}
}