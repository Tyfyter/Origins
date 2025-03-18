#pragma warning disable CS0649
#pragma warning disable IDE0044
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using System.Reflection;
using Terraria.Localization;

namespace Origins.Reflection {
	public class ConfigManagerMethods : ILoadable {
		private delegate string GetConfigKey_Del(MemberInfo memberInfo, string dataName);
		private static GetConfigKey_Del _GetConfigKey_LabelKeyAttribute;
		private static GetConfigKey_Del _GetConfigKey_TooltipKeyAttribute;

		public void Load(Mod mod) {
			MethodInfo GetConfigKey = typeof(ConfigManager).GetMethod("GetConfigKey", BindingFlags.NonPublic | BindingFlags.Static);
			_GetConfigKey_LabelKeyAttribute = GetConfigKey.MakeGenericMethod(typeof(LabelKeyAttribute)).CreateDelegate<GetConfigKey_Del>();
			_GetConfigKey_TooltipKeyAttribute = GetConfigKey.MakeGenericMethod(typeof(TooltipKeyAttribute)).CreateDelegate<GetConfigKey_Del>();
		}
		public void Unload() {
			_GetConfigKey_LabelKeyAttribute = null;
			_GetConfigKey_TooltipKeyAttribute = null;
		}
		public static LocalizedText GetConfigLabel(MemberInfo memberInfo) => Language.GetOrRegister(_GetConfigKey_LabelKeyAttribute(memberInfo, "Label"));
		public static LocalizedText GetConfigTooltip(MemberInfo memberInfo, bool register = false) {
			string key = _GetConfigKey_TooltipKeyAttribute(memberInfo, "Tooltip");
			if (register && !Language.Exists(key)) return LocalizedText.Empty;
			return Language.GetOrRegister(key);
		}
	}
}