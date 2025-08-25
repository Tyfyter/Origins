using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles.Weapons;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;
using Origins.Dev;
using PegasusLib;
using Origins.Projectiles;

namespace Origins.Items.Weapons.Demolitionist {
	public class Hand_Grenade_Launcher : ModItem, ICustomWikiStat {
		static short glowmask;
		public string[] Categories => [
			"Launcher"
		];
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToLauncher(16, 50, 44, 18);
			Item.shoot = ProjectileID.Grenade;
			Item.useAmmo = ItemID.Grenade;
			Item.shootSpeed = 5f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
			Item.glowMask = glowmask;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				if (type == ModContent.ProjectileType<Felnum_Shock_Grenade_P>()) {
					//damage -= 15;
					type = ModContent.ProjectileType<Awe_Grenade_P>();
					velocity *= 1.25f;
					knockback *= 3;
					Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
					return false;
				}
				if (type == ModContent.ProjectileType<Impact_Grenade_P>()) {
					type = ModContent.ProjectileType<Impact_Grenade_Blast>();
					position += velocity.SafeNormalize(Vector2.Zero) * 40;
					damage *= 2;
					knockback *= 3; Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
					return false;
				}
				if (type == ModContent.ProjectileType<Acid_Grenade_P>()) {
					position += velocity.SafeNormalize(Vector2.Zero) * 40;
					type = ModContent.ProjectileType<Brine_Droplet>();
					damage -= 20;
					for (int i = Main.rand.Next(2); ++i < 5;) {
						Projectile.NewProjectileDirect(source, position, velocity.RotatedByRandom(0.1 * i) * 0.6f, type, damage / 2, knockback, player.whoAmI).scale = 0.85f;
					}
					return false;
				}
				if (type == ModContent.ProjectileType<Crystal_Grenade_P>()) {
					position += velocity.SafeNormalize(Vector2.Zero) * 40;
					type = ModContent.ProjectileType<Crystal_Grenade_Shard>();
					damage -= 10;
					for (int i = Main.rand.Next(3); ++i < 10;) {
						int p = Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.025 * i) * 0.6f, type, damage / 2, knockback, player.whoAmI);
						Main.projectile[p].timeLeft += 90;
						Main.projectile[p].extraUpdates++;
					}
					return false;
				}
			}
			if (type == ModContent.ProjectileType<Acid_Grenade_P>()) {
				damage -= 15;// not doing anything, also, why is it here?
			}
			return true;
		}
	}
	public class Awe_Grenade_P : ModProjectile {
		Vector2 oldVelocity;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Awe Grenade");
			Origins.MagicTripwireRange[Type] = 32;
			ProjectileID.Sets.Explosive[Type] = true;
			ProjectileID.Sets.RocketsSkipDamageForPlayers[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.timeLeft = 45;
			Projectile.penetrate = 1;
		}
		public override void AI() {
			if (Projectile.localAI[0] != 0 && !Projectile.velocity.WithinRange(oldVelocity, 16)) {
				Projectile.timeLeft = 0;
			}
			Projectile.localAI[0] = 1;
			oldVelocity = Projectile.velocity;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) => true;
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item38.WithVolume(0.75f), Projectile.Center);
			SoundEngine.PlaySound(Origins.Sounds.DeepBoom.WithVolume(5), Projectile.Center);
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Awe_Grenade_Blast>(), Projectile.damage, 24, Projectile.owner);
		}
	}
	public class Awe_Grenade_Blast : ModProjectile, ISelfDamageEffectProjectile {
		
		public override string Texture => "Origins/Projectiles/Pixel";
		const int duration = 15;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = duration;
			Projectile.width = Projectile.height = 160;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = duration;
			Projectile.tileCollide = false;
			Projectile.appliesImmunityTimeOnSingleHits = true;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 closest = Projectile.Center.Clamp(targetHitbox.TopLeft(), targetHitbox.BottomRight());
			return (Projectile.Center - closest).Length() <= 160 * ((duration - Projectile.timeLeft) / (float)duration) * Projectile.scale;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= 1 - ((duration - Projectile.timeLeft) / (float)duration) * 0.6f;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.SourceDamage *= 1 - ((duration - Projectile.timeLeft) / (float)duration) * 0.95f;
		}
		public override void AI() {
			if (Projectile.localAI[0] == 0) ExplosiveGlobalProjectile.DealSelfDamage(Projectile, ImmunityCooldownID.DD2OgreKnockback);
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.spriteBatch.Restart(
				sortMode: SpriteSortMode.Immediate,
				samplerState: SamplerState.PointClamp,
				transformMatrix: Main.LocalPlayer.gravDir == 1f ? Main.GameViewMatrix.ZoomMatrix : Main.GameViewMatrix.TransformationMatrix
			);
			float percent = (duration - Projectile.timeLeft) / (float)duration;
			DrawData data = new DrawData(Main.Assets.Request<Texture2D>("Images/Misc/Perlin").Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 600, 600), new Color(new Vector4(0.35f, 0.35f, 0.35f, 0.6f) * (1f - percent)), 0, new Vector2(300f, 300f), new Vector2(percent, percent / 1.61803399f) * Projectile.scale, SpriteEffects.None, 0);
			GameShaders.Misc["ForceField"].UseColor(new Vector3(2f));
			GameShaders.Misc["ForceField"].Apply(data);
			data.Draw(Main.spriteBatch);
			Main.spriteBatch.Restart();
			return false;
		}

		public void OnSelfDamage(Player player, Player.HurtInfo info, double damageDealt) {
			if (damageDealt > 0) Projectile.localAI[0] = 1;
		}
	}
	public class Impact_Grenade_Blast : ModProjectile {
		
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2ExplosiveTrapT1Explosion;
		protected override bool CloneNewInstances => true;
		float dist;

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
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
