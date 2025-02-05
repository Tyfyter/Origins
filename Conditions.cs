using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins {
	public class RecipeConditions : ILoadable {
		public static Condition ShimmerTransmutation { get; private set; } = new Condition(Language.GetOrRegister("Mods.Origins.Conditions.ShimmerTransmutation"), () => false);
		public static Condition RivenWater { get; private set; } = new Condition(Language.GetOrRegister("Mods.Origins.Conditions.RivenWater"), () => Main.LocalPlayer.adjWater && Main.LocalPlayer.InModBiome<Riven_Hive>());
		public void Load(Mod mod) { }
		public void Unload() {
			foreach (FieldInfo item in GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Static)) {
				item.SetValue(null, null);
			}
		}
	}
}
