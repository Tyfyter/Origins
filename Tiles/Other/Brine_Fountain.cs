using Origins.World.BiomeData;

namespace Origins.Tiles.Other {
	public class Brine_Fountain : WaterFountainBase<Brine_Pool> {
		public override int Height => 3;
		public override int Frames => 5;
		public override void SetBiomeActive() => Brine_Pool.forcedBiomeActive = true;
	}
}