using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Other.Dyes;
using Origins.Items.Weapons.Melee;
using PegasusLib.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Graphics {
	public interface ITangelaHaver {
		public int? TangelaSeed { get; set; }
	}
	class AntiGrayArmorShaderData() : ArmorShaderData(ModContent.Request<Effect>("Origins/Effects/AntiGrayDye"), "AntiGrayDye") {
		public override void Apply(Entity entity, DrawData? drawData = null) {
			if (drawData is DrawData data) {
				data.shader = 0;
				TangelaVisual.drawDatas.Add(new(data, entity?.whoAmI ?? drawData.Value.texture.Name?.GetHashCode() ?? 0));
			}
			base.Apply(entity, drawData);
		}
	}
	class TangelaArmorShaderData() : ArmorShaderData(ModContent.Request<Effect>("Origins/Effects/Tangela"), "Tangela"), ITangelaHaver {
		private readonly Asset<Texture2D> _uImage2 = ModContent.Request<Texture2D>("Terraria/Images/Misc/Perlin");
		public int? TangelaSeed { get; set; }
		public override void Apply(Entity entity, DrawData? drawData = null) {
			if (drawData is DrawData data) {
				data.shader = TangelaVisual.ShaderID;
				data.color = Color.White;
				TangelaVisual.drawDatas.Add(new(data, entity?.whoAmI ?? drawData.Value.texture.Name?.GetHashCode() ?? 0));
			}
			if (_uImage2 != null) {
				Main.graphics.GraphicsDevice.Textures[2] = _uImage2.Value;
				Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;
				Shader.Parameters["uImageSize2"]?.SetValue(_uImage2.Size());
			}
			base.Apply(entity, drawData);
		}
	}
	public static class TangelaVisual {
		public static int ShaderID { get; private set; }
		public static int FakeShaderID => Tangela_Dye.ShaderID;
		class ArmorShaderDataWithAnotherImage(Asset<Effect> shader, string passName) : ArmorShaderData(shader, passName) {
			private Asset<Texture2D> _uImage2;
			public override void Apply(Entity entity, DrawData? drawData = null) {
				if (_uImage2 != null) {
					Main.graphics.GraphicsDevice.Textures[2] = _uImage2.Value;
					Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;
					Shader.Parameters["uImageSize2"]?.SetValue(_uImage2.Size());
				}
				base.Apply(entity, drawData);
			}

			public ArmorShaderData UseImage2(Asset<Texture2D> asset) {
				_uImage2 = asset;
				return this;
			}
		}
		public static void LoadShader() {
			GameShaders.Armor.BindShader(ModContent.ItemType<Krakram>(), new ArmorShaderDataWithAnotherImage(
				ModContent.Request<Effect>("Origins/Effects/Tangela"),
				"Tangela"
			))
			.UseImage2(ModContent.Request<Texture2D>("Terraria/Images/Misc/Perlin"))
			.UseImage(ModContent.Request<Texture2D>("Terraria/Images/Misc/noise"));
			ShaderID = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<Krakram>());

			Filters.Scene.OnPostDraw += Scene_OnPostDraw;
		}
		static DateTime changeModeTime = DateTime.MinValue;
		public static bool DrawOver { get; private set; } = false;
		public static readonly List<TangelaDrawData> drawDatas = [];
		static readonly List<RenderTarget2D> renderTargetsToDispose = [];
		public static void SendRenderTargetForDisposal(ref RenderTarget2D renderTarget) {
			renderTargetsToDispose.Add(renderTarget);
			renderTarget = null;
		}
		private static void Scene_OnPostDraw() {
			if (DateTime.Now > changeModeTime) {
				DrawOver = Main.rand.NextBool(3);
				changeModeTime = DateTime.Now + TimeSpan.FromSeconds(Main.rand.Next(4, 11)) / (1 + DrawOver.ToInt());
			}
			if (drawDatas.Count <= 0) return;
			if (Main.gameMenu && Overlays.Scene["Origins:ZoneDefiled"] is Tangela_Resaturate_Overlay resaturateOverlay) {
				resaturateOverlay.Draw(Main.spriteBatch);
			}
			if (DrawOver && !Main.gameMenu) {
				try {
					Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
					ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(ShaderID, Main.LocalPlayer);
					for (int i = 0; i < drawDatas.Count; i++) {
						(DrawData data, int seed, Vector2 extraOffset) = drawDatas[i];
						if (data.shader == ShaderID) {
							FastRandom random = new(seed);
							shader.Shader.Parameters["uOffset"]?.SetValue(new Vector2(random.NextFloat(), random.NextFloat()) * 512 + extraOffset);
							shader.Apply(null, data);
						} else {
							continue;
						}
						data.Draw(Main.spriteBatch);
					}
				} finally {
					Main.spriteBatch.End();
				}
			}
			ClearQueues();
		}
		internal static void ClearQueues() {
			drawDatas.Clear();
			for (int i = 0; i < renderTargetsToDispose.Count; i++) {
				renderTargetsToDispose[i].Dispose();
			}
			renderTargetsToDispose.Clear();
		}
		public static void DrawTangela(this ITangelaHaver tangelaHaver, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, Vector2 extraOffset = default) {
			if (!tangelaHaver.TangelaSeed.HasValue) tangelaHaver.TangelaSeed = Main.rand.Next();
			DrawTangela(texture, position, sourceRectangle, rotation, origin, scale, effects, tangelaHaver.TangelaSeed.Value, extraOffset);
		}
		public static void DrawTangela(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, int tangelaSeed, Vector2 extraOffset = default) {
			DrawTangela(new(
				texture,
				position,
				sourceRectangle,
				Color.White,
				rotation,
				origin,
				scale,
				effects
			), tangelaSeed, extraOffset);
		}
		public static void DrawTangela(DrawData data, int tangelaSeed, Vector2 extraOffset = default) {
			if (OriginsModIntegrations.DrawingAOMap) return;
			data.shader = ShaderID;
			data.color = Color.White;
			drawDatas.Add(new(data, tangelaSeed, extraOffset));
			if (DrawOver && !Main.gameMenu) return;
			SpriteBatchState state = Main.spriteBatch.GetState();
			try {
				Main.spriteBatch.Restart(state, SpriteSortMode.Immediate);
				ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(ShaderID, Main.LocalPlayer);
				FastRandom random = new(tangelaSeed);
				shader.Shader.Parameters["uOffset"]?.SetValue(new Vector2(random.NextFloat(), random.NextFloat()) * 512 + extraOffset);
				shader.Apply(null, data);
				data.Draw(Main.spriteBatch);
			} finally {
				Main.spriteBatch.Restart(state);
			}
		}
		public static void DrawAntiGray(DrawData data) {
			drawDatas.Add(new(data, default, default));
			data.Draw(Main.spriteBatch);
		}
	}
	public record struct TangelaDrawData(DrawData Data, int Seed, Vector2 ExtraOffset = default);
	public class Tangela_Resaturate_Overlay() : Overlay(EffectPriority.High, RenderLayers.All), IUnloadable {
		static uint lastGameFrameCount;
		public override void Draw(SpriteBatch spriteBatch) {
			if (spriteBatch is null) return;
			if (lastGameFrameCount == Origins.gameFrameCount) return;
			lastGameFrameCount = Origins.gameFrameCount;
			if (!Lighting.NotRetro) {
				TangelaVisual.ClearQueues();
				return;
			}
			if (TangelaVisual.DrawOver && !Main.gameMenu) {
				bool anyAntiGray = false;
				for (int i = 0; i < TangelaVisual.drawDatas.Count; i++) {
					if (TangelaVisual.drawDatas[i].Data.shader != TangelaVisual.ShaderID) {
						anyAntiGray = true;
						break;
					}
				}
				if (!anyAntiGray) return;
			}
			if (renderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return;
			}
			try {
				Origins.shaderOroboros.Capture();
				if (Main.gameMenu) Main.spriteBatch.Restart(Main.spriteBatch.GetState(), transformMatrix: Main.UIScaleMatrix);
				else Main.spriteBatch.Restart(Main.spriteBatch.GetState(), transformMatrix: Main.GameViewMatrix.EffectMatrix);
				for (int i = 0; i < TangelaVisual.drawDatas.Count; i++) {
					DrawData data = TangelaVisual.drawDatas[i].Data;
					if (!TangelaVisual.DrawOver || data.shader != TangelaVisual.ShaderID) data.Draw(spriteBatch);
				}
			} finally {
				Origins.shaderOroboros.DrawContents(renderTarget, Color.White, Main.Transform);
				Origins.shaderOroboros.Reset(default);
			}
			Filters.Scene["Origins:ZoneDefiled"].GetShader().UseImage(renderTarget, 1);
			Filters.Scene["Origins:MaskedRasterizeFilter"].GetShader().UseImage(renderTarget, 2);
		}
		public override void Update(GameTime gameTime) { }
		public override void Activate(Vector2 position, params object[] args) { }
		public override void Deactivate(params object[] args) { }
		public override bool IsVisible() {
			return Mask_Rasterize.TimeSinceActive < 2 || Filters.Scene["Origins:ZoneDefiled"].IsActive();
		}

		public void Unload() {
			if (renderTarget is not null) {
				Main.QueueMainThreadAction(renderTarget.Dispose);
				Main.OnResolutionChanged -= Resize;
			}
		}
		internal RenderTarget2D renderTarget;
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			renderTarget.Dispose();
			SetupRenderTargets();
		}
		void SetupRenderTargets() {
			if (renderTarget is not null && !renderTarget.IsDisposed) return;
			renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}
}
