using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Backgrounds;
using Origins.Core;
using Origins.Tiles;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Light;
using Terraria.ModLoader;

namespace Origins.Events {
	public class Smog_Storm : ModBiome {
		static readonly Smog_Storm_Sky sky = new();
		const string biomeName = "Origins:SmogStorm";
		public override int Music => Origins.Music.SmogStorm;
		public override SceneEffectPriority Priority => SceneEffectPriority.Event;
		public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<Riven_Surface_Background>();
		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => OriginExtensions.BiomeUGBackground<Riven_Underground_Background>();
		public override void SetStaticDefaults() {
			SkyManager.Instance.Bind(biomeName, sky);
			try {
				MonoModHooks.Add(typeof(SystemLoader).GetMethod(nameof(SystemLoader.ModifyLightingBrightness)), ModifyLightingBrightness);
				IL_LightMap.BlurLine += IL_LightMap_BlurLine;
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(ModifyLightingBrightness), e)) throw;
			}

		}
		void IL_LightMap_BlurLine(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.After, i => i.MatchCall<LightMap>("get_" + nameof(LightMap.LightDecayThroughAir)));
			c.EmitDelegate<Func<float, float>>(decay => {
				decay -= decay * sky.Opacity * 0.15f;
				return decay;
			});
			c.GotoNext(MoveType.After, i => i.MatchCall<LightMap>("get_" + nameof(LightMap.LightDecayThroughAir)));
			c.EmitDelegate<Func<float, float>>(decay => {
				decay -= decay * sky.Opacity * 0.25f;
				return decay;
			});
			c.GotoNext(MoveType.After, i => i.MatchCall<LightMap>("get_" + nameof(LightMap.LightDecayThroughAir)));
			c.EmitDelegate<Func<float, float>>(decay => {
				decay -= decay * sky.Opacity * 0.5f;
				return decay;
			});
		}

		delegate void orig_ModifyLightingBrightness(ref float negLight, ref float negLight2);
		void ModifyLightingBrightness(orig_ModifyLightingBrightness orig, ref float negLight, ref float negLight2) {
			orig(ref negLight, ref negLight2);
			if (OriginsModIntegrations.FancyLightingEngine) negLight -= negLight * sky.Opacity * 0.5f;
		}
		public override bool IsBiomeActive(Player player) => Main.WindyEnoughForKiteDrops && Main.LocalPlayer.InModBiome<Ashen_Biome>();
		public override void SpecialVisuals(Player player, bool isActive) {
			if (SkyManager.Instance[biomeName] is CustomSky sky && isActive != sky.IsActive()) {
				if (isActive) {
					SkyManager.Instance.Activate(biomeName);
				} else {
					SkyManager.Instance.Deactivate(biomeName);
				}
			}
			if (Overlays.Scene[biomeName] is Overlay overlay && isActive != overlay.IsVisible()) {
				if (isActive) {
					Overlays.Scene.Activate(biomeName);
				} else {
					Overlays.Scene.Deactivate(biomeName);
				}
			}
		}
	}
	public class Smog_Storm_Sky : CustomSky {
		bool isActive;
		public override void Activate(Vector2 position, params object[] args) {
			if (isActive.TrySet(true)) Opacity = 0;
		}
		public override void Deactivate(params object[] args) {
			isActive = false;
		}
		public override bool IsActive() => Opacity > 0;
		public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth) { }
		public override void Reset() { }
		public override Color OnTileColor(Color inColor) => Color.Lerp(inColor, new(inColor.R / 5, 1, 1), Opacity);
		public override void Update(GameTime gameTime) {
			MathUtils.LinearSmoothing(ref Opacity, isActive.ToInt(), 1f / 180);
		}
	}
}
