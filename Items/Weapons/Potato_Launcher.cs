using Microsoft.Xna.Framework;
using Origins.Items.Other.Consumables.Food;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Items.Weapons {
    public class Potato_Launcher : ModItem {
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FlintlockPistol);
			Item.damage = 25;
			Item.DamageType = DamageClass.Generic;
			Item.useTime = 32;
			Item.useAnimation = 32;
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
				Item.useTime /= 8;
				Item.useAnimation /= 8;
			} else if (type == ModContent.ProjectileType<Magic.Hot_Potato_P>()) {
				velocity *= 0.6f;
			}
		}
        public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
    }
	public class Potato_P : ModProjectile {
		public override string Texture => "Origins/Items/Other/Consumables/Food/Potato";
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
			bool foundTarget = false;
			for (int i = 0; i < 200; i++) {
				NPC currentNPC = Main.npc[i];
				if (currentNPC.CanBeChasedBy(this)) {
					Vector2 currentDiff = currentNPC.Center - Projectile.Center;
					float dist = currentDiff.Length();
					currentDiff /= dist;
					float weight = Vector2.Dot(Projectile.velocity, currentDiff) * (300f / (dist + 100));
					if (weight > targetWeight && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, currentNPC.position, currentNPC.width, currentNPC.height)) {
						targetWeight = weight;
						targetDiff = currentDiff;
						foundTarget = true;
					}
				}
			}

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
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Potato_Mine";
		public override void OnKill(int timeLeft) {
			//TODO: figure out how to get potato mines working and make them explode with the same AoE in this
		}
	}
}
