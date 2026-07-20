using PegasusLib.Reflection;
using Terraria;
using DelegateMethods = PegasusLib.Reflection.DelegateMethods;

namespace Origins.Reflection {
	internal class ItemMethods : ReflectionLoader {
		public delegate bool Hook_TryGetPrefixStatMultipliersForItem(int rolledPrefix, out float dmg, out float kb, out float spd, out float size, out float shtspd, out float mcst, out int crt);
		[ReflectionParentType(typeof(Item))]
		static Hook_TryGetPrefixStatMultipliersForItem TryGetPrefixStatMultipliersForItem { get; set; }
		public static bool TryGetPrefixStatMultipliers(Item item, int prefix, out float dmg, out float kb, out float spd, out float size, out float shtspd, out float mcst, out int crt) {
			DelegateMethods._target.SetValue(TryGetPrefixStatMultipliersForItem, item);
			return TryGetPrefixStatMultipliersForItem(prefix, out dmg, out kb, out spd, out size, out shtspd, out mcst, out crt);
		}
	}
}
