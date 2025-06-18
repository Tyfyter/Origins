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
				if (renderTarget is null || justCheckingString) return true;
				//spriteBatch?.Draw(TextureAssets.MagicPixel.Value, position, new(0, 0, (int)options.Width, (int)options.Height), Color.Red);
				spriteBatch?.Draw(renderTarget, position, new(0, 0, (int)options.Width, (int)options.Height), Color.White);
				return true;
			}
			~Journal_Portrait_Snippet() {
				Main.QueueMainThreadAction(() => renderTarget?.Dispose());
			}
		}
		public record struct Options(JournalImageShader Shader = JournalImageShader.None, float Sharpness = 1, Color Color = default, float Width = 230f, float Height = 112f) {
			public readonly bool Sketch => Shader == JournalImageShader.Sketch;
		}
		record struct SnippetOption(string Name, [StringSyntax(StringSyntaxAttribute.Regex)] string Data, Action<string> Action) {
			public readonly string Pattern => Name + Data;
			public static SnippetOption CreateColorOption(string name, Action<Color> setter) {
				return new(name, "[\\da-fA-F]{3,8}", match => {
					int Parse(int index, int size) {
						int startIndex = (index * size);
						return Convert.ToInt32(match[startIndex..(startIndex + size)], 16);
					}
					switch (match.Length) {
						case 8:
						setter(new Color(Parse(0, 2), Parse(1, 2), Parse(2, 2), Parse(3, 2)));
						break;
						case 6:
						setter(new Color(Parse(0, 2), Parse(1, 2), Parse(2, 2)));
						break;
						case 4:
						setter(new Color(Parse(0, 1) * 16 - 1, Parse(1, 1) * 16 - 1, Parse(2, 1) * 16 - 1, Parse(3, 1) * 16 - 1));
						break;
						case 3:
						setter(new Color(Parse(0, 1) * 16 - 1, Parse(1, 1) * 16 - 1, Parse(2, 1) * 16 - 1));
						break;
						default:
						throw new FormatException($"Malformed color code {match}");
					}
				});
			}
		}
		static void ParseOptions(string optionsText, params SnippetOption[] options) {
			Regex regex = new($"(?:{string.Join("|", options.Select(so => $"({so.Pattern})"))})+");
			GroupCollection groups = regex.Match(optionsText).Groups;
			for (int i = 0; i < groups.Count - 1; i++) {
				string match = groups[i + 1].Value;
				if (match.Length <= 0) continue;
				options[i].Action(match[options[i].Name.Length..]);
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			Options settings = new(Color: baseColor);
			ParseOptions(options,
				new SnippetOption("w", "[\\d\\.]+", match => settings.Width = float.Parse(match)),
				new SnippetOption("h", "[\\d\\.]+", match => settings.Height = float.Parse(match)),
				new SnippetOption("s", "[\\d\\.]+", match => settings.Sharpness = float.Parse(match)),
				new SnippetOption("d", "", match => settings.Shader = JournalImageShader.Sketch),
				new SnippetOption("t", "", match => settings.Shader = JournalImageShader.Transparent),
				SnippetOption.CreateColorOption("c", value => settings.Color = value)
			);
			return new Journal_Portrait_Snippet(text, settings);
		}
	}
}