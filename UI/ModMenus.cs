using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Origins.Backgrounds;
using ReLogic.Content;
using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.UI {
	public class Riven_Hive_Mod_Menu : ModMenu {
		public override Asset<Texture2D> Logo => TextureAssets.Logo2;
		public override int Music => Origins.Music.Riven;
		public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<Riven_Surface_Background>();
		public override string DisplayName => Language.GetOrRegister(Mod.GetLocalizationKey("ModMenu.Riven_Hive")).Value;
	}
	public class Defiled_Wastelands_Mod_Menu : ModMenu {
		public override Asset<Texture2D> Logo => TextureAssets.Logo2;
		public override int Music => Origins.Music.Defiled;
		public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<Defiled_Surface_Background>();
		public override string DisplayName => Language.GetOrRegister(Mod.GetLocalizationKey("ModMenu.Defiled_Wastelands")).Value;
		public static void EnableShaderOnMenu(ILContext il) {
			ILCursor c = new(il);
			ILLabel aft = default;
			c.GotoNext(MoveType.After,
				static i => i.MatchLdsfld(typeof(Filters), nameof(Filters.Scene)),
				static i => i.MatchLdstr("Sepia"),
				static i => i.MatchCallvirt(out _),
				static i => i.MatchCallvirt<Filter>(nameof(Filter.IsInUse)),
				i => i.MatchBrfalse(out aft)
			);
			c.GotoLabel(aft);
			c.EmitLdloca((VariableDefinition)c.Prev.Operand);
			c.EmitDelegate((ref bool cantShade) => {
				if (!OriginAccessibilityConfig.Instance.DisableDefiledWastelandsShader && Main.gameMenu && MenuLoader.CurrentMenu is Defiled_Wastelands_Mod_Menu) {
					Filter defiledFilter = Filters.Scene["Origins:ZoneDefiled"];
					Effect effect = defiledFilter?.GetShader()?.Shader;
					if (effect is not null) {
						if (!Filters.Scene["Origins:ZoneDefiled"].IsActive()) Filters.Scene.Activate("Origins:ZoneDefiled", default);
						defiledFilter.Opacity = Math.Max(OriginClientConfig.Instance.DefiledShaderNoise, float.Epsilon);
						defiledFilter.GetShader()
						.UseProgress(0.9f)
						.UseIntensity(OriginClientConfig.Instance.DefiledShaderJitter * 0.0035f)
						.UseOpacity(MathHelper.Clamp(OriginClientConfig.Instance.DefiledShaderNoise * 5, float.Epsilon, 1));
						cantShade = false;
					}
				}
			});
		}
		public override void Update(bool isOnTitleScreen) {
		}
	}
}
