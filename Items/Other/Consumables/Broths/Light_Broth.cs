using Origins.Projectiles;
using Terraria;
using Terraria.ID;

namespace Origins.Items.Other.Consumables.Broths {
	public class Light_Broth : BrothBase {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				new(233, 173, 117),
				new(207, 110, 21),
				new(151, 82, 18)
			];
		}
		public override int Duration => 6;
		public override void ModifyHurt(Projectile minion, ref int damage, bool fromDoT, IArtifactDamageSource damageSource) {
			damage -= damage / 4;
		}
	}
}
