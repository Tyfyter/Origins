using Microsoft.Xna.Framework.Graphics;
using PegasusLib.Graphics;
using PegasusLib.Reflection;
using ReLogic.Content;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Dev {
	public class SpriteGenerator : ReflectionLoader {
		public static FastFieldInfo<SpriteBatch, bool> beginCalled { get; private set; }
		public static Texture2D Generate(Action<SpriteBatch> draw, (int X, int Y) size) {
			RenderTarget2D renderTarget = new(Main.graphics.GraphicsDevice, size.X, size.Y);
			SpriteBatch spriteBatch = new(Main.graphics.GraphicsDevice);
			SpriteBatchState state = null;
			if (beginCalled.GetValue(Main.spriteBatch)) {
				state = Main.spriteBatch.GetState();
				Main.spriteBatch.End();
			}
			renderTarget.GraphicsDevice.SetRenderTarget(renderTarget);
			renderTarget.GraphicsDevice.Clear(Color.Transparent);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
			SpriteBatch realMainSB = Main.spriteBatch;
			Vector2 screenPosition = Main.screenPosition;
			int screenWidth = Main.screenWidth;
			int screenHeight = Main.screenHeight;
			bool screenMaximized = Main.screenMaximized;
			try {
				Main.spriteBatch = spriteBatch;
				Main.screenWidth = size.X;
				Main.screenHeight = size.Y;
				draw(spriteBatch);
			} finally {
				Main.spriteBatch = realMainSB;
				Main.screenPosition = screenPosition;
				Main.screenWidth = screenWidth;
				Main.screenHeight = screenHeight;
				Main.screenMaximized = screenMaximized;
			}
			spriteBatch.End();
			renderTarget.GraphicsDevice.SetRenderTarget(null);
			if (state is SpriteBatchState spriteBatchState) {
				Main.spriteBatch.Begin(spriteBatchState);
			}
			return renderTarget;
		}
		public static Texture2D GenerateArmorSprite(Player player, ItemSlotSet itemSlotSet, PlayerShaderSet shaderSet = default) {
			return Generate(
				(spriteBatch) => {
					player.ResetEffects();
					player.direction = 1;
					Main.screenPosition = player.position - new Vector2(8, 7);
					itemSlotSet.Apply(player);
					shaderSet.Apply(player);
					for (int i = 0; i < 3; i++) {
						for (int j = 0; j < 9; j++) {
							Lighting.AddLight(player.position + new Vector2(i / 2f, j / 8f) * player.Size, Vector3.One);
						}
					}
					Main.PlayerRenderer.DrawPlayer(Main.Camera, player, player.position + new Vector2(0, player.gfxOffY), 0, Vector2.Zero);
				},
				(32, 52)
			);
		}
		public static Texture2D GenerateTileSprite(int type, int style) {
			if (TileObjectData.GetTileData(type, style) is TileObjectData objectData) {
				return Generate(
					(spriteBatch) => {
						Texture2D texture = TextureAssets.Tile[type].Value;
						Vector2 p = new(16);
						int baseFrameX = 0;
						int baseFrameY = 0;
						int styleX = objectData.CalculatePlacementStyle(style, 0, 0);
						int styleY = 0;
						if (objectData.StyleWrapLimit > 0) {
							styleY = styleX / objectData.StyleWrapLimit * objectData.StyleLineSkip;
							styleX %= objectData.StyleWrapLimit;
						}
						if (objectData.StyleHorizontal) {
							baseFrameX = objectData.CoordinateFullWidth * styleX;
							baseFrameY = objectData.CoordinateFullHeight * styleY;
						} else {
							baseFrameX = objectData.CoordinateFullWidth * styleY;
							baseFrameY = objectData.CoordinateFullHeight * styleX;
						}
						for (int i = 0; i < objectData.Width; i++) {
							int frameX = baseFrameX + i * (objectData.CoordinateWidth + objectData.CoordinatePadding);
							int frameY = baseFrameY;
							for (int j = 0; j < objectData.Height; j++) {
								spriteBatch.Draw(texture, p * new Vector2(i, j), new Rectangle(frameX, frameY, 18, 18), Color.White);
								frameY += objectData.CoordinateHeights[j] + objectData.CoordinatePadding;
							}
						}
					},
					(objectData.CoordinateFullWidth, objectData.CoordinateFullHeight)
				);
			} else {
				return Generate(
					(spriteBatch) => {
						Texture2D texture = TextureAssets.Tile[type].Value;
						Vector2 p = new(16);
						int f = 18;
						Rectangle frame = new(0, 0, 16, 16);
						for (int i = 0; i < 3; i++) {
							for (int j = 0; j < 3; j++) {
								switch (i + j * 3) {
									case 0 + 0 * 3:
									frame.X = f * 2;
									frame.Y = f * 3;
									break;
									case 1 + 0 * 3:
									frame.X = f * 2;
									frame.Y = f * 0;
									break;
									case 2 + 0 * 3:
									frame.X = f * 3;
									frame.Y = f * 3;
									break;

									case 0 + 1 * 3:
									frame.X = f * 0;
									frame.Y = f * 1;
									break;
									case 1 + 1 * 3:
									frame.X = f * 2;
									frame.Y = f * 1;
									break;
									case 2 + 1 * 3:
									frame.X = f * 4;
									frame.Y = f * 1;
									break;

									case 0 + 2 * 3:
									frame.X = f * 2;
									frame.Y = f * 4;
									break;
									case 1 + 2 * 3:
									frame.X = f * 2;
									frame.Y = f * 2;
									break;
									case 2 + 2 * 3:
									frame.X = f * 3;
									frame.Y = f * 4;
									break;
								}
								spriteBatch.Draw(texture, p * new Vector2(i, j), frame, Color.White);
							}
						}
					},
					(16 * 3, 16 * 3)
				);
			}
		}
		public static (Texture2D texture, int frames)[] GenerateAnimationSprite(Texture2D texture, params (Rectangle frame, int frames)[] frames) {
			return frames.Select(frame => (Generate(
				(spriteBatch) => {
					spriteBatch.Draw(texture, Vector2.Zero, frame.frame, Color.White);
				},
				(frame.frame.Width, frame.frame.Height)
			), frame.frames)).ToArray();
		}
		public static (Texture2D texture, int frames)[] GenerateAnimationSprite(Texture2D texture, DrawAnimation animation) {
			(Texture2D texture, int frames)[] frames = new (Texture2D texture, int frames)[animation.FrameCount];
			int frameNum = animation.Frame;
			for (int i = 0; i < animation.FrameCount; i++) {
				animation.Frame = i;
				Rectangle frame = animation.GetFrame(texture);
				frames[i] = (Generate(
					(spriteBatch) => {
						spriteBatch.Draw(texture, Vector2.Zero, frame, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
					},
					(frame.Width, frame.Height)
				), animation.TicksPerFrame);
			}
			animation.Frame = frameNum;
			return frames;
		}
		public static (Texture2D texture, int frames)[] GenerateAnimationSprite(NPC npc, Rectangle area, int ticks, int framesPerFrame = 1) {
			(Texture2D texture, int frames)[] frames = new (Texture2D texture, int frames)[ticks];
			UnlockableNPCEntryIcon icon = new(npc.netID);
			EntryIconDrawSettings iconDrawSettings = new() {
				IsPortrait = true,
				iconbox = area
			};
			for (int i = 0; i < ticks; i++) {
				icon.Update(default, default, iconDrawSettings);
				frames[i] = (Generate(
					(spriteBatch) => {
						icon.Draw(default, spriteBatch, iconDrawSettings);
					},
					(area.Width, area.Height)
				), framesPerFrame);
			}
			return frames;
		}
		public static (Texture2D texture, int frames)[] GenerateAnimationSprite(Texture2D texture, int frames, int framesPerFrame) {
			(Texture2D texture, int frames)[] sprites = new (Texture2D texture, int frames)[frames];
			Rectangle frame = texture.Frame(verticalFrames: frames);
			for (int i = 0; i < frames; i++) {
				sprites[i] = (Generate(
					(spriteBatch) => {
						spriteBatch.Draw(texture, Vector2.Zero, frame, Color.White);
					},
					(frame.Width, frame.Height)
				), framesPerFrame);
				frame.Y += frame.Height;
			}
			return sprites;
		}
	}
	public class DyeTextureCommand : ModCommand {
#if !DEBUG
		public override bool IsLoadingEnabled(Mod mod) => false;
#endif
		public override CommandType Type => CommandType.Chat;
		public override string Command => "dye";
		public override string Usage => "/dye <texture> <dye item> <save path>";
		public override string Description => "Dyes a texture and saves it to a file";
		public override bool IsCaseSensitive => true;
		public override void Action(CommandCaller caller, string input, string[] args) {
			if (!ModContent.RequestIfExists(args[0], out Asset<Texture2D> texture, AssetRequestMode.ImmediateLoad)) {
				caller.Reply($"Texture \"{args[0]}\" does not exist");
				return;
			}
			if (!int.TryParse(args[1], out int dyeID) && !ItemID.Search.TryGetId(args[1], out dyeID)) {
				caller.Reply($"Item \"{args[1]}\" does not exist");
				return;
			}
			if (dyeID < 0 || dyeID > ItemLoader.ItemCount) {
				caller.Reply($"Item \"{args[1]}\" does not exist");
				return;
			}
			ArmorShaderData shader = GameShaders.Armor.GetShaderFromItemId(dyeID);
			if (shader == null) {
				caller.Reply($"Item \"{args[1]}\" does not have an associated shader");
				return;
			}
			args[2] = string.Join(" ", args[2..]);
			Array.Resize(ref args, 3);
			FileInfo saveFile = new(args[2]);
			if (saveFile.Exists) {
				caller.Reply($"Cannot overwrite existing file \"{args[2]}\"");
				return;
			}
			SpriteGenerator.Generate(spriteBatch => {
				DrawData data = new(texture.Value, Vector2.Zero, Color.White);
				shader.Apply(Main.LocalPlayer, data);
				data.Draw(spriteBatch);
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			}, (texture.Width(), texture.Height())).SaveAsPng(saveFile.OpenWrite(), texture.Width(), texture.Height());
		}
	}
}
