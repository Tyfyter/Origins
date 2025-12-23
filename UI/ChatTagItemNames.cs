using Microsoft.Xna.Framework.Graphics;
using Origins.UI.Snippets;
using ReLogic.Graphics;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class ChatTagItemNames : ILoadable {
		public void Load(Mod mod) {
			On_Main.GUIHotbarDrawInner += On_Main_GUIHotbarDrawInner;
			MonoModHooks.Add(typeof(DynamicSpriteFont).GetMethod("InternalDraw", BindingFlags.NonPublic | BindingFlags.Instance), (orig_InternalDraw orig, DynamicSpriteFont self, string text, SpriteBatch spriteBatch, Vector2 startPosition, Color color, float rotation, Vector2 origin, ref Vector2 scale, SpriteEffects spriteEffects, float depth) => {
				Wiggle_Handler.origin = origin;
				Centered_Snippet_Handler.origin = origin;
				if (isDrawingPopup) {
					using Flag<bool> flag = new(() => ref isDrawingPopup, false);
					ChatManager.DrawColorCodedString(spriteBatch, self, ChatManager.ParseMessage(text, color).ToArray(), startPosition, color, rotation, Vector2.Zero, scale, out _, 0);
					return;
				}
				if (isDrawingHotbar) {
					//using Flag<bool> flag = new(() => ref isDrawingHotbar, false);
					isDrawingHotbar = false;
					Color baseColor = Color.White;
					if (OriginClientConfig.Instance.ShowRarityInHotbar) baseColor = PegasusLib.PegasusLib.GetRarityColor(heldItemRarity, heldItemExpert, heldItemMaster);
					ChatManager.DrawColorCodedString(spriteBatch, self, ChatManager.ParseMessage(text, baseColor).ToArray(), startPosition, color, rotation, origin, scale, out _, 0);
					return;
				}
				orig(self, text, spriteBatch, startPosition, color, rotation, origin, ref scale, spriteEffects, depth);
			});
			On_Main.DrawItemTextPopups += (orig, scaleTarget) => {
				using Flag<bool> _ = new(() => ref isDrawingPopup, true);
				orig(scaleTarget);
			};
		}
		delegate void orig_InternalDraw(DynamicSpriteFont self, string text, SpriteBatch spriteBatch, Vector2 startPosition, Color color, float rotation, Vector2 origin, ref Vector2 scale, SpriteEffects spriteEffects, float depth);
		delegate void hook_InternalDraw(orig_InternalDraw orig, DynamicSpriteFont self, string text, SpriteBatch spriteBatch, Vector2 startPosition, Color color, float rotation, Vector2 origin, ref Vector2 scale, SpriteEffects spriteEffects, float depth);
		static bool isDrawingHotbar;
		static int heldItemRarity;
		static bool heldItemExpert;
		static bool heldItemMaster;
		static bool isDrawingPopup;
		private void On_Main_GUIHotbarDrawInner(On_Main.orig_GUIHotbarDrawInner orig, Main self) {
			using Flag<bool> _ = new(() => ref isDrawingHotbar, true);
			heldItemRarity = Main.LocalPlayer.HeldItem?.rare ?? 0;
			heldItemExpert = Main.LocalPlayer.HeldItem?.expert ?? false;
			heldItemMaster = Main.LocalPlayer.HeldItem?.master ?? false;
			orig(self);
		}

		public void Unload() { }
		class Flag<T> : IDisposable {
			public delegate ref T GetFlag();
			private readonly GetFlag flag;
			private readonly T oldValue;
			public Flag(GetFlag flag, T newValue) {
				this.flag = flag;
				oldValue = flag();
				flag() = newValue;
			}
			public void Dispose() {
				flag() = oldValue;
			}
		}
	}
}
