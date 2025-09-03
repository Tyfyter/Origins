using Microsoft.Xna.Framework.Graphics;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Weapons.Ranged;
using Origins.NPCs.Dungeon;
using Origins.Projectiles;
using PegasusLib;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Explosive_Barrel : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"ThrownExplosive",
			"ExpendableWeapon"
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.damage = 40;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.width = 18;
			Item.height = 20;
			Item.UseSound = SoundID.Item1;
			Item.useAnimation = 40;
			Item.useTime = 40;
			Item.knockBack = 5.75f;
			Item.DamageType = DamageClasses.ThrownExplosive;
			Item.shoot = ModContent.ProjectileType<Explosive_Barrel_P>();
			Item.shootSpeed = 9;
			Item.value = Item.sellPrice(silver: 1);
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.ArmorPenetration += 12;
		}
		public override void UseItemFrame(Player player) {
			if (player.itemAnimation >= player.itemAnimationMax / 2) {
				player.bodyFrame.Y = player.bodyFrame.Height * 1;
				player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, (MathHelper.Pi) * -player.direction);
			} else {
				player.bodyFrame.Y = player.bodyFrame.Height * 2;
				player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, (MathHelper.PiOver2 + 1) * -player.direction);
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			float speed = velocity.Length();
			if (speed == 0) return true;
			velocity /= speed;
			speed *= speed / 9f;
			velocity = new Vector2(player.direction * speed, 0).RotatedBy(player.direction * (velocity.Y * 0.5f - 0.5f));
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback);
			return false;
		}
	}
	public class Explosive_Barrel_P : ModProjectile {
		public override string Texture => typeof(Cellarkeep_Barrel).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ThrownExplosive;
			Projectile.height = Projectile.width = 26;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		int directionY = 1;
		public override void AI() {
			Player thrower = Main.player[Projectile.owner];
			if (Projectile.ai[0] == 0 && thrower.itemAnimation >= thrower.itemAnimationMax / 2) {
				Projectile.friendly = false;
				Projectile.Bottom = thrower.Top;
				Projectile.position.Y += 8;
				Projectile.direction = thrower.direction;
				thrower.heldProj = Projectile.whoAmI;
			} else {
				Projectile.ai[0] = 1;
				Projectile.friendly = true;
				Projectile.velocity.Y += 0.4f;
				if (++Projectile.frameCounter > 4) {
					if (Projectile.direction == 1) {
						switch ((Projectile.spriteDirection, directionY)) {
							case (1, 1):
							Projectile.spriteDirection = -1;
							break;
							case (-1, 1):
							directionY = -1;
							break;
							case (-1, -1):
							Projectile.spriteDirection = 1;
							break;
							case (1, -1):
							directionY = 1;
							break;
						}
					} else {
						switch ((Projectile.spriteDirection, directionY)) {
							case (1, 1):
							directionY = -1;
							break;
							case (1, -1):
							Projectile.spriteDirection = -1;
							break;
							case (-1, -1):
							directionY = 1;
							break;
							case (-1, 1):
							Projectile.spriteDirection = 1;
							break;
						}
					}
					Projectile.frameCounter = 0;
				}
				float speed = Projectile.velocity.X;
				Vector4 slopeCollision = Collision.SlopeCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
				Projectile.position = slopeCollision.XY();
				Projectile.velocity = slopeCollision.ZW();
				Projectile.velocity = Collision.TileCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
				if (Projectile.velocity.X != speed) {
					if (CritType.ModEnabled && Projectile.ai[1].TrySet(1)) {
						Projectile.velocity.X = -speed;
					} else {
						Projectile.Kill();
					}
				}
			}

		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, sound: SoundID.Item62);
		}
		public override bool PreDraw(ref Color lightColor) {
			SpriteEffects effects = SpriteEffects.None;
			if (Projectile.spriteDirection == -1) effects |= SpriteEffects.FlipHorizontally;
			if (directionY == -1) effects |= SpriteEffects.FlipVertically;
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.position - Main.screenPosition,
				null,
				lightColor,
				0,
				default,
				1,
				effects,
			0);
			return false;
		}
	}
	public class Explosive_Barrel_Crit_Type : CritType<Explosive_Barrel> {
		public override LocalizedText Description => CritMod.GetLocalization($"CritTypes.ShadowbeamStaff.Description");
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) => projectile?.ai[1] > 0;
		public override float CritMultiplier(Player player, Item item) => 3f;
	}
}
