using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PegasusLib.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Graphics {
	public class Mask_Rasterize : ILoadable {
		public static bool AnyActive => anyActive;
		public static int TimeSinceActive { get; private set; }
		static bool anyActive;
		static Stack<int> queue;
		static Stack<DrawData> drawDataQueue;
		static Stack<(Vector2 pos, Vector2 target, float rotation, int type)> unmissQueue;
		public static ScreenTarget AuraTarget { get; private set; }
		public void Load(Mod mod) {
			if (Main.dedServ) return;
			queue = [];
			drawDataQueue = [];
			unmissQueue = [];
			AuraTarget = new(
				MaskAura,
				() => {
					if (anyActive) {
						anyActive = false;
						TimeSinceActive = 0;
						return Lighting.NotRetro;
					} else {
						TimeSinceActive++;
						return false;
					}
				},
				0
			);
			On_Main.DrawInfernoRings += Main_DrawInfernoRings;
		}
		public void Unload() {
			AuraTarget = null;
			queue = null;
			drawDataQueue = null;
			unmissQueue = null;
		}
		private void Main_DrawInfernoRings(On_Main.orig_DrawInfernoRings orig, Main self) {
			orig(self);
			if (Main.dedServ) return;
			if (Lighting.NotRetro) DrawAura();
		}

		static void MaskAura(SpriteBatch spriteBatch) {
			if (Main.dedServ) return;
			try {
				GraphicsUtils.drawingEffect = true;
				while (queue.Count != 0) {
					Main.instance.DrawProj(queue.Pop());
				}
				while (drawDataQueue.Count != 0) {
					drawDataQueue.Pop().Draw(spriteBatch);
				}
			} finally {
				GraphicsUtils.drawingEffect = false;
			}
		}

		static void DrawAura() {
			if (Main.dedServ) return;
			Main.LocalPlayer.ManageSpecialBiomeVisuals("Origins:MaskedRasterizeFilter", anyActive, Main.LocalPlayer.Center);
			if (anyActive) {
				Filters.Scene["Origins:MaskedRasterizeFilter"].GetShader().UseImage(AuraTarget.RenderTarget, 1);
			}
		}
		public static bool QueueProjectile(int index) {
			if (GraphicsUtils.drawingEffect) return false;
			anyActive = true;
			queue.Push(index);
			return true;
		}
		public static bool QueueDrawData(DrawData data) {
			if (GraphicsUtils.drawingEffect) return false;
			anyActive = true;
			drawDataQueue.Push(data);
			return true;
		}
	}
}
