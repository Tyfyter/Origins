using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Melee;
using PegasusLib.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Origins.Graphics {
	public interface ITangelaHaver {
		public int? TangelaSeed { get; set; }
	}
	public static class TangelaVisual {
		public static int ShaderID { get; private set; }
		class ArmorShaderDataWithAnotherImage(Asset<Effect> shader, string passName) : ArmorShaderData(shader, passName) {
			private Asset<Texture2D> _uImage2;
			public override void Apply(Entity entity, DrawData? drawData = null) {
				if (_uImage2 != null) {
					Main.graphics.GraphicsDevice.Textures[2] = _uImage2.Value;
					Shader.Parameters["uImageSize2"]?.SetValue(_uImage2.Size());
				}
				base.Apply(entity, drawData);
			}

			public ArmorShaderData UseImage2(Asset<Texture2D> asset) {
				_uImage2 = asset;
				return this;
			}
		}
		public static void LoadShader() {
			GameShaders.Armor.BindShader(ModContent.ItemType<Krakram>(), new ArmorShaderDataWithAnotherImage(
				ModContent.Request<Effect>("Origins/Effects/Tangela"),
				"Tangela"
			))
			.UseImage2(ModContent.Request<Texture2D>("Terraria/Images/Misc/Perlin"))
			.UseImage(ModContent.Request<Texture2D>("Terraria/Images/Misc/noise"));
			ShaderID = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<Krakram>());
			Filters.Scene.OnPostDraw += Scene_OnPostDraw;
		}
		static readonly List<DrawData> drawDatas = [];
		private static void Scene_OnPostDraw() {
			if (drawDatas.Count <= 0) return;
			try {
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
				for (int i = 0; i < drawDatas.Count; i++) {
					DrawData data = drawDatas[i];
					GameShaders.Armor.GetSecondaryShader(ShaderID, Main.LocalPlayer).Apply(null, data);
					data.Draw(Main.spriteBatch);
				}
			} finally {
				Main.spriteBatch.End();
			}
			drawDatas.Clear();
		}
		public static void DrawTangela(this ITangelaHaver tangelaHaver, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects) {
			if (!tangelaHaver.TangelaSeed.HasValue) tangelaHaver.TangelaSeed = Main.rand.Next();
			drawDatas.Add(new(
				texture,
				position,
				sourceRectangle,
				Color.White,
				rotation,
				origin,
				scale,
				effects,
				tangelaHaver.TangelaSeed.Value
			));
		}
	}
}
