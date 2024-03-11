using ReLogic.Content;
using System.Collections.Generic;
using System.Reflection;
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