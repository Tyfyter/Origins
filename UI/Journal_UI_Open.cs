using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
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
using Terraria.GameContent;
using Terraria.UI.Gamepad;
using Terraria.DataStructures;

namespace Origins.UI {
	public class Journal_UI_Open : UIState {
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Main.inventoryScale = 0.85f;
			int width = 30;
			int num4 = 30;
			int num = 566;
			int num2 = 244 + num4 + 4;
			if ((Main.LocalPlayer.chest != -1 || Main.npcShop > 0) && !Main.recBigList) {
				num2 += 168;
				Main.inventoryScale = 0.755f;
				num += 5;
			}
			Rectangle rectangle = new Rectangle(num, num2, width, num4);
			bool flag = false;
			Texture2D texture = Journal_UI_Button.Texture.Value;
			Vector2 position = rectangle.Center.ToVector2();
			Vector2 origin = new Vector2(15, 15);
			Color white = Color.White;
			if (rectangle.Contains(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
				flag = true;
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					Main.LocalPlayer.SetTalkNPC(-1);
					Main.npcChatCornerItem = 0;
					Main.npcChatText = "";
					Main.mouseLeftRelease = false;
					SoundEngine.PlaySound(SoundID.MenuTick);
					OriginSystem.instance.journalUI.SetState(new Journal_UI_Open());
					//IngameFancyUI.OpenUIState(BestiaryUI);
					//BestiaryUI.OnOpenPage();
				}
				spriteBatch.Draw(texture, position, new Rectangle(0, 64, 30, 30), white, 0f, origin * 0, 1f, SpriteEffects.None, 0);
			}
			spriteBatch.Draw(texture, position, new Rectangle(0, 32, 30, 30), white, 0f, origin, 1f, SpriteEffects.None, 0);
			spriteBatch.Restart(SpriteSortMode.Immediate, transformMatrix: Main.UIScaleMatrix);
			DrawData data = new(texture, position, new Rectangle(0, 0, 30, 30), white, 0f, origin * 2, 1f, SpriteEffects.None, 0) {
				shader = Main.LocalPlayer.HeldItem.dye > 0 ? Main.LocalPlayer.HeldItem.dye : 81
			};
			Terraria.Graphics.Shaders.GameShaders.Armor.ApplySecondary(data.shader, Main.LocalPlayer, data);
			data.Draw(spriteBatch);
			spriteBatch.Restart(transformMatrix: Main.UIScaleMatrix);
			//UILinkPointNavigator.SetPosition(15001, position);
			if (!Main.mouseText && flag) {
				Main.instance.MouseText(Language.GetTextValue("Mods.Origins.Interface.Journal"), 0, 0);
			}
		}
	}
}