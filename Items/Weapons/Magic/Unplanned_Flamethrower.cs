using Origins.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Awesome_Flames : ModProjectile {
		static AutoLoadingTexture starsTexture = typeof(Awesome_Flames).GetDefaultTMLName("_Stars");
		static AutoLoadingTexture starsColormap = typeof(Awesome_Flames).GetDefaultTMLName("_Stars_Colormap");
		public static float Lifetime => 108f;
		public static float FadeTime => 15f;
		public static float MinSize => 16f;
		public static float MaxSize => 66f;
		private readonly float[] sizes = new float[32];
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 32;
		}
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 6;
			Projectile.penetrate = 4;
			Projectile.friendly = true;
			Projectile.alpha = 255;
			Projectile.extraUpdates = 2;
			Projectile.DamageType = DamageClasses.Incantation;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			for (int i = 0; i < Projectile.oldPos.Length; i++)
				Projectile.oldRot[i] = Main.rand.NextFloatDirection();
		}
		float Size => Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize, MaxSize);
		public override void AI() {
			Projectile.localAI[0] += 1f;
			for (int i = sizes.Length - 1; i > 0; i--) {
				sizes[i] = sizes[i - 1];
			}
			sizes[0] = Size;
			if (Projectile.localAI[2] == 1) {
				Lighting.AddLight(Projectile.Center, 0f, 0.85f, 0.4f);
			}
			//Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FrostStaff);
			Projectile.ai[0]++;
			Projectile.scale = Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize / 96f, MaxSize / 96f);
			Projectile.alpha = (int)(200 * (1 - (Projectile.localAI[0] / Lifetime)));
			Projectile.rotation += 0.3f * Projectile.direction;
			if (Projectile.ai[0] > Lifetime - FadeTime) {
				if (++Projectile.localAI[2] > FadeTime) Projectile.Kill();
			}
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			int scale = (int)(Size / 2) - hitbox.Width;
			hitbox.Inflate(scale, scale);
		}
		public override bool PreDraw(ref Color lightColor) {
			//dstNoise = "Origins/Textures/SC_Mask";
			float progress = Projectile.ai[0] / Lifetime;
			float alphaMult = 1 - (Projectile.localAI[2] / FadeTime);
			Flamethrower_Drawer.Draw(Projectile,
				1 - progress,
				TextureAssets.Projectile[Type].Value,
				new Color(40, 60, 128),
				sizes,
				0,
				smokeAmount: progress,
				sizeProgressOverride: i => Math.Min(1 - ((Projectile.ai[0] - i) / Lifetime), 1) * 0.25f,
				alphaMultiplier: 0.55f * alphaMult,
				tint: i => new Color(0, 80, 128) * alphaMult
			);
			Flamethrower_Drawer.Draw(Projectile,
				1 - progress,
				starsColormap,
				Color.Black,
				sizes,
				8,
				smokeAmount: 0.15f,
				sizeProgressOverride: i => Math.Min(1 - ((Projectile.ai[0] - i) / Lifetime), 1) * 0.25f,
				alphaMultiplier: 0.5f * alphaMult,
				tint: i => new Color(255, 255, 255, 128) * alphaMult,
				pattern: starsTexture
			);
			return false;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity = Vector2.Zero;
			return false;
		}
	}
}
