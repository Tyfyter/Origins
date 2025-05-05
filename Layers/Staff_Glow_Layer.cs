using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using PegasusLib;

namespace Origins.Layers {
	public class Staff_Glow_Layer : PlayerDrawLayer {
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => Item.staff[drawInfo.heldItem.type] && drawInfo.heldItem.glowMask != -1;
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--) {
				DrawData data = drawInfo.DrawDataCache[i];
				if (data.texture == TextureAssets.Item[drawInfo.heldItem.type]?.Value) {
					data.texture = TextureAssets.GlowMask[drawInfo.heldItem.glowMask].Value;
					data.color = Color.White;
					drawInfo.DrawDataCache.Insert(i + 1, data);
					break;
				}
			}
		}
	}
}
