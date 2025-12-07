using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using PegasusLib;
using PegasusLib.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
		public Color? SetOutlineColor(float progress);
		public DrawData[] OutlineDrawDatas { get; }
		public int OutlineSteps { get; }
		public float OutlineOffset { get; }
		public MiscShaderData OutlineShader => GameShaders.Misc["Origins:SilhouetteShader"];
	}

	public class OutlineDrawerSystem : ModSystem {

		public static HashSet<IOutlineDrawer> needsOutline = [];

		public override void Load() {
			On_Main.DoDraw_DrawNPCsOverTiles += DrawOutline;
		}
		FastFieldInfo<MiscShaderData, Vector3> _uColor = new("_uColor", BindingFlags.NonPublic);
		private void DrawOutline(On_Main.orig_DoDraw_DrawNPCsOverTiles orig, Main self) {
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			foreach (IOutlineDrawer needsOutline in needsOutline) {
				int step = 0;
				MiscShaderData shader = needsOutline.OutlineShader;
				for (float i = 0; i < MathHelper.TwoPi; i += MathHelper.TwoPi / needsOutline.OutlineSteps) {
					step++;
					float progress = step / (float)needsOutline.OutlineSteps;
					if (needsOutline.SetOutlineColor(progress) is Color color) {
						Vector3 oldColor = _uColor.GetValue(shader);
						shader.UseColor(color);
						shader.Apply();
						shader.UseColor(oldColor);
					} else {
						shader.Apply();
					}
					for (int j = 0; j < needsOutline.OutlineDrawDatas.Length; j++) {
						DrawData drawData = needsOutline.OutlineDrawDatas[j];
						drawData.position += new Vector2(MathF.Sin(i), MathF.Cos(i)).RotatedBy(MathHelper.ToRadians((float)Main.timeForVisualEffects) * 5) * needsOutline.OutlineOffset;
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

	public static class OutlineDrawer {
		public static void DrawOutline(this IOutlineDrawer needsOutline) {
			OutlineDrawerSystem.needsOutline.Add(needsOutline);
		}
	}
}
