using CalamityMod.NPCs.TownNPCs;
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
			Item.damage = 12;
			Item.knockBack = 2f;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 14;
			Item.shootSpeed = 9f;
			Item.width = 24;
			Item.height = 38;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = true;
			Item.value = Item.sellPrice(silver: 1, copper: 50);
			Item.rare = ItemRarityID.Blue;
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
		public int MaxLife { get; set; }
		public float Life { get; set; }
		public static int ID { get; private set; }
		public bool DrawHealthBar(Vector2 position, float light, bool inBuffList) {
			if (Parent != -1) return false;
			Main.instance.DrawHealthBar(
				position.X, position.Y,
				(int)(Size + 1),
				Main.projFrames[Type],
				light,
				0.85f
			);
			return true;
		}
		public override Rectangle RestRegion => base.RestRegion.Modified(-8, 16, 16, -16);
		bool ISkipInMinionIndex.Skip => Parent != -1;
		public ref float Size => ref Projectile.ai[0];
		public ref float Child => ref Projectile.ai[1];
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
		Vector2 HitboxSize => Size switch {
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
			MaxLife = 25;
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
		protected override void BasicAI() {
			Projectile.hide = true;
			if (Parent >= 0) {
				if (Projectile.GetRelatedProjectile(2) is not Projectile shell || !shell.active || shell.type != Type || shell.ai[1] != Projectile.identity) {
					Parent = -1;
				} else {
					Projectile.Bottom = shell.Bottom;
					Projectile.velocity = shell.velocity;
					DoActiveCheck();
					return;
				}
			}
			Projectile.hide = false;
			Projectile.velocity *= 0.97f;
			base.BasicAI();
			Projectile.rotation = Projectile.direction * Projectile.velocity.Y * -0.1f;
			Projectile.velocity.Y += 0.2f;
		}
		public override void MoveTowardsTarget() {
			bool foundTarget = targetingData.TargetID != -1;
			Rectangle targetHitbox = foundTarget ? targetingData.targetHitbox : RestRegion;
			Vector2 targetPos = Projectile.Center.Clamp(targetHitbox);
			Projectile.direction = Math.Sign(targetPos.X - Projectile.Center.X);
			if (Projectile.direction == 0) Projectile.direction = 1;
			if (!foundTarget && targetPos.X == Projectile.Center.X) {
				Projectile.velocity.X *= 0.9f;
				return; 
			}
			if (Projectile.velocity.Y == 0) {
				Projectile.velocity.Y -= 4;
				Projectile.velocity.X += 4 * Projectile.direction;
			} else {
				Projectile.velocity.X += 0.1f * Projectile.direction;
			}
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
		public void OnHurt(int damage, bool fromDoT) {
			SoundEngine.PlaySound(SoundID.NPCHit3.WithPitch(1f).WithVolume(0.25f), Projectile.Center);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			(width, height) = HitboxSize.ToPoint();
			hitboxCenterFrac = new(0.5f, 1f);
			return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			if (Parent == -1) {
				(hitbox.Width, hitbox.Height) = HitboxSize.ToPoint();
				hitbox.X -= hitbox.Width / 2;
				hitbox.Y -= hitbox.Height;
			} else hitbox = default;
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
