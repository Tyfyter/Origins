using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Magic {
	public class The_Calibrator : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "MagicGun"
        ];
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.RainbowGun] = Type;
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.RainbowGun;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 48;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.width = 28;
			Item.height = 30;
			Item.useAnimation = 30;
			Item.useTime = 6;
			Item.reuseDelay = 7;
			Item.mana = 16;
			Item.shoot = ModContent.ProjectileType<The_Calibrator_P>();
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Yellow;
			Item.UseSound = Origins.Sounds.PhaserCrash.WithPitch(1);
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
		}
		public override Vector2? HoldoutOffset() => new(-8, -8);
		public override void HoldStyle(Player player, Rectangle heldItemFrame) {
			if (player.reuseDelay > 0) {

			}
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.reuseDelay <= 0 && player.ownedProjectileCounts[Item.shoot] > 0) {
				player.reuseDelay = Item.reuseDelay;
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			position += velocity * 5.3f + new Vector2(velocity.Y, -velocity.X) * player.direction * player.gravDir;
			velocity = velocity.RotatedByRandom(0.5f);
			float offset = Main.rand.NextFloat(0.2f, 1f);
			float hue = Main.rand.NextFloat(0, 1);
			float time = Main.rand.NextFloat(8, 19);
			Projectile.NewProjectile(
				source,
				position,
				velocity.RotatedBy(-offset),
				type,
				damage,
				knockback,
				player.whoAmI,
				offset / time,
				ai2: hue
			);
			Projectile.NewProjectile(
				source,
				position,
				velocity.RotatedBy(offset),
				type,
				damage,
				knockback,
				player.whoAmI,
				offset / -time,
				ai2: (hue + 1f / 3) % 1
			);
			SoundEngine.PlaySound(Item.UseSound, position);
			return false;
		}
	}
	public class The_Calibrator_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.timeLeft = 60;
			Projectile.penetrate = 5;
			Projectile.extraUpdates = 2;
			Projectile.aiStyle = 0;
			Projectile.width = Projectile.height = 10;
			Projectile.hide = true;
			Projectile.light = 0;
		}
		public override void AI() {
			Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[0]);
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi / 2;
			Color color = Main.hslToRgb(Projectile.ai[2], 1, 0.75f);
			Dust dust = Dust.NewDustPerfect(Projectile.Center, 309, null, newColor: color);
			dust.velocity = dust.velocity * 0.125f + Projectile.velocity * 0.15f;
			dust.noGravity = true;
			if (Projectile.ai[1] > 10) {
				Rectangle hitbox = Projectile.Hitbox;
				SoundStyle? sound = null;
				foreach (Projectile other in Main.ActiveProjectiles) {
					if (Projectile.whoAmI == other.whoAmI) continue;
					if (other.type == Type && other.owner == Projectile.owner && other.ai[1] > 10 && hitbox.Intersects(other.Hitbox)) {
						Projectile.Kill();
						other.Kill();
						Vector2 pos = (Projectile.Center + other.Center) * 0.5f;
						float hue;
						if (Math.Abs(Projectile.ai[2] - other.ai[2]) < 0.5f) {
							hue = (Projectile.ai[2] + other.ai[2]) * 0.5f;
						} else {
							hue = ((Projectile.ai[2] + other.ai[2]) * 0.5f + 0.5f) % 1;
						}
						color = Main.hslToRgb(hue, 1, 0.75f);
						for (int i = 0; i < 12; i++) {
							Dust.NewDustPerfect(pos, 309, null, newColor: color);
						}
						Player owner = Main.player[Projectile.owner];
						Vector2 diff = pos - owner.MountedCenter;
						owner.direction = Math.Sign(diff.X);
						owner.itemRotation = (diff * owner.direction).ToRotation();
						diff = diff.SafeNormalize(default);
						pos = owner.MountedCenter + diff * 48 + new Vector2(diff.Y, -diff.X) * 9 * owner.direction * owner.gravDir;
						Projectile.NewProjectile(
							owner.GetSource_ItemUse(owner.HeldItem),
							pos,
							diff * The_Calibrator_Beam.tick_motion,
							The_Calibrator_Beam.ID,
							Projectile.damage,
							Projectile.knockBack,
							Projectile.owner,
							ai0: hue
						);
						sound ??= Origins.Sounds.EnergyRipple.WithPitch(1);
						SoundEngine.PlaySound(sound, pos);
					}
				}
			} else {
				Projectile.ai[1]++;
			}
		}
		public override void OnKill(int timeLeft) {
		}
	}
	public class The_Calibrator_Beam : ModProjectile {
		public const int tick_motion = 8;
		const int max_length = 1200;
		public override string Texture => "Origins/Items/Weapons/Magic/The_Calibrator_P";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = max_length / tick_motion;
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = max_length + 16;
			Origins.HomingEffectivenessMultiplier[Type] = 4;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.extraUpdates = 25;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		public override bool PreDraw(ref Color lightColor) {
			The_Calibrator_Drawer drawer = default;
			drawer.Draw(Projectile);
			return false;
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			hitbox.Inflate(2, 2);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			for (int i = 1; i < Projectile.ai[1]; i++) {
				Vector2 pos = Projectile.oldPos[^i];
				if (pos == default) {
					break;
				} else if (projHitbox.Recentered(pos).Intersects(targetHitbox)) {
					return true;
				}
			}
			return null;
		}
		public override void AI() {
			if (Projectile.numUpdates == -1 && ++Projectile.ai[2] >= 14) {
				Projectile.Kill();
				return;
			}
			if (Projectile.velocity != default) {
				if (++Projectile.ai[1] > ProjectileID.Sets.TrailCacheLength[Type]) {
					StopMovement();
				} else {
					int index = (int)Projectile.ai[1];
					Projectile.oldPos[^index] = Projectile.Center;
					Projectile.oldRot[^index] = Projectile.velocity.ToRotation();
				}
			}
			if (++Projectile.frameCounter > 5) {
				Projectile.frame = (Projectile.frame + 1) % 4;
				Projectile.frameCounter = 0;
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Vector2 direction = oldVelocity.SafeNormalize(default);
			if (direction != default) {
				float[] samples = new float[3];
				Collision.LaserScan(
					Projectile.Center,
					direction,
					5,
					32,
					samples
				);
				if (samples.Average() > tick_motion * 0.5f) {
					Projectile.Center += direction * tick_motion;
					int index = (int)++Projectile.ai[1];
					if (index <= Projectile.oldPos.Length) {
						Projectile.oldPos[^index] = Projectile.Center;
						Projectile.oldRot[^index] = oldVelocity.ToRotation();
					}
				}
				StopMovement();
			}
			return false;
		}
		void StopMovement() {
			Projectile.velocity = Vector2.Zero;
			Projectile.extraUpdates = 0;
			Projectile.netUpdate = true;
		}
	}
	public struct The_Calibrator_Drawer {
		public const int TotalIllusions = 1;

		public const int FramesPerImportantTrail = 60;

		private static readonly VertexStrip _vertexStrip = new();

		public float Length;

		public Color color;
		public void Draw(Projectile proj) {
			Color _color = Main.hslToRgb(proj.ai[0], 1, 0.6f);
			_color.A = 0;
			color = _color * (1 - MathF.Pow(proj.ai[2] / 14f, 6));
			_color = Main.hslToRgb(proj.ai[0], 1, 0.5f);
			MiscShaderData miscShaderData = GameShaders.Misc["Origins:Beam"];
			float uTime = (float)Main.timeForVisualEffects / 44;
			int length = proj.oldPos.Length;
			float[] rot = new float[length];
			Vector2[] pos = new Vector2[length];
			Vector2 unit = default;
			int dustTimer = 0;
			for (int i = 0; i < length; i++) {
				Index reverseIndex = ^(i + 1);
				if (proj.oldPos[reverseIndex] == default) {
					length = i;
					Array.Resize(ref rot, length);
					Array.Resize(ref pos, length);
					break;
				}
				rot[i] = proj.oldRot[reverseIndex];
				pos[i] = proj.oldPos[reverseIndex];
				Lighting.AddLight(pos[i], color.ToVector3());
				unit = new Vector2(1, 0).RotatedBy(rot[i]);
				if (Main.rand.Next(++dustTimer) > 6) {
					Dust.NewDustPerfect(
						pos[i] + (new Vector2(unit.Y, -unit.X) * Main.rand.NextFloat(-4, 4)),
						309,
						unit * 5,
						newColor: _color,
						Scale: 0.85f
					).noGravity = true;
					dustTimer = Main.rand.NextBool() ? 2 : 0;
				}
			}
			if (length > 0) Dust.NewDustPerfect(
				pos[length - 1] + (new Vector2(unit.Y, -unit.X) * Main.rand.NextFloat(-4, 4)),
				309,
				unit * 5,
				newColor: _color
			).noGravity = true;
			Asset<Texture2D> texture = TextureAssets.Projectile[The_Calibrator_Beam.ID];
			miscShaderData.UseImage0(texture);
			miscShaderData.UseShaderSpecificData(texture.UVFrame(verticalFrames: 4, frameY: proj.frame));
			float endLength = (16f / The_Calibrator_Beam.tick_motion) / length;
			miscShaderData.Shader.Parameters["uLoopData"].SetValue(new Vector2(
				16f / 48f,
				endLength
			));
			miscShaderData.Apply();
			_vertexStrip.PrepareStrip(pos, rot, StripColors, StripWidth, -Main.screenPosition, length, includeBacksides: true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}

		private readonly Color StripColors(float progressOnStrip) => color;
		private readonly float StripWidth(float progressOnStrip) => 10f;
	}
}
