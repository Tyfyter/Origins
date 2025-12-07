using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics;

namespace Origins.Graphics.Primitives {
	public class VertexRectangle {

		private VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];
		private static GraphicsDevice GraphicsDevice => Main.instance.GraphicsDevice;
		
		public void Draw(Vector2 position, Color color = default, Vector2 size = default, float rotation = 0, Vector2 rotationCenter = default) {
			vertices[0].Position = new Vector3((position + new Vector2((float)-size.X * 0.5f, (float)-size.Y * 0.5f)).RotatedBy(rotation, rotationCenter), 0);
			vertices[1].Position = new Vector3((position + new Vector2(size.X * 0.5f, (float)-size.Y * 0.5f)).RotatedBy(rotation, rotationCenter), 0);
			vertices[2].Position = new Vector3((position + new Vector2((float)-size.X * 0.5f, size.Y * 0.5f)).RotatedBy(rotation, rotationCenter), 0);
			vertices[3].Position = new Vector3((position + new Vector2(size.X * 0.5f, size.Y * 0.5f)).RotatedBy(rotation, rotationCenter), 0);

			vertices[0].TextureCoordinate = Vector2.Zero;
			vertices[1].TextureCoordinate = new Vector2(1, 0);
			vertices[2].TextureCoordinate = new Vector2(0, 1);
			vertices[3].TextureCoordinate = Vector2.One;


			short[] dices = [0, 1, 2, 3, 1, 2];
			GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length, dices, 0, 2);
		}
	}
}
