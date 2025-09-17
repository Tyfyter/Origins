using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Projectiles;
using Origins.Projectiles.Weapons;
using PegasusLib;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;
namespace Origins.Items.Weapons.Demolitionist {
	public class Bomb_Launcher : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Launcher"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToLauncher(20, 50, 78, 30, true);
			Item.shoot = ProjectileID.Bomb;
			Item.useAmmo = ItemID.Bomb;
			Item.shootSpeed = 6f;
			Item.value = Item.sellPrice(silver: 80);
			Item.rare = ItemRarityID.Green;
			Item.ArmorPenetration -= 10;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-16, 2);
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				if (type == ModContent.ProjectileType<Impact_Bomb_P>()) {
					type = ModContent.ProjectileType<Impact_Bomb_Blast>();
					position += velocity.SafeNormalize(Vector2.Zero) * 40;
					damage *= 2;
					knockback = 8;
					Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
					return false;
				}
				if (type == ModContent.ProjectileType<Acid_Bomb_P>()) {
					position += velocity.SafeNormalize(Vector2.Zero) * 40;
					type = ModContent.ProjectileType<Brine_Droplet>();
					//damage -= 20;
					for (int i = Main.rand.Next(2); ++i < 5;) {
						Projectile.NewProjectileDirect(source, position, velocity.RotatedByRandom(0.1 * i) * 0.6f, type, damage / 3, knockback, player.whoAmI).scale = 0.85f;
					}
					return false;
				}
				if (type == ModContent.ProjectileType<Crystal_Bomb_P>()) {
					position += velocity.SafeNormalize(Vector2.Zero) * 40;
					type = ModContent.ProjectileType<Crystal_Grenade_Shard>();
					damage -= 10;
					for (int i = Main.rand.Next(3); ++i < 10;) {
						int p = Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.025 * i) * 0.6f, type, damage / 3, knockback, player.whoAmI);
						Main.projectile[p].timeLeft += 90;
						Main.projectile[p].extraUpdates++;
					}
					return false;
				}
				if (type == ModContent.ProjectileType<Shrapnel_Bomb_P>()) {
					position += velocity.SafeNormalize(Vector2.Zero) * 40;
					velocity /= 2.75f;
					type = Impeding_Shrapnel_Shard.ID;
					damage -= 10;
					for (int i = Main.rand.Next(3); ++i < 10;) {
						Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.025 * i) * 0.6f, type, damage, knockback, player.whoAmI, ai2: 0.15f);
					}
					return false;
				}
				if (type == ProjectileID.ScarabBomb) {
					Projectile proj = Projectile.NewProjectileDirect(source, position + velocity.SafeNormalize(velocity / 11) * 16, velocity, type, damage, knockback, player.whoAmI);
					proj.timeLeft = 2;
					proj.GetGlobalProjectile<ExplosiveGlobalProjectile>().selfDamageModifier *= 0;
					return false;
				}
			}
			return true;
		}
	}
	public class Impact_Bomb_Blast : ModProjectile {
		
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2ExplosiveTrapT1Explosion;
		protected override bool CloneNewInstances => true;
		float dist;

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 8;
			Projectile.width = Projectile.height = 5;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			if (Main.netMode != NetmodeID.Server && !TextureAssets.Projectile[694].IsLoaded) {
				Main.instance.LoadProjectile(694);
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Vector2 unit = Projectile.velocity.SafeNormalize(Vector2.Zero);
			Projectile.Center = player.MountedCenter + unit * 36 + unit.RotatedBy(MathHelper.PiOver2 * player.direction) * -2;
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.soundDelay <= 0) {
				SoundEngine.PlaySound(SoundID.Item14.WithPitchRange(1, 1), Projectile.Center);
				Projectile.soundDelay = Projectile.timeLeft * 20;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 closest = (Projectile.Center + Projectile.velocity * 2).Clamp(targetHitbox.TopLeft(), targetHitbox.BottomRight());
			double rot = GeometryUtils.AngleDif((closest - Projectile.Center).ToRotation(), Projectile.rotation, out _) + 0.5f;
			dist = (float)((Projectile.Center - closest).Length() * rot / 5.5f) + 1;
			return (Projectile.Center - closest).Length() <= 48 / rot;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage /= dist;
		}
		public override bool PreDraw(ref Color lightColor) {
			int frame = (8 - Projectile.timeLeft) / 2;
			Main.EntitySpriteDraw(TextureAssets.Projectile[694].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 80 * frame, 80, 80), lightColor, Projectile.rotation + MathHelper.PiOver2, new Vector2(40, 80), 1f, SpriteEffects.None, 0);
			return false;
		}
	}
}
