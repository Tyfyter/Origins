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
	public record struct GlowData(AutoLoadingAsset<Texture2D> Texture, Func<Player, Color> ColorFunc);
	public abstract class Accessory_Glow_Layer(string playerSlot, PlayerDrawLayer parent, string assetArray) : PlayerDrawLayer {
		readonly FastFieldInfo<Player, int> _playerSlot = new(playerSlot, BindingFlags.Public | BindingFlags.NonPublic);
		readonly FastStaticFieldInfo<Asset<Texture2D>[]> _assetArray = new(typeof(TextureAssets), assetArray, BindingFlags.Public | BindingFlags.NonPublic);
		readonly Dictionary<int, GlowData> glowMasks = [];
		public override Position GetDefaultPosition() => new AfterParent(parent);
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => glowMasks.ContainsKey(_playerSlot.GetValue(drawInfo.drawPlayer));
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			int slotValue = _playerSlot.GetValue(drawInfo.drawPlayer);
			Asset<Texture2D>[] slotTextures = _assetArray.GetValue();
			for (int i = 0; i < drawInfo.DrawDataCache.Count; i++) {
				DrawData data = drawInfo.DrawDataCache[i];
				if (data.texture == slotTextures[slotValue]?.Value) {
					GlowData glowData = glowMasks[slotValue];
					data.texture = glowData.Texture.Value;
					data.color = glowData.ColorFunc(drawInfo.drawPlayer);
					drawInfo.DrawDataCache.Insert(i + 1, data);
					break;
				}
			}
		}
		public static void AddGlowMask<T>(int slot, string texture, Func<Player, Color> colorFunc = null) where T : Accessory_Glow_Layer {
			ModContent.GetInstance<T>().glowMasks.Add(slot, new(texture, colorFunc ?? (_ => Color.White)));
		}
		public void LoadAllTextures() {
			foreach (GlowData glowData in glowMasks.Values) {
				glowData.Texture.LoadAsset();
			}
		}
	}

	public class Face_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.face), PlayerDrawLayers.FaceAcc, nameof(TextureAssets.AccFace)) { }
	public class Neck_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.neck), PlayerDrawLayers.NeckAcc, nameof(TextureAssets.AccNeck)) { }
	public class Shield_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.shield), PlayerDrawLayers.Shield, nameof(TextureAssets.AccShield)) { }
	public class Waist_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.waist), PlayerDrawLayers.WaistAcc, nameof(TextureAssets.AccWaist)) { }
	public class Back_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.back), PlayerDrawLayers.BackAcc, nameof(TextureAssets.AccBack)) { }
	public class HandsOff_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.handoff), PlayerDrawLayers.OffhandAcc, nameof(TextureAssets.AccHandsOffComposite)) { }
}
