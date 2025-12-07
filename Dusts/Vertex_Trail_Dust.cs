using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Ammo.Canisters;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Vertex_Trail_Dust : ModDust {
		public override string Texture => "Terraria/Images/Item_1";
		public override void SetStaticDefaults() {
			Deprioritized_Dust.Set[Type] = -1;
		}
		public override bool Update(Dust dust) {
			dust.position = Main.LocalPlayer.Center;
			if (dust.customData is TrailData data) {
				dust.active = data.Update();
			} else {
				dust.active = false;
			}
			return false;
		}
		public override bool MidUpdate(Dust dust) {
			return true;
		}
		public override bool PreDraw(Dust dust) {
			if (dust.customData is TrailData data) data.Draw();
			return false;
		}
		private static readonly VertexStrip _vertexStrip = new();
		public class TrailData(Vector2[] oldPos, float[] oldRot, VertexStrip.StripColorFunction color, VertexStrip.StripHalfWidthFunction width, int extraUpdates = 0) {
			Vector2[] oldPos = oldPos;
			float[] oldRot = oldRot;
			public bool Update() {
				int size = oldPos.Length - (1 + extraUpdates);
				if (size <= 0) return false;
				Array.Resize(ref oldPos, size);
				Array.Resize(ref oldRot, size);
				return true;
			}
			public void Draw() {
				MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
				miscShaderData.UseSaturation(-2.8f);
				miscShaderData.UseOpacity(4f);
				miscShaderData.Apply();
				_vertexStrip.PrepareStripWithProceduralPadding(oldPos, oldRot, color, width, -Main.screenPosition);
				_vertexStrip.DrawTrail();
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			}
		}
	}
}