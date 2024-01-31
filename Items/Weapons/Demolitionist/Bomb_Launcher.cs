using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles.Weapons;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Demolitionist {
	public class Bomb_Launcher : ModItem {
		
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GrenadeLauncher);
			Item.damage = 2;
			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.shoot = ProjectileID.Bomb;
			Item.useAmmo = ItemID.Bomb;
			Item.knockBack = 1.6f;
			Item.shootSpeed = 6f;
			Item.value = Item.sellPrice(silver: 80);
			Item.rare = ItemRarityID.Green;
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
					SoundEngine.PlaySound(SoundID.Item14.WithPitchRange(1, 1), position);
					damage *= 10;
					knockback *= 3; Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
					return false;
				}
				if (type == ModContent.ProjectileType<Acid_Bomb_P>()) {
					position += velocity.SafeNormalize(Vector2.Zero) * 40;
					type = ModContent.ProjectileType<Acid_Shot>();
					damage -= 20;
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
			}
			if (type == ModContent.ProjectileType<Acid_Bomb_P>()) {
				damage -= 15;
			}
			return true;
		}
	}
	public class Awe_Bomb_P : ModProjectile {
		Vector2 oldVelocity;
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.timeLeft = 45;
			Projectile.penetrate = 1;
		}
		public override void OnSpawn(IEntitySource source) {
			oldVelocity = Projectile.velocity;
		}
		public override void AI() {
			float diff = (Projectile.velocity - oldVelocity).LengthSquared();
			if (diff > 256) {
				Projectile.timeLeft = 0;
			}
			oldVelocity = Projectile.velocity;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			return true;
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item38.WithVolume(0.75f), Projectile.Center);
			SoundEngine.PlaySound(Origins.Sounds.DeepBoom.WithVolume(5), Projectile.Center);
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Awe_Bomb_Blast>(), Projectile.damage, 24, Projectile.owner);
		}
	}
	public class Awe_Bomb_Blast : ModProjectile {
		
		public override string Texture => "Origins/Projectiles/Pixel";
		const int duration = 15;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.friendly = true;
			Projectile.hostile = true;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = duration;
			Projectile.width = Projectile.height = 160;
			Projectile.penetrate = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = duration;
			Projectile.tileCollide = false;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 closest = Projectile.Center.Clamp(targetHitbox.TopLeft(), targetHitbox.BottomRight());
			return (Projectile.Center - closest).Length() <= 160 * ((duration - Projectile.timeLeft) / (float)duration) * Projectile.scale;
		}
		public override bool? CanHitNPC(NPC target) {
			return target.friendly ? false : base.CanHitNPC(target);
		}
		public override bool CanHitPlayer(Player target) {
			Vector2 closest = Projectile.Center.Clamp(target.TopLeft, target.BottomRight);
			return (Projectile.Center - closest).Length() <= 160 * ((duration - Projectile.timeLeft) / (float)duration) * Projectile.scale;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= 1 - ((duration - Projectile.timeLeft) / (float)duration) * 0.6f;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.SourceDamage *= 1- ((duration - Projectile.timeLeft) / (float)duration) * 0.95f;
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
			Projectile.penetrate = 1;
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
