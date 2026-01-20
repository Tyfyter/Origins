using Origins.Graphics.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class ShimmerConstructDust : ModDust {

		public override bool PreDraw(Dust dust) {

			ShimmerConstructDustShader.Draw(dust);

			return false;
		}

	}

	public readonly struct ShimmerConstructDustShader {
		private static readonly VertexRectangle VertexRectangle = new();
		public static void Draw(Dust dust) {
			MiscShaderData shader = GameShaders.Misc["Origins:SC_DustEffect"];
			shader.UseColor(Color.CornflowerBlue);
			shader.Apply();

			VertexRectangle.Draw(dust.position - Main.screenPosition, dust.color, new Vector2(dust.scale) * 128, dust.rotation, dust.position - Main.screenPosition);

			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}
	}
}
