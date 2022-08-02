using Newtonsoft.Json;
using Origins.Items.Armor.Vanity.Terlet.PlagueTexan;
using Origins.Items.Weapons.Explosives;
using Origins.NPCs.Defiled;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace Origins {
	[Label("Settings")]
	public class OriginConfig : ModConfig {
		public static OriginConfig Instance;
		public override ConfigScope Mode => ConfigScope.ServerSide;
		[Label("Use alternate world evil biomes")]
		[OptionStrings(new string[] { "never", "50/50", "always" })]
		[DefaultValue("50/50")]
		public string altWorldEvil;
		[JsonIgnore]
		public sbyte worldTypeSkew = 0;

		[Header("Vanilla Buffs")]

		[Label("Infected Wood Items")]
		[DefaultValue(true)]
		public bool WoodBuffs = true;

		[Header("Other")]

		[Label("Universal Grass Merge")]
		[ReloadRequired]
		[DefaultValue(true)]
		public bool GrassMerge = true;

		[OnDeserialized]
		internal void OnDeserialized(StreamingContext context) {
			switch(altWorldEvil) {
				case "never":
				worldTypeSkew = -1;
				break;
				case "always":
				worldTypeSkew = 1;
				break;
				default:
				worldTypeSkew = 0;
				break;
			}
		}
	}
	[Label("Client Settings")]
	public class OriginClientConfig : ModConfig {
		public static OriginClientConfig Instance;
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("Use Double Tap For Set Bonus Abilities")]
		[DefaultValue(false)]
		public bool SetBonusDoubleTap = false;
	}
}
