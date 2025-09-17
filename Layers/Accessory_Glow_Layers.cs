using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using PegasusLib;

namespace Origins.Layers {
	public record struct GlowData(AutoLoadingAsset<Texture2D> Texture, Func<Player, Color> ColorFunc, Func<Player, int, int> ShaderFunc = null);
	public abstract class Accessory_Glow_Layer(string playerSlot, PlayerDrawLayer parent, EquipType type) : PlayerDrawLayer {
		readonly FastFieldInfo<Player, int> _playerSlot = new(playerSlot, BindingFlags.Public | BindingFlags.NonPublic);
		readonly Dictionary<int, GlowData> glowMasks = [];
		public override Position GetDefaultPosition() => new AfterParent(parent);
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => glowMasks.ContainsKey(_playerSlot.GetValue(drawInfo.drawPlayer));
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			int slotValue = _playerSlot.GetValue(drawInfo.drawPlayer);
			Texture2D expectedTexture = GetTextureArray(type)[slotValue].Value;
			for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--) {
				DrawData data = drawInfo.DrawDataCache[i];
				if (data.texture == expectedTexture) {
					GlowData glowData = glowMasks[slotValue];
					data.texture = glowData.Texture.Value;
					data.color = glowData.ColorFunc(drawInfo.drawPlayer);
					if (glowData.ShaderFunc is not null) data.shader = glowData.ShaderFunc(drawInfo.drawPlayer, data.shader);
					drawInfo.DrawDataCache.Insert(i + 1, data);
				}
			}
		}
		public static void AddGlowMask<T>(int slot, string texture, Func<Player, Color> colorFunc = null, Func<Player, int, int> ShaderFunc = null) where T : Accessory_Glow_Layer {
			ModContent.GetInstance<T>().glowMasks.Add(slot, new(texture, colorFunc ?? (_ => Color.White), ShaderFunc));
		}
		public static void AddGlowMask(EquipType equipType, int slot, string texture, Func<Player, Color> colorFunc = null, Func<Player, int, int> ShaderFunc = null) {
			byEquipType[equipType].glowMasks.Add(slot, new(texture, colorFunc ?? (_ => Color.White), ShaderFunc));
		}
		public void LoadAllTextures() {
			foreach (GlowData glowData in glowMasks.Values) {
				glowData.Texture.LoadAsset();
			}
		}
		public sealed override void Load() {
			byEquipType.Add(type, this);
		}
		static readonly Dictionary<EquipType, Accessory_Glow_Layer> byEquipType = [];
		public static Accessory_Glow_Layer GetByEquipType(EquipType type) => byEquipType[type];
		static Asset<Texture2D>[] GetTextureArray(EquipType type)
		=> type switch {
			EquipType.Head => TextureAssets.ArmorHead,
			EquipType.Body => TextureAssets.ArmorBodyComposite,
			EquipType.Legs => TextureAssets.ArmorLeg,
			EquipType.HandsOn => TextureAssets.AccHandsOnComposite,
			EquipType.HandsOff => TextureAssets.AccHandsOffComposite,
			EquipType.Back => TextureAssets.AccBack,
			EquipType.Front => TextureAssets.AccFront,
			EquipType.Shoes => TextureAssets.AccShoes,
			EquipType.Waist => TextureAssets.AccWaist,
			EquipType.Wings => TextureAssets.Wings,
			EquipType.Shield => TextureAssets.AccShield,
			EquipType.Neck => TextureAssets.AccNeck,
			EquipType.Face => TextureAssets.AccFace,
			EquipType.Beard => TextureAssets.AccBeard,
			EquipType.Balloon => TextureAssets.AccBalloon,
			_ => null,
		};
	}

	public class Head_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.head), PlayerDrawLayers.Head, EquipType.Head) { }
	public class Body_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.body), PlayerDrawLayers.Torso, EquipType.Body) { }
	public class Legs_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.legs), PlayerDrawLayers.Leggings, EquipType.Legs) { }
	public class HandsOn_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.handon), PlayerDrawLayers.HandOnAcc, EquipType.HandsOn) { }
	public class HandsOff_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.handoff), PlayerDrawLayers.OffhandAcc, EquipType.HandsOff) { }
	public class Back_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.back), PlayerDrawLayers.BackAcc, EquipType.Back) { }
	public class Front_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.front), PlayerDrawLayers.FrontAccFront, EquipType.Front) { }
	public class Shoes_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.shoe), PlayerDrawLayers.Shoes, EquipType.Shoes) { }
	public class Waist_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.waist), PlayerDrawLayers.WaistAcc, EquipType.Waist) { }
	public class Wings_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.wings), PlayerDrawLayers.Wings, EquipType.Wings) { }
	public class Shield_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.shield), PlayerDrawLayers.Shield, EquipType.Shield) { }
	public class Neck_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.neck), PlayerDrawLayers.NeckAcc, EquipType.Neck) { }
	public class Face_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.face), PlayerDrawLayers.FaceAcc, EquipType.Face) { }
	public class Beard_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.beard), PlayerDrawLayers.Head, EquipType.Beard) { }
	public class Balloon_Glow_Layer() : Accessory_Glow_Layer(nameof(Player.balloon), PlayerDrawLayers.BalloonAcc, EquipType.Balloon) { }
}
