using Microsoft.Xna.Framework;
using Origins.Items.Other.Consumables.Food;
using Origins.Projectiles;
using Origins.Tiles.Other;
using PegasusLib;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Items.Weapons {
    public class Potato_Launcher : ModItem {
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FlintlockPistol);
			Item.damage = 17;
			Item.DamageType = DamageClass.Generic;
			Item.useTime = 27;
			Item.useAnimation = 27;
			Item.useAmmo = ModContent.ItemType<Potato>();
			Item.shoot = ModContent.ProjectileType<Potato_P>();
			Item.knockBack = 2f;
			Item.shootSpeed = 12f;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(silver: 65);
			Item.rare = ItemRarityID.Blue;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == Potato_Battery_P.ID) {
				int frameReduction = player.itemAnimationMax / 3;
				player.itemTime -= frameReduction;
				player.itemTimeMax -= frameReduction;
				player.itemAnimation -= frameReduction;
				player.itemAnimationMax -= frameReduction;
			} else if (type == ModContent.ProjectileType<Magic.Hot_Potato_P>()) {
				velocity *= 0.6f;
			}
			position += velocity.SafeNormalize(default).RotatedBy(player.direction * -MathHelper.PiOver2) * 6;
		}
        public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
    }
	public class Potato_P : ModProjectile {
		public override string Texture => "Origins/Items/Other/Consumables/Food/Potato";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SnowBallFriendly);
			Projectile.DamageType = DamageClass.Generic;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.scale = 0.6f;
		}
	}
	public class Potato_Battery_P : Potato_P {
		public static int ID { get; private set; }
		public override string Texture => "Origins/Items/Accessories/Potato_Battery";
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void AI() {
			float targetWeight = 4.5f;
			Vector2 targetDiff = default;
			bool foundTarget = Main.player[Projectile.owner].DoHoming((target) => {
				Vector2 currentDiff = target.Center - Projectile.Center;
				float dist = currentDiff.Length();
				currentDiff /= dist;
				float weight = Vector2.Dot(Projectile.velocity, currentDiff) * (300f / (dist + 100));
				if (weight > targetWeight && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, target.position, target.width, target.height)) {
					targetWeight = weight;
					targetDiff = currentDiff;
					return true;
				}
				return false;
			});

			if (foundTarget) {
				PolarVec2 velocity = (PolarVec2)Projectile.velocity;
				OriginExtensions.AngularSmoothing(
					ref velocity.Theta,
					targetDiff.ToRotation(),
					0.003f + velocity.R * 0.0015f
				);
				Projectile.velocity = (Vector2)velocity;
			}
		}
	}
	public class Potato_Mine_P : Potato_P {
		public static int ID { get; private set; }
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Potato_Mine";
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.penetrate = -1;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Explode();
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Explode();
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			Explode();
		}
		void Explode() {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item62);
			Projectile.Damage();
			Projectile.Kill();
		}
	}
}
