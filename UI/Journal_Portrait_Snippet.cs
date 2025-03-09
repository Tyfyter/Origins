using Microsoft.Xna.Framework.Graphics;
using PegasusLib.Graphics;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Journal_Portrait_Handler : ITagHandler {
		internal static ShaderLayerTargetHandler shaderOroboros = new();
		public class Journal_Portrait_Snippet : TextSnippet {
			RenderTarget2D renderTarget;
			readonly int id;
			readonly float sharpness;
			readonly bool sketched;
			public Journal_Portrait_Snippet(string text, Options options) : base(text) {
				Color = options.Color;
				sharpness = options.Sharpness;
				sketched = options.Sketch;
				if (!NPCID.Search.TryGetId(text, out id)) {
					id = -1;
					return;
				}
				Text = "";
			}
			void SetupRenderTarget() {
				renderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
				UIBestiaryEntryIcon element = new UIBestiaryEntryIcon(Main.BestiaryDB.FindEntryByNPCID(id), isPortrait: true);
				element.Width.Set(230f, 0);
				element.Height.Set(112f, 0);
				element.Recalculate();
				element.Update(new GameTime());
				bool wasSpritebatchRunning = Main.spriteBatch.IsRunning();
				SpriteBatchState state = Main.spriteBatch.GetState();
				if (wasSpritebatchRunning) {
					Main.spriteBatch.End();
				}
				RenderTargetBinding[] oldRenderTargets = Main.graphics.GraphicsDevice.GetRenderTargets();
				Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
				Main.graphics.GraphicsDevice.Clear(Color.Transparent);
				Main.spriteBatch.Begin();
				if (sketched) {
					shaderOroboros.Capture();
					element.Draw(Main.spriteBatch);
					Origins.journalDrawingShader.UseSaturation(sharpness);
					Origins.journalDrawingShader.UseColor(Color);
					shaderOroboros.Stack(Origins.journalDrawingShader);
					shaderOroboros.Release();
				} else {
					element.Draw(Main.spriteBatch);
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
				size = new(230f, 112f);
				if (renderTarget is null) return true;
				spriteBatch?.Draw(renderTarget, position, new(0, 0, 230, 112), Color.White);
				return true;
			}
			~Journal_Portrait_Snippet() {
				Main.QueueMainThreadAction(() => renderTarget?.Dispose());
			}
		}
		public record struct Options(bool Sketch = false, float Sharpness = 1, Color Color = default);
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			Regex regex = new("(?:(s[\\d\\.]+)|(d))+");
			Options settings = new(Color:baseColor);
			foreach (Group group in regex.Match(options).Groups.Values) {
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
				}
			}
			return new Journal_Portrait_Snippet(text, settings);
		}
	}
}