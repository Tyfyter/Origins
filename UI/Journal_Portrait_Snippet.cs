using Microsoft.Xna.Framework.Graphics;
using PegasusLib.Graphics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI.Chat;
using CalamityMod.Projectiles.Magic;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.Utilities;
using Origins.Graphics;
using Terraria.GameContent;
using Terraria.ModLoader;
using PegasusLib;

namespace Origins.UI {
	public class Journal_Portrait_Handler : ITagHandler {
		internal static ShaderLayerTargetHandler shaderOroboros = new();
		public class Journal_Portrait_Snippet : TextSnippet {
			RenderTarget2D renderTarget;
			readonly int id;
			readonly Options options;
			public Journal_Portrait_Snippet(string text, Options options) : base(text) {
				Color = options.Color;
				this.options = options;
				if (!NPCID.Search.TryGetId(text, out id)) {
					id = -1;
					return;
				}
				Text = "";
			}
			void SetupRenderTarget() {
				renderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
				BestiaryEntry entry = Main.BestiaryDB.FindEntryByNPCID(id);
				Rectangle dimensions = new(0, 0, (int)options.Width, (int)options.Height);
				BestiaryUICollectionInfo _collectionInfo = new() {
					OwnerEntry = entry,
					UnlockState = BestiaryEntryUnlockState.CanShowStats_2
				};
				EntryIconDrawSettings settings = new() {
					iconbox = dimensions,
					IsPortrait = true,
					IsHovered = false
				};
				if (NPCID.Sets.NPCBestiaryDrawOffset.TryGetValue(id, out NPCID.Sets.NPCBestiaryDrawModifiers value) && value.CustomTexturePath != null) {
					ModContent.Request<Texture2D>(value.CustomTexturePath, ReLogic.Content.AssetRequestMode.ImmediateLoad);
				}
				entry.Icon.Update(_collectionInfo, dimensions, settings);
				bool wasSpritebatchRunning = Main.spriteBatch.IsRunning();
				SpriteBatchState state = Main.spriteBatch.GetState();
				if (wasSpritebatchRunning) {
					Main.spriteBatch.End();
				}
				RenderTargetBinding[] oldRenderTargets = Main.graphics.GraphicsDevice.GetRenderTargets();
				Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
				Main.graphics.GraphicsDevice.Clear(Color.Transparent);
				Main.spriteBatch.Begin();
				switch (options.Shader) {
					case JournalImageShader.Sketch:
					shaderOroboros.Capture();
					Main.spriteBatch.Restart(Main.spriteBatch.GetState(), rasterizerState: RasterizerState.CullNone);
					entry.Icon.Draw(_collectionInfo, Main.spriteBatch, settings);
					Origins.journalDrawingShader.UseSaturation(options.Sharpness);
					Origins.journalDrawingShader.UseColor(Color);
					shaderOroboros.Stack(Origins.journalDrawingShader);
					shaderOroboros.Release();
					break;

					case JournalImageShader.Transparent:
					shaderOroboros.Capture();
					Main.spriteBatch.Restart(Main.spriteBatch.GetState(), rasterizerState: RasterizerState.CullNone);
					entry.Icon.Draw(_collectionInfo, Main.spriteBatch, settings);
					shaderOroboros.Stack(Origins.journalTransparentShader);
					shaderOroboros.Release();
					break;

					default:
					entry.Icon.Draw(_collectionInfo, Main.spriteBatch, settings);
					break;
				}
				if (TangelaVisual.drawDatas.Count > 0) {
					try {
						Main.spriteBatch.Restart(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
						ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(TangelaVisual.ShaderID, Main.LocalPlayer);
						for (int i = 0; i < TangelaVisual.drawDatas.Count; i++) {
							(DrawData data, int seed, Vector2 extraOffset) = TangelaVisual.drawDatas[i];
							FastRandom random = new(seed);
							shader.Shader.Parameters["uOffset"]?.SetValue(new Vector2(random.NextFloat(), random.NextFloat()) * 512 + extraOffset);
							shader.Apply(null, data);
							data.Draw(Main.spriteBatch);
						}
					} finally {
						Main.spriteBatch.Restart();
					}
					TangelaVisual.drawDatas.Clear();
				}
				Main.spriteBatch.End();
				if (wasSpritebatchRunning) {
					Main.spriteBatch.Begin(state);
				}
				Main.spriteBatch.GraphicsDevice.UseOldRenderTargets(oldRenderTargets);
			}
			public override void Update() {
				if (renderTarget is null) SetupRenderTarget();
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				if (!string.IsNullOrWhiteSpace(Text) || id == -1) {
					size = default;
					return false;
				}
				size = new(options.Width, options.LineHeight ?? options.Height);
				if (renderTarget is null || justCheckingString) return true;
				//spriteBatch?.Draw(TextureAssets.MagicPixel.Value, position, new(0, 0, (int)options.Width, (int)options.Height), Color.Red);
				spriteBatch?.Draw(renderTarget, position, new(0, 0, (int)options.Width, (int)options.Height), Color.White);
				return true;
			}
			~Journal_Portrait_Snippet() {
				Main.QueueMainThreadAction(() => renderTarget?.Dispose());
			}
		}
		public record struct Options(JournalImageShader Shader = JournalImageShader.None, float Sharpness = 1, Color Color = default, float Width = 230f, float Height = 112f, float? LineHeight = null) {
			public readonly bool Sketch => Shader == JournalImageShader.Sketch;
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			Options settings = new(Color: baseColor);
			SnippetHelper.ParseOptions(options,
				SnippetOption.CreateFloatOption("lh", match => settings.LineHeight = match),
				SnippetOption.CreateFloatOption("w", match => settings.Width = match),
				SnippetOption.CreateFloatOption("h", match => settings.Height = match),
				SnippetOption.CreateFloatOption("s", match => settings.Sharpness = match),
				new SnippetOption("d", "", match => settings.Shader = JournalImageShader.Sketch),
				new SnippetOption("t", "", match => settings.Shader = JournalImageShader.Transparent),
				SnippetOption.CreateColorOption("c", value => settings.Color = value)
			);
			return new Journal_Portrait_Snippet(text, settings);
		}
	}
}