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
	public class Journal_UI_Button : UIState {
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
			if (rectangle.Contains(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
				flag = true;
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					Main.LocalPlayer.SetTalkNPC(-1);
					Main.npcChatCornerItem = 0;
					Main.npcChatText = "";
					Main.mouseLeftRelease = false;
					SoundEngine.PlaySound(SoundID.MenuTick);
					//IngameFancyUI.OpenUIState(BestiaryUI);
					//BestiaryUI.OnOpenPage();
				}
			}
			Texture2D value = TextureAssets.Item[ItemID.Book].Value;
			Vector2 position = rectangle.Center.ToVector2();
			Rectangle rectangle2 = value.Bounds;
			Vector2 origin = rectangle2.Size() / 2f;
			Color white = Color.White;
			spriteBatch.Restart(SpriteSortMode.Immediate, transformMatrix:Main.UIScaleMatrix);
			DrawData data = new(value, position, rectangle2, white, 0f, origin, 1f, SpriteEffects.None, 0) {
				shader = Main.LocalPlayer.HeldItem.dye > 0 ? Main.LocalPlayer.HeldItem.dye  : 81
			};
			Terraria.Graphics.Shaders.GameShaders.Armor.ApplySecondary(data.shader, Main.LocalPlayer, data);
			data.Draw(spriteBatch);
			spriteBatch.Restart(transformMatrix: Main.UIScaleMatrix);
			UILinkPointNavigator.SetPosition(310, position);
			if (!Main.mouseText && flag) {
				Main.instance.MouseText(Language.GetTextValue("Mods.Origins.Interface.Journal"), 0, 0);
			}
		}
	}
}