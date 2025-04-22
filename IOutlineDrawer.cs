using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using PegasusLib.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins {
	public interface IOutlineDrawer {
		public abstract Color SetOutlineColor(float progress);
		public abstract DrawData[] OutlineDrawDatas { get; }
		public abstract int OutlineSteps { get; }
		public abstract float OutlineOffset { get; }
		
	}

	public class OutlineDrawerSystem : ModSystem 
	{

		public static HashSet<IOutlineDrawer> needsOutline = new();

		public override void Load() {
			On_Main.DoDraw_DrawNPCsOverTiles += DrawOutline;
		}
		
			private void DrawOutline(On_Main.orig_DoDraw_DrawNPCsOverTiles orig, Main self) {
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
			MiscShaderData shader = GameShaders.Misc["Origins:SilhouetteShader"];

			foreach (var needsOutline in needsOutline) 
			{
				int step = 0;
				for (float i = 0; i < MathHelper.TwoPi; i += MathHelper.TwoPi / needsOutline.OutlineSteps) {
					step++;
					float progress = step / (float)needsOutline.OutlineSteps;
					Color color = needsOutline.SetOutlineColor(progress);
					shader.UseColor(color);
					shader.Apply();
					for (int j =0; j < needsOutline.OutlineDrawDatas.Length; j++) {
						var drawData = needsOutline.OutlineDrawDatas[j];
						drawData.position += new Vector2(MathF.Sin(i), MathF.Cos(i)) * needsOutline.OutlineOffset;
						drawData.position -= Main.screenPosition;
						Main.EntitySpriteDraw(drawData);

					}
				}
			}
			needsOutline.Clear();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			Main.spriteBatch.End();
			orig.Invoke(self);

		}
	}

	public static class OutlineDrawer 
	{


		public static void DrawOutline(this IOutlineDrawer needsOutline) {

			OutlineDrawerSystem.needsOutline.Add(needsOutline);

		}

	}
}
