using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.UI;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader.UI;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Exceptions;
using Terraria.Localization;
using Origins.Reflection;
using Terraria;
using ReLogic.Content;

namespace Origins {
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Enum)]
	sealed class ConfigFlagsAttribute<TEnum>() : CustomModConfigItemAttribute(typeof(FlagsEnumConfigElement<TEnum>)) where TEnum : struct, Enum {}
	public class FlagsEnumConfigElement<TEnum> : ConfigElement<TEnum> where TEnum : struct, Enum {
		protected bool pendingChanges = false;
		TEnum[] possibleFlags;
		Dictionary<TEnum, (LocalizedText label, LocalizedText tooltip)> flagLocalizations;
		Asset<Texture2D> _toggleTexture = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle");
		public override void OnBind() {
			base.OnBind();
			base.TextDisplayFunction = TextDisplayOverride ?? base.TextDisplayFunction;
			pendingChanges = true;
			normalTooltip = TooltipFunction?.Invoke() ?? string.Empty;
			TooltipFunction = () => tooltip;
			possibleFlags = Enum.GetValues<TEnum>();
			flagLocalizations = [];
			for (int i = 0; i < possibleFlags.Length; i++) {
				FieldInfo field = typeof(TEnum).GetField(possibleFlags[i].ToString(), BindingFlags.Public | BindingFlags.Static);
				flagLocalizations[possibleFlags[i]] = (ConfigManagerMethods.GetConfigLabel(field), ConfigManagerMethods.GetConfigTooltip(field));
			}
			SetupList();
		}
		public Func<string> TextDisplayOverride { get; set; }
		float height = 0;
		bool opened = false;
		string normalTooltip;
		string tooltip = string.Empty;
		protected void SetupList() {
			RemoveAllChildren();
			height = 28;
			foreach (TEnum current in possibleFlags) {
				if (!IsFlag(current)) continue;
				TEnum flag = current;
				string text = flagLocalizations[flag].label.Value;
				Vector2 size = FontAssets.MouseText.Value.MeasureString(text) * 0.8f;
				UIPanel panel = new() {
					Left = new(0, 0),
					Top = new(height + 4, 0),
					Width = new(-8, 1),
					Height = new(size.Y + 4, 0),
					HAlign = 0.5f,
					PaddingTop = 0
				};
				UIText textElement = new(text, 0.8f) {
					Width = new(0, 0),
					Top = new(0, 0.5f),
					VAlign = 0.5f,
					TextColor = Value.HasFlag(flag) ? Color.White : Color.Gray
				};
				LocalizedText tooltipLocalization = flagLocalizations[flag].tooltip;
				panel.OnUpdate += element => {
					if (element is not UIPanel panel) return;
					if (panel.IsMouseHovering) {
						panel.BackgroundColor = UICommon.DefaultUIBlue;
						tooltip = tooltipLocalization?.Value;
					} else {
						panel.BackgroundColor = new Color(47, 62, 113) * 0.7f;
					}
				};
				panel.OnLeftClick += (_, _) => {
					Value = XOR(Value, flag);
					SetupList();
				};
				panel.Append(textElement);
				UIImageFramed toggleIndicator = new(_toggleTexture, new Rectangle(Value.HasFlag(flag).ToInt() * 16, 0, 14, 14)) {
					Height = new(_toggleTexture.Height(), 0),
					Top = new(-2, 0.5f),
					HAlign = 1f,
					VAlign = 0.5f,
				};
				panel.Append(toggleIndicator);
				Append(panel);
				height += size.Y + 8;
			}
			height += 4;
			Recalculate();
		}
		public override void LeftClick(UIMouseEvent evt) {
			if (opened) {
				CalculatedStyle innerDimensions = GetInnerDimensions();
				Texture2D buttonTexture = opened ? ExpandedTexture.Value : CollapsedTexture.Value;
				Vector2 buttonPos = innerDimensions.Position() + new Vector2(innerDimensions.Width - (buttonTexture.Width + 4), 32 * 0.5f - buttonTexture.Height * 0.5f);
				if (Main.MouseScreen.Between(buttonPos, buttonPos + buttonTexture.Size())) opened = false;
			} else {
				opened = true;
			}
		}
		public override void Update(GameTime gameTime) {
			SetHeight();
			tooltip = normalTooltip;
			if (opened) base.Update(gameTime);
		}
		void SetHeight() {
			float targetHeight = opened ? height : 32;
			if (Height.Pixels != targetHeight) {
				Height.Pixels = targetHeight;
				Parent.Height.Pixels = targetHeight;
				this.Recalculate();
				Parent.Recalculate();
			}
		}
		static TEnum XOR(TEnum a, TEnum b) {
			Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
			if (underlyingType == typeof(sbyte) || underlyingType == typeof(byte)) return Unsafe.BitCast<byte, TEnum>((byte)(Unsafe.BitCast<TEnum, byte>(a) ^ Unsafe.BitCast<TEnum, byte>(b)));
			else if (underlyingType == typeof(short) || underlyingType == typeof(ushort)) return Unsafe.BitCast<ushort, TEnum>((ushort)(Unsafe.BitCast<TEnum, ushort>(a) ^ Unsafe.BitCast<TEnum, ushort>(b)));
			else if (underlyingType == typeof(int) || underlyingType == typeof(uint)) return Unsafe.BitCast<uint, TEnum>((Unsafe.BitCast<TEnum, uint>(a) ^ Unsafe.BitCast<TEnum, uint>(b)));
			else if (underlyingType == typeof(long) || underlyingType == typeof(ulong)) return Unsafe.BitCast<ulong, TEnum>((Unsafe.BitCast<TEnum, ulong>(a) ^ Unsafe.BitCast<TEnum, ulong>(b)));
			else throw new InvalidOperationException($"Unsupported enum underlying type: {underlyingType}");
		}
		protected override void DrawChildren(SpriteBatch spriteBatch) {
			CalculatedStyle innerDimensions = GetInnerDimensions();
			Texture2D buttonTexture = opened ? ExpandedTexture.Value : CollapsedTexture.Value;
			Vector2 buttonPos = innerDimensions.Position() + new Vector2(innerDimensions.Width - (buttonTexture.Width + 4), 32 * 0.5f - buttonTexture.Height * 0.5f);
			if (opened) base.DrawChildren(spriteBatch);
			string text = OriginExtensions.CombineWithAnd(Value.GetFlags().Select(flag => flagLocalizations[flag].label).ToArray()).Value;
			if (string.IsNullOrWhiteSpace(text)) text = Language.GetOrRegister("Mods.Origins.FlagsEnumConfigElement.NoneSelected", () => "None").Value;
			Vector2 size = FontAssets.MouseText.Value.MeasureString(text) * 0.8f;
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.MouseText.Value,
				text,
				innerDimensions.Position() + new Vector2(innerDimensions.Width - (size.X + 8 + buttonTexture.Width), (32 - size.Y) * 0.5f + 4),
				Color.White,
				0f,
				Vector2.Zero,
				Vector2.One * 0.8f
			);
			Color buttonColor = Color.White;
			if (Main.MouseScreen.Between(buttonPos, buttonPos + buttonTexture.Size())) {
				buttonColor = new Color(220, 220, 220);
			}
			spriteBatch.Draw(
				buttonTexture,
				buttonPos,
				buttonColor
			);
		}
		public static bool IsFlag(TEnum value) {
			Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
			ulong x;
			if (underlyingType == typeof(sbyte) || underlyingType == typeof(byte)) x = Unsafe.BitCast<TEnum, byte>(value);
			else if (underlyingType == typeof(short) || underlyingType == typeof(ushort)) x = Unsafe.BitCast<TEnum, ushort>(value);
			else if (underlyingType == typeof(int) || underlyingType == typeof(uint)) x = Unsafe.BitCast<TEnum, uint>(value);
			else if (underlyingType == typeof(long) || underlyingType == typeof(ulong)) x = Unsafe.BitCast<TEnum, ulong>(value);
			else throw new InvalidOperationException($"Unsupported enum underlying type: {underlyingType}");
			return x > 0 && (x & (x - 1)) == 0;
		}
	}
}
