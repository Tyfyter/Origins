using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Accessory_Glow_Loader : ILoadable {
		public static Accessory_Glow_Loader Instance { get; private set; }
		public Accessory_Glow_Loader() => Instance = this;
		public void Load(Mod mod) { }
		public void Unload() => Instance = null;
		public Dictionary<int, AutoLoadingAsset<Texture2D>> shieldGlowMasks = [];
	}
	public class Shield_Glow_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => Accessory_Glow_Loader.Instance.shieldGlowMasks.ContainsKey(drawInfo.drawPlayer.shield);
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Shield);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			for (int i = 0; i < drawInfo.DrawDataCache.Count; i++) {
				DrawData data = drawInfo.DrawDataCache[i];
				if (data.texture == TextureAssets.AccShield[drawInfo.drawPlayer.shield].Value) {
					data.texture = Accessory_Glow_Loader.Instance.shieldGlowMasks[drawInfo.drawPlayer.shield].Value;
					data.color = Color.White;
					drawInfo.DrawDataCache.Add(data);
					break;
				}
			}
		}
	}
}
