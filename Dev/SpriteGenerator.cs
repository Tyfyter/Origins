﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Dev {
	public static class SpriteGenerator {
		public static Texture2D Generate(Action<SpriteBatch> draw, (int X, int Y) size) {
			RenderTarget2D renderTarget = new(Main.graphics.GraphicsDevice, size.X, size.Y);
			SpriteBatch spriteBatch = new(Main.graphics.GraphicsDevice);
			renderTarget.GraphicsDevice.SetRenderTarget(renderTarget);
			renderTarget.GraphicsDevice.Clear(Color.Transparent);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
			SpriteBatch realMainSB = Main.spriteBatch;
			Vector2 screenPosition = Main.screenPosition;
			int screenWidth = Main.screenWidth;
			int screenHeight = Main.screenHeight;
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
			}
			spriteBatch.End();
			return renderTarget;
		}
		public static Texture2D GenerateArmorSprite(Player player, ItemSlotSet itemSlotSet, PlayerShaderSet shaderSet = default) {
			return Generate(
				(spriteBatch) => {
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
						Rectangle frame = new Rectangle(0, 0, 16, 16);
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
	}
	public class SingleItemEnumerator<T> : IEnumerable<T> {
		readonly T item;
		public SingleItemEnumerator(T item) {
			this.item = item;
		}
		public IEnumerator<T> GetEnumerator() {
			yield return item;
		}
		IEnumerator IEnumerable.GetEnumerator() {
			yield return item;
		}
	}
}