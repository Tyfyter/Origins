using System.Collections.Generic;
using System.Reflection;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
using PegasusLib;

namespace Origins.Reflection {
	public class ItemDropDatabaseMethods : ILoadable {
		public static FastFieldInfo<ItemDropDatabase, Dictionary<int, List<IItemDropRule>>> _entriesByNpcNetId;
		public static FastFieldInfo<ItemDropDatabase, Dictionary<int, List<IItemDropRule>>> _entriesByItemId;
		public static FastFieldInfo<ItemDropDatabase, List<IItemDropRule>> _globalEntries;
		public void Load(Mod mod) {
			_entriesByNpcNetId = new(nameof(_entriesByNpcNetId), BindingFlags.NonPublic);
			_entriesByItemId = new(nameof(_entriesByItemId), BindingFlags.NonPublic);
			_globalEntries = new(nameof(_globalEntries), BindingFlags.NonPublic);
		}
		public void Unload() {
			_entriesByNpcNetId = null;
			_entriesByItemId = null;
			_globalEntries = null;
		}
	}
}