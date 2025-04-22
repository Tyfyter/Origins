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
using Origins.Graphics;
using Terraria.ID;

namespace Origins.Layers {
	public abstract class Accessory_Tangela_Layer(string playerSlot, PlayerDrawLayer parent, string assetArray) : PlayerDrawLayer {
		readonly FastFieldInfo<Player, int> _playerSlot = new(playerSlot, BindingFlags.Public | BindingFlags.NonPublic);
		readonly FastStaticFieldInfo<Asset<Texture2D>[]> _assetArray = new(typeof(TextureAssets), assetArray, BindingFlags.Public | BindingFlags.NonPublic);
		readonly Dictionary<int, AutoLoadingAsset<Texture2D>> tangelaMasks = [];
		public override Position GetDefaultPosition() => new AfterParent(parent);
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => tangelaMasks.ContainsKey(_playerSlot.GetValue(drawInfo.drawPlayer));
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			int slotValue = _playerSlot.GetValue(drawInfo.drawPlayer);
			Asset<Texture2D>[] slotTextures = _assetArray.GetValue();
			for (int i = 0; i < drawInfo.DrawDataCache.Count; i++) {
				DrawData data = drawInfo.DrawDataCache[i];
				if (data.texture == slotTextures[slotValue]?.Value) {
					data.texture = tangelaMasks[slotValue].Value;
					data.shader = TangelaVisual.FakeShaderID;
					drawInfo.DrawDataCache.Insert(i + 1, data);
				}
			}
		}
		public static void AddTangelaMask<T>(int slot, string texture) where T : Accessory_Tangela_Layer {
			ModContent.GetInstance<T>().tangelaMasks.Add(slot, texture);
		}
		public void LoadAllTextures() {
			foreach (AutoLoadingAsset<Texture2D> data in tangelaMasks.Values) data.LoadAsset();
		}
	}

	public class Head_Tangela_Layer() : Accessory_Tangela_Layer(nameof(Player.head), PlayerDrawLayers.Head, nameof(TextureAssets.ArmorHead)) { }
	public class Body_Tangela_Layer() : Accessory_Tangela_Layer(nameof(Player.body), PlayerDrawLayers.ArmOverItem, nameof(TextureAssets.ArmorBodyComposite)) { }
}
