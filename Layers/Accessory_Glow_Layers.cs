using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Accessory_Glow_Loader : ILoadable {
		internal static Accessory_Glow_Loader Instance { get; private set; }
		public Accessory_Glow_Loader() => Instance = this;
		public void Load(Mod mod) { }
		public void Unload() => Instance = null;
		private static void AddGlowMask(Dictionary<int, GlowData> dict, int slot, string texture, Func<Player, Color> colorFunc = null) {
			dict.Add(slot, new (texture, colorFunc ?? (_ => Color.White)));
		}
		internal Dictionary<int, GlowData> shieldGlowMasks = [];
		public static void AddShieldGlowMask(int slot, string texture, Func<Player, Color> colorFunc = null) => AddGlowMask(Instance.shieldGlowMasks, slot, texture, colorFunc ?? (_ => Color.White));
		internal Dictionary<int, GlowData> waistGlowMasks = [];
		public static void AddWaistGlowMask(int slot, string texture, Func<Player, Color> colorFunc = null) => AddGlowMask(Instance.waistGlowMasks, slot, texture, colorFunc ?? (_ => Color.White));
	}
	public record struct GlowData(AutoLoadingAsset<Texture2D> Texture, Func<Player, Color> ColorFunc);
	public abstract class Accessory_Glow_Layer : PlayerDrawLayer {
		public abstract int GetSlotValue(Player player);
		public abstract Asset<Texture2D>[] SlotTextureArray { get; }
		public abstract Dictionary<int, GlowData> SlotGlowDatas { get; }
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => SlotGlowDatas.ContainsKey(GetSlotValue(drawInfo.drawPlayer));
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			int slotValue = GetSlotValue(drawInfo.drawPlayer);
			Asset<Texture2D>[] slotTextures = SlotTextureArray;
			for (int i = 0; i < drawInfo.DrawDataCache.Count; i++) {
				DrawData data = drawInfo.DrawDataCache[i];
				if (data.texture == slotTextures[slotValue].Value) {
					GlowData glowData = SlotGlowDatas[slotValue];
					data.texture = glowData.Texture.Value;
					data.color = glowData.ColorFunc(drawInfo.drawPlayer);
					drawInfo.DrawDataCache.Add(data);
					break;
				}
			}
		}
	}
	public class Shield_Glow_Layer : Accessory_Glow_Layer {
		public override int GetSlotValue(Player player) => player.shield;
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Shield);
		public override Asset<Texture2D>[] SlotTextureArray => TextureAssets.AccShield;
		public override Dictionary<int, GlowData> SlotGlowDatas => Accessory_Glow_Loader.Instance.shieldGlowMasks;
	}
	public class Waist_Glow_Layer : Accessory_Glow_Layer {
		public override int GetSlotValue(Player player) => player.waist;
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.WaistAcc);
		public override Asset<Texture2D>[] SlotTextureArray => TextureAssets.AccWaist;
		public override Dictionary<int, GlowData> SlotGlowDatas => Accessory_Glow_Loader.Instance.waistGlowMasks;
	}
}
