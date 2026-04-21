using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Backgrounds;
using Origins.Core;
using Origins.Tiles;
using Origins.World.BiomeData;
using ReLogic.Threading;
using System;
using System.Diagnostics;
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
			player.ManageSpecialBiomeVisuals(biomeName, isActive, player.MountedCenter);
			if (isActive) Filters.Scene["Origins:SmogStorm"].GetShader().UseImage(LineOfSight.Texture.Value, 1);
		}
		class LineOfSight : ILoadable {
			const int resolutionResolution = 5;
			const int minResolution = resolutionResolution <= 20 ? 20 : resolutionResolution;
			static Texture2D texture;
			static readonly float[] buffer = new float[400];
			static int targetResolution = 100;
			static Texture2D[] textures = new Texture2D[buffer.Length / resolutionResolution];
			void ILoadable.Load(Mod mod) {
				Main.QueueMainThreadAction(ReinitializeTexture);
			}
			void ILoadable.Unload() {
				for (int i = 0; i < textures.Length; i++) textures[i].Dispose();
				texture = null;
				textures = null;
			}

			static bool? IgnoreGlass(Tile tile) {
				if (!tile.HasTile || !Main.tileBlockLight[tile.TileType]) return true;
				return null;
			}
			static void ReinitializeTexture() {
				textures[targetResolution / resolutionResolution - 1] ??= new(Main.graphics.GraphicsDevice, targetResolution, 1, false, SurfaceFormat.Single);
				texture = textures[targetResolution / resolutionResolution - 1];
			}
			static readonly TimeSpan targetTimeSpan = new(0, 0, 0, 0, 2);
			public static FrameCachedValue<Texture2D> Texture = new(() => {
				int resolution = targetResolution;
				if (Main.gameMenu) return texture;
				if (texture?.Width != targetResolution) ReinitializeTexture();
				dynamicResolution.Start();
				FastParallel.For(0, resolution, (min, max, _) => {
					for (int i = min; i < max; i++) {
						try {
							Vector2 dir = (i * MathHelper.TwoPi / targetResolution).ToRotationVector2();
							Vector2 position = Main.LocalPlayer.MountedCenter;
							buffer[i] = CollisionExtensions.Raymarch(position, dir, IgnoreGlass, 16 * 20);
						} finally { }
					}
				});
				dynamicResolution.Finish(targetTimeSpan, ref targetResolution);
				texture.SetData(0, new Rectangle(0, 0, resolution, 1), buffer, 0, resolution);
				return texture;
			});
			static Benchmarker dynamicResolution;
			struct Benchmarker {
				Stopwatch stopwatch;
				TimeSpan[] times;
				int index;
				public void Start() {
					stopwatch ??= new();
					stopwatch.Restart();
					times ??= new TimeSpan[20];
				}
				public void Finish(TimeSpan target, ref int targetResolution) {
					stopwatch.Stop();
					times[index++ % times.Length] = stopwatch.Elapsed;
					TimeSpan total = default;
					for (int i = 0; i < times.Length; i++) total += times[i];
					if (targetResolution > minResolution && total > target * (times.Length * 1.25f)) {
						targetResolution -= resolutionResolution;
					} else if (targetResolution < buffer.Length && total <= target * (times.Length * 0.75f)) {
						targetResolution += resolutionResolution;
					}
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
