using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.NPCs;
using Origins.NPCs.Defiled;
using Origins.NPCs.Riven;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Projectiles.Enemies {
	public class Defiled_Goop : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Magic/Infusion_P";
		public const float assimilation_amount = 0.04f;
		public AssimilationAmount Assimilation = assimilation_amount;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
			this.AddAssimilation<Defiled_Assimilation>(Assimilation);
		}
		public override void SetDefaults() {
			Projectile.hostile = true;
			Projectile.aiStyle = 0;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.penetrate = -1;
			Projectile.hide = true;
			Projectile.timeLeft = 180;
		}
		public override void AI() {
			Projectile.velocity.Y += 0.08f;
			for (int i = 0; i < 3; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Paint, newColor: new Color(70, 60, 80));
				dust.velocity *= 0.5f;
				dust.noGravity = true;
			}
		}
	}
}
