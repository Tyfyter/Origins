using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;

namespace Origins.Items.Other.Consumables.Renewals {
	public class DefiledRenewal : TORenewals<Gray_Solution, DefiledNukeProj> {
		public override string Texture => "Origins/Items/Weapons/Ammo/Gray_Solution";
		public DefiledRenewal() : base() {
		}
	}
	public class DefiledSupremeRenewal : TORenewals<DefiledRenewal, DefiledNukeSupremeProj> {
		public override string Texture => "Origins/Items/Weapons/Ammo/Gray_Solution";
		public DefiledSupremeRenewal() : base(true) {
		}
	}
	public class DefiledNukeProj : TORenewal_P<Gray_Solution_P, Defiled_Wastelands_Alt_Biome> {
		public override string Texture => "Origins/Items/Weapons/Ammo/Gray_Solution";
		public DefiledNukeProj() : base() {
		}
	}
	public class DefiledNukeSupremeProj : TORenewal_P<Gray_Solution_P, Defiled_Wastelands_Alt_Biome> {
		public override string Texture => "Origins/Items/Weapons/Ammo/Gray_Solution";
		public DefiledNukeSupremeProj() : base(true) {
		}
	}
}
