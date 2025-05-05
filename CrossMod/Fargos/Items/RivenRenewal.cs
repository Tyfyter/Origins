using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;

namespace Origins.CrossMod.Fargos.Items {
	public class RivenRenewal : TORenewals<Teal_Solution, RivenNukeProj> {
		public RivenRenewal() : base() { }
	}
	public class RivenSupremeRenewal : TORenewals<RivenRenewal, RivenNukeSupremeProj> {
		public RivenSupremeRenewal() : base(true) { }
	}
	public class RivenNukeProj : TORenewal_P<Teal_Solution_P, Riven_Hive_Alt_Biome> {
		public override string Texture => "Origins/CrossMod/Fargos/Items/RivenRenewal";
		public RivenNukeProj() : base() { }
	}
	public class RivenNukeSupremeProj : TORenewal_P<Teal_Solution_P, Riven_Hive_Alt_Biome> {
		public override string Texture => "Origins/CrossMod/Fargos/Items/RivenSupremeRenewal";
		public RivenNukeSupremeProj() : base(true) { }
	}
}
