using PegasusLib.Reflection;
using Terraria;
using Terraria.UI;

namespace Origins.Reflection {
	internal class ItemSortingMethods : ReflectionLoader {
		public delegate void Del_Sort(Item[] inv, params int[] ignoreSlots);
		[ReflectionParentType(typeof(ItemSorting))]
		public static Del_Sort Sort { get; private set; }
	}
}
