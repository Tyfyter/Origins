using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Graphics.Primitives;
using Origins.Graphics.Unlighting;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Projectiles;
using Origins.Reflection;
using Origins.Tiles;
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
		public override string BestiaryIcon => "Origins/UI/WorldGen/IconEvilAshen";
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
				ProjectileID.FairyQueenRangedItemShot,
				ProjectileID.FairyQueenMagicItemShot,
				ModContent.ProjectileType<Laser_Target_Locator_Marker>(),
				ModContent.ProjectileType<Retool_Arm_Laser_Beam>(),
				ModContent.ProjectileType<Third_Eye_Deathray>(),
				ModContent.ProjectileType<Flare_Launcher_Glow_P>(),
				ModContent.ProjectileType<Big_Bang_P>(),
				ModContent.ProjectileType<Tank_Rifle_P>(),
				ModContent.ProjectileType<NPCs.Ashen.Boss.Fire_Cannons_State.Trenchmaker_Cannon_P>(),
			];
			foreach (int type in toNormalDraw) CutThroughSmogStorm[type] = normalDraw;
			CutThroughSmogStorm[ProjectileID.DD2SquireSonicBoom] = DrawWave;
			CutThroughSmogStorm[ProjectileID.MagicLantern] = ClearSmog(5);
			CutThroughSmogStorm[ProjectileID.Wisp] = ClearSmog(7);
			CutThroughSmogStorm[ProjectileID.DD2PetGhost] = ClearSmog(10);
			CutThroughSmogStorm[ProjectileID.FairyQueenPet] = ClearSmog(11);
			On_NPC.CanBeChasedBy += On_NPC_CanBeChasedBy;
		}
		static bool On_NPC_CanBeChasedBy(On_NPC.orig_CanBeChasedBy orig, NPC self, object attacker, bool ignoreDontTakeDamage) {
			Player player;
			attacker ??= CurrentProjectile.Projectile;
			switch (attacker) {
				case Player _player:
				player = _player;
				break;
				case ModPlayer _player:
				player = _player.Player;
				break;
				case Projectile projectile:
				if (!projectile.friendly || !projectile.TryGetOwner(out player)) player = null;
				break;
				case ModProjectile proj:
				if (!proj.Projectile.friendly || !proj.Projectile.TryGetOwner(out player)) player = null;
				break;
				default:
				player = null;
				break;
			}
			if ((player?.InModBiome<Smog_Storm>() ?? false) && !self.Hitbox.IsWithin(player.MountedCenter, 16 * (player.detectCreature ? 35 : 16))) return false;
			return orig(self, attacker, ignoreDontTakeDamage);
		}

		public static Action<Projectile> ClearSmog(float scale, bool useOpacity = true) => projectile => {
			float opacity = useOpacity ? projectile.Opacity : 1;
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Flare_Launcher_Glow_P.ID].Value,
				projectile.Center - Main.screenPosition,
				null,
				new Color(255, 255, 255, 0) * opacity,
				0,
				new Vector2(45, 45),
				scale * opacity,
				SpriteEffects.None
			);
		};
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
			if (active) {
				Filters.Scene["Origins:SmogStorm"].GetShader().UseOpacity(sky.Opacity);
				SkyManager.Instance["Sandstorm"].Opacity = 0;
				Filters.Scene["Sandstorm"].Opacity = 0;
				Overlays.Scene["Sandstorm"].Opacity = 0;
			}
		}
		public static void DrawLightMap(Color color) {
			Main.graphics.GraphicsDevice.Textures[0] = lightBufferTexture;
			Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			if (borrowedLightmap) {
				lightMapStamp.DrawWeirdFancyLighting(activeLightTileArea, color, activeLightMatrix, true);
			} else {
				lightMapStamp.Draw(activeLightTileArea, color, activeLightMatrix, true);
			}
		}
		static bool active;
		static readonly VertexRectangle lightMapStamp = new();
		static RenderTarget2D lightBufferTarget;
		static RenderTarget2D lightBufferTarget2;
		static Texture2D lightBufferTexture;
		static Rectangle activeLightTileArea;
		static Rectangle workingLightTileArea;
		static Matrix activeLightMatrix;
		static Matrix workingLightMatrix = Matrix.Identity;
		static bool borrowedLightmap = false;
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
				activeLightMatrix = workingLightMatrix;
				Terraria.Graphics.Shaders.ScreenShaderData shader = Filters.Scene["Origins:SmogStorm"].GetShader();
				shader.Shader.Parameters["uTexture"].SetValue(SmogEffect.Texture.Value);

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
				DrawLightMap(Color.Wheat);
				Beacon_Light_TE_System.DrawBeacons(5);
				Main.spriteBatch.Restart(Main.spriteBatch.GetState());
				Main.graphics.GraphicsDevice.SetRenderTarget(lightBufferTarget2);
				DrawLightMap(Color.White);
				if (Main.LocalPlayer.detectCreature) {
					MainReflection.DrawNPCs(true);
					MainReflection.DrawNPCs(false);
				}
				DrawLightMap(Color.White * 0.5f);
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
				if (ModLoader.TryGetMod("FancyLighting", out Mod mod) && mod.Version >= new Version(1, 1)) Origins.TryHookEvent("FancyLighting", "FancyLighting.SmoothLighting", "PostUpdateLightMap", PostUpdateLightMap);
			}
			static int skipVanilla = 0;
			static void PostUpdateLightMap(Texture2D lightMapTexture, Matrix samplingTransformation, Rectangle lightMapArea, bool cameraMode) {
				skipVanilla = 2;
				workingLightTileArea = lightMapArea;
				workingLightMatrix = samplingTransformation;
				lightBufferTexture = lightMapTexture;
				borrowedLightmap = true;
			}
			static void On_LightingEngine_Present(On_LightingEngine.orig_Present orig, LightingEngine self) {
				orig(self);
				if (self is Anti_LightingEngine.The_Engine) return;
				if (skipVanilla > 0) {
					if (--skipVanilla <= 0) workingLightMatrix = Matrix.Identity;
					return;
				}
				workingLightTileArea = LightingMethods._activeProcessedArea.GetValue(self);
				workingLightTileArea.Width++;
				workingLightTileArea.Height++;

				if (borrowedLightmap || lightBufferTexture?.Width != workingLightTileArea.Width || lightBufferTexture.Height != workingLightTileArea.Height) {
					lightBufferTexture?.Dispose();
					lightBufferTexture = InitBufferTexture(workingLightTileArea.Width, workingLightTileArea.Height);
					borrowedLightmap = false;
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
			}
			private static Texture2D InitBufferTexture(int width, int height) {
				if (!AssetRepository.IsMainThread) {
					return Main.RunOnMainThread(() => InitBufferTexture(width, height)).GetAwaiter().GetResult();
				}

				return new Texture2D(Main.instance.GraphicsDevice, width, height, mipMap: false, format: SurfaceFormat.Vector4);
			}
			public override void Unload() => Main.QueueMainThreadAction(() => {
				for (int i = 0; i < textures.Length; i++) textures[i]?.Dispose();
				texture = null;
				textures = null;
			});

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
		public class SpawnRates : SpawnPool {
			public const float WindPail = 0.8f;
			public const float Cartwheeler_Large = 0.26f;
			public const float Cartwheeler_Medium = 0.27f;
			public const float Cartwheeler_Small = 0.27f;
			public const float Reject_2 = 0.8f;
			public override string Name => $"{nameof(Smog_Storm)}_{base.Name}";
			public override void SetStaticDefaults() {
				Priority = SpawnPoolPriority.Event;
			}
			public override bool IsActive(NPCSpawnInfo spawnInfo) {
				if (!Main.WindyEnoughForKiteDrops) return false;
				return TileLoader.GetTile(spawnInfo.SpawnTileType) is IAshenTile ashenTile && ashenTile.CountsForSpawns(spawnInfo);
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
			Smog_Storm.DrawLightMap(Color.Wheat * Opacity * 0.75f);
		}
		public override void Reset() { }
		public override Color OnTileColor(Color inColor) => Color.Lerp(inColor, new(inColor.R / 5, 1, 1), Opacity);
		public override void Update(GameTime gameTime) {
			MathUtils.LinearSmoothing(ref Opacity, isActive.ToInt(), 1f / 180);
		}
	}
}
