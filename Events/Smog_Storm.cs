using CalamityMod.NPCs.Yharon;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Graphics.Primitives;
using Origins.Graphics.Unlighting;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Reflection;
using Origins.Tiles.Ashen;
using Origins.World.BiomeData;
using PegasusLib.Graphics;
using ReLogic.Content;
using ReLogic.Threading;
using System;
using System.Diagnostics;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Events {
	[ReinitializeDuringResizeArrays]
	public class Smog_Storm : ModBiome {
		public static Action<Projectile>[] CutThroughSmogStorm = ProjectileID.Sets.Factory.CreateNamedSet(nameof(CutThroughSmogStorm))
		.Description("The provided action will be called while drawing into the brightness mask during a smog storm")
		.RegisterCustomSet<Action<Projectile>>(null);
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
			Action<Projectile> normalDraw = Main.instance.DrawProjDirect;
			Span<int> toNormalDraw = [
				ProjectileID.MagnetSphereBall,
				ProjectileID.TerraBlade2,
				ProjectileID.TerraBlade2Shot,
				ProjectileID.LastPrismLaser,
				ProjectileID.PhantasmalDeathray,
				ProjectileID.FairyQueenHymn,
				ProjectileID.FairyQueenLance,
				ProjectileID.FairyQueenSunDance,
				ProjectileID.HallowBossLastingRainbow,
				ProjectileID.HallowBossRainbowStreak,
				ProjectileID.HallowBossDeathAurora,
				ProjectileID.HallowBossSplitShotCore,
				ModContent.ProjectileType<Laser_Target_Locator_Marker>(),
				ModContent.ProjectileType<Retool_Arm_Laser_Beam>(),
				ModContent.ProjectileType<Third_Eye_Deathray>(),
				ModContent.ProjectileType<Flare_Launcher_Glow_P>(),
			];
			foreach (int type in toNormalDraw) CutThroughSmogStorm[type] = normalDraw;
			CutThroughSmogStorm[ProjectileID.DD2SquireSonicBoom] = DrawWave;
		}
		public static void DrawWave(Projectile projectile) {
			Main.instance.PrepareDrawnEntityDrawing(projectile, Retool_Arm_Laser.ShaderID, null);
			Texture2D texture = TextureAssets.Projectile[ProjectileID.DD2SquireSonicBoom].Value;
			Vector2 diff = (projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 14;
			Color color = new(255, 255, 255, 0);
			DrawData data = new(texture,
				projectile.Center - Main.screenPosition,
				null,
				color,
				projectile.rotation,
				texture.Size() * new Vector2(0.5f, 0.25f),
				2,
				SpriteEffects.None
			);
			const float count = 10;
			for (int i = 0; i < count; i++) {
				data.color = color * (1 - MathF.Pow(i / count, 0.5f));
				Main.EntitySpriteDraw(data);
				data.position -= diff;
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
			active = sky.Opacity != 0;
			if (active) Filters.Scene["Origins:SmogStorm"].GetShader().UseOpacity(sky.Opacity);
		}
		public static void DrawLightMap(Color color) {
			Main.graphics.GraphicsDevice.Textures[0] = lightBufferTexture;
			Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			lightMapStamp.Draw(activeLightTileArea, color, true);
		}
		static bool active;
		static readonly VertexRectangle lightMapStamp = new();
		static RenderTarget2D lightBufferTarget;
		static RenderTarget2D lightBufferTarget2;
		static Texture2D lightBufferTexture;
		static Rectangle activeLightTileArea;
		static Vector2 activeLightPosition;
		static Rectangle workingLightTileArea;
		static Vector2 workingLightPosition;
		class SmogEffect : ModSystem, IBroken {
			static string IBroken.BrokenReason => "TODO: mask EoL";
			const int resolutionResolution = 5;
			const int minResolution = resolutionResolution <= 20 ? 20 : resolutionResolution;
			static Texture2D texture;
			static readonly float[] buffer = new float[400];
			static int targetResolution = 100;
			static Texture2D[] textures = new Texture2D[buffer.Length / resolutionResolution];
			public override void PostUpdateEverything() {
				if (!active) return;
				activeLightTileArea = workingLightTileArea;
				activeLightPosition = workingLightPosition;
				Terraria.Graphics.Shaders.ScreenShaderData shader = Filters.Scene["Origins:SmogStorm"].GetShader();
				//shader.UseImage(LineOfSight.Texture.Value, 1);
				shader.Shader.Parameters["uTexture"].SetValue(SmogEffect.Texture.Value);
				//shader.Shader.Parameters["uLightRegion"].SetValue(new Vector4(activeLightTileArea.TopLeft() * 16, activeLightTileArea.Width * 16, activeLightTileArea.Height * 16));
				//Vector2 zoomCompensation = Main.ScreenSize.ToVector2() * 0.5f * (Vector2.One - Vector2.One / Main.GameViewMatrix.Zoom);
				//shader.Shader.Parameters["uLightOffset"].SetValue(activeLightPosition + zoomCompensation - new Vector2(Main.offScreenRange, Main.offScreenRange));

				if (lightBufferTarget?.Width != Main.screenWidth || lightBufferTarget.Height != Main.screenHeight) {
					lightBufferTarget?.Dispose();
					lightBufferTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, mipMap: false, preferredFormat: SurfaceFormat.Vector4, preferredDepthFormat: DepthFormat.None);
				}

				if (lightBufferTarget2?.Width != Main.screenWidth || lightBufferTarget2.Height != Main.screenHeight) {
					lightBufferTarget2?.Dispose();
					lightBufferTarget2 = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, mipMap: false, preferredFormat: SurfaceFormat.Vector4, preferredDepthFormat: DepthFormat.None);
				}
				RenderTargetBinding[] oldRenderTargets = Main.graphics.GraphicsDevice.GetRenderTargets();
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
				Main.graphics.GraphicsDevice.SetRenderTarget(lightBufferTarget);
				DrawLightMap(Color.White);
				Beacon_Light_TE_System.DrawBeacons(5);
				Main.spriteBatch.Restart(Main.spriteBatch.GetState());
				Main.graphics.GraphicsDevice.SetRenderTarget(lightBufferTarget2);
				DrawLightMap(Color.White);
				using ScopedOverride<int> _ = Main.CurrentDrawnEntityShader.ScopedOverride(Retool_Arm_Laser.ShaderID);
				Beacon_Light_TE_System.DrawBeacons(10);
				foreach (Projectile proj in Main.ActiveProjectiles) CutThroughSmogStorm[proj.type]?.Invoke(proj);
				Main.spriteBatch.End();
				Main.graphics.GraphicsDevice.UseOldRenderTargets(oldRenderTargets);
				shader.UseImage(lightBufferTarget, 0);
				shader.UseImage(lightBufferTarget2, 1);
			}
			public override void Load() {
				Main.QueueMainThreadAction(ReinitializeTexture);
				On_LightingEngine.Present += On_LightingEngine_Present;
			}
			static void On_LightingEngine_Present(On_LightingEngine.orig_Present orig, LightingEngine self) {
				orig(self);
				if (self is Anti_LightingEngine.The_Engine) return;
				workingLightTileArea = LightingMethods._activeProcessedArea.GetValue(self);
				workingLightTileArea.Width++;
				workingLightTileArea.Height++;

				if (lightBufferTexture?.Width != workingLightTileArea.Width || lightBufferTexture.Height != workingLightTileArea.Height) {
					lightBufferTexture?.Dispose();
					lightBufferTexture = InitBufferTexture(workingLightTileArea.Width, workingLightTileArea.Height);
				}
				Vector3[] _colors = LightingMethods._colors.GetValue(LightingMethods._activeLightMap.GetValue(self));
				if (_colors.All(c => c == default)) return;
				Vector4[] colors = new Vector4[_colors.Length];
				for (int i = 0; i < workingLightTileArea.Width; i++) {
					for (int j = 0; j < workingLightTileArea.Height; j++) {
						colors[i + j * workingLightTileArea.Width] = new(_colors[i * (workingLightTileArea.Height - 1) + j], 1);
					}
				}
				lightBufferTexture.SetData(colors);
				workingLightPosition = Main.screenPosition;
			}
			private static Texture2D InitBufferTexture(int width, int height) {
				if (!AssetRepository.IsMainThread) {
					return Main.RunOnMainThread(() => InitBufferTexture(width, height)).GetAwaiter().GetResult();
				}

				return new Texture2D(Main.instance.GraphicsDevice, width, height, mipMap: false, format: SurfaceFormat.Vector4);
			}
			public override void Unload() {
				for (int i = 0; i < textures.Length; i++) textures[i]?.Dispose();
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
							buffer[i] = CollisionExtensions.Raymarch(position, dir, IgnoreGlass, 16 * 10);
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
		public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth) {
			if (Main.gameMenu) Opacity = 0;
			Smog_Storm.DrawLightMap(Color.White * Opacity * 0.75f);
		}
		public override void Reset() { }
		public override Color OnTileColor(Color inColor) => Color.Lerp(inColor, new(inColor.R / 5, 1, 1), Opacity);
		public override void Update(GameTime gameTime) {
			MathUtils.LinearSmoothing(ref Opacity, isActive.ToInt(), 1f / 180);
		}
	}
}
