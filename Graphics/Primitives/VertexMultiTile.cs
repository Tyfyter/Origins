using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace Origins.Graphics.Primitives; 
public readonly struct VertexMultiTile(int width, int height) {
	readonly VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[width * height];
	readonly VertexPositionColorTexture[] glowVertices = new VertexPositionColorTexture[width * height];
	readonly short[] dices = Enumerable.Range(0, width - 1).SelectMany(x => Enumerable.Range(0, height - 1).SelectMany<int, short>(y => [
			(short)(x + y * width), (short)(x + 1 + y * width), (short)(x + width + y * width),
			(short)(x + 1 + y * width), (short)(x + 1 + width + y * width), (short)(x + width + y * width),
		])).ToArray();
	private readonly int width = width;
	private readonly int height = height;
	public void Draw(int i, int j, SpriteBatch spriteBatch, params IEnumerable<ILayer> layers) {
		bool usesLit = false;
		foreach (ILayer layer in layers) {
			if (layer.UsesVertices && !layer.UsesGlowVertices) {
				usesLit = true;
				break;
			}
		}
		Vector2 offset = new Vector2(i, j) * 16 - Main.screenPosition;
		Vector2 divisor = new(1f / (width - 1), 1f / (height - 1));
		for (int n = 0; n < vertices.Length; n++) {
			Vector2 pos = new(n % width, n / width);
			if (usesLit) {
				vertices[n].TextureCoordinate = pos * divisor;
				vertices[n].Position = new(pos * 16 + offset, 0);
				vertices[n].Color = Lighting.GetColor(i + n % width, j + n / width);
			}
			glowVertices[n].TextureCoordinate = pos * divisor;
			glowVertices[n].Position = new(pos * 16 + offset, 0);
			glowVertices[n].Color = Color.White;
		}
		Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
		Main.pixelShader.CurrentTechnique.Passes[0].Apply();

		foreach (ILayer layer in layers) layer.Draw(layer.UsesGlowVertices ? glowVertices : vertices, dices, i, j, spriteBatch);
	}
	public interface ILayer {
		public bool UsesVertices => true;
		public bool UsesGlowVertices => false;
		public void Draw(VertexPositionColorTexture[] vertices, short[] dices, int i, int j, SpriteBatch spriteBatch);
	}
	public static ILayer Lit(Texture2D texture) => new LitLayer(texture);
	public static ILayer Glow(Texture2D texture) => new GlowLayer(texture);
	readonly struct LitLayer(Texture2D texture) : ILayer {
		readonly void ILayer.Draw(VertexPositionColorTexture[] vertices, short[] dices, int i, int j, SpriteBatch spriteBatch) {
			Main.graphics.GraphicsDevice.Textures[0] = texture;
			Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, dices, 0, dices.Length / 3);
		}
	}
	readonly struct GlowLayer(Texture2D texture) : ILayer {
		bool ILayer.UsesGlowVertices => true;
		readonly void ILayer.Draw(VertexPositionColorTexture[] vertices, short[] dices, int i, int j, SpriteBatch spriteBatch) {
			Main.graphics.GraphicsDevice.Textures[0] = texture;
			Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, dices, 0, dices.Length / 3);
		}
	}
}
