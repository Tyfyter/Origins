using CalamityMod.Projectiles.Magic;
using Humanizer;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Graphics {
	public class SceneFiltersIgnoredDrawing : ModSystem {

		public override void Load() {
			Terraria.Graphics.Effects.Filters.Scene.OnPostDraw += Scene_OnPostDraw;
		}

		public static List<DrawData> drawDatas = new();

		private void Scene_OnPostDraw() {

			try {
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
				for (int i = 0; i < drawDatas.Count; i++) {
					DrawData data = drawDatas[i];
					data.Draw(Main.spriteBatch);
				}
			} finally {
				Main.spriteBatch.End();
			}
			drawDatas.Clear();
		}

		public static void DrawWithoutFilters(DrawData drawData) {
			drawDatas.Add(drawData);
		}

		public static void DrawWithoutFilters(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects) {
			drawDatas.Add(new(texture, position, sourceRectangle, color, rotation, origin, scale, effects));
		}
	}
}
