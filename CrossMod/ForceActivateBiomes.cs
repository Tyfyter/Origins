using Fargowiltas.Common.Configs;
using MonoMod.Cil;
using Origins.Water;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins.CrossMod {
	public class ForceActivateBiomes : ILoadable {
		public static HashSet<ModBiome> forcedModBiomes = [];
		internal static List<ForceActivateBiomeController> controllers = [];
		public void Load(Mod mod) {
			MonoModHooks.Modify(typeof(BiomeLoader).GetMethod(nameof(BiomeLoader.UpdateBiomes)), IL_BiomeLoader_UpdateBiomes);
			On_Player.UpdateBiomes += On_Player_UpdateBiomes;
		}

		private static void IL_BiomeLoader_UpdateBiomes(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.Before, i => i.MatchCallOrCallvirt<ModBiome>(nameof(ModBiome.IsBiomeActive)));
			c.Index--;
			c.EmitDup();
			c.Index += 2;
			c.EmitDelegate((ModBiome biome, bool active) => active || forcedModBiomes.Contains(biome));
		}
		private static void On_Player_UpdateBiomes(On_Player.orig_UpdateBiomes orig, Player self) {
			forcedModBiomes.Clear();
			Brine_Pool.forcedBiomeActive = false;
			Defiled_Wastelands.forcedBiomeActive = false;
			Riven_Hive.forcedBiomeActive = false;
			for (int i = 0; i < controllers.Count; i++) {
				controllers[i].CheckBiomes(self);
			}
			orig(self);
		}
		public void Unload() {
			controllers = null;
		}
	}
	public abstract class ForceActivateBiomeController : ILoadable {
		public void Load(Mod mod) {
			ForceActivateBiomes.controllers.Add(this);
		}
		public abstract void CheckBiomes(Player player);
		public void Unload() { }
	}
	[ExtendsFromMod("Fargowiltas")]
	public class FargosFountainsForceFbiomes : ForceActivateBiomeController {
		public Dictionary<int, (ModBiome biome, Action setActive)> fountainAssociations = [];
		public static void AddFountainAssociation(ModBiome biome, int waterStyle, Action setActive = null) {
			ModContent.GetInstance<FargosFountainsForceFbiomes>().fountainAssociations[waterStyle] = (biome, setActive);
		}
		public override void CheckBiomes(Player player) {
			int fountain = Main.SceneMetrics.ActiveFountainColor;
			if (FargoServerConfig.Instance.Fountains && fountainAssociations.TryGetValue(Main.SceneMetrics.ActiveFountainColor, out (ModBiome biome, Action setActive) association)) {
				ForceActivateBiomes.forcedModBiomes.Add(association.biome);
				association.setActive?.Invoke();
			}
		}
	}
}
