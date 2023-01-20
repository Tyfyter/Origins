using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
    public class Slashswarm : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Slashswarm");
            glowmask = Origins.AddGlowMask(this, "");
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.CrystalStorm);
			Item.damage = 16;
			Item.width = 20;
			Item.height = 22;
			Item.useTime = 8;
			Item.useAnimation = 24;
            Item.shoot = ModContent.ProjectileType<Slashswarm_P>();
            Item.shootSpeed = 8.75f;
            Item.mana = 16;
            Item.knockBack = 0f;
			Item.value = Item.buyPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.reuseDelay = 8;
			Item.glowMask = glowmask;
        }
    }
    public class Slashswarm_P : ModProjectile {
		public static int ID { get; private set; } = -1;
		public override string Texture => "Origins/Items/Weapons/Summoner/Minions/Amoeba_Bubble";
		public override string GlowTexture => Texture;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Amoeba Bubble");
			Main.projFrames[Projectile.type] = 4;
			ID = Type;
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Grenade);
            Projectile.aiStyle = 0;
            Projectile.penetrate = 5;
			Projectile.width = 30;
			Projectile.height = 30;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
			Projectile.scale = 0.75f;
			Projectile.alpha = 150;
		}
		public override void AI() {
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= 7) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame = 0;
				}
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			float speed = oldVelocity.Length();
			Projectile.velocity = Projectile.velocity + Projectile.velocity - oldVelocity;
			Projectile.velocity *= speed / Projectile.velocity.Length();
			Projectile.penetrate--;
			Split();
			return false;
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			Vector2 hitbox = Projectile.Hitbox.Center.ToVector2();
			Vector2 intersect = Rectangle.Intersect(Projectile.Hitbox, target.Hitbox).Center.ToVector2();
			bool bounced = false;
			if (hitbox.X != intersect.X) {
				Projectile.velocity.X = -Projectile.velocity.X;
				bounced = true;
			}
			if (hitbox.Y != intersect.Y) {
				Projectile.velocity.Y = -Projectile.velocity.Y;
				bounced = true;
			}
			if (!bounced) {
				if (Math.Abs(Projectile.velocity.X) > Math.Abs(Projectile.velocity.Y)) {
					Projectile.velocity.X = -Projectile.velocity.X;
				} else if (Math.Abs(Projectile.velocity.Y) > Math.Abs(Projectile.velocity.X)) {
					Projectile.velocity.Y = -Projectile.velocity.Y;
				}
			}
			Split();
		}
		void Split() {
			if (Type != Slashswarm_P.ID) return;
			for (int i = 0; i < 2; i++) {
				Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center - new Vector2(15),
					OriginExtensions.Vec2FromPolar(Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi), Projectile.velocity.Length()),
					Slashswarm_Mini_P.ID,
					Projectile.damage / 4,
					Projectile.knockBack,
					Projectile.owner
				);
			}
		}
	}
	public class Slashswarm_Mini_P : Slashswarm_P {
		public static new int ID { get; private set; } = -1;
		public override string Texture => "Origins/Items/Weapons/Summoner/Minions/Amoeba_Bubble";
		public override string GlowTexture => Texture;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Smaller Amoeba Bubble");
			Main.projFrames[Projectile.type] = 4;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.aiStyle = 0;
			Projectile.penetrate = 2;
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 60;
			Projectile.scale = 0.5f;
			Projectile.alpha = 150;
		}
	}
}
