using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Accessories {
	public class Strange_Tooth : ModItem {
        public string[] Categories => [
        ];
        public override void SetDefaults() {
			Item.DefaultToAccessory(28, 20);
			Item.DamageType = DamageClass.Summon;
			Item.damage = 9;
			Item.knockBack = 2;
			Item.useTime = Item.useAnimation = 45;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.master = true;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().strangeToothItem = Item;
		}
		public override int ChoosePrefix(UnifiedRandom rand) {
			return OriginExtensions.AccessoryOrSpecialPrefix(Item, rand, PrefixCategory.AnyWeapon, PrefixCategory.Magic);
		}
	}
	public class Strange_Tooth_Buff : ModBuff {
		public static int ID { get; private set; }
		public override string Texture => "Origins/Buffs/Strange_Tooth_Buff";
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
	}
	public class Strange_Tooth_Minion : ModProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
			Main.projFrames[Type] = 4;
			Main.projPet[Type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Type] = true;

			OriginsSets.Projectiles.MinionBuffReceiverPriority[Type] = 0.25f;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 0f;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 2;
			Projectile.netImportant = true;
		}
		public override bool MinionContactDamage() {
			return true;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			int buffIndex = player.FindBuffIndex(Strange_Tooth_Buff.ID);
			if (buffIndex > -1) {
				if (player.dead || !player.active || (originPlayer.strangeToothItem?.IsAir ?? true)) {
					player.DelBuff(buffIndex);
					return;
				}
				player.buffTime[buffIndex] = 2;
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Top + new Vector2(player.direction * -player.width / 2, 0);
			idlePosition.X -= 48f * player.direction;

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				Projectile.position = idlePosition;
				Projectile.velocity *= 0.1f;
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
			/*if (Player.currentLife < maxLife) {
                Drop ItemID.Heart with chance (Percentage of life lost)
            }*/
			#endregion

			#region Find target
			// Starting search distance
			float targetDist = 2000f;
			float targetAngle = -2;
			Vector2 targetCenter = Projectile.Center;
			int target = -1;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (!isPriorityTarget && targetDist > 700f) {
					targetDist = 700f;
				}
				if (isPriorityTarget && Projectile.ai[1] < 0) foundTarget = true;
				if (npc.CanBeChasedBy()) {
					Vector2 diff = npc.Hitbox.ClosestPointInRect(Projectile.Center) - Projectile.Center;
					float dist = diff.Length();
					if (dist > targetDist) return;
					float dot = OriginExtensions.NormDotWithPriorityMult(diff, Projectile.velocity, targetPriorityMultiplier);
					bool inRange = dist <= targetDist;
					bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
					if (((dot >= targetAngle && inRange) || !foundTarget) && lineOfSight) {
						targetDist = dist;
						targetAngle = dot;
						targetCenter = npc.Center;
						target = npc.whoAmI;
						foundTarget = true;
					}
				}
			}
			bool foundTarget = originPlayer.GetMinionTarget(targetingAlgorithm);


			Projectile.friendly = foundTarget;
			#endregion

			#region Movement
			// Default movement parameters (here for attacking)
			float speed = 6f;
			float turnSpeed = 1f;
			float currentSpeed = Projectile.velocity.Length();
			Projectile.tileCollide = true;
			if (foundTarget) {
				Projectile.tileCollide = true;
				if (Projectile.ai[0] != target) {
					Projectile.ai[0] = target;
					Projectile.ai[1] = 0;
				} else {
					if (++Projectile.ai[1] > 180) {
						Projectile.ai[1] = -30;
					}
				}
				if ((int)Math.Ceiling(targetAngle) == -1) {
					targetCenter.Y -= 16;
				}
			} else {
				if (distanceToIdlePosition > 640f) {
					Projectile.tileCollide = false;
					speed = 16f;
				} else if (distanceToIdlePosition < 64f) {
					speed = 4f;
					turnSpeed = 0;
				} else {
					speed = 6f;
				}
				if (!Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, idlePosition, 1, 1)) {
					Projectile.tileCollide = false;
				}
			}
			OriginExtensions.LinearSmoothing(ref currentSpeed, speed, currentSpeed < 1 ? 1 : 0.1f);
			Vector2 direction = foundTarget ? targetCenter - Projectile.Center : vectorToIdlePosition;
			Projectile.velocity = (Projectile.velocity + direction.SafeNormalize(default) * turnSpeed).SafeNormalize(default) * currentSpeed;
			#endregion

			#region Animation and visuals
			// So it will lean slightly towards the direction it's moving
			Projectile.rotation = (float)Math.Atan(Projectile.velocity.Y / Projectile.velocity.X);
			Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);

			// This is a simple "loop through all frames from top to bottom" animation
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= 5) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame = 0;
				}
			}
			#endregion
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => fallThrough = true;
		public override void OnKill(int timeLeft) {
			if (Main.myPlayer == Projectile.owner) {
				Player owner = Main.player[Projectile.owner];
				if (owner.statLife < owner.statLifeMax2 && Main.rand.NextBool(10)) {
					int item = Item.NewItem(Projectile.GetSource_Death(), Projectile.Hitbox, ItemID.Heart);
					if (Main.netMode == NetmodeID.MultiplayerClient) {
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
					}
				}
			}
		}
	}
}
