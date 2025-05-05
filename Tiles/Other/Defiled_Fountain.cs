using Origins.Water;
using Origins.World.BiomeData;
using Terraria.ModLoader;

namespace Origins.Tiles.Other {
	public class Defiled_Fountain : WaterFountainBase<Defiled_Wastelands> {
		public override void SetBiomeActive() => Defiled_Wastelands.forcedBiomeActive = true;
		public override ModWaterStyle WaterStyle => ModContent.GetInstance<Defiled_Water_Style>();
	}
}