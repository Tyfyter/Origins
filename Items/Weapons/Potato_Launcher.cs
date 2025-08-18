using Origins.Core;
using Origins.Items.Other.Consumables.Food;
using Origins.Projectiles;
using PegasusLib;
using PegasusLib.Networking;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

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
	public class Greater_Summoning_Potato_P : Potato_P {
		public static int ID { get; private set; }
		public override string Texture => "Origins/Items/Other/Consumables/Greater_Summoning_Potion_AF";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ID = Type;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target is NPC npc && !OriginsSets.NPCs.TargetDummies[npc.type]) new Greater_Summoning_Potato_Action(npc.netID, target.Center, target.velocity).Perform();
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			int type = NPCID.SmallBaldZombie;
			if (!target.Male) type = NPCID.BigFemaleZombie;
			new Greater_Summoning_Potato_Action(type, target.Center, target.velocity).Perform();
		}
	}

	public record class Greater_Summoning_Potato_Action(int type, Vector2 position, Vector2 velocity) : SyncedAction {
		public Greater_Summoning_Potato_Action() : this(default, default, default) { }
		public override SyncedAction NetReceive(BinaryReader reader) => this with {
			type = reader.ReadInt32(),
			position = reader.ReadPackedVector2(),
			velocity = reader.ReadPackedVector2()
		};
		public override void NetSend(BinaryWriter writer) {
			writer.Write(type);
			writer.WritePackedVector2(position);
			writer.WritePackedVector2(velocity);
		}
		protected override void Perform() {
			if (!NetmodeActive.MultiplayerClient) {
				NPC npc = NPC.NewNPCDirect(NPC.GetSource_None(), position, type);
				npc.velocity = velocity * MathF.Pow(1.2f, npc.knockBackResist);
				npc.value = 0;
				npc.SpawnedFromStatue = true;
			}
			SoundEngine.PlaySound(SoundID.Item2, position);
		}
	}
}
