using Microsoft.Xna.Framework.Graphics;
using Origins.Reflection;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class BetterFlavorTextBestiaryInfoElement(string languageKey) : FlavorTextBestiaryInfoElement(languageKey), IBestiaryInfoElement {
		public virtual string Key => FlavorTextBestiaryInfoElementMethods._key.GetValue(this);
		public new UIElement ProvideUIElement(BestiaryUICollectionInfo info) {
			if (info.UnlockState < BestiaryEntryUnlockState.CanShowStats_2) {
				return null;
			}
			UIPanel panel = new(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Stat_Panel"), null, 12, 7) {
				Width = new StyleDimension(-11f, 1f),
				Height = new StyleDimension(109f, 0f),
				BackgroundColor = new Color(43, 56, 101),
				BorderColor = Color.Transparent,
				Left = new StyleDimension(3f, 0f),
				PaddingLeft = 4f,
				PaddingRight = 4f
			};
			BetterUIText uIText = new(Language.GetText(Key), 0.8f) {
				HAlign = 0f,
				VAlign = 0f,
				Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
				Height = StyleDimension.FromPixelsAndPercent(0f, 1f)
			};
			uIText.OnTextChange += () => {
				panel.Height = new StyleDimension(uIText.MinHeight.Pixels, 0f);
			};
			panel.Append(uIText);
			return panel;
		}
		public class BetterUIText : UIElement {
			private object _text = "";

			private float _textScale = 1f;

			private Vector2 _textSize = Vector2.Zero;

			private TextSnippet[] _visibleText;

			private string _lastTextReference;

			public string Text => _text.ToString();

			public float TextOriginX { get; set; }

			public float TextOriginY { get; set; }

			public float WrappedTextBottomPadding { get; set; }

			public Color TextColor { get; set; } = Color.White;

			public Color ShadowColor { get; set; } = Color.Black;

			public event Action OnTextChange;

			public BetterUIText(string text, float textScale = 1f) {
				TextOriginX = 0.5f;
				TextOriginY = 0f;
				WrappedTextBottomPadding = 20f;
				InternalSetText(text, textScale);
			}

			public BetterUIText(LocalizedText text, float textScale = 1f) {
				TextOriginX = 0.5f;
				TextOriginY = 0f;
				WrappedTextBottomPadding = 20f;
				InternalSetText(text, textScale);
			}

			public override void Recalculate() {
				InternalSetText(_text, _textScale);
				base.Recalculate();
			}

			public void SetText(string text) {
				InternalSetText(text, _textScale);
			}

			public void SetText(LocalizedText text) {
				InternalSetText(text, _textScale);
			}

			protected override void DrawSelf(SpriteBatch spriteBatch) {
				base.DrawSelf(spriteBatch);
				VerifyTextState();
				CalculatedStyle innerDimensions = GetInnerDimensions();
				Vector2 position = innerDimensions.Position();

				position.Y -= 2f * _textScale;

				position.X += (innerDimensions.Width - _textSize.X) * TextOriginX;
				position.Y += (innerDimensions.Height - _textSize.Y) * TextOriginY;
				float num = _textScale;

				DynamicSpriteFont value = FontAssets.MouseText.Value;
				float width = innerDimensions.Width;
				Vector2 vector = ChatManager.GetStringSize(value, _visibleText, new Vector2(_textScale), width);
				Color baseColor = ShadowColor * (TextColor.A / 255f);
				Vector2 origin = new Vector2(0f, 0f) * vector;
				Vector2 baseScale = new(num);
				ChatManager.DrawColorCodedStringShadow(spriteBatch, value, _visibleText, position, baseColor, 0f, origin, baseScale, width, 1.5f);
				ChatManager.DrawColorCodedString(spriteBatch, value, _visibleText, position, Color.White, 0f, origin, baseScale, out _, width);
			}

			private void VerifyTextState() {
				if ((object)_lastTextReference != Text) {
					InternalSetText(_text, _textScale);
				}
			}

			private void InternalSetText(object text, float textScale) {
				DynamicSpriteFont dynamicSpriteFont = FontAssets.MouseText.Value;
				_text = text;
				_textScale = textScale;
				_lastTextReference = _text.ToString();
				_visibleText = ChatManager.ParseMessage(_lastTextReference, TextColor).ToArray();
				Vector2 vector = ChatManager.GetStringSize(dynamicSpriteFont, _visibleText, new Vector2(textScale), GetInnerDimensions().Width);
				Vector2 vector2 = _textSize = vector + Vector2.UnitY * WrappedTextBottomPadding;
				MinHeight.Set(vector2.Y + PaddingTop + PaddingBottom, 0f);
				ChatManager.ConvertNormalSnippets(_visibleText);
				OnTextChange?.Invoke();
			}
		}
	}
	public class GaslightingFlavorTextBestiaryInfoElement(string languageKey, string altLanguageKey) : BetterFlavorTextBestiaryInfoElement(languageKey) {
		public override string Key => Main.rand.NextBool() ? altLanguageKey : FlavorTextBestiaryInfoElementMethods._key.GetValue(this);
	}
}
