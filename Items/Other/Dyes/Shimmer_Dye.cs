using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.GameContent.Liquid;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Items.Other.Dyes {
	public class Shimmer_Dye : Dye_Item, ICustomWikiStat {
		public static int ID { get; private set; }
		public static int ShaderID { get; private set; }
		public string[] Categories => [
			WikiCategories.SpecialEffectDye
		];
		public override void SetStaticDefaults() {
			ID = Type;
			GameShaders.Armor.BindShader(Type, new DelegatedArmorShaderData(
				Mod.Assets.Request<Effect>("Effects/Shimmer"),
				"Shimmer",
				(self, entity, data) => {
					Vector2 pos = entity is null ? Main.screenPosition : ((data?.position ?? entity.position) + Main.screenPosition) * 0.25f;
					self.Shader.Parameters["uOffset"].SetValue(pos);
				}
			));
			ShaderID = GameShaders.Armor.GetShaderIdFromItemId(Type);
			Item.ResearchUnlockCount = 3;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			char[] chars = tooltips[0].Text.ToCharArray();
			StringBuilder builder = new();
			for (int i = 0; i < chars.Length; i++) {
				builder.Append($"[c/{new Color(LiquidRenderer.GetShimmerBaseColor(i, 0)).Hex3()}:{chars[i]}]");
			}
			tooltips[0].Text = builder.ToString();
		}
	}
}
