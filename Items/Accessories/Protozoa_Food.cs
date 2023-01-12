using Microsoft.Xna.Framework;
using Origins.Buffs;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Accessories {
    public class Protozoa_Food : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Protozoa Food");
            Tooltip.SetDefault("Increases life regeneration at low health");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
			Item.damage = 13;
			Item.knockBack = 3;
			Item.useTime = Item.useAnimation = 45;
			Item.mana = 10;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Summon];
			Item.accessory = true;
            Item.width = 21;
            Item.height = 20;
            Item.rare = ItemRarityID.Master;
            Item.master = true;
			Item.value = Item.sellPrice(gold: 6);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
            player.maxMinions += 1;
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.protozoaFood = true;
			originPlayer.protozoaFoodItem = Item;
		}
		public override int ChoosePrefix(UnifiedRandom rand) {
			int prefix = 0;
			if (rand.NextBool(19, 36 + 19)) {
				prefix = rand.Next(62, 81);
			} else {
				switch (rand.Next(36)) {
					case 0:
					prefix = 26;
					break;
					case 1:
					prefix = 27;
					break;
					case 2:
					prefix = 28;
					break;
					case 3:
					prefix = 29;
					break;
					case 4:
					prefix = 30;
					break;
					case 5:
					prefix = 31;
					break;
					case 6:
					prefix = 32;
					break;
					case 7:
					prefix = 33;
					break;
					case 8:
					prefix = 34;
					break;
					case 9:
					prefix = 35;
					break;
					case 10:
					prefix = 52;
					break;
					case 11:
					prefix = 36;
					break;
					case 12:
					prefix = 37;
					break;
					case 13:
					prefix = 38;
					break;
					case 14:
					prefix = 53;
					break;
					case 15:
					prefix = 54;
					break;
					case 16:
					prefix = 55;
					break;
					case 17:
					prefix = 39;
					break;
					case 18:
					prefix = 40;
					break;
					case 19:
					prefix = 56;
					break;
					case 20:
					prefix = 41;
					break;
					case 21:
					prefix = 57;
					break;
					case 22:
					prefix = 42;
					break;
					case 23:
					prefix = 43;
					break;
					case 24:
					prefix = 44;
					break;
					case 25:
					prefix = 45;
					break;
					case 26:
					prefix = 46;
					break;
					case 27:
					prefix = 47;
					break;
					case 28:
					prefix = 48;
					break;
					case 29:
					prefix = 49;
					break;
					case 30:
					prefix = 50;
					break;
					case 31:
					prefix = 51;
					break;
					case 32:
					prefix = 59;
					break;
					case 33:
					prefix = 60;
					break;
					case 34:
					prefix = 61;
					break;
					case 35:
					prefix = 83;
					break;
				}
			}
			PrefixLoader.Roll(Item, ref prefix, 36 + 19, rand, PrefixCategory.AnyWeapon, PrefixCategory.Magic, PrefixCategory.Accessory);
			return prefix;
		}
		public override bool MagicPrefix() => true;
		public override bool WeaponPrefix() => false;
	}
	public class Mini_Protozoa_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Riven/Amoeba_Ball";
		public override string GlowTexture => Texture;
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			ID = Projectile.type;
			DisplayName.SetDefault("Little Protozoa");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 1;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = false;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public sealed override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Summon];
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 0f;
			Projectile.penetrate = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.alpha = 150;
		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		// This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
		public override bool MinionContactDamage() {
			return true;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];


			#region Active check
			if (!player.dead && Projectile.ai[0] != 1 && player.active && player.GetModPlayer<OriginPlayer>().protozoaFood) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Top;
			idlePosition.X -= 48f * player.direction;

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
				Projectile.ai[0] = 1;
				Projectile.netUpdate = true;
			}

			// If your minion is flying, you want to do this independently of any conditions
			float overlapVelocity = 0.04f;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width) {
					if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
					else Projectile.velocity.X += overlapVelocity;

					if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
					else Projectile.velocity.Y += overlapVelocity;
				}
			}
			#endregion

			#region Find target
			// Starting search distance
			float distanceFromTarget = 2000f;
			Vector2 targetCenter = Projectile.position;
			int target = -1;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (!isPriorityTarget && distanceFromTarget > 700f) {
					distanceFromTarget = 700f;
				}
				if (npc.CanBeChasedBy()) {
					float between = Vector2.Distance(npc.Center, Projectile.Center);
					bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
					bool inRange = between < distanceFromTarget;
					bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
					// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
					// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
					bool closeThroughWall = between < 100f;
					if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall)) {
						distanceFromTarget = between;
						targetCenter = npc.height / (float)npc.width > 1 ? npc.Top + new Vector2(0, 8) : npc.Center;
						target = npc.whoAmI;
						foundTarget = true;
					}
				}
			}
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);

			Projectile.friendly = foundTarget;
			#endregion

			#region Movement

			// Default movement parameters (here for attacking)
			float speed = 24f;
			float inertia = 12f;

			if (foundTarget) {
				Projectile.tileCollide = true;
				// Minion has a target: attack (here, fly towards the enemy)
				//if (distanceFromTarget > 40f || !projectile.Hitbox.Intersects(Main.npc[target].Hitbox)) {
				// The immediate range around the target (so it doesn't latch onto it when close)
				Vector2 direction = targetCenter - Projectile.Center;
				direction.Normalize();
				direction *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
				//}
			} else {
				Projectile.tileCollide = false;
				if (distanceToIdlePosition > 600f) {
					speed = 24f;
					inertia = 36f;
				} else {
					speed = 6f;
					inertia = 48f;
				}
				if (distanceToIdlePosition > 12f) {
					// The immediate range around the player (when it passively floats about)

					// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
					vectorToIdlePosition.Normalize();
					vectorToIdlePosition *= speed;
					Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
				} else if (Projectile.velocity == Vector2.Zero) {
					// If there is a case where it's not moving at all, give it a little "poke"
					Projectile.velocity.X = -0.15f;
					Projectile.velocity.Y = -0.05f;
				}
			}
			#endregion

			#region Animation and visuals
			// So it will lean slightly towards the direction it's moving
			Projectile.rotation = Projectile.velocity.X * 0.05f;

			// This is a simple "loop through all frames from top to bottom" animation
			/*int frameSpeed = 5;
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= frameSpeed) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame = 0;
				}
			}*/
			#endregion
		}
		public override Color? GetAlpha(Color lightColor) {
			return new Color((lightColor.R + 255) / 510f, (lightColor.G + 255) / 510f, (lightColor.B + 255) / 510f, 0.5f);
		}
		public override bool PreKill(int timeLeft) {
			if (!Projectile.friendly || timeLeft == 0) return true;
			SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.position);
			Projectile.position -= new Vector2(20);
			Projectile.width += 40;
			Projectile.height += 40;
			Projectile.penetrate = 15;
			Projectile.Damage();
			return true;
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			target.AddBuff(Slow_Debuff.ID, Main.rand.Next(120, 180));
		}
	}
}
