using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Origins.Graphics {
	public class Mask_Rasterize : ILoadable {
		static bool anyActive;
		static Stack<int> queue;
		public static ScreenTarget AuraTarget { get; private set; }
		public void Load(Mod mod) {
			if (Main.dedServ) return;
			queue = [];
			AuraTarget = new(
				MaskAura,
				() => {
					if (anyActive) {
						anyActive = false;
						return Lighting.NotRetro;
					} else {
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
	}
}
