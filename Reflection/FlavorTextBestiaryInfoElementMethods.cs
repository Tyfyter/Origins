using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;

namespace Origins.Reflection;
public class FlavorTextBestiaryInfoElementMethods : ILoadable {
	public static FastFieldInfo<FlavorTextBestiaryInfoElement, string> _key;
	public void Load(Mod mod) {
		_key = new(nameof(_key), BindingFlags.NonPublic | BindingFlags.Instance);
	}

	public void Unload() {
		_key = null;
	}
}
