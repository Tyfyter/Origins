using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Origins.Backgrounds;
using Origins.Graphics;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.World.BiomeData;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Localization;
using Terraria.ModLoader;
using static Origins.World.BiomeData.Brine_Pool;

namespace Origins.UI {
	public class Riven_Hive_Mod_Menu : ModMenu {
		public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>("Origins/UI/Logos/Riven_Terraria");
		public override int Music => Origins.Music.Riven;
		public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<Riven_Surface_Background>();
		public override string DisplayName => Language.GetOrRegister(Mod.GetLocalizationKey("ModMenu.Riven_Hive")).Value;
		Asset<Texture2D> Loglow;
		Asset<Texture2D> Subtitle;
		public override void SetStaticDefaults() {
			Loglow = ModContent.Request<Texture2D>("Origins/UI/Logos/Riven_Terraria_Glow");
			Subtitle = ModContent.Request<Texture2D>("Origins/UI/Logos/Origins_Subheader");
		}
		public override void PostDrawLogo(SpriteBatch spriteBatch, Vector2 logoDrawCenter, float logoRotation, float logoScale, Color drawColor) {
			spriteBatch.Draw(Loglow.Value, logoDrawCenter, null, Riven_Hive.GetGlowAlpha(drawColor), logoRotation, Loglow.Size() * 0.5f, logoScale, SpriteEffects.None, 0);
			//Can't add Origins subtitle because it's as big as the actual logo
			//spriteBatch.Draw(Subtitle.Value, logoDrawCenter + logoRotation.ToRotationVector2() * 32 + (logoRotation + MathHelper.PiOver2).ToRotationVector2() * 100, null, drawColor, logoRotation, new(243 * 0.5f, 50), logoScale * 2, SpriteEffects.None, 0);
		}
	}
	public class Defiled_Wastelands_Mod_Menu : ModMenu {
		public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>("Origins/UI/Logos/Defiled_Terraria");
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
						.UseOpacity(MathHelper.Clamp(OriginClientConfig.Instance.DefiledShaderNoise * 5, float.Epsilon, 1))
						.Shader.Parameters["uTimeScale"].SetValue(OriginClientConfig.Instance.DefiledShaderSpeed);
						cantShade = false;
					}
				}
			});
		}
		public override void Update(bool isOnTitleScreen) {
		}
		Asset<Texture2D> tangela;
		public override void SetStaticDefaults() {
			tangela = ModContent.Request<Texture2D>("Origins/UI/Logos/Defiled_Terraria_Tangela");
		}
		public override void PostDrawLogo(SpriteBatch spriteBatch, Vector2 logoDrawCenter, float logoRotation, float logoScale, Color drawColor) {
			TangelaVisual.DrawTangela(
				tangela.Value,
				logoDrawCenter,
				null,
				logoRotation,
				tangela.Size() * 0.5f,
				new(logoScale),
				SpriteEffects.None,
				0
			);
			//Can't add Origins subtitle because it's as big as the actual logo
			//spriteBatch.Draw(Subtitle.Value, logoDrawCenter + logoRotation.ToRotationVector2() * 32 + (logoRotation + MathHelper.PiOver2).ToRotationVector2() * 100, null, drawColor, logoRotation, new(243 * 0.5f, 50), logoScale * 2, SpriteEffects.None, 0);
		}
	}
	public class Aether_Mod_Menu : ModMenu {
		public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>("Origins/UI/Logos/Shimmer_Terraria");
		public override int Music => Origins.Music.TheDive;
		public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<Placeholder_Surface_Background>();
		public override string DisplayName => Language.GetOrRegister(Mod.GetLocalizationKey("ModMenu.Aether")).Value;
		public override bool IsAvailable => false; // Waiting on https://github.com/tModLoader/tModLoader/pull/4716
		public override void PostDrawLogo(SpriteBatch spriteBatch, Vector2 logoDrawCenter, float logoRotation, float logoScale, Color drawColor) {
			//Can't add Origins subtitle because it's as big as the actual logo
			//spriteBatch.Draw(Subtitle.Value, logoDrawCenter + logoRotation.ToRotationVector2() * 32 + (logoRotation + MathHelper.PiOver2).ToRotationVector2() * 100, null, drawColor, logoRotation, new(243 * 0.5f, 50), logoScale * 2, SpriteEffects.None, 0);
		}
	}
}
