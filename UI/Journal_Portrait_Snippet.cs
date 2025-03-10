using Microsoft.Xna.Framework.Graphics;
using PegasusLib.Graphics;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI.Chat;

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
				//entry.Icon.Update(_collectionInfo, dimensions, settings);
				bool wasSpritebatchRunning = Main.spriteBatch.IsRunning();
				SpriteBatchState state = Main.spriteBatch.GetState();
				if (wasSpritebatchRunning) {
					Main.spriteBatch.End();
				}
				RenderTargetBinding[] oldRenderTargets = Main.graphics.GraphicsDevice.GetRenderTargets();
				Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
				Main.graphics.GraphicsDevice.Clear(Color.Transparent);
				Main.spriteBatch.Begin();
				if (options.Sketch) {
					shaderOroboros.Capture();
					entry.Icon.Draw(_collectionInfo, Main.spriteBatch, settings);
					Origins.journalDrawingShader.UseSaturation(options.Sharpness);
					Origins.journalDrawingShader.UseColor(Color);
					shaderOroboros.Stack(Origins.journalDrawingShader);
					shaderOroboros.Release();
				} else {
					entry.Icon.Draw(_collectionInfo, Main.spriteBatch, settings);
				}
				Main.spriteBatch.End();
				if (wasSpritebatchRunning) {
					Main.spriteBatch.Begin(state);
				}
				Main.spriteBatch.UseOldRenderTargets(oldRenderTargets);
			}
			public override void Update() {
				if (renderTarget is null) SetupRenderTarget();
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				if (!string.IsNullOrWhiteSpace(Text) || id == -1) {
					size = default;
					return false;
				}
				size = new(options.Width, options.Height);
				if (renderTarget is null) return true;
				spriteBatch?.Draw(renderTarget, position, new(0, 0, (int)options.Width, (int)options.Height), Color.White);
				return true;
			}
			~Journal_Portrait_Snippet() {
				Main.QueueMainThreadAction(() => renderTarget?.Dispose());
			}
		}
		public record struct Options(bool Sketch = false, float Sharpness = 1, Color Color = default, float Width = 230f, float Height = 112f);
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			Regex regex = new("(?:(s[\\d\\.]+)|(d)|(w[\\d\\.]+)|(h[\\d\\.]+))+");
			Options settings = new(Color: baseColor);
			bool first = true;
			foreach (Group group in regex.Match(options).Groups.Values) {
				if (first) {
					first = false;
					continue;
				}
				if (group.Value.Length <= 0) continue;
				switch (group.Value[0]) {
					///sharpness
					case 's':
					settings.Sharpness = float.Parse(group.Value[1..]);
					break;
					///sketch instead of full color
					case 'd':
					settings.Sketch = true;
					break;
					///width
					case 'w':
					settings.Width = float.Parse(group.Value[1..]);
					break;
					///height
					case 'h':
					settings.Height = float.Parse(group.Value[1..]);
					break;
				}
			}
			return new Journal_Portrait_Snippet(text, settings);
		}
	}
}