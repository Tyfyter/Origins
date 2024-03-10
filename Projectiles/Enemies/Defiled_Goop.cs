using Microsoft.Xna.Framework;
using Origins.NPCs;
using Origins.NPCs.Defiled;
using Origins.NPCs.Defiled.Boss;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Projectiles.Enemies {
	public class Defiled_Goop : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Magic/Infusion_P";
		public AssimilationAmount Assimilation = 0.04f;
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
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.GetModPlayer<OriginPlayer>().DefiledAssimilation += Assimilation.GetValue(null, target);
		}
	}
}
