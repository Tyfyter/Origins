using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;

namespace Origins.CrossMod.Fargos.Items {
	public class DefiledRenewal : TORenewals<Gray_Solution, DefiledNukeProj> {
		public DefiledRenewal() : base() { }
	}
	public class DefiledSupremeRenewal : TORenewals<DefiledRenewal, DefiledNukeSupremeProj> {
		public DefiledSupremeRenewal() : base(true) { }
	}
	public class DefiledNukeProj : TORenewal_P<Gray_Solution_P, Defiled_Wastelands_Alt_Biome> {
		public override string Texture => "Origins/CrossMod/Fargos/Items/DefiledRenewal";
		public DefiledNukeProj() : base() { }
	}
	public class DefiledNukeSupremeProj : TORenewal_P<Gray_Solution_P, Defiled_Wastelands_Alt_Biome> {
		public override string Texture => "Origins/CrossMod/Fargos/Items/DefiledSupremeRenewal";
		public DefiledNukeSupremeProj() : base(true) { }
	}
}
