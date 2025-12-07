using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.NPCs;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Summoner {
	public class Joint_Pop : ModItem {
		public override void SetDefaults() {
			Item.DefaultToWhip(ModContent.ProjectileType<Joint_Pop_P>(), 33, 5, 4, 60);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.SummonMeleeSpeed];
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Blue;
		}
		public override bool MeleePrefix() => true;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI).scale *= player.GetAdjustedItemScale(Item);
			return false;
		}
	}
	public class Joint_Pop_P : ModProjectile, IWhipProjectile {
		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsAWhip[Type] = true;
			OriginsSets.Projectiles.NoMultishot[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.DefaultToWhip();
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.penetrate = 3;
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true; // This prevents the projectile from hitting through solid tiles.
			Projectile.extraUpdates = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		private float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Without PiOver2, the rotation would be off by 90 degrees counterclockwise.

			Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;

			Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

			float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;

			if (Timer >= swingTime || owner.ItemAnimationEndingOrEnded) {
				Projectile.Kill();
				return;
			}

			owner.heldProj = Projectile.whoAmI;

			// These two lines ensure that the timing of the owner's use animation is correct.
			owner.itemAnimation = owner.itemAnimationMax - (int)(Timer / Projectile.MaxUpdates);
			owner.itemTime = owner.itemAnimation;

			if (Timer == swingTime / 2) {
				// Plays a whipcrack sound at the tip of the whip.
				List<Vector2> points = Projectile.WhipPointsForCollision;
				Projectile.FillWhipControlPoints(Projectile, points);
				SoundEngine.PlaySound(SoundID.Item153, points[^1]);
			}
		}

		public void GetWhipSettings(out float timeToFlyOut, out int segments, out float rangeMultiplier) {
			timeToFlyOut = Main.player[Projectile.owner].itemAnimationMax * Projectile.MaxUpdates;
			segments = 20;
			rangeMultiplier = 0.7f * Projectile.scale;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.penetrate > 2) {
				List<Vector2> points = Projectile.WhipPointsForCollision;
				Projectile.FillWhipControlPoints(Projectile, points);
				Rectangle boomBox = new((int)points[^1].X - 48, (int)points[^1].Y - 48, 96, 96);
				if (target.Hitbox.Intersects(boomBox) && !Main.player[Projectile.owner].Hitbox.Intersects(boomBox)) {
					Projectile.NewProjectile(
						Projectile.GetSource_OnHit(target),
						points[^1],
						default,
						ModContent.ProjectileType<Joint_Pop_Explosion>(),
						Projectile.damage * 2,
						Projectile.knockBack * 2,
						Projectile.owner
					);
				} else {
					Projectile.penetrate++;
				}
			} else {
				Projectile.penetrate++;
			}
			target.AddBuff(Joint_Pop_Buff.ID, 240);
		}

		public override bool PreDraw(ref Color lightColor) {
			List<Vector2> list = new List<Vector2>();
			Projectile.FillWhipControlPoints(Projectile, list);

			SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Main.instance.LoadProjectile(Type);
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Vector2 pos = list[0];

			for (int i = 0; i < list.Count; i++) {
				// These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
				// You can change them if they don't!
				Rectangle frame = new Rectangle(0, 0, 48, 28);
				Vector2 origin = new Vector2(24, 14);
				Vector2 scale = new Vector2(0.85f) * Projectile.scale;

				if (i == list.Count - 1) {
					frame.Y = 112;
				} else if (i > 10) {
					frame.Y = 84;
				} else if (i > 5) {
					frame.Y = 56;
				} else if (i > 0) {
					frame.Y = 28;
				}

				Vector2 element = list[i];
				Vector2 diff;
				if (i == list.Count - 1) {
					diff = element - list[i - 1];
				} else {
					diff = list[i + 1] - element;
				}

				float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
				Color color = Lighting.GetColor(element.ToTileCoordinates());

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

				pos += diff;
			}
			return false;
		}
	}
	public class Joint_Pop_Explosion : ModProjectile, IIsExplodingProjectile {
		public override string Texture => "Origins/CrossMod/Thorium/Items/Weapons/Bard/Sonorous_Shredder_P";
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.SummonMeleeSpeed];
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item62);
				Projectile.ai[0] = 1;
			}
			if (Projectile.owner == Main.myPlayer && Projectile.ai[1] == 0) {
				Player player = Main.LocalPlayer;
				if (player.active && !player.dead && !player.immune) {
					Rectangle projHitbox = Projectile.Hitbox;
					ProjectileLoader.ModifyDamageHitbox(Projectile, ref projHitbox);
					Rectangle playerHitbox = new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height);
					if (projHitbox.Intersects(playerHitbox)) {
						player.Hurt(
							PlayerDeathReason.ByProjectile(Main.myPlayer, Projectile.whoAmI),
							Main.DamageVar(Projectile.damage, -player.luck),
							Math.Sign(player.Center.X - Projectile.Center.X),
							true
						);
						Projectile.ai[1] = 1;
					}
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Slow_Debuff.ID, 180);
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding() => true;
	}
	public class Joint_Pop_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().jointPopDebuff = true;
		}
	}
}
