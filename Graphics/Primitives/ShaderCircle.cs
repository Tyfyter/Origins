using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Core.Shaders;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Graphics.Primitives;
[ReinitializeDuringResizeArrays]
public static class ShaderCircle {
	static readonly VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4] {
		new(default, default, Vector2.Zero),
		new(default, default, new Vector2(1, 0)),
		new(default, default, new Vector2(0, 1)),
		new(default, default, Vector2.One)
	};
	static readonly short[] dices = [0, 1, 2, 3, 1, 2];
	static readonly AdvancedMiscShaderData circleShader;
	static Parameter uScale;
	static ShaderCircle() {
		circleShader = new(ModContent.Request<Effect>("Origins/Effects/Radial"), "Circle");
		circleShader.LoadThen(() => {
			circleShader.CreateParameter(ref uScale, nameof(uScale), 0f);
		});
	}
	public static void Draw(Vector2 position, float radius, Color color, float innerRadius = 0) {
		vertices[0].Position = new Vector3(position + new Vector2(-radius, -radius), 0);
		vertices[1].Position = new Vector3(position + new Vector2(radius, -radius), 0);
		vertices[2].Position = new Vector3(position + new Vector2(-radius, radius), 0);
		vertices[3].Position = new Vector3(position + new Vector2(radius, radius), 0);

		vertices[0].Color = color;
		vertices[1].Color = color;
		vertices[2].Color = color;
		vertices[3].Color = color;
		circleShader.Apply(null, uScale with { Value = innerRadius / radius });
		Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length, dices, 0, 2);
	}
}
