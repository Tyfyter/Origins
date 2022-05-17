using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Items.Weapons.Other {
	public class Rigged_Cubix : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Rigged Cubix");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.RubyStaff);
			item.damage = 88;
			item.crit = -3;
			item.magic = true;
			item.noMelee = true;
			item.width = 28;
			item.height = 30;
			item.useTime = 3;
			item.useAnimation = 54;
			item.shootSpeed = 14;
			item.mana = 4;
			item.value = 5000;
            item.shoot = ModContent.ProjectileType<Rigged_Cubix_P>();
			item.rare = ItemRarityID.Green;
			item.UseSound = null;
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			Vector2 vel = new Vector2(speedX, speedY).RotatedByRandom(0.075f);
			speedX = vel.X;
			speedY = vel.Y;
			if (player.itemAnimation != 0 && !player.CheckMana(item, pay:true)) {
				return false;
			}
			Main.PlaySound(SoundID.Item12, position);
			return true;
		}
	}
    public class Rigged_Cubix_P : ModProjectile {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Rigged Cubix");
		}
		public override void SetDefaults() {
			projectile.magic = true;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.width = 26;
			projectile.height = 26;
			//projectile.extraUpdates = 1;
			projectile.usesLocalNPCImmunity = true;
			projectile.timeLeft = 600;
			projectile.localNPCHitCooldown = projectile.timeLeft / 10;
			projectile.aiStyle = 0;
			projectile.tileCollide = false;
		}
        public override void AI() {
			//projectile.rotation += 16f * projectile.direction;
			projectile.extraUpdates = 0;
			if (projectile.ai[0] > 0) {
				int targetID = (int)projectile.ai[0];
				NPC target = Main.npc[targetID];
				if (target.active && projectile.localNPCImmunity[targetID] <= 0) {
					PolarVec2 velocity = (PolarVec2)projectile.velocity;
					PolarVec2 diff = (PolarVec2)(target.Center - projectile.Center);
					OriginExtensions.AngularSmoothing(ref velocity.Theta, diff.Theta, diff.R < 96 ? 0.35f : 0.25f);
					projectile.velocity = (Vector2)velocity;
					//projectile.rotation += 0f * projectile.direction;
					projectile.extraUpdates = 1;
				} else {
					projectile.ai[0] = -1;
				}
			} else {
				float distanceFromTarget = 480f;
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy() && projectile.localNPCImmunity[i] <= 0) {
						float between = Vector2.Distance(npc.Center, projectile.Center);
						bool inRange = between < distanceFromTarget;
						if (inRange) {
							distanceFromTarget = between;
							projectile.ai[0] = npc.whoAmI + 0.1f;
						}
					}
				}
			}
		}
    }
}
