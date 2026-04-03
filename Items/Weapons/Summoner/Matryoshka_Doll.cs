using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Origins.Buffs;
using Origins.Items.Weapons.Summoner;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Projectiles.ArtifactMinionExtensions;

namespace Origins.Items.Weapons.Summoner {
	public class Matryoshka_Doll : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 60;
			Item.knockBack = 2f;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 14;
			Item.shootSpeed = 9f;
			Item.width = 24;
			Item.height = 38;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = true;
			Item.value = Item.sellPrice(silver: 1, copper: 50);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Matryoshka_Doll_Buff.ID;
			Item.shoot = Matryoshka_Doll_Minion.ID;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, (int)player.GetTotalDamage(Item.DamageType).CombineWith(player.OriginPlayer().artifactDamage).Undo(damage), knockback);
			return false;
		}
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Matryoshka_Doll_Buff : MinionBuff {
		public static int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Matryoshka_Doll_Minion.ID
		];
		public override bool IsArtifact => true;
		protected override void SetBuffFlag(Player player) => player.OriginPlayer().matryoshkaDoll = true;
	}
	[ReinitializeDuringResizeArrays]
	public class Matryoshka_Doll_Minion : MinionBase, IArtifactMinion, ISkipInMinionIndex {
		public static float Gravity => 0.2f;
		public static float Drag => 0.98f;
		public int MaxLife { get; set; }
		public float Life { get; set; }
		public static int ID { get; private set; }
		public bool DrawHealthBar(Vector2 position, float light, bool inBuffList) {
			if (Parent != -1) return false;
			if (inBuffList) {
				Main.instance.DrawHealthBar(
					position.X, position.Y,
					(int)(Size + 1),
					Main.projFrames[Type],
					light,
					0.85f
				);
			} else {
				Main.instance.DrawHealthBar(
					position.X, position.Y,
					(int)Life,
					MaxLife,
					light,
					0.85f
				);
			}
			return true;
		}
		public float SacrificeAvoidance => (Life / MaxLife) * (Projectile?.minionSlots ?? 1) + Size / 6f;
		public override Rectangle RestRegion => base.RestRegion.Modified(-8, (int)(Flying + 1) * 16, 16, (int)(Flying + 1) * -16);
		bool ISkipInMinionIndex.Skip => Parent != -1;
		public ref float Size => ref Projectile.ai[0];
		public ref float Flying => ref Projectile.ai[1];
		public ref float Child => ref Projectile.localAI[0];
		public ref float Parent => ref Projectile.ai[2];
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 7;
			// Sets the amount of frames this minion has on its spritesheet
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			ID = Type;
		}
		Point HitboxSize => Size switch {
			0 => new(8, 12),
			1 => new(10, 14),
			2 => new(14, 16),
			3 => new(18, 22),
			4 => new(22, 30),
			5 => new(24, 36),
			_ => new(28, 42),
		};
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 6;
			Projectile.manualDirectionChange = true;
			Projectile.netImportant = true;
			MaxLife = 100;
		}
		public override void OnSpawn(IEntitySource source) {
			Child = -1;
			Parent = -1;
			foreach (Projectile child in Main.ActiveProjectiles) {
				if (child.type != Type) continue;
				if (child.whoAmI == Projectile.whoAmI) continue;
				if (child.ai[2] >= 0) continue;
				if (child.ai[0] < 6) {
					Size = child.ai[0] + 1;
					Child = child.identity;
					child.ai[2] = Projectile.identity;

					Projectile.netUpdate = true;
					child.netUpdate = true;
					child.netSpam = 0;
					break;
				}
			}
		}
		public void PostApplyLifePrefixes(IEntitySource source) => MaxLife = (int)(MaxLife * float.Pow(1.468f, Size));
		protected override void BasicAI() {
			Projectile.hide = true;
			if (Parent >= 0) {
				if (Projectile.GetRelatedProjectile(2) is not Projectile shell || !shell.active || shell.type != Type || shell.localAI[0] != Projectile.identity) {
					Parent = -1;
				} else {
					Projectile.Bottom = shell.Bottom;
					Projectile.velocity = shell.velocity;
					Flying = 0;
					DoActiveCheck();
					return;
				}
			}
			Vector2 bottom = Projectile.Bottom;
			(Projectile.width, Projectile.height) = HitboxSize;
			Projectile.Bottom = bottom;
			Projectile.hide = false;
			Projectile.velocity *= Drag;
			if (Flying == 0) {
				Projectile.tileCollide = true;
				base.BasicAI();
				Projectile.rotation = Math.Clamp(Projectile.direction * Projectile.velocity.Y * -0.1f, -0.5f, 0.5f);
				Projectile.velocity.Y += Gravity;
			} else {
				Projectile.tileCollide = false;
				targetingData.TargetID = -1;
				DoActiveCheck();

				Vector2 targetPos = Projectile.Center.Clamp(RestRegion);
				Vector2 direction = (targetPos - Projectile.Center).Normalized(out float distance);
				float speed = distance switch {
					< 300f => 0.3f,
					< 600f => 0.6f,
					_ => 0.9f
				};
				Projectile.velocity += direction * speed;
				if (Vector2.Dot(Projectile.velocity.Normalized(out _), direction) < 0.25f)
					Projectile.velocity *= 0.8f;

				Projectile.velocity = Projectile.velocity.Normalized(out speed);
				if (speed > 8) speed *= 0.96f;
				Projectile.velocity *= Math.Min(speed, 15);
				Rectangle hitbox = Projectile.Hitbox;
				if (hitbox.Intersects(RestRegion) && !hitbox.OverlapsAnyTiles() && hitbox.Add(Vector2.UnitY * 24).OverlapsAnyTiles(false)) Flying = 0;
			}
		}
		public override void MoveTowardsTarget() {
			bool foundTarget = targetingData.TargetID != -1;
			Rectangle targetHitbox = foundTarget ? targetingData.targetHitbox : RestRegion;
			Vector2 targetPos = Projectile.Center.Clamp(targetHitbox);
			if (!Projectile.Center.IsWithin(Main.player[Projectile.owner].MountedCenter, foundTarget ? 1200 : 600)) {
				Flying = 1;
				return;
			}
			Projectile.direction = Math.Sign(targetPos.X - Projectile.Center.X);
			if (Projectile.direction == 0) Projectile.direction = 1;
			if (!foundTarget && targetPos.X == Projectile.Center.X) {
				Projectile.velocity.X *= 0.9f;
				return; 
			}
			if (Projectile.velocity.Y == 0) {
				Projectile.velocity.Y -= Math.Abs(targetPos.X - Projectile.Center.X) > 64 ? 4 : JumpHeight(Projectile.Bottom.Y - targetHitbox.Y);
				if (targetPos.X >= Projectile.TopLeft.X && targetPos.X <= Projectile.TopRight.X) Projectile.velocity.X += 4 * Projectile.direction;
			} else {
				Projectile.velocity.X += 0.1f * Projectile.direction;
			}
		}
		public static float JumpHeight(float targetHeight) {
			if (targetHeight <= 0) return 0;
			Max(ref targetHeight, 28);
			Min(ref targetHeight, 1080);
			float currentSpeed = 0;
			while (targetHeight > 0) {
				currentSpeed += Gravity;
				currentSpeed /= Drag;
				targetHeight -= currentSpeed;
			}
			return currentSpeed;
		}
		public override ref bool HasBuff(Player player) => ref player.OriginPlayer().matryoshkaDoll;
		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => Parent == -1;
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity *= 0.9f;
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.damage > 0) {
				hit.HitDirection *= -1;
				hit.Knockback = (9 - Size) * 0.75f;
				hit.Crit = false;
				Projectile.velocity = OriginExtensions.GetKnockbackFromHit(hit);
				this.DamageArtifactMinion(target.damage, new NPCDamageSource(target));
				if (Projectile.GetRelatedProjectile(1) is Projectile child && child.active && child.type == Type && child.ai[2] == Projectile.identity) {
					child.velocity = Projectile.velocity;
				}
			}
		}
		public void ModifyHurt(ref int damage, bool fromDoT) {
			if (!fromDoT) damage = (int)(damage - Size);
		}
		public void OnHurt(int damage, bool fromDoT) {
			if (!fromDoT) SoundEngine.PlaySound(SoundID.NPCHit3.WithPitch(1f - Size / 3f).WithVolume(1f), Projectile.Center);
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Math.Min((int)Size, Main.projFrames[Type]));
			Main.EntitySpriteDraw(
				texture,
				Projectile.Bottom + Vector2.UnitY * 2 - Main.screenPosition,
				frame,
				lightColor,
				Projectile.rotation,
				frame.Size() * new Vector2(0.5f, 1),
				Projectile.scale,
				Projectile.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None
			);
			return false;
		}
	}
}
