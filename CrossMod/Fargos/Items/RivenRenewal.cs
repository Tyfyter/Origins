using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;

namespace Origins.Items.Other.Consumables.Renewals {
	public class RivenRenewal : TORenewals<Teal_Solution, RivenNukeProj> {
		public override string Texture => "Origins/Items/Weapons/Ammo/Teal_Solution";
		public RivenRenewal() : base() {
		}
	}
	public class RivenSupremeRenewal : TORenewals<RivenRenewal, RivenNukeSupremeProj> {
		public override string Texture => "Origins/Items/Weapons/Ammo/Teal_Solution";
		public RivenSupremeRenewal() : base(true) {
		}
	}
	public class RivenNukeProj : TORenewal_P<Teal_Solution_P, Riven_Hive_Alt_Biome> {
		public override string Texture => "Origins/Items/Weapons/Ammo/Teal_Solution";
		public RivenNukeProj() : base() {
		}
	}
	public class RivenNukeSupremeProj : TORenewal_P<Teal_Solution_P, Riven_Hive_Alt_Biome> {
		public override string Texture => "Origins/Items/Weapons/Ammo/Teal_Solution";
		public RivenNukeSupremeProj() : base(true) {
		}
	}
}
