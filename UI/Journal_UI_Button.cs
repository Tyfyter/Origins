using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;
using System.Linq;
using Terraria.GameContent;
using Terraria.DataStructures;
using Origins.Questing;
using PegasusLib;
using PegasusLib.Graphics;

namespace Origins.UI {
	public class Journal_UI_Button : UIState {
		public static AutoCastingAsset<Texture2D> Texture;
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
			Rectangle rectangle = new(num, num2, width, num4);
			bool flag = false;
			Texture2D texture = Texture.Value;
			Vector2 position = rectangle.Center.ToVector2();
			Vector2 origin = new(15, 15);
			Color white = Color.White;
			OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
			int journalShader = originPlayer?.journalDye?.dye??0;
			if (rectangle.Contains(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
				flag = true;
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					Main.LocalPlayer.SetTalkNPC(-1);
					Main.npcChatCornerItem = 0;
					Main.mouseLeftRelease = false;
					SoundEngine.PlaySound(SoundID.MenuTick);
					IngameFancyUI.OpenUIState(new Journal_UI_Open());
					//OriginSystem.instance.journalUI.SetState(new Journal_UI_Open());
					//IngameFancyUI.OpenUIState(BestiaryUI);
					//BestiaryUI.OnOpenPage();
				} else if (Main.mouseRight && Main.mouseRightRelease && (Main.mouseItem.dye > 0 || Main.mouseItem.IsAir)) {
					Main.mouseLeft = Main.mouseLeftRelease = true;
					Main.mouseRight = false;
					originPlayer.journalDye ??= new();
					ItemSlot.Handle(ref originPlayer.journalDye);
					Main.mouseLeft = Main.mouseLeftRelease = false;
					Main.mouseRight = true;
				} else if (OriginsModIntegrations.GoToKeybindKeybindPressed) {
					OriginsModIntegrations.SearchKeybind("Journal");
				}
				if (Main.LocalPlayer.HeldItem.dye > 0) {
					journalShader = Main.LocalPlayer.HeldItem.dye;
				}
				spriteBatch.Draw(texture, position, new Rectangle(0, 64, 30, 30), white, 0f, origin, 1f, SpriteEffects.None, 0);
			}
			spriteBatch.Draw(texture, position, new Rectangle(0, 32, 30, 30), white, 0f, origin, 1f, SpriteEffects.None, 0);
			SpriteBatchState oldstate = spriteBatch.GetState();
			spriteBatch.Restart(oldstate, SpriteSortMode.Immediate);
			DrawData data = new(texture, position, new Rectangle(0, 0, 30, 30), white, 0f, origin, 1f, SpriteEffects.None, 0) {
				shader = journalShader
			};
			Terraria.Graphics.Shaders.GameShaders.Armor.ApplySecondary(data.shader, Main.LocalPlayer, data);
			data.Draw(spriteBatch);
			spriteBatch.Restart(oldstate);
			if (((OriginPlayer.LocalOriginPlayer?.unreadJournalEntries?.Count ?? 0) > 0) || Quest_Registry.Quests.Any(q => q.HasNotification)) {
				float scaleValue = MathHelper.Lerp(0.5f, 1.15f, Main.mouseTextColor / 255f);
				Main.spriteBatch.Draw(
					TextureAssets.QuicksIcon.Value,
					rectangle.TopRight() + new Vector2(-4, 2),
					null,
					Main.MouseTextColorReal,
					0,
					TextureAssets.QuicksIcon.Size() * new Vector2(0.5f, 0.5f),
					(Main.mouseTextColor / 255f) * Main.UIScale,
					SpriteEffects.None,
				0);
			}
			//UILinkPointNavigator.SetPosition(15001, position);
			if (!Main.mouseText && flag) {
				Main.instance.MouseText(Language.GetTextValue("Mods.Origins.Interface.Journal"), 0, 0);
			}
		}
	}
}