using Origins.Water;
using Origins.World.BiomeData;
using Terraria.ModLoader;

namespace Origins.Tiles.Other {
	public class Ashen_Fountain : WaterFountainBase<Ashen_Biome> {
		public override string Texture => typeof(Defiled_Fountain).GetDefaultTMLName();
		public override void SetBiomeActive() => Ashen_Biome.forcedBiomeActive = true;
		public override ModWaterStyle WaterStyle => ModContent.GetInstance<Defiled_Water_Style>();
	}
}