using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace Origins {
	[Label("Settings")]
	public class OriginConfig : ModConfig {
		public static OriginConfig Instance;
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[Header("VanillaBuffs")]

		[Label("Infected Wood Items")]
		[DefaultValue(true)]
		public bool WoodBuffs = true;

		[Header("Other")]

		[Label("Universal Grass Merge")]
		[ReloadRequired]
		[DefaultValue(true)]
		public bool GrassMerge = true;
		internal void Save() {
			Directory.CreateDirectory(ConfigManager.ModConfigPath);
			string filename = Mod.Name + "_" + Name + ".json";
			string path = Path.Combine(ConfigManager.ModConfigPath, filename);
			string json = JsonConvert.SerializeObject(this, ConfigManager.serializerSettings);
			File.WriteAllText(path, json);
		}
	}
	[Label("Client Settings")]
	public class OriginClientConfig : ModConfig {
		public static OriginClientConfig Instance;
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("Use Double Tap For Set Bonus Abilities")]
		[DefaultValue(false)]
		public bool SetBonusDoubleTap = false;

		[Header("Journal")]

		[Label("Alternate Journal Layout")]
		[DefaultValue(false)]
		public bool TabbyJournal = false;

		[Label("Open Journal Entries on Unlock")]
		[DefaultValue(true)]
		public bool OpenJournalOnUnlock = true;

		[Label("Animated Ravel Transformation")]
		[DefaultValue(true)]
		public bool AnimatedRavel = true;
	}
}
