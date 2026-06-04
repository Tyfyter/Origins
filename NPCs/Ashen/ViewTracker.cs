using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Core.Shaders;
using ReLogic.Threading;
using System;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;

namespace Origins.NPCs.Ashen {
	[Obsolete("Unfinished")]
	file class ViewTracker {
		public readonly float[] distances;
		Triangle viewTriangle;
		Texture2D texture;
		readonly Task createTexture;
		float height, halfBase;
		public ViewTracker(int precision) {
			distances = new float[precision];
			createTexture = Main.RunOnMainThread(() => texture = new(Main.graphics.GraphicsDevice, precision, 1, false, SurfaceFormat.Single));
		}
		public void SetViewTriangle(Vector2 viewPos, Vector2 viewDirection, float height, float halfBase) {
			viewTriangle = ConstructViewTriangle(viewPos, viewDirection, height, halfBase);
			this.height = height;
			this.halfBase = halfBase;
		}
		public static Triangle ConstructViewTriangle(Vector2 viewPos, Vector2 viewDirection, float height, float halfBase) => new(
			viewPos,
			viewPos + viewDirection * height + viewDirection.Perpendicular(1) * halfBase,
			viewPos + viewDirection * height + viewDirection.Perpendicular(-1) * halfBase
		);
		public void UpdateDistances() {
			if (viewTriangle.a == default && viewTriangle.b == default && viewTriangle.c == default) return;
			float inverseCount = 1f / distances.Length;
			FastParallel.For(0, distances.Length, (min, max, _) => {
				Vector2 position = Main.LocalPlayer.MountedCenter;
				for (int i = min; i < max; i++) {
					try {
						Vector2 diff = Vector2.Lerp(viewTriangle.b - viewTriangle.a, viewTriangle.c - viewTriangle.a, i * inverseCount);
						distances[i] = CollisionExtensions.Raymarch(viewTriangle.a, diff.Normalized(out float maxDist), IgnoreGlass, maxDist);
					} finally { }
				}
			});
		}
		static bool? IgnoreGlass(Tile tile) {
			if (!tile.HasTile || !Main.tileBlockLight[tile.TileType]) return true;
			return null;
		}
		static Parameter uImageSize0;
		public void Draw(Color color, Vector2 screenPos) {
			MiscShaderData shader = GameShaders.Misc["Origins:ViewTriangle"];
			shader.CreateParameter(ref uImageSize0, nameof(uImageSize0), new Vector2(halfBase, height));
			shader.UseSamplerState(SamplerState.LinearClamp)
			.Apply(default, uImageSize0);
			vertices[2].TextureCoordinate.X = 1 + 1f / distances.Length;
			vertices[0].Color = color;
			vertices[0].Position = new(viewTriangle.a - screenPos, 0);
			vertices[1].Position = new(viewTriangle.b - screenPos, 0);
			vertices[2].Position = new(viewTriangle.c - screenPos, 0);
			if (!createTexture.IsCompleted) return;
			texture.SetData(distances);
			Main.graphics.GraphicsDevice.Textures[0] = texture;
			Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
			Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length, dices, 0, 2);
		}
		static readonly VertexPositionColorTexture[] vertices = [
			new() { TextureCoordinate = new(0.5f, 0) },
			new() { TextureCoordinate = Vector2.UnitY },
			new() { TextureCoordinate = Vector2.One }
		];
		static readonly short[] dices = [0, 1, 2];
	}
}
