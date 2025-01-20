using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Terraria.UI.Chat;
using Terraria.ModLoader.UI;
using Origins.Reflection;
using Newtonsoft.Json;
using System.Reflection;
using tModPorter;
using FullSerializer;
using System.Globalization;
using System.ComponentModel;
using System.IO;

namespace Origins {
	[CustomModConfigItem(typeof(DamageClassDefinitionConfigElement))]
	[TypeConverter(typeof(ToFromStringConverter<DamageClassDefinition>))]
	public class DamageClassDefinition : EntityDefinition, IEquatable<DamageClassDefinition> {
		public static readonly Func<TagCompound, DamageClassDefinition> DESERIALIZER = Load;
		public override bool IsUnloaded => !(Mod == "Terraria" && Name == "None" || Mod == "" && Name == "");
		public string FullName => $"{Mod}/{Name}";
		public DamageClass DamageClass => ModContent.TryFind(FullName, out DamageClass @class) ? @class : null;
		public override int Type => DamageClass?.Type ?? -1;
		public DamageClassDefinition() : base() { }
		/// <summary><b>Note: </b>As ModConfig loads before other content, make sure to only use <see cref="DamageClassDefinition(string, string)"/> for modded content in ModConfig classes. </summary>
		public DamageClassDefinition(int type) : base(DamageClassLoader.GetDamageClass(type).FullName) { }
		public DamageClassDefinition(string key) : base(key) { }
		public DamageClassDefinition(string mod, string name) : base(mod, name) { }
		public static DamageClassDefinition FromString(string s) => new(s);
		public static DamageClassDefinition Load(TagCompound tag) => new(tag.GetString("mod"), tag.GetString("name"));
		public bool Equals(DamageClassDefinition other) => other?.FullName == FullName;
		public override string DisplayName => IsUnloaded || Type == -1 ? Language.GetTextValue("Mods.ModLoader.Unloaded") : Name;
		public override bool Equals(object obj) => Equals(obj as DamageClassDefinition);
		public override int GetHashCode() => FullName.GetHashCode();
	}
	public class DamageClassDefinitionConfigElement : ConfigElement<DamageClassDefinition> {
		protected bool pendingChanges = false;
		public override void OnBind() {
			base.OnBind();
			base.TextDisplayFunction = TextDisplayOverride ?? base.TextDisplayFunction;
			pendingChanges = true;
			SetupList();
			normalTooltip = TooltipFunction?.Invoke() ?? string.Empty;
			TooltipFunction = () => tooltip;
		}
		public Func<string> TextDisplayOverride { get; set; }
		float height = 0;
		bool opened = false;
		string normalTooltip;
		string tooltip = string.Empty;
		protected void SetupList() {
			RemoveAllChildren();
			height = 30;
			foreach (DamageClass @class in DamageClasses.All) {
				if (DamageClasses.HideInConfig.Contains(@class)) continue;
				string text = @class.DisplayName.Value.Trim();
				Vector2 size = FontAssets.MouseText.Value.MeasureString(text) * 0.8f;
				UIPanel panel = new() {
					Left = new(0, 0),
					Top = new(height + 4, 0),
					Width = new(-8, 1),
					Height = new(size.Y + 4, 0),
					HAlign = 0.5f,
					PaddingTop = 0
				};
				UIText element = new(text, 0.8f) {
					Width = new(0, 1),
					Top = new(0, 0.5f),
					VAlign = 0.5f
				};
				panel.OnUpdate += element => {
					if (element is not UIPanel panel) return;
					if (panel.IsMouseHovering) {
						panel.BackgroundColor = UICommon.DefaultUIBlue;
						tooltip = @class.FullName;
					} else {
						panel.BackgroundColor = UICommon.MainPanelBackground;
					}
				};
				panel.OnLeftClick += (_, _) => {
					if (Value.FullName != @class.FullName) Value = new(@class.FullName);
					opened = false;
					SetupList();
				};
				element.TextColor = Value.FullName == @class.FullName ? Color.Goldenrod : Color.White;
				panel.Append(element);
				Append(panel);
				height += size.Y + 8;
			}
			Recalculate();
		}
		public override void LeftClick(UIMouseEvent evt) {
			opened = true;
			string json = JsonConvert.SerializeObject(OriginConfig.Instance, default(JsonSerializerSettings));
			OriginConfig config = new();
			JsonConvert.PopulateObject(json, config, default(JsonSerializerSettings));
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
		protected override void DrawChildren(SpriteBatch spriteBatch) {
			if (opened) {
				base.DrawChildren(spriteBatch);
			} else {
				string text = $"{Value.DamageClass?.DisplayName.Value.Trim() ?? ""} ({Value.FullName})";
				Vector2 size = FontAssets.MouseText.Value.MeasureString(text) * 0.8f;
				CalculatedStyle innerDimensions = GetInnerDimensions();
				ChatManager.DrawColorCodedStringWithShadow(
					spriteBatch,
					FontAssets.MouseText.Value,
					text,
					innerDimensions.Position() + new Vector2(innerDimensions.Width - size.X, (innerDimensions.Height - size.Y) * 0.5f + 4),
					Color.White,
					0f,
					Vector2.Zero,
					Vector2.One * 0.8f
				);
			}
		}
	}
}
