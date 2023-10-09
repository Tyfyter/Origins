using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Reflection {
	public class AssetRepositoryMethods : ILoadable {
		public static FastFieldInfo<AssetRepository, Dictionary<string, IAsset>> _assets;
		public void Load(Mod mod) {
			_assets = new(nameof(_assets), BindingFlags.NonPublic);
		}
		public void Unload() {
			_assets = null;
		}
	}
}