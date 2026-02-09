using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.GameContent.TilePaintSystemV2;

namespace Origins.Graphics {
	public class CustomTilePaintLoader : ILoadable {
		public class CustomTileTargetHolder : ARenderTargetHolder {
			public CustomTileVariationKey Key;
			public Asset<Texture2D> asset;
			public override void Prepare() {
				asset.Wait?.Invoke();
				PrepareTextureIfNecessary(asset.Value);
			}
			public override void PrepareShader() {
				PrepareShader(Key.PaintColor, settingss[Key.CustomTileType]);
			}
		}
		public struct CustomTileVariationKey {
			public static readonly CustomTileVariationKey Invalid = new() {
				CustomTileType = -1
			};
			public int CustomTileType;
			public int PaintColor;
			public readonly bool Equals(CustomTileVariationKey other) => CustomTileType == other.CustomTileType && PaintColor == other.PaintColor;
			public override readonly bool Equals(object obj) => obj is CustomTileVariationKey other && Equals(other);
			public override readonly int GetHashCode() => HashCode.Combine(CustomTileType, PaintColor);
			public static bool operator ==(CustomTileVariationKey left, CustomTileVariationKey right) => left.Equals(right);
			public static bool operator !=(CustomTileVariationKey left, CustomTileVariationKey right) => !left.Equals(right);
		}
		private static CustomTilePaintDictionary _glowsRenders = [];
		private static CustomTilePaintRequestList _requests = [];
		private static List<TreePaintingSettings> settingss = [];
		public static int Count { get; private set; } = 0;
		public void Load(Mod mod) {
			On_TilePaintSystemV2.PrepareAllRequests += (orig, self) => {
				orig(self);
				PrepareAllRequests();
			};
		}
		public void Unload() {
			Main.QueueMainThreadAction(_glowsRenders.Unload);
			Main.QueueMainThreadAction(_requests.Unload);
			_glowsRenders = null;
			_requests = null;
			settingss = null;
			Count = 0;
		}
		public static Texture2D TryGetTileAndRequestIfNotReady(CustomTileVariationKey tileVariationkey, int paintColor, Asset<Texture2D> asset) {
			if (paintColor == PaintID.None) return asset.Value;
			tileVariationkey.PaintColor = paintColor;
			if (_glowsRenders.TryGetValue(tileVariationkey, out CustomTileTargetHolder value) && value.IsReady) {
				return value.Target;
			}
			RequestTile(in tileVariationkey, asset);
			return Asset<Texture2D>.DefaultValue;
		}
		public static void RequestTile(in CustomTileVariationKey lookupKey, Asset<Texture2D> asset) {
			if (!_glowsRenders.TryGetValue(lookupKey, out CustomTileTargetHolder value)) {
				value = new CustomTileTargetHolder {
					Key = lookupKey,
					asset = asset
				};
				_glowsRenders.Add(lookupKey, value);
			}
			if (!value.IsReady) {
				_requests.Add(value);
			}
		}
		public static void PrepareAllRequests() {
			if (_requests.Count != 0) {
				for (int i = 0; i < _requests.Count; i++) {
					if (_requests[i].IsReady) continue;
					_requests[i].Prepare();
				}
				_requests.Clear();
			}
		}
		public static CustomTileVariationKey CreateKey(TreePaintingSettings settings = null) {
			settings ??= new TreePaintingSettings {
				UseSpecialGroups = false
			};
			settingss.Add(settings);
			return new() { CustomTileType = Count++ };
		}
	}
	internal class CustomTilePaintDictionary : Dictionary<CustomTilePaintLoader.CustomTileVariationKey, CustomTilePaintLoader.CustomTileTargetHolder> {
		public void Unload() {
			foreach (CustomTilePaintLoader.CustomTileTargetHolder item in Values) item.Clear();
			Clear();
		}
	}
	internal class CustomTilePaintRequestList : List<CustomTilePaintLoader.CustomTileTargetHolder> {
		public void Unload() {
			foreach (CustomTilePaintLoader.CustomTileTargetHolder item in this) item.Clear();
			Clear();
		}
	}
}
