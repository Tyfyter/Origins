using PegasusLib;
using System.Reflection;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;

namespace Origins.Reflection;
public class ModBestiaryInfoElementMethods : ILoadable {
	public static FastFieldInfo<ModBestiaryInfoElement, Mod> _mod;
	public void Load(Mod mod) {
		_mod = new(nameof(_mod), BindingFlags.NonPublic | BindingFlags.Instance);
	}

	public void Unload() {
		_mod = null;
	}
}
