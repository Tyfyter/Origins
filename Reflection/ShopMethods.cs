using PegasusLib.Reflection;
using System.Reflection;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Reflection {
	public class ShopMethods : ILoadable {
		private delegate void AddHappinessReportText_Del(string textKeyInCategory, object substitutes = null, int otherNPCType = 0);
		private static AddHappinessReportText_Del _AddHappinessReportText;
		public void Load(Mod mod) {
			_AddHappinessReportText = typeof(ShopHelper).GetMethod("AddHappinessReportText", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate<AddHappinessReportText_Del>(new ShopHelper());
		}
		public void Unload() {
			_AddHappinessReportText = null;
		}
		public static void AddHappinessReportText(ShopHelper instance, string textKeyInCategory, object substitutes = null, int otherNPCType = 0) {
			DelegateMethods._target.SetValue(_AddHappinessReportText, instance);
			_AddHappinessReportText(textKeyInCategory, substitutes, otherNPCType);
		}
	}
}